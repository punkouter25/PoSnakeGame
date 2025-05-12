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
        }        /// <summary>
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
            
            try
            {
                _logger.LogInformation("Configuring HighScoreService HTTP client with base URL: {BaseUrl}", baseUrl);
                
                // Configure the HttpClient with the appropriate base address
                _httpClient.BaseAddress = new Uri(baseUrl);
                
                // Set default headers that might help with CORS and content negotiation
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "PoSnakeGame-Blazor-Client");
                
                // Set a reasonable timeout
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                _logger.LogInformation("Successfully configured HTTP client for HighScoreService");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring HighScoreService HTTP client with base URL: {BaseUrl}", baseUrl);
                throw;
            }
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
        }        /// <summary>
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
            
            // Max number of retry attempts
            const int maxRetries = 3;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Log the attempt number when retrying
                    if (attempt > 1)
                    {
                        _logger.LogInformation("Retry attempt {Attempt}/{MaxRetries} saving high score for {Initials}", 
                            attempt, maxRetries, highScore.Initials);
                    }
                    
                    _logger.LogInformation("Saving high score for {Initials} via API endpoint 'highscores/save'", 
                        highScore.Initials);
                        
                    // Force Content-Type header to ensure proper JSON formatting
                    using var request = new HttpRequestMessage(HttpMethod.Post, "highscores/save");
                    request.Content = JsonContent.Create(highScore);
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    
                    // Add a timeout to the request
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var response = await _httpClient.SendAsync(request, cts.Token);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully saved high score for {Initials}", highScore.Initials);
                        
                        try
                        {
                            // Try to read the response content
                            var savedScore = await response.Content.ReadFromJsonAsync<HighScore>();
                            return savedScore ?? highScore;
                        }
                        catch
                        {
                            // If we can't deserialize the response but the status code was successful,
                            // just return the original high score
                            _logger.LogWarning("Could not deserialize response, but high score was saved successfully");
                            return highScore;
                        }
                    }
                    else
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("API returned non-success status code {StatusCode}. Response: {Response}", 
                            response.StatusCode, responseContent);
                            
                        // Only retry for certain status codes (5xx server errors)
                        if ((int)response.StatusCode < 500 || attempt == maxRetries)
                        {
                            throw new HttpRequestException($"API returned status code: {response.StatusCode}. Response: {responseContent}");
                        }
                        
                        // Wait before retrying (exponential backoff)
                        await Task.Delay(attempt * 1000);
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    _logger.LogError(ex, "Error saving high score for {Initials} (Attempt {Attempt}/{MaxRetries})", 
                        highScore.Initials, attempt, maxRetries);
                        
                    // If this was our last attempt, rethrow
                    if (attempt == maxRetries)
                    {
                        // On the last attempt, if we still fail, return the original high score
                        // instead of throwing to provide a better user experience
                        _logger.LogError("Failed to save high score after {MaxRetries} attempts", maxRetries);
                        return highScore;
                    }
                    
                    // Wait before retrying (exponential backoff)
                    await Task.Delay(attempt * 1000);
                }
            }
            
            // We should never reach here due to the return in the last catch block
            return highScore;
        }/// <summary>
        /// Checks if a given score qualifies as a high score via the API.
        /// </summary>
        /// <param name="score">The score to check.</param>
        /// <returns>True if it's a high score, false otherwise.</returns>
        public async Task<bool> IsHighScoreAsync(int score)
        {
            try
            {
                // First check if there are any high scores at all
                var highScores = await GetHighScoresAsync();
                
                // If there are no high scores yet, any score should qualify
                if (highScores.Count == 0)
                {
                    _logger.LogInformation("No existing high scores found. Score {Score} automatically qualifies.", score);
                    return true;
                }
                
                // Otherwise check with the API
                _logger.LogInformation("Checking if score {Score} is a high score via API.", score);
                // Relative path includes the score
                var isHighScore = await _httpClient.GetFromJsonAsync<bool>($"highscores/check/{score}");
                return isHighScore;
            }
            catch (Exception ex)
            {
                // Separate exception logging from context logging to avoid formatting issues
                _logger.LogError(ex, "An exception occurred while checking if score is a high score via API.");
                _logger.LogInformation("Context for the above error: Score={Score}", score);
                
                // In case of error, let's be lenient and assume it might be a high score
                // This ensures users can still submit their scores even if the API check fails
                return true;
            }
        }
    }
}
