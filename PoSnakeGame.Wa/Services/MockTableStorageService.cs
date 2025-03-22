using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace PoSnakeGame.Wa.Services
{
    public class MockTableStorageService : ITableStorageService
    {
        private readonly ILogger<MockTableStorageService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:7071/api"; // Azure Functions local URL

        public MockTableStorageService(ILogger<MockTableStorageService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting top {Count} scores from Azure Function", count);
                var scores = await _httpClient.GetFromJsonAsync<List<HighScore>>("highscores");
                return scores ?? new List<HighScore>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high scores from Azure Function");
                return new List<HighScore>();
            }
        }

        public async Task<bool> IsHighScore(int score)
        {
            try
            {
                _logger.LogInformation("Checking if {Score} is a high score via Azure Function", score);
                var result = await _httpClient.GetFromJsonAsync<bool>($"highscores/check/{score}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking high score via Azure Function");
                return true; // Assume it's a high score on error to allow submission
            }
        }

        public async Task SaveHighScoreAsync(HighScore highScore)
        {
            try
            {
                _logger.LogInformation("Saving high score for {Initials} with score {Score} via Azure Function", 
                    highScore.Initials, highScore.Score);
                
                await _httpClient.PostAsJsonAsync("highscores", highScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score via Azure Function");
                throw;
            }
        }
    }
}