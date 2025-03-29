using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models; // Required for HighScore model

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Service for interacting with the High Score API endpoints.
    /// Follows SOLID principles.
    /// </summary>
    public class HighScoreService
    {
        private readonly ILogger<HighScoreService> _logger;
        private readonly HttpClient _httpClient;

        public HighScoreService(ILogger<HighScoreService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _logger.LogInformation("HighScoreService instantiated."); // Debug log
        }

        /// <summary>
        /// Configures the HTTP client with the appropriate base URL.
        /// Called during service registration.
        /// </summary>
        /// <param name="baseUrl">The base URL for the API (e.g., https://localhost:7193/api/)</param>
        public void ConfigureHttpClient(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                 _logger.LogError("Base URL for HighScoreService is null or empty.");
                 throw new ArgumentNullException(nameof(baseUrl), "API Base URL cannot be null or empty.");
            }
            // Ensure baseUrl ends with a slash for proper URL combination
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            _logger.LogInformation("Configuring HighScoreService HTTP client with base URL: {BaseUrl}", baseUrl);
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        /// <summary>
        /// Gets the top high scores from the API.
        /// </summary>
        /// <returns>A list of HighScore objects.</returns>
        public async Task<List<HighScore>> GetHighScoresAsync()
        {
            try
            {
                _logger.LogInformation("Getting high scores from API endpoint 'highscores' at {BaseAddress}", _httpClient.BaseAddress);
                // Relative path is 'highscores' since BaseAddress includes '/api/'
                var scores = await _httpClient.GetFromJsonAsync<List<HighScore>>("highscores");
                return scores ?? new List<HighScore>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high scores from API at {BaseAddress}highscores. Error: {Error}",
                    _httpClient.BaseAddress, ex.Message);
                // Return empty list or rethrow depending on desired error handling
                return new List<HighScore>();
            }
        }

        /// <summary>
        /// Saves a new high score via the API.
        /// </summary>
        /// <param name="highScore">The HighScore object to save.</param>
        /// <returns>The saved HighScore object (potentially updated by the API).</returns>
        public async Task<HighScore> SaveHighScoreAsync(HighScore highScore)
        {
            if (highScore == null)
            {
                _logger.LogWarning("SaveHighScoreAsync called with null HighScore object.");
                throw new ArgumentNullException(nameof(highScore));
            }
            try
            {
                _logger.LogInformation("Saving high score for {Initials} via API endpoint 'highscores/save' at {BaseAddress}",
                    highScore.Initials, _httpClient.BaseAddress);
                // Relative path is 'highscores/save'
                var response = await _httpClient.PostAsJsonAsync("highscores/save", highScore);
                response.EnsureSuccessStatusCode(); // Throw exception if API returned error status

                // Read the response content back, as the API might have added/modified properties (like RowKey/Timestamp)
                var savedScore = await response.Content.ReadFromJsonAsync<HighScore>();
                return savedScore ?? highScore; // Return saved score or original if deserialization fails
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score for {Initials} via API at {BaseAddress}highscores/save. Error: {Error}",
                    highScore.Initials, _httpClient.BaseAddress, ex.Message);
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Checks if a given score qualifies as a high score via the API.
        /// </summary>
        /// <param name="score">The score to check.</param>
        /// <returns>True if it's a high score, false otherwise.</returns>
        public async Task<bool> IsHighScoreAsync(int score)
        {
            try
            {
                // Remove BaseAddress from log message
                _logger.LogInformation("Checking if score {Score} is a high score via API.",
                    score);
                // Relative path includes the score
                var isHighScore = await _httpClient.GetFromJsonAsync<bool>($"highscores/check/{score}");
                return isHighScore;
            }
            catch (Exception ex)
            {
                // Separate exception logging from context logging to avoid formatting issues
                _logger.LogError(ex, "An exception occurred while checking if score is a high score via API.");
                _logger.LogInformation("Context for the above error: Score={Score}", score);
                // Default to false in case of error, or rethrow depending on desired behavior
                return false;
            }
        }
    }
}
