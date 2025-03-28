using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Infrastructure.Services; // Added for ITableStorageService
using System.Text.Json; // Added for JsonSerializer

namespace PoSnakeGame.Functions
{
    public class HelloWorldFunction
    {
        private readonly ILogger<HelloWorldFunction> _logger;
        private readonly ITableStorageService _tableStorageService;

        // Inject ITableStorageService
        public HelloWorldFunction(ILogger<HelloWorldFunction> logger, ITableStorageService tableStorageService)
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        [Function("HelloWorld")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", "options", Route = "hello")] HttpRequestData req,
            FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger HelloWorld function processed a request.");

            string tableStorageStatus = "Table Storage Not Checked";
            try
            {
                // Example: Attempt to check if the table exists or perform a simple query
                // Replace "TestTable" with an actual table name if needed, or implement a specific check method
                bool tableExists = await _tableStorageService.TableExistsAsync("HighScores"); // Assuming HighScores table is used
                tableStorageStatus = tableExists ? "Connected to Azure Table Storage (HighScores table exists)" : "Could not confirm connection to Azure Table Storage (HighScores table not found)";
                _logger.LogInformation($"Table Storage check result: {tableStorageStatus}");
            }
            catch (Exception ex)
            {
                tableStorageStatus = $"Error connecting to Azure Table Storage: {ex.Message}";
                _logger.LogError(ex, "Error interacting with Azure Table Storage.");
            }

            // Create response
            var response = req.CreateResponse(HttpStatusCode.OK);
            // The Content-Type header will be set by WriteAsJsonAsync

            // Add permissive CORS headers directly in the function
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Allow requests from any origin
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"); // Allowed methods
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization"); // Allowed headers
            response.Headers.Add("Access-Control-Allow-Credentials", "true"); // Allow credentials
            response.Headers.Add("Access-Control-Max-Age", "86400"); // Cache preflight response for 24 hours

            // Handle OPTIONS preflight request for CORS
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Handling OPTIONS preflight request.");
                // No body needed for OPTIONS response, just headers
                return response; 
            }

            // Return a response with timestamp and table storage status
            var result = new
            {
                Message = "Hello from PoSnakeGame Functions API!",
                Timestamp = DateTime.UtcNow,
                Status = "Function Responding",
                TableStorageStatus = tableStorageStatus, // Include table storage status
                RequestMethod = req.Method,
                SecurityLevel = "Open - No Authorization Required"
            };

            // Debug information to help with troubleshooting
            _logger.LogInformation($"HelloWorld function responding with: {JsonSerializer.Serialize(result)}");
            _logger.LogInformation($"Request method: {req.Method}, Headers: {string.Join(", ", req.Headers.Select(h => $"{h.Key}:{string.Join(",", h.Value)}"))}");

            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
