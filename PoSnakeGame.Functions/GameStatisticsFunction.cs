using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Azure.Data.Tables;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace PoSnakeGame.Functions
{
    public static class GameStatisticsFunction
    {
        private static readonly string TableName = "GameStatistics";
        
        [FunctionName("GetGameStatistics")]
        public static async Task<IActionResult> GetGameStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "statistics")] HttpRequest req,
            [Table("HighScores")] TableClient highScoresTable,
            [Table(TableName)] TableClient statsTable,
            ILogger log)
        {
            log.LogInformation("Getting game statistics");

            var stats = new Dictionary<string, int>();
            
            // Get total games
            var totalGames = 0;
            await foreach (var entry in statsTable.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'GameCount'"))
            {
                totalGames = int.Parse(entry.GetString("Count") ?? "0");
            }
            stats["totalGames"] = totalGames;

            // Get high score statistics
            var scores = new List<int>();
            await foreach (var score in highScoresTable.QueryAsync<TableEntity>())
            {
                if (int.TryParse(score.GetString("Score"), out int value))
                {
                    scores.Add(value);
                }
            }

            stats["highestScore"] = scores.Any() ? scores.Max() : 0;
            stats["averageScore"] = scores.Any() ? (int)scores.Average() : 0;

            // Get other statistics
            var foodEaten = 0;
            var longestSnake = 0;
            var totalPlaytime = 0;

            await foreach (var stat in statsTable.QueryAsync<TableEntity>())
            {
                switch (stat.PartitionKey)
                {
                    case "FoodEaten":
                        foodEaten = int.Parse(stat.GetString("Count") ?? "0");
                        break;
                    case "LongestSnake":
                        longestSnake = int.Parse(stat.GetString("Length") ?? "0");
                        break;
                    case "PlayTime":
                        totalPlaytime = int.Parse(stat.GetString("Minutes") ?? "0");
                        break;
                }
            }

            stats["totalFoodEaten"] = foodEaten;
            stats["longestSnake"] = longestSnake;
            stats["totalPlaytime"] = totalPlaytime;

            return new OkObjectResult(stats);
        }

        [FunctionName("UpdateGameStatistics")]
        public static async Task<IActionResult> UpdateGameStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "statistics")] HttpRequest req,
            [Table(TableName)] TableClient tableClient,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var stats = JsonSerializer.Deserialize<Dictionary<string, int>>(requestBody);

            if (stats == null)
            {
                return new BadRequestObjectResult("Invalid statistics data");
            }

            // Update game count
            var gameCountEntity = new TableEntity("GameCount", "total")
            {
                { "Count", (await GetCurrentCount(tableClient, "GameCount", "total") + 1).ToString() }
            };
            await tableClient.UpsertEntityAsync(gameCountEntity);

            // Update food eaten
            if (stats.TryGetValue("foodEaten", out int foodEaten))
            {
                var foodEntity = new TableEntity("FoodEaten", "total")
                {
                    { "Count", (await GetCurrentCount(tableClient, "FoodEaten", "total") + foodEaten).ToString() }
                };
                await tableClient.UpsertEntityAsync(foodEntity);
            }

            // Update longest snake
            if (stats.TryGetValue("snakeLength", out int snakeLength))
            {
                var currentLongest = await GetCurrentCount(tableClient, "LongestSnake", "record");
                if (snakeLength > currentLongest)
                {
                    var snakeEntity = new TableEntity("LongestSnake", "record")
                    {
                        { "Length", snakeLength.ToString() }
                    };
                    await tableClient.UpsertEntityAsync(snakeEntity);
                }
            }

            // Update playtime
            if (stats.TryGetValue("playTime", out int playTime))
            {
                var timeEntity = new TableEntity("PlayTime", "total")
                {
                    { "Minutes", (await GetCurrentCount(tableClient, "PlayTime", "total") + playTime).ToString() }
                };
                await tableClient.UpsertEntityAsync(timeEntity);
            }

            return new OkResult();
        }

        private static async Task<int> GetCurrentCount(TableClient tableClient, string partitionKey, string rowKey)
        {
            try
            {
                var entity = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
                return int.Parse(entity.Value.GetString("Count") ?? "0");
            }
            catch
            {
                return 0;
            }
        }
    }
}