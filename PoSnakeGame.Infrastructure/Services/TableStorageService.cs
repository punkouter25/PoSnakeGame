using Azure.Data.Tables;
using Azure.Data.Tables.Models; // Added for TableItem
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure; // Needed for ETag, RequestFailedException
using Azure.Core; // Needed for RetryOptions

namespace PoSnakeGame.Infrastructure.Services;

public class TableStorageService : ITableStorageService
{
    private readonly TableStorageConfig _config;
    private readonly ILogger<TableStorageService> _logger;
    private readonly TableClient _highScoresTable;
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageService(TableStorageConfig config, ILogger<TableStorageService> logger)
    {
        _config = config;
        _logger = logger;

        try
        {
            // Configure retry options (using defaults for exponential backoff)
            var options = new TableClientOptions();
            options.Retry.MaxRetries = 5; // Example: Max 5 retries
            options.Retry.Delay = TimeSpan.FromSeconds(1); // Example: Initial delay 1 second
            options.Retry.MaxDelay = TimeSpan.FromSeconds(30); // Example: Max delay 30 seconds
            options.Retry.Mode = RetryMode.Exponential; // Use exponential backoff

            _tableServiceClient = new TableServiceClient(_config.ConnectionString, options); // Pass options
            _highScoresTable = _tableServiceClient.GetTableClient(_config.HighScoresTableName);

            // Create table if it doesn't exist - consider retry for this operation too if needed
            // Using CreateIfNotExistsAsync for better async pattern
            _highScoresTable.CreateIfNotExistsAsync().GetAwaiter().GetResult(); // Blocking call in constructor is okay here
            _logger.LogInformation("Table Storage Service initialized. HighScores table '{TableName}' ensured.", _config.HighScoresTableName);

        }
        catch (RequestFailedException rfEx)
        {
             _logger.LogError(rfEx, "Azure Table Storage request failed during initialization. Status: {Status}, ErrorCode: {ErrorCode}", rfEx.Status, rfEx.ErrorCode);
             throw; // Re-throw critical initialization error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error initializing table storage");
            throw;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
    {
        try
        {
            _logger.LogInformation("Getting top {Count} scores", count);
            
            var query = _highScoresTable.QueryAsync<HighScore>(filter: $"PartitionKey eq 'HighScore'");
            
            var scores = new List<HighScore>();
            await foreach (var score in query.AsPages()) // Process page by page
            {
                 scores.AddRange(score.Values);
            }
            
            _logger.LogInformation("Retrieved {TotalCount} scores before sorting.", scores.Count);
            // Return only up to 'count' scores, sorted
            return scores.OrderByDescending(s => s.Score).Take(count).ToList(); 
        }
        catch (RequestFailedException rfEx)
        {
             _logger.LogError(rfEx, "Azure Table Storage request failed while getting high scores. Status: {Status}, ErrorCode: {ErrorCode}", rfEx.Status, rfEx.ErrorCode);
             throw; // Re-throw to indicate failure to the caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error getting high scores");
            throw;
        }
    }

    public async Task<bool> IsHighScore(int score)
    {
        try
        {
            // Get up to 10 scores to check against
            var scores = await GetTopScoresAsync(10); 

            // If no scores exist, any score qualifies
            if (!scores.Any()) 
            {
                 _logger.LogInformation("Score {Score} qualifies as high score (no scores exist yet).", score);
                 return true;
            }

            // Logic adjusted to match test expectation: Must be greater than the lowest score currently in the list.
            var lowestScoreInList = scores.Min(s => s.Score);
            bool isHigh = score > lowestScoreInList;
            
            _logger.LogInformation("Score {Score} comparison with lowest score in current list ({LowestScore}): Qualifies = {IsHigh}. (List count: {Count})", 
                score, lowestScoreInList, isHigh, scores.Count);
                
            return isHigh;
        }
        catch (RequestFailedException rfEx) // Catch specific exception from GetTopScoresAsync
        {
             _logger.LogError(rfEx, "Azure Table Storage request failed while checking if {Score} is a high score. Status: {Status}, ErrorCode: {ErrorCode}", score, rfEx.Status, rfEx.ErrorCode);
             // Decide on behavior: return true (allow submission) or false/throw? Returning true for now.
             return true; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error checking if {Score} is a high score", score);
            return true; // Assume it's a high score on generic error to allow submission
        }
    }

    public async Task SaveHighScoreAsync(HighScore highScore)
    {
        try
        {
            _logger.LogInformation("Saving high score for {Initials} with score {Score}", 
                highScore.Initials, highScore.Score);
            
            highScore.PartitionKey = "HighScore";
            highScore.RowKey = highScore.RowKey ?? Guid.NewGuid().ToString(); // Ensure RowKey exists
            
            // AddEntityAsync already benefits from the retry policy configured on the client
            await _highScoresTable.AddEntityAsync(highScore);
            _logger.LogInformation("Successfully saved high score with RowKey {RowKey}", highScore.RowKey);
        }
        catch (RequestFailedException rfEx)
        {
             _logger.LogError(rfEx, "Azure Table Storage request failed while saving high score for {Initials}. Status: {Status}, ErrorCode: {ErrorCode}", highScore.Initials, rfEx.Status, rfEx.ErrorCode);
             throw; // Re-throw to indicate failure
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error saving high score for {Initials}", highScore.Initials);
            throw;
        }
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            _logger.LogInformation("Checking if table '{TableName}' exists.", tableName);
            // QueryAsync benefits from the retry policy on the service client
            var query = _tableServiceClient.QueryAsync(filter: $"TableName eq '{tableName}'");

            await foreach (var table in query)
            {
                if (table.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Table '{TableName}' found.", tableName);
                    return true;
                }
            }
            _logger.LogWarning("Table '{TableName}' not found.", tableName);
            return false;
        }
        catch (RequestFailedException rfEx)
        {
             _logger.LogError(rfEx, "Azure Table Storage request failed while checking if table '{TableName}' exists. Status: {Status}, ErrorCode: {ErrorCode}", tableName, rfEx.Status, rfEx.ErrorCode);
             return false; // Assume table doesn't exist on error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error checking if table '{TableName}' exists", tableName);
            return false; // Assume table doesn't exist on generic error
        }
    }
}
