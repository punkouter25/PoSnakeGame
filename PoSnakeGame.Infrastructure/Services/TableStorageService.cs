using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoSnakeGame.Infrastructure.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableStorageConfig _config;
    private readonly ILogger<TableStorageService> _logger;
    private readonly TableClient _highScoresTable;

    public TableStorageService(TableStorageConfig config, ILogger<TableStorageService> logger)
    {
        _config = config;
        _logger = logger;
        
        try
        {
            var tableServiceClient = new TableServiceClient(_config.ConnectionString);
            _highScoresTable = tableServiceClient.GetTableClient(_config.HighScoresTableName);
            
            // Create table if it doesn't exist
            _highScoresTable.CreateIfNotExists();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing table storage");
            throw;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
    {
        try
        {
            _logger.LogInformation("Getting top {Count} scores", count);
            
            // Query all records with PartitionKey = "HighScore"
            var query = _highScoresTable.QueryAsync<HighScore>(filter: $"PartitionKey eq 'HighScore'");
            
            var scores = new List<HighScore>();
            await foreach (var score in query)
            {
                scores.Add(score);
            }
            
            // Order by score descending and take the specified count
            return scores.OrderByDescending(s => s.Score).Take(count).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high scores");
            throw;
        }
    }

    public async Task<bool> IsHighScore(int score)
    {
        try
        {
            var scores = await GetTopScoresAsync();
            
            // If we have fewer than 10 scores, it's automatically a high score
            if (scores.Count < 10)
            {
                return true;
            }
            
            // Otherwise, check if it's higher than the lowest score
            var lowestHighScore = scores.Min(s => s.Score);
            return score > lowestHighScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if {Score} is a high score", score);
            return true; // Assume it's a high score on error to allow submission
        }
    }

    public async Task SaveHighScoreAsync(HighScore highScore)
    {
        try
        {
            _logger.LogInformation("Saving high score for {Initials} with score {Score}", 
                highScore.Initials, highScore.Score);
            
            // Ensure we have a partition key and row key
            highScore.PartitionKey = "HighScore";
            highScore.RowKey = highScore.RowKey ?? Guid.NewGuid().ToString();
            
            // Add the entity to the table
            await _highScoresTable.AddEntityAsync(highScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving high score");
            throw;
        }
    }
}