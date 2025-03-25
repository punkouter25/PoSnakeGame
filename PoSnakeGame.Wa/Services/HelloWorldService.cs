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
        }

        /// <summary>
        /// Configures the HTTP client with the appropriate base URL
        /// This follows the Strategy pattern by allowing dynamic reconfiguration
        /// </summary>
        /// <param name="baseUrl">The base URL for the Azure Function API</param>
        public void ConfigureHttpClient(string baseUrl)
        {
            // Ensure baseUrl ends with a slash for proper URL combination
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            
            _logger.LogInformation("Configuring HelloWorldService HTTP client with base URL: {BaseUrl}", baseUrl);
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        /// <summary>
        /// Gets the hello world message from the Azure Function
        /// </summary>
        /// <returns>The hello world message</returns>
        public async Task<string> GetHelloWorldMessageAsync()
        {
            try
            {
                _logger.LogInformation("Calling HelloWorld Azure Function at {BaseAddress}", _httpClient.BaseAddress);
                
                // Since baseUrl already includes 'api/', we only need to append 'hello'
                var response = await _httpClient.GetAsync("hello");
                response.EnsureSuccessStatusCode();
                
                var message = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received response from HelloWorld function: {message}");
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HelloWorld Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                return "Error: Could not connect to HelloWorld function";
            }
        }
    }
}
