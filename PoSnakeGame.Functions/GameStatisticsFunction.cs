using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Azure.Data.Tables;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Net;

namespace PoSnakeGame.Functions
{
    public class GameStatisticsFunction
    {
        [Function("GetGameStatistics")]
        public async Task<HttpResponseData> GetGameStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "statistics")] HttpRequestData req,
            [TableInput("HighScores")] TableClient highScoresTable,
            [TableInput("GameStatistics")] TableClient statsTable,
            FunctionContext context)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse);
                return preflightResponse;
            }

            var logger = context.GetLogger<GameStatisticsFunction>();
            logger.LogInformation("Getting game statistics");

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

            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response);
            await response.WriteAsJsonAsync(stats);
            return response;
        }

        [Function("UpdateGameStatistics")]
        public async Task<HttpResponseData> UpdateGameStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "statistics/update")] HttpRequestData req,
            [TableInput("GameStatistics")] TableClient tableClient,
            FunctionContext context)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                preflightResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                preflightResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
                preflightResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                return preflightResponse;
            }

            var logger = context.GetLogger<GameStatisticsFunction>();
            var stats = await req.ReadFromJsonAsync<Dictionary<string, int>>();

            if (stats == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                badResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                badResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
                badResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                await badResponse.WriteStringAsync("Invalid statistics data");
                return badResponse;
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

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return response;
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

        private static void AddCorsHeaders(HttpResponseData response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }
    }
}
