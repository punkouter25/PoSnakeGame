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
        public Task PlaySoundAsync(string soundFileName, double volume = 1.0)
        {
            // Sounds disabled - do nothing.
            _logger.LogDebug("Sound playback disabled for: {SoundFile}", soundFileName);
            return Task.CompletedTask; // Immediately return a completed task
        }
    }
}
