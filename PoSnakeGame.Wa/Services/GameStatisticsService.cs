using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoSnakeGame.Wa.Services
{
    public class GameStatisticsService
    {
        private readonly ILogger<GameStatisticsService> _logger;
        private readonly HttpClient _httpClient;
        
        public GameStatisticsService(ILogger<GameStatisticsService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Configures the HTTP client with the appropriate base URL
        /// </summary>
        /// <param name="baseUrl">The base URL for the statistics API</param>
        public void ConfigureHttpClient(string baseUrl)
        {
            // Ensure baseUrl ends with a slash for proper URL combination
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            
            _logger.LogInformation("Configuring GameStatisticsService HTTP client with base URL: {BaseUrl}", baseUrl);
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<Dictionary<string, int>> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting game statistics from Azure Function at {BaseAddress}", _httpClient.BaseAddress);
                // Since baseUrl already includes 'api/', we just need the endpoint name
                var stats = await _httpClient.GetFromJsonAsync<Dictionary<string, int>>("statistics");
                return stats ?? new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game statistics from Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                return new Dictionary<string, int>();
            }
        }

        public async Task UpdateStatisticsAsync(Dictionary<string, int> stats)
        {
            try
            {
                _logger.LogInformation("Updating game statistics via Azure Function at {BaseAddress}", _httpClient.BaseAddress);
                // Since baseUrl already includes 'api/', we just need the endpoint name
                await _httpClient.PostAsJsonAsync("statistics/update", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game statistics via Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                throw;
            }
        }
    }
}
