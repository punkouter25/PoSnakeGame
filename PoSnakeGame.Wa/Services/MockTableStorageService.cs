using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Implementation of the ITableStorageService that calls Azure Functions to interact with table storage.
    /// This follows the Adapter pattern by adapting the Azure Functions API to match the ITableStorageService interface.
    /// </summary>
    public class TableStorageService : ITableStorageService
    {
        private readonly ILogger<TableStorageService> _logger;
        private readonly HttpClient _httpClient;
        private bool _isConfigured = false;

        // Constants for endpoint paths
        private const string HIGH_SCORES_ENDPOINT = "highscores";
        private const string CHECK_HIGH_SCORE_ENDPOINT = "highscores/check";
        private const string SAVE_HIGH_SCORE_ENDPOINT = "highscores/save";

        public TableStorageService(ILogger<TableStorageService> logger, HttpClient httpClient)
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
            try
            {
                // Ensure baseUrl ends with a slash for proper URL combination
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                _logger.LogInformation("Configuring TableStorageService HTTP client with base URL: {BaseUrl}", baseUrl);
                _httpClient.BaseAddress = new Uri(baseUrl);

                // Set default headers for improved error handling
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Add a debug request header to help with CORS troubleshooting
                _httpClient.DefaultRequestHeaders.Add("X-Client-App", "PoSnakeGame-WebAssembly");

                _isConfigured = true;
                _logger.LogDebug("TableStorageService HTTP client configured successfully with BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring TableStorageService HTTP client: {Error}", ex.Message);
                _isConfigured = false;
            }
        }

        public async Task<List<HighScore>> GetTopScoresAsync(int count = 10)
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("HttpClient not configured. Returning empty list of high scores.");
                return new List<HighScore>();
            }

            try
            {
                _logger.LogInformation("Getting top {Count} scores from Azure Function at {BaseAddress}", count, _httpClient.BaseAddress);

                // Create the complete URL for better debugging
                var requestUrl = $"{HIGH_SCORES_ENDPOINT}?count={count}";
                _logger.LogDebug("Making GET request to: {RequestUrl}", requestUrl);

                // Make the HTTP request and capture the response for detailed error information
                var response = await _httpClient.GetAsync(requestUrl);

                // Check if the request was successful and log appropriate information
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Received successful response with content: {Content}", jsonContent);

                    var scores = JsonSerializer.Deserialize<List<HighScore>>(jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation("Successfully retrieved {Count} high scores", scores?.Count ?? 0);
                    return scores ?? new List<HighScore>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP request failed with status code {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return new List<HighScore>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high scores from Azure Function at {BaseAddress}. Error: {Error}",
                    _httpClient.BaseAddress, ex.Message);

                // Log additional context to help with debugging
                if (ex is HttpRequestException)
                {
                    _logger.LogDebug("This is likely a network or CORS error. Check that the Azure Functions app is running and CORS is properly configured.");
                }

                return new List<HighScore>();
            }
        }

        public async Task<bool> IsHighScore(int score)
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("HttpClient not configured. Assuming it's a high score.");
                return true;
            }

            try
            {
                _logger.LogInformation("Checking if {Score} is a high score via Azure Function at {BaseAddress}", score, _httpClient.BaseAddress);

                // Create the complete URL for better debugging
                var requestUrl = $"{CHECK_HIGH_SCORE_ENDPOINT}/{score}";
                _logger.LogDebug("Making GET request to: {RequestUrl}", requestUrl);

                // Make the HTTP request and capture the response for detailed error information
                var response = await _httpClient.GetAsync(requestUrl);

                // Check if the request was successful and log appropriate information
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Received successful response with content: {Content}", jsonContent);

                    var result = JsonSerializer.Deserialize<bool>(jsonContent);
                    _logger.LogInformation("Is high score check result: {Result}", result);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP request failed with status code {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);

                    return true; // Assume it's a high score on error to allow submission
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking high score via Azure Function at {BaseAddress}. Error: {Error}",
                    _httpClient.BaseAddress, ex.Message);

                // Log additional context to help with debugging
                if (ex is HttpRequestException)
                {
                    _logger.LogDebug("This is likely a network or CORS error. Check that the Azure Functions app is running and CORS is properly configured.");
                }

                return true; // Assume it's a high score on error to allow submission
            }
        }

        public async Task SaveHighScoreAsync(HighScore highScore)
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("HttpClient not configured. Cannot save high score.");
                throw new InvalidOperationException("TableStorageService HTTP client not configured.");
            }

            try
            {
                _logger.LogInformation("Saving high score for {Initials} with score {Score} via Azure Function at {BaseAddress}",
                    highScore.Initials, highScore.Score, _httpClient.BaseAddress);

                // Create the complete URL for better debugging
                var requestUrl = SAVE_HIGH_SCORE_ENDPOINT;
                _logger.LogDebug("Making POST request to: {RequestUrl} with data: {Data}",
                    requestUrl, JsonSerializer.Serialize(highScore));

                // Make the HTTP request and capture the response for detailed error information
                var response = await _httpClient.PostAsJsonAsync(requestUrl, highScore);

                // Check if the request was successful and log appropriate information
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully saved high score for {Initials}", highScore.Initials);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP request failed with status code {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);

                    throw new HttpRequestException($"Failed to save high score. Status code: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score via Azure Function at {BaseAddress}. Error: {Error}",
                    _httpClient.BaseAddress, ex.Message);

                // Log additional context to help with debugging
                if (ex is HttpRequestException)
                {
                    _logger.LogDebug("This is likely a network or CORS error. Check that the Azure Functions app is running and CORS is properly configured.");
                }

                throw;
            }
        }

        // Mock implementation for TableExistsAsync
        public Task<bool> TableExistsAsync(string tableName)
        {
            _logger.LogInformation("MockTableStorageService: Checking if table '{TableName}' exists (returning true).", tableName);
            // Simulate table always exists for the mock
            return Task.FromResult(true);
        }
    }
}
