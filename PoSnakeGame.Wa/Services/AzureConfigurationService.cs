using System.Text.Json;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Service to handle retrieving configuration settings for Azure resources.
    /// This follows the SOLID principle of single responsibility by focusing only on configuration.
    /// </summary>
    public class AzureConfigurationService
    {
        private readonly ILogger<AzureConfigurationService> _logger;
        private readonly HttpClient _httpClient;

        public AzureConfigurationService(ILogger<AzureConfigurationService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the base URL for the Function App from configuration.
        /// First tries to get it from environment variables (for deployed Azure environment),
        /// then falls back to localhost (for local development).
        /// </summary>
        public async Task<string> GetFunctionAppBaseUrlAsync()
        {
            try
            {
                // In Azure Static Web Apps, environment variables are accessible via a special endpoint
                // This approach works for both local and production environments
                var response = await _httpClient.GetAsync("/.auth/me");
                if (response.IsSuccessStatusCode)
                {
                    // We're in an Azure Static Web App environment
                    _logger.LogInformation("Running in Azure Static Web App environment");
                    
                    // Try to get environment variable
                    try
                    {
                        var configResponse = await _httpClient.GetAsync("/__configuration");
                        if (configResponse.IsSuccessStatusCode)
                        {
                            var configJson = await configResponse.Content.ReadAsStringAsync();
                            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);
                            
                            if (config != null && config.TryGetValue("FUNCTIONS_BASE_URL", out var functionUrl))
                            {
                                // Ensure URL has a trailing slash
                                functionUrl = EnsureTrailingSlash(functionUrl);
                                _logger.LogInformation("Found Function App URL in configuration: {Url}", functionUrl);
                                return functionUrl;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error getting configuration: {Error}", ex.Message);
                    }
                    
                    // If we couldn't get the config, use Azure URL pattern
                    _logger.LogWarning("Falling back to default Azure Function URL pattern");
                    return "https://posnakegame-functions.azurewebsites.net/";
                }
                
                // We're in local development
                _logger.LogInformation("Running in local development environment");
                return "http://localhost:7071/";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining Function App base URL");
                // Fall back to local development URL
                return "http://localhost:7071/";
            }
        }

        /// <summary>
        /// Ensures that a URL has a trailing slash, which is important for proper URL resolution
        /// when combining base URLs with relative paths
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>The URL with a trailing slash</returns>
        private string EnsureTrailingSlash(string url)
        {
            // Using the Adapter pattern to standardize URL format
            if (string.IsNullOrEmpty(url))
            {
                return "/";
            }
            
            // Add trailing slash if not present
            return url.EndsWith("/") ? url : url + "/";
        }
    }
}