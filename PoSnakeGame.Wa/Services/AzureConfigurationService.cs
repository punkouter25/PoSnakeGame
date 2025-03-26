using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

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
        private readonly IWebAssemblyHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;

        public AzureConfigurationService(
            ILogger<AzureConfigurationService> logger, 
            HttpClient httpClient,
            IWebAssemblyHostEnvironment hostEnvironment,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the base URL for the Function App from configuration.
        /// First checks appsettings.json, then falls back to environment detection.
        /// </summary>
        public async Task<string> GetFunctionAppBaseUrlAsync()
        {
            try
            {
                // Debug information to help with troubleshooting
                _logger.LogDebug("Current environment: {Environment}", _hostEnvironment.Environment);
                
                // First, check if ApiBaseUrl is defined in appsettings.json
                var apiBaseUrl = _configuration["ApiBaseUrl"];
                if (!string.IsNullOrEmpty(apiBaseUrl))
                {
                    // Make sure we don't include "/api" in the base URL since it will be added later
                    _logger.LogInformation("Using API base URL from configuration: {Url}", apiBaseUrl);
                    
                    // If the apiBaseUrl ends with "/api" or "/api/", remove it
                    if (apiBaseUrl.EndsWith("/api/"))
                    {
                        apiBaseUrl = apiBaseUrl.Substring(0, apiBaseUrl.Length - 5);
                        _logger.LogDebug("Removed '/api/' from base URL, now: {Url}", apiBaseUrl);
                    }
                    else if (apiBaseUrl.EndsWith("/api"))
                    {
                        apiBaseUrl = apiBaseUrl.Substring(0, apiBaseUrl.Length - 4);
                        _logger.LogDebug("Removed '/api' from base URL, now: {Url}", apiBaseUrl);
                    }
                    
                    return EnsureTrailingSlash(apiBaseUrl);
                }
                
                // If not found in configuration, use environment-specific defaults
                if (_hostEnvironment.IsDevelopment())
                {
                    _logger.LogInformation("Running in development environment, using local Azurite");
                    return "http://localhost:7071/";
                }
                
                // Production environment - try to get from configuration
                _logger.LogInformation("Running in production environment: {Env}", _hostEnvironment.Environment);
                
                // In Azure Static Web Apps, environment variables are accessible via a special endpoint
                try
                {
                    var configResponse = await _httpClient.GetAsync("/__configuration");
                    if (configResponse.IsSuccessStatusCode)
                    {
                        var configJson = await configResponse.Content.ReadAsStringAsync();
                        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);
                        
                        if (config != null && config.TryGetValue("FUNCTIONS_BASE_URL", out var functionUrl))
                        {
                            // Ensure URL has a trailing slash but doesn't include "/api"
                            if (functionUrl.EndsWith("/api/"))
                            {
                                functionUrl = functionUrl.Substring(0, functionUrl.Length - 5);
                            }
                            else if (functionUrl.EndsWith("/api"))
                            {
                                functionUrl = functionUrl.Substring(0, functionUrl.Length - 4);
                            }
                            
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining Function App base URL");
                // Fall back to local development URL
                return "http://localhost:7071/";
            }
        }

        /// <summary>
        /// Gets the CORS allowed origins from the configuration
        /// </summary>
        public string[] GetCorsAllowedOrigins()
        {
            try
            {
                // Try to get from config section
                var corsSection = _configuration.GetSection("CORS:AllowedOrigins");
                if (corsSection.Exists())
                {
                    var origins = corsSection.Get<string[]>();
                    if (origins != null && origins.Length > 0)
                    {
                        _logger.LogInformation("Found {Count} CORS allowed origins in configuration", origins.Length);
                        return origins;
                    }
                }
                
                // Default allowed origins if not in config
                _logger.LogWarning("Using default CORS allowed origins");
                return new[]
                {
                    "http://localhost:5000",
                    "http://localhost:5001",
                    "https://localhost:5000", 
                    "https://localhost:5001",
                    "http://localhost:5297",
                    "https://localhost:7047"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CORS allowed origins");
                return new[] { "*" }; // Fallback to wildcard
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