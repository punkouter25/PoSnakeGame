using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace PoSnakeGame.Wa.Services
{
    public class MockTableStorageService : ITableStorageService
    {
        private readonly ILogger<MockTableStorageService> _logger;
        private readonly HttpClient _httpClient;
        
        public MockTableStorageService(ILogger<MockTableStorageService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Configures the HTTP client with the appropriate base URL
        /// </summary>
        /// <param name="baseUrl">The base URL for the high scores API</param>
        public void ConfigureHttpClient(string baseUrl)
        {
            // Ensure baseUrl ends with a slash for proper URL combination
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            
            _logger.LogInformation("Configuring MockTableStorageService HTTP client with base URL: {BaseUrl}", baseUrl);
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting top {Count} scores from Azure Function at {BaseAddress}", count, _httpClient.BaseAddress);
                // Since baseUrl already includes 'api/', we just need the endpoint name
                var scores = await _httpClient.GetFromJsonAsync<List<HighScore>>("highscores");
                return scores ?? new List<HighScore>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high scores from Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                return new List<HighScore>();
            }
        }

        public async Task<bool> IsHighScore(int score)
        {
            try
            {
                _logger.LogInformation("Checking if {Score} is a high score via Azure Function at {BaseAddress}", score, _httpClient.BaseAddress);
                // Since baseUrl already includes 'api/', we just need the endpoint name
                var result = await _httpClient.GetFromJsonAsync<bool>($"highscores/check/{score}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking high score via Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                return true; // Assume it's a high score on error to allow submission
            }
        }

        public async Task SaveHighScoreAsync(HighScore highScore)
        {
            try
            {
                _logger.LogInformation("Saving high score for {Initials} with score {Score} via Azure Function at {BaseAddress}", 
                    highScore.Initials, highScore.Score, _httpClient.BaseAddress);
                
                // Since baseUrl already includes 'api/', we just need the endpoint name
                await _httpClient.PostAsJsonAsync("highscores/save", highScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score via Azure Function at {BaseAddress}. Error: {Error}", 
                    _httpClient.BaseAddress, ex.Message);
                throw;
            }
        }
    }
}
