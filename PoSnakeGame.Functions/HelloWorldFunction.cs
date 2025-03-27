using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace PoSnakeGame.Functions
{
    public class HelloWorldFunction
    {
        [Function("HelloWorld")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", "options", Route = "hello")] HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger<HelloWorldFunction>();
            logger.LogInformation("C# HTTP trigger HelloWorld function processed a request.");

            // Create response
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            // Add permissive CORS headers directly in the function
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Max-Age", "86400"); // 24 hours
            
            // Return a response with timestamp to verify it's working correctly
            var result = new 
            {
                Message = "Hello from PoSnakeGame Functions API!",
                Timestamp = DateTime.UtcNow,
                Status = "Connected",
                RequestMethod = req.Method,
                SecurityLevel = "Open - No Authorization Required"
            };
            
            // Debug information to help with troubleshooting
            logger.LogInformation($"HelloWorld function responding with: {System.Text.Json.JsonSerializer.Serialize(result)}");
            logger.LogInformation($"Request method: {req.Method}, Headers: {string.Join(", ", req.Headers.Select(h => $"{h.Key}:{string.Join(",", h.Value)}"))}");
            
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}