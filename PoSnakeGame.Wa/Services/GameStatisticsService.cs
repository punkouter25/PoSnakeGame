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
            _httpClient.BaseAddress = new Uri("http://localhost:7071/api");
        }

        public async Task<Dictionary<string, int>> GetStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting game statistics from Azure Function");
                var stats = await _httpClient.GetFromJsonAsync<Dictionary<string, int>>("statistics");
                return stats ?? new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game statistics from Azure Function");
                return new Dictionary<string, int>();
            }
        }

        public async Task UpdateStatisticsAsync(Dictionary<string, int> stats)
        {
            try
            {
                _logger.LogInformation("Updating game statistics via Azure Function");
                await _httpClient.PostAsJsonAsync("statistics/update", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game statistics via Azure Function");
                throw;
            }
        }
    }
}
