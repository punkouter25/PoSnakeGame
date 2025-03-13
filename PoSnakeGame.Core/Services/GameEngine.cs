using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PoSnakeGame.Core.Services;

/// <summary>
/// Manages the game loop and timing
/// Implements the Strategy pattern to decouple the game loop from the game logic
/// </summary>
public class GameEngine
{
    private readonly GameService _gameService;
    private readonly ILogger<GameEngine> _logger;
    private readonly Stopwatch _stopwatch = new();
    
    private Task? _gameLoopTask;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private readonly int _targetFPS = 10; // Target frames per second - REDUCED FROM 30 to 10 to slow down gameplay
    private readonly int _frameTimeMs; // Target time per frame in milliseconds
    private float _deltaTime; // Time between frames in seconds
    
    public bool IsRunning { get; private set; }
    
    public GameEngine(GameService gameService, ILogger<GameEngine> logger)
    {
        _gameService = gameService;
        _logger = logger;
        _frameTimeMs = 1000 / _targetFPS;
        _deltaTime = 1.0f / _targetFPS;
        
        _logger.LogInformation("GameEngine created with target FPS: {TargetFPS}", _targetFPS);
    }
    
    public void Start()
    {
        if (IsRunning) return;
        
        _logger.LogInformation("Starting game engine");
        
        _cancellationTokenSource = new CancellationTokenSource();
        _gameLoopTask = Task.Run(GameLoop, _cancellationTokenSource.Token);
        IsRunning = true;
    }
    
    public async Task StopAsync()
    {
        if (!IsRunning) return;
        
        _logger.LogInformation("Stopping game engine");
        
        // Cancel the token to signal the game loop to stop
        _cancellationTokenSource?.Cancel();
        
        // Don't wait for the task to complete, as it might be blocked on UI operations
        // Just mark the engine as stopped
        IsRunning = false;
        
        // Allow a short delay for cancellation to propagate
        await Task.Delay(100);
        
        // Clean up resources
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _gameLoopTask = null;
    }
    
    // Synchronous version for backward compatibility
    public void Stop()
    {
        // Fire and forget the async method
        _ = StopAsync();
    }
    
    private async Task GameLoop()
    {
        _logger.LogDebug("Game loop started");
        _gameService.StartGame();
        _stopwatch.Restart();
        
        while (!_cancellationTokenSource!.Token.IsCancellationRequested && _gameService.IsGameRunning)
        {
            long frameStartTime = _stopwatch.ElapsedMilliseconds;
            
            // Update game state
            _gameService.Update(_deltaTime);
            
            // Calculate how long to sleep to maintain target FPS
            long frameTime = _stopwatch.ElapsedMilliseconds - frameStartTime;
            int sleepTime = (int)(_frameTimeMs - frameTime);
            
            if (sleepTime > 0)
            {
                await Task.Delay(sleepTime, _cancellationTokenSource.Token);
            }
            else
            {
                // If we're running behind, yield to other tasks but don't sleep
                await Task.Yield();
                
                // Log if we're significantly behind
                if (sleepTime < -10)
                {
                    _logger.LogWarning("Game loop running slow by {Milliseconds}ms", -sleepTime);
                }
            }
            
            // Recalculate actual delta time for next frame
            long actualFrameTime = _stopwatch.ElapsedMilliseconds - frameStartTime;
            _deltaTime = actualFrameTime / 1000.0f; // Convert to seconds
            
            // Limit delta time to avoid spiral of death if frame takes too long
            if (_deltaTime > 0.1f) _deltaTime = 0.1f;
            
            // Debug logging every second
            if (frameStartTime / 1000 != (_stopwatch.ElapsedMilliseconds) / 1000)
            {
                _logger.LogDebug("FPS: {FPS}", 1000.0f / actualFrameTime);
            }
        }
        
        _stopwatch.Stop();
        _logger.LogDebug("Game loop ended");
    }
}
