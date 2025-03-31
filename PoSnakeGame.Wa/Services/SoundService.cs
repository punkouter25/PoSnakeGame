using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using PoSnakeGame.Core.Interfaces; // Correct namespace for ISoundService

namespace PoSnakeGame.Wa.Services
{
    /// <summary>
    /// Service implementation for playing sound effects.
    /// </summary>
    public class SoundService : ISoundService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<SoundService> _logger; // Add logger

        public SoundService(IJSRuntime jsRuntime, ILogger<SoundService> logger) // Inject logger
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task PlaySoundAsync(string soundFileName, double volume = 1.0)
        {
            try
            {
                // Ensure volume is clamped between 0.0 and 1.0
                var clampedVolume = Math.Max(0.0, Math.Min(1.0, volume));
                
                // Call the JavaScript function
                await _jsRuntime.InvokeVoidAsync("playSound", soundFileName, clampedVolume);
                _logger.LogDebug("Requested playback for sound: {SoundFile} at volume {Volume}", soundFileName, clampedVolume);
            }
            catch (JSException jsEx)
            {
                // Log JS-specific errors
                 _logger.LogError(jsEx, "JavaScript error playing sound {SoundFile}: {ErrorMessage}", soundFileName, jsEx.Message);
                 // Decide if you want to re-throw or handle silently
            }
            catch (Exception ex)
            {
                // Log other potential errors (e.g., component disposal during async operation)
                _logger.LogError(ex, "Generic error playing sound {SoundFile}", soundFileName);
                // Decide if you want to re-throw or handle silently
            }
        }
    }
}
