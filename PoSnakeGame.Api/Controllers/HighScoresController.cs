using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using Azure.Data.Tables; // No longer needed as we use the service abstraction

namespace PoSnakeGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route will be /api/highscores
    public class HighScoresController : ControllerBase
    {
        private readonly ILogger<HighScoresController> _logger;
        private readonly ITableStorageService _tableStorageService;
        private const string HighScoresTableName = "HighScores"; // Define table name constant

        public HighScoresController(ILogger<HighScoresController> logger, ITableStorageService tableStorageService)
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
            _logger.LogInformation("HighScoresController instantiated."); // Debug log
        }

        // GET: api/highscores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HighScore>>> GetHighScores()
        {
            _logger.LogInformation("Getting top 10 high scores via service.");
            try
            {
                // The service method GetTopScoresAsync handles retrieval and sorting
                var topScores = await _tableStorageService.GetTopScoresAsync(10); // Use the service method

                _logger.LogInformation($"Retrieved {topScores.Count} high scores."); // Debug log count
                return Ok(topScores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving high scores.");
                return StatusCode(500, "Internal server error retrieving high scores.");
            }
        }

        // POST: api/highscores/save
        [HttpPost("save")]
        public async Task<ActionResult<HighScore>> SaveHighScore([FromBody] HighScore highScore)
        {
            if (highScore == null)
            {
                _logger.LogWarning("SaveHighScore called with null data.");
                return BadRequest("Invalid high score data.");
            }

            _logger.LogInformation($"Attempting to save high score for {highScore.Initials} with score {highScore.Score}."); // Use Initials

            try
            {
                // The service method SaveHighScoreAsync handles table creation and entity setup
                // We might not need to set PartitionKey/RowKey here if the service does it.
                // Let's assume the service expects a complete HighScore object for now.
                // If SaveHighScoreAsync modifies the object (e.g., adds RowKey), that's fine.
                highScore.PartitionKey = "HighScore"; // Keep setting PK for clarity, service might override/ignore
                highScore.RowKey = Guid.NewGuid().ToString(); // Keep setting RK for clarity, service might override/ignore

                await _tableStorageService.SaveHighScoreAsync(highScore); // Use the service method

                _logger.LogInformation($"Successfully called SaveHighScoreAsync for {highScore.Initials}.");
                // Return the original object or fetch the saved one if needed, depends on service behavior.
                // Returning the input object is simpler for now.
                return Ok(highScore); // Return the input object (service might have updated it)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving high score for {highScore.Initials}."); // Use Initials
                return StatusCode(500, "Internal server error saving high score.");
            }
        }

        // GET: api/highscores/check/{score}
        [HttpGet("check/{score}")]
        public async Task<ActionResult<bool>> IsHighScore(int score)
        {
            _logger.LogInformation($"Checking if score {score} is a high score via service.");
            try
            {
                // Use the service method directly
                bool isHighScore = await _tableStorageService.IsHighScore(score);

                _logger.LogInformation($"Service determined score {score} qualifies as high score: {isHighScore}");
                return Ok(isHighScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if {score} is a high score.");
                return StatusCode(500, "Internal server error checking high score.");
            }
        }

        // Note: CORS is handled globally by the middleware configured in Program.cs
        // No need for AddCorsHeaders method here. OPTIONS requests are handled by the framework.
    }
}
