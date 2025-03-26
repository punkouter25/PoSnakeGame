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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "highscores")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            FunctionContext context)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse, req);
                return preflightResponse;
            }

            var logger = context.GetLogger<HighScoresFunction>();
            logger.LogInformation("Getting high scores");
            
            // Create table if it doesn't exist
            await tableClient.CreateIfNotExistsAsync();
            
            var scores = new List<HighScore>();
            await foreach (var score in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(score);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response, req);
            await response.WriteAsJsonAsync(scores.OrderByDescending(s => s.Score).Take(10));
            return response;
        }

        [Function("SaveHighScore")]
        public async Task<HttpResponseData> SaveHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "highscores/save")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            FunctionContext context)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse, req);
                return preflightResponse;
            }

            var logger = context.GetLogger<HighScoresFunction>();
            
            // Create table if it doesn't exist
            await tableClient.CreateIfNotExistsAsync();
            
            var highScore = await req.ReadFromJsonAsync<HighScore>();

            if (highScore == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCorsHeaders(badResponse, req);
                await badResponse.WriteStringAsync("Invalid high score data");
                return badResponse;
            }

            highScore.PartitionKey = "HighScore";
            highScore.RowKey = Guid.NewGuid().ToString();
            highScore.Timestamp = DateTimeOffset.UtcNow;

            await tableClient.AddEntityAsync(highScore);

            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response, req);
            await response.WriteAsJsonAsync(highScore);
            return response;
        }

        [Function("IsHighScore")]
        public async Task<HttpResponseData> IsHighScore(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "highscores/check/{score}")] HttpRequestData req,
            [TableInput("HighScores")] TableClient tableClient,
            int score,
            FunctionContext context)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse, req);
                return preflightResponse;
            }

            var logger = context.GetLogger<HighScoresFunction>();
            logger.LogInformation($"Checking if {score} is a high score");

            // Create table if it doesn't exist
            await tableClient.CreateIfNotExistsAsync();

            var scores = new List<HighScore>();
            await foreach (var highScore in tableClient.QueryAsync<HighScore>())
            {
                scores.Add(highScore);
            }

            var topScores = scores.OrderByDescending(s => s.Score).Take(10);
            bool isHighScore = scores.Count < 10 || (scores.Count > 0 && score > topScores.Min(s => s.Score));

            var response = req.CreateResponse(HttpStatusCode.OK);
            AddCorsHeaders(response, req);
            await response.WriteAsJsonAsync(isHighScore);
            return response;
        }

        private static void AddCorsHeaders(HttpResponseData response, HttpRequestData req)
        {
            // Get the origin from the request headers safely
            string origin = null;
            if (req.Headers.Contains("Origin"))
            {
                origin = req.Headers.GetValues("Origin").FirstOrDefault() ?? "";
            }
            
            // Check if the origin is in our allowed list
            var allowedOrigins = new[]
            {
                "http://localhost:5000",
                "http://localhost:5001",
                "https://localhost:5000",
                "https://localhost:5001",
                "http://localhost:5297",
                "https://localhost:7047",
                "http://127.0.0.1:5000",
                "http://127.0.0.1:5001",
                "https://127.0.0.1:5000", 
                "https://127.0.0.1:5001",
                "http://127.0.0.1:5297",
                "https://127.0.0.1:7047",
                "https://zealous-river-059a32e0f.6.azurestaticapps.net",
                "https://posnakegame-web.azurestaticapps.net"
            };
            
            // Only add the specific origin if it's in our allowed list
            if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
            }
            else
            {
                // Fallback to the main production URL or localhost for development
                response.Headers.Add("Access-Control-Allow-Origin", "https://localhost:7047");
            }
            
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Max-Age", "86400"); // 24 hours
        }
    }
}
