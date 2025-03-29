using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Service for interacting with the HelloWorld Azure Function
    /// This follows the SOLID principle of single responsibility
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
        /// <returns>The hello world message as a JSON string</returns>
        public async Task<string> GetHelloWorldMessageAsync()
        {
            try
            {
                _logger.LogInformation("Calling HelloWorld API endpoint at {BaseAddress}", _httpClient.BaseAddress);
                
                // Use the controller name as the relative path
                var response = await _httpClient.GetAsync("HelloWorld"); 
                response.EnsureSuccessStatusCode();
                
                var message = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Received response from HelloWorld function: {Message}", message);
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling HelloWorld Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                return "Error: Could not connect to HelloWorld function";
            }
        }

        /// <summary>
        /// Gets the hello world connection status from the Azure Function
        /// Returns a strongly-typed object representing the response
        /// </summary>
        /// <returns>A HelloWorldResponse object or null if there was an error</returns>
        public async Task<HelloWorldResponse> GetConnectionStatusAsync()
        {
            try
            {
                _logger.LogInformation("Checking API connectivity at {BaseAddress}", _httpClient.BaseAddress);
                
                // Using GetFromJsonAsync for cleaner JSON handling - use controller name
                var response = await _httpClient.GetFromJsonAsync<HelloWorldResponse>("HelloWorld"); 
                
                if (response != null)
                {
                    _logger.LogInformation("API connection successful: {Status}", response.Status);
                    return response;
                }
                
                _logger.LogWarning("API returned null response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API connectivity: {Error}", ex.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// Represents the response from the HelloWorld function
    /// </summary>
    public class HelloWorldResponse
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
    }
}
