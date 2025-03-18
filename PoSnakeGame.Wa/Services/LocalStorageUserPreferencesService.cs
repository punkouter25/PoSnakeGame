using Microsoft.JSInterop;
using PoSnakeGame.Core.Interfaces;
using PoSnakeGame.Core.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Implementation of IUserPreferencesService that uses browser local storage
    /// </summary>
    public class LocalStorageUserPreferencesService : IUserPreferencesService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string StorageKey = "userPreferences";

        public LocalStorageUserPreferencesService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<UserPreferences> GetUserPreferencesAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
                
                if (string.IsNullOrEmpty(json))
                {
                    return new UserPreferences();
                }
                
                return JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();
            }
            catch (Exception)
            {
                // Return default preferences if there's an error
                return new UserPreferences();
            }
        }

        public async Task SaveUserPreferencesAsync(UserPreferences preferences)
        {
            var json = JsonSerializer.Serialize(preferences);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
    }
} 