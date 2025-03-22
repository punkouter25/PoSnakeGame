using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Service for interacting with the HelloWorld Azure Function
    /// </summary>
    public class HelloWorldService
    {
        private readonly ILogger<HelloWorldService> _logger;
        private readonly HttpClient _httpClient;
        public HelloWorldService(ILogger<HelloWorldService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:7071");
        }

        /// <summary>
        /// Gets the hello world message from the Azure Function
        /// </summary>
        /// <returns>The hello world message</returns>
        public async Task<string> GetHelloWorldMessageAsync()
        {
            try
            {
                _logger.LogInformation("Calling HelloWorld Azure Function");
                
                // Call the hello endpoint using the injected HttpClient
                var response = await _httpClient.GetAsync("api/hello");
                response.EnsureSuccessStatusCode();
                
                var message = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received response from HelloWorld function: {message}");
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HelloWorld Azure Function");
                return "Error: Could not connect to HelloWorld function";
            }
        }
    }
}
