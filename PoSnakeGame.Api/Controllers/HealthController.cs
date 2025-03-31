using Microsoft.AspNetCore.Mvc;
using PoSnakeGame.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace PoSnakeGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ITableStorageService tableStorageService, ILogger<HealthController> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        [HttpGet("CheckTableStorage")]
        public async Task<IActionResult> CheckTableStorage()
        {
            _logger.LogInformation("Attempting to check Table Storage connection.");
            try
            {
                // Attempt a simple operation, like checking if the HighScores table exists.
                // This implicitly tests the connection and credentials.
                bool exists = await _tableStorageService.TableExistsAsync("HighScores"); // Assuming HighScores table should exist
                _logger.LogInformation("Table Storage connection check successful. Table 'HighScores' exists: {Exists}", exists);
                return Ok(new { Status = "Connected", Message = $"Table Storage connection successful. HighScores table exists: {exists}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to or query Azure Table Storage.");
                // Return a specific status code or message indicating failure
                return StatusCode(503, new { Status = "Disconnected", Message = $"Failed to connect to Table Storage: {ex.Message}" }); // 503 Service Unavailable
            }
        }

        // Optional: Add more checks here (e.g., check other dependencies)
    }
}
