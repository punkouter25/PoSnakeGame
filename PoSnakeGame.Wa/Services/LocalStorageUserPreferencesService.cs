using Blazored.LocalStorage; // Use Blazored.LocalStorage
using PoSnakeGame.Core.Interfaces;
using PoSnakeGame.Core.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Implements IUserPreferencesService using Blazored.LocalStorage for Blazor WebAssembly.
    /// </summary>
    public class LocalStorageUserPreferencesService : IUserPreferencesService
    {
        private readonly ILocalStorageService _localStorage; // Changed type
        private readonly ILogger<LocalStorageUserPreferencesService> _logger;
        private const string PreferencesKey = "PoSnakeGameUserPreferences";

        // Inject ILocalStorageService and ILogger
        public LocalStorageUserPreferencesService(
            ILocalStorageService localStorage, // Changed type
            ILogger<LocalStorageUserPreferencesService> logger)
        {
            _localStorage = localStorage;
            _logger = logger;
            _logger.LogInformation("LocalStorageUserPreferencesService initialized.");
        }

        /// <summary>
        /// Loads preferences from local storage. Returns defaults if not found or error occurs.
        /// </summary>
        public async Task<UserPreferences> LoadPreferencesAsync()
        {
            _logger.LogInformation("Attempting to load user preferences from local storage.");
            try
            {
                // Check if the key exists first
                bool keyExists = await _localStorage.ContainKeyAsync(PreferencesKey);
                if (keyExists)
                {
                    var preferencesJson = await _localStorage.GetItemAsStringAsync(PreferencesKey); // Get as string
                    if (!string.IsNullOrEmpty(preferencesJson))
                    {
                         _logger.LogInformation("Successfully retrieved preferences string from local storage.");
                        var preferences = JsonSerializer.Deserialize<UserPreferences>(preferencesJson);
                        if (preferences != null)
                        {
                            _logger.LogInformation("Successfully deserialized preferences. Color: {Color}", preferences.PlayerSnakeColorHex);
                            return preferences;
                        }
                        else
                        {
                             _logger.LogWarning("Failed to deserialize preferences string from local storage.");
                        }
                    }
                    else
                    {
                         _logger.LogWarning("Preferences string retrieved from local storage was null or empty.");
                    }
                }
                else
                {
                     _logger.LogInformation("Preferences key '{Key}' not found in local storage. Returning default preferences.", PreferencesKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading preferences from local storage. Returning default preferences.");
                // Optionally remove the potentially corrupted item
                try { await _localStorage.RemoveItemAsync(PreferencesKey); } catch { /* Ignore remove error */ }
            }

            // Return default preferences if loading failed or no preferences were found
            return new UserPreferences(); 
        }

        /// <summary>
        /// Saves preferences to local storage.
        /// </summary>
        public async Task SavePreferencesAsync(UserPreferences preferences)
        {
             if (preferences == null)
            {
                _logger.LogWarning("Attempted to save null preferences.");
                return; // Or throw an ArgumentNullException
            }

            _logger.LogInformation("Attempting to save user preferences to local storage. Color: {Color}", preferences.PlayerSnakeColorHex);
            try
            {
                var preferencesJson = JsonSerializer.Serialize(preferences);
                await _localStorage.SetItemAsStringAsync(PreferencesKey, preferencesJson); // Save as string
                _logger.LogInformation("Successfully saved preferences to local storage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving preferences to local storage.");
                // Consider how to handle save failures - maybe notify the user?
            }
        }
    }
}
