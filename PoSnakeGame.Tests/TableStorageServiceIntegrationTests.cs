using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using PoSnakeGame.Infrastructure.Services;
using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Core.Models;
using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using System.Linq;
using System.Collections.Generic;

namespace PoSnakeGame.Tests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Initialized in InitializeAsync.
    // Integration tests require Azurite emulator to be running
    public class TableStorageServiceIntegrationTests : IAsyncLifetime
    {
        private TableStorageService _service;
        private TableClient _tableClient;
        private readonly string _tableName = "TestHighScores" + Guid.NewGuid().ToString("N"); // Unique table name for test run
        private readonly string _connectionString = "UseDevelopmentStorage=true"; // Standard Azurite connection

        public async Task InitializeAsync()
        {
            // Setup: Create the service and ensure the test table exists and is empty
            var config = new TableStorageConfig
            {
                ConnectionString = _connectionString,
                HighScoresTableName = _tableName // Use the unique test table name
            };
            var logger = NullLogger<TableStorageService>.Instance; // Use NullLogger for tests

            // Instantiate the service - this will create the table via its constructor
            _service = new TableStorageService(config, logger);

            // Get a client reference for direct manipulation/cleanup
            var serviceClient = new TableServiceClient(_connectionString);
            _tableClient = serviceClient.GetTableClient(_tableName);
            
            // Ensure table is clean before tests (delete all entities)
            await ClearTableAsync(); 
        }

        public async Task DisposeAsync()
        {
            // Teardown: Delete the test table after tests run
            try
            {
                await _tableClient.DeleteAsync();
            }
            catch (Exception ex)
            {
                // Log error during cleanup if necessary
                Console.WriteLine($"Error deleting test table {_tableName}: {ex.Message}");
            }
        }

        private async Task ClearTableAsync()
        {
            var entities = _tableClient.QueryAsync<TableEntity>();
            await foreach (var entity in entities)
            {
                await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
            }
        }

        [Fact]
        public async Task SaveHighScoreAsync_ShouldSaveEntityToTable()
        {
            // Arrange
            var highScore = new HighScore
            {
                Initials = "INT",
                Score = 500,
                Date = DateTime.UtcNow, // Ensure Date is UTC
                GameDuration = 120.5f,
                SnakeLength = 15,
                FoodEaten = 10
                // PartitionKey and RowKey will be set by the service
            };

            // Act
            await _service.SaveHighScoreAsync(highScore);

            // Assert
            // Retrieve the entity directly to verify it was saved
            // Note: highScore.RowKey is set within SaveHighScoreAsync, so we need to query
            var savedEntities = _tableClient.QueryAsync<HighScore>(filter: $"PartitionKey eq 'HighScore' and Initials eq 'INT'");
            var savedList = new List<HighScore>();
            await foreach(var entity in savedEntities) { savedList.Add(entity); }

            Assert.Single(savedList);
            Assert.Equal("INT", savedList[0].Initials);
            Assert.Equal(500, savedList[0].Score);
        }

        [Fact]
        public async Task GetTopScoresAsync_ShouldReturnCorrectlySortedScores()
        {
            // Arrange
            await _service.SaveHighScoreAsync(new HighScore { Initials = "P1", Score = 100, Date = DateTime.UtcNow });
            await _service.SaveHighScoreAsync(new HighScore { Initials = "P2", Score = 300, Date = DateTime.UtcNow });
            await _service.SaveHighScoreAsync(new HighScore { Initials = "P3", Score = 200, Date = DateTime.UtcNow });

            // Act
            var topScores = await _service.GetTopScoresAsync(10);

            // Assert
            Assert.Equal(3, topScores.Count);
            Assert.Equal(300, topScores[0].Score);
            Assert.Equal("P2", topScores[0].Initials);
            Assert.Equal(200, topScores[1].Score);
            Assert.Equal("P3", topScores[1].Initials);
            Assert.Equal(100, topScores[2].Score);
            Assert.Equal("P1", topScores[2].Initials);
        }
        
        [Fact]
        public async Task GetTopScoresAsync_ShouldLimitResults()
        {
            // Arrange
            for(int i = 1; i <= 5; i++)
            {
                 await _service.SaveHighScoreAsync(new HighScore { Initials = $"T{i}", Score = i * 10, Date = DateTime.UtcNow });
            }

            // Act
            var topScores = await _service.GetTopScoresAsync(3); // Request top 3

            // Assert
            Assert.Equal(3, topScores.Count);
            Assert.Equal(50, topScores[0].Score); // Highest score
            Assert.Equal(40, topScores[1].Score);
            Assert.Equal(30, topScores[2].Score); // Third highest
        }

        [Theory]
        [InlineData(150, 3, 100, true)] // Score 150, 3 scores exist (lowest is >100), qualifies (> lowest)
        [InlineData(50, 3, 100, false)] // Score 50, 3 scores exist (lowest is >100), does not qualify (<= lowest) - Reverted expectation
        [InlineData(150, 12, 100, true)] // Score 150, 12 scores exist (100 lowest), qualifies (> lowest)
        [InlineData(50, 12, 100, false)] // Score 50, 12 scores exist (100 lowest), does not qualify (<= lowest)
        [InlineData(100, 12, 100, false)] // Score 100, 12 scores exist (100 lowest), does not qualify (<= lowest)
        public async Task IsHighScore_ShouldReturnCorrectResult(int scoreToCheck, int existingScoresCount, int lowestScoreValue, bool expectedResult)
        {
             // Arrange
            await ClearTableAsync(); // Ensure clean state
            // Add existing scores (lowest will be lowestScoreValue if count >= 10)
            for (int i = 0; i < existingScoresCount; i++)
            {
                // Ensure the lowest score is indeed lowestScoreValue if we have >= 10 scores
                int score = (existingScoresCount >= 10 && i == 0) ? lowestScoreValue : lowestScoreValue + i + 1; 
                await _service.SaveHighScoreAsync(new HighScore { Initials = $"E{i}", Score = score, Date = DateTime.UtcNow });
            }

            // Act
            var isHigh = await _service.IsHighScore(scoreToCheck);

            // Assert
            Assert.Equal(expectedResult, isHigh);
        }
        
        [Fact]
        public async Task TableExistsAsync_ShouldReturnTrue_ForExistingTable()
        {
            // Arrange (Table is created in InitializeAsync)
            
            // Act
            var exists = await _service.TableExistsAsync(_tableName);
            
            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task TableExistsAsync_ShouldReturnFalse_ForNonExistingTable()
        {
            // Arrange
            string nonExistentTableName = "NonExistentTable" + Guid.NewGuid().ToString("N");
            
            // Act
            var exists = await _service.TableExistsAsync(nonExistentTableName);
            
            // Assert
            Assert.False(exists);
        }
    }
#pragma warning restore CS8618
}
