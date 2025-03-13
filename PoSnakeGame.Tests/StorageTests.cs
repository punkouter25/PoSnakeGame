using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;

namespace PoSnakeGame.Tests;

public class StorageTests
{
    private readonly TableStorageConfig _config;
    private readonly ILogger<TableStorageService> _logger;

    public StorageTests()
    {
        _config = new TableStorageConfig
        {
            ConnectionString = "UseDevelopmentStorage=true", // Uses local Azurite
            HighScoresTableName = "TestHighScores",
            GameStatisticsTableName = "TestGameStats"
        };

        // Create test logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _logger = loggerFactory.CreateLogger<TableStorageService>();
    }

    [Fact]
    public async Task HighScore_Should_Be_Saved_And_Retrieved()
    {
        // Arrange
        var service = new TableStorageService(_config, _logger);
        var score = new HighScore
        {
            Initials = "TST",
            Score = 100,
            Date = DateTime.UtcNow,
            GameDuration = 25.5f,
            SnakeLength = 5,
            FoodEaten = 4
        };

        // Act
        await service.SaveHighScoreAsync(score);
        var scores = await service.GetTopScoresAsync(10);

        // Assert
        Assert.Contains(scores, s => 
            s.Initials == score.Initials && 
            s.Score == score.Score);
    }
}