using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PoSnakeGame.Functions
{
    public class HelloWorldFunction
    {
        [Function("HelloWorld")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "hello")] HttpRequestData req)
        {
            // Handle preflight OPTIONS request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.OK);
                AddCorsHeaders(preflightResponse);
                return preflightResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            AddCorsHeaders(response);
            await response.WriteStringAsync("Hello World from Snake Game Functions!");
            
            return response;
        }

        private static void AddCorsHeaders(HttpResponseData response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }
    }
}
