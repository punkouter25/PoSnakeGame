using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Infrastructure.Services;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PoSnakeGame.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route will be /api/helloworld
    public class HelloWorldController : ControllerBase
    {
        private readonly ILogger<HelloWorldController> _logger;
        private readonly ITableStorageService _tableStorageService;

        public HelloWorldController(ILogger<HelloWorldController> logger, ITableStorageService tableStorageService)
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
            _logger.LogInformation("HelloWorldController instantiated."); // Debug log
        }

        // Combined endpoint for multiple methods, similar to the Function
        // Using HttpGet as the primary, but the route itself is accessible via others defined in CORS
        [HttpGet] // Corresponds to the GET route in the Function
        [HttpPost] // Explicitly allow POST
        [HttpPut] // Explicitly allow PUT
        [HttpDelete] // Explicitly allow DELETE
        [HttpOptions] // Explicitly allow OPTIONS for CORS preflight
        public async Task<IActionResult> GetHelloWorldStatus()
        {
            _logger.LogInformation("Processing request for HelloWorld status.");

            string tableStorageStatus = "Table Storage Not Checked";
            try
            {
                // Check connection using the HighScores table as in the original function
                bool tableExists = await _tableStorageService.TableExistsAsync("HighScores");
                tableStorageStatus = tableExists ? "Connected to Azure Table Storage (HighScores table exists)" : "Could not confirm connection to Azure Table Storage (HighScores table not found)";
                _logger.LogInformation($"Table Storage check result: {tableStorageStatus}");
            }
            catch (Exception ex)
            {
                tableStorageStatus = $"Error connecting to Azure Table Storage: {ex.Message}";
                _logger.LogError(ex, "Error interacting with Azure Table Storage.");
            }

            var result = new
            {
                Message = "Hello from PoSnakeGame ASP.NET Core API!", // Updated message
                Timestamp = DateTime.UtcNow,
                Status = "API Responding",
                TableStorageStatus = tableStorageStatus,
                RequestMethod = HttpContext.Request.Method, // Get method from HttpContext
                SecurityLevel = "Open - No Authorization Required"
            };

            // Debug log before returning
            _logger.LogInformation($"HelloWorldController responding with: {JsonSerializer.Serialize(result)}");
            // Explicitly convert header values to array to resolve string.Join ambiguity in .NET 9+
            _logger.LogInformation($"Request method: {HttpContext.Request.Method}, Headers: {string.Join(", ", HttpContext.Request.Headers.Select(h => $"{h.Key}:{string.Join(",", h.Value.ToArray())}"))}");

            // For OPTIONS requests, ASP.NET Core handles CORS preflight automatically
            // if UseCors is configured correctly. We just return OK.
            if (HttpContext.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                 _logger.LogInformation("Handling OPTIONS preflight request implicitly via CORS middleware.");
                 return Ok(); // Return 200 OK for preflight
            }

            return Ok(result);
        }
    }
}
