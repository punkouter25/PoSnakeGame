using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Azure.Data.Tables;
using PoSnakeGame.Core.Models;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Net;

namespace PoSnakeGame.Functions
{
    public class HighScoresFunction
    {
        [Function("GetHighScores")]
        public async Task<HttpResponseData> GetHighScores(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "highscores")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            FunctionContext context)
        {
            var logger = context.GetLogger<HighScoresFunction>();
            logger.LogInformation("Getting high scores");
            
            var scores = new List<HighScore>();
            await foreach (var score in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(score);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(scores.OrderByDescending(s => s.Score).Take(10));
            return response;
        }

        [Function("SaveHighScore")]
        public async Task<HttpResponseData> SaveHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "highscores")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            FunctionContext context)
        {
            var logger = context.GetLogger<HighScoresFunction>();
            var highScore = await req.ReadFromJsonAsync<HighScore>();

            if (highScore == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid high score data");
                return badResponse;
            }

            highScore.PartitionKey = "HighScore";
            highScore.RowKey = Guid.NewGuid().ToString();
            highScore.Timestamp = DateTimeOffset.UtcNow;

            await tableClient.AddEntityAsync(highScore);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(highScore);
            return response;
        }

        [Function("IsHighScore")]
        public async Task<HttpResponseData> IsHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "highscores/check/{score}")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            int score,
            FunctionContext context)
        {
            var logger = context.GetLogger<HighScoresFunction>();
            logger.LogInformation($"Checking if {score} is a high score");

            var scores = new List<HighScore>();
            await foreach (var highScore in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(highScore);
            }

            var topScores = scores.OrderByDescending(s => s.Score).Take(10);
            bool isHighScore = scores.Count < 10 || score > topScores.Min(s => s.Score);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(isHighScore);
            return response;
        }
    }
}