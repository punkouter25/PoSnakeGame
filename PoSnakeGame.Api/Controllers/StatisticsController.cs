using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure; // Required for RequestFailedException

namespace PoSnakeGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route will be /api/statistics
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> _logger;
        private readonly TableServiceClient _tableServiceClient;
        private const string HighScoresTableName = "HighScores";
        private const string StatsTableName = "GameStatistics";

        public StatisticsController(ILogger<StatisticsController> logger, TableServiceClient tableServiceClient)
        {
            _logger = logger;
            _tableServiceClient = tableServiceClient;
            _logger.LogInformation("StatisticsController instantiated."); // Debug log
        }

        // GET: api/statistics
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, int>>> GetGameStatistics()
        {
            _logger.LogInformation("Getting game statistics.");
            try
            {
                // Get TableClients
                var highScoresTable = _tableServiceClient.GetTableClient(HighScoresTableName);
                var statsTable = _tableServiceClient.GetTableClient(StatsTableName);
                await highScoresTable.CreateIfNotExistsAsync(); // Ensure tables exist
                await statsTable.CreateIfNotExistsAsync();

                var stats = new Dictionary<string, int>();

                // Get total games
                int totalGames = await GetStatValueAsync(statsTable, "GameCount", "total", "Count");
                stats["totalGames"] = totalGames;

                // Get high score statistics
                var scores = new List<int>();
                await foreach (var scoreEntity in highScoresTable.QueryAsync<TableEntity>())
                {
                    if (scoreEntity.TryGetValue("Score", out object scoreValue) && scoreValue is int value)
                    {
                        scores.Add(value);
                    }
                    // Handle potential string scores if needed from older data
                    else if (scoreEntity.TryGetValue("Score", out object scoreStringValue) && scoreStringValue is string scoreString && int.TryParse(scoreString, out int parsedValue))
                    {
                         scores.Add(parsedValue);
                    }
                }
                stats["highestScore"] = scores.Any() ? scores.Max() : 0;
                stats["averageScore"] = scores.Any() ? (int)scores.Average() : 0;

                // Get other statistics
                stats["totalFoodEaten"] = await GetStatValueAsync(statsTable, "FoodEaten", "total", "Count");
                stats["longestSnake"] = await GetStatValueAsync(statsTable, "LongestSnake", "record", "Length");
                stats["totalPlaytime"] = await GetStatValueAsync(statsTable, "PlayTime", "total", "Minutes");

                _logger.LogInformation("Successfully retrieved game statistics.");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving game statistics.");
                return StatusCode(500, "Internal server error retrieving game statistics.");
            }
        }

        // POST: api/statistics/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateGameStatistics([FromBody] Dictionary<string, int> stats)
        {
             _logger.LogInformation("Attempting to update game statistics.");
            if (stats == null)
            {
                _logger.LogWarning("UpdateGameStatistics called with null data.");
                return BadRequest("Invalid statistics data.");
            }

            try
            {
                var statsTable = _tableServiceClient.GetTableClient(StatsTableName);
                await statsTable.CreateIfNotExistsAsync(); // Ensure table exists

                // Update game count
                int currentGameCount = await GetStatValueAsync(statsTable, "GameCount", "total", "Count");
                var gameCountEntity = new TableEntity("GameCount", "total") { { "Count", currentGameCount + 1 } };
                await statsTable.UpsertEntityAsync(gameCountEntity);
                _logger.LogInformation($"Updated game count to {currentGameCount + 1}");

                // Update food eaten
                if (stats.TryGetValue("foodEaten", out int foodEaten) && foodEaten > 0)
                {
                    int currentFoodCount = await GetStatValueAsync(statsTable, "FoodEaten", "total", "Count");
                    var foodEntity = new TableEntity("FoodEaten", "total") { { "Count", currentFoodCount + foodEaten } };
                    await statsTable.UpsertEntityAsync(foodEntity);
                    _logger.LogInformation($"Updated food eaten count to {currentFoodCount + foodEaten}");
                }

                // Update longest snake
                if (stats.TryGetValue("snakeLength", out int snakeLength))
                {
                    int currentLongest = await GetStatValueAsync(statsTable, "LongestSnake", "record", "Length");
                    if (snakeLength > currentLongest)
                    {
                        var snakeEntity = new TableEntity("LongestSnake", "record") { { "Length", snakeLength } };
                        await statsTable.UpsertEntityAsync(snakeEntity);
                        _logger.LogInformation($"Updated longest snake to {snakeLength}");
                    }
                }

                // Update playtime (assuming playtime is in minutes)
                if (stats.TryGetValue("playTime", out int playTime) && playTime > 0)
                {
                    int currentPlayTime = await GetStatValueAsync(statsTable, "PlayTime", "total", "Minutes");
                    var timeEntity = new TableEntity("PlayTime", "total") { { "Minutes", currentPlayTime + playTime } };
                    await statsTable.UpsertEntityAsync(timeEntity);
                     _logger.LogInformation($"Updated total playtime to {currentPlayTime + playTime} minutes");
                }

                _logger.LogInformation("Successfully updated game statistics.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game statistics.");
                return StatusCode(500, "Internal server error updating game statistics.");
            }
        }

        // Helper to get a specific statistic value, returning 0 if not found or error
        private async Task<int> GetStatValueAsync(TableClient tableClient, string partitionKey, string rowKey, string valueKey)
        {
            try
            {
                var entity = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                if (entity.Value.TryGetValue(valueKey, out object rawValue))
                {
                    if (rawValue is int intValue) return intValue;
                    if (rawValue is long longValue) return (int)longValue; // Handle potential long values
                    if (rawValue is string stringValue && int.TryParse(stringValue, out int parsedValue)) return parsedValue;
                }
                return 0; // Key not found or wrong type
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"Statistic entity not found: PK={partitionKey}, RK={rowKey}. Returning 0.");
                return 0; // Entity not found
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, $"Error getting statistic value for PK={partitionKey}, RK={rowKey}, ValueKey={valueKey}. Returning 0.");
                 return 0; // Other error
            }
        }

        // Note: CORS is handled globally by the middleware configured in Program.cs
    }
}
