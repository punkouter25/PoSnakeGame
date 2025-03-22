using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Azure.Data.Tables;
using PoSnakeGame.Core.Models;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace PoSnakeGame.Functions
{
    public static class HighScoresFunction
    {
        private static readonly string TableName = "HighScores";

        [FunctionName("GetHighScores")]
        public static async Task<IActionResult> GetHighScores(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "highscores")] HttpRequest req,
            [Table(TableName)] TableClient tableClient,
            ILogger log)
        {
            log.LogInformation("Getting high scores");
            
            var scores = new List<HighScore>();
            await foreach (var score in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(score);
            }

            return new OkObjectResult(scores.OrderByDescending(s => s.Score).Take(10));
        }

        [FunctionName("SaveHighScore")]
        public static async Task<IActionResult> SaveHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "highscores")] HttpRequest req,
            [Table(TableName)] TableClient tableClient,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var highScore = JsonSerializer.Deserialize<HighScore>(requestBody);

            if (highScore == null)
            {
                return new BadRequestObjectResult("Invalid high score data");
            }

            highScore.PartitionKey = "HighScore";
            highScore.RowKey = Guid.NewGuid().ToString();
            highScore.Timestamp = DateTimeOffset.UtcNow;

            await tableClient.AddEntityAsync(highScore);

            return new OkObjectResult(highScore);
        }

        [FunctionName("IsHighScore")]
        public static async Task<IActionResult> IsHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "highscores/check/{score}")] HttpRequest req,
            [Table(TableName)] TableClient tableClient,
            int score,
            ILogger log)
        {
            log.LogInformation($"Checking if {score} is a high score");

            var scores = new List<HighScore>();
            await foreach (var highScore in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(highScore);
            }

            var topScores = scores.OrderByDescending(s => s.Score).Take(10);
            bool isHighScore = scores.Count < 10 || score > topScores.Min(s => s.Score);

            return new OkObjectResult(isHighScore);
        }
    }
}