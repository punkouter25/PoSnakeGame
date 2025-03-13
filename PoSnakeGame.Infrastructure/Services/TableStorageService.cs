using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Configuration;

namespace PoSnakeGame.Infrastructure.Services;

public class TableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly TableStorageConfig _config;
    private readonly ILogger<TableStorageService> _logger;

    public TableStorageService(TableStorageConfig config, ILogger<TableStorageService> logger)
    {
        _config = config;
        _logger = logger;
        _tableServiceClient = new TableServiceClient(_config.ConnectionString);
        
        // Ensure tables exist
        var highScoresTable = _tableServiceClient.GetTableClient(_config.HighScoresTableName);
        var statsTable = _tableServiceClient.GetTableClient(_config.GameStatisticsTableName);
        
        highScoresTable.CreateIfNotExists();
        statsTable.CreateIfNotExists();
    }

    public async Task SaveHighScoreAsync(HighScore score)
    {
        try
        {
            var table = _tableServiceClient.GetTableClient(_config.HighScoresTableName);
            await table.AddEntityAsync(score);
            _logger.LogInformation("Saved high score for {Initials}: {Score}", score.Initials, score.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving high score");
            throw;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
    {
        try
        {
            var table = _tableServiceClient.GetTableClient(_config.HighScoresTableName);
            var scores = table.QueryAsync<HighScore>(score => score.PartitionKey == "HighScore");
            
            var topScores = new List<HighScore>();
            await foreach (var score in scores)
            {
                topScores.Add(score);
            }

            return topScores.OrderByDescending(s => s.Score).Take(count).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving high scores");
            throw;
        }
    }

    public async Task<bool> IsHighScore(int score)
    {
        var topScores = await GetTopScoresAsync();
        return topScores.Count < 10 || score > topScores.Min(s => s.Score);
    }
}