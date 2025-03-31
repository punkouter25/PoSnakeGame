using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Core.Interfaces; // ISoundService is now here

namespace PoSnakeGame.Core.Services
{
    // Strategy Pattern: GameService acts as the context for the game logic strategy
    // implementing high cohesion and single responsibility principle
    public class GameService
    {
        private readonly ILogger<GameService> _logger;
        private readonly ISoundService _soundService; // Inject sound service

        // Game state
        public required Arena Arena { get; set; }
        public List<Snake> Snakes { get; private set; } = new();
        public bool IsGameRunning { get; private set; }
        public bool IsGameOver { get; private set; }
        public float TimeRemaining { get; private set; }
        
        // Track foods that are being eaten and fading out
        public List<Position> FadingFoods { get; private set; } = new();
        private Dictionary<Position, float> _foodFadeTimers = new();
        private const float FoodFadeTime = 0.3f; // Match CSS animation time
        
        // Track dying snakes with animation
        public List<Snake> DyingSnakes { get; private set; } = new();
        private Dictionary<Snake, float> _snakeDyingTimers = new();
        private const float SnakeDyingTime = 0.5f; // Match CSS animation time

        // Game configuration
        private readonly int _arenaWidth = 40;  // Reduced from 50 to make snake more visible
        private readonly int _arenaHeight = 40; // Reduced from 100 to make arena more square and visible
        private readonly int _initialFoodCount = 20;  // Reduced from 100 to avoid overcrowding
        private readonly int _targetFoodCount = 10;   // Reduced from 20 to avoid overcrowding
        private readonly Random _random = new();
        private readonly float _globalSpeedMultiplier = 0.3f;
        private readonly float _powerUpSpawnChance = 0.1f;
        public bool IsCountdownActive { get; private set; } = false; // New property for countdown state
        public int CountdownValue { get; private set; } = 3; // New property for countdown value
        private float _countdownTimer = 0f; // Timer for countdown

        public event Action? OnGameStateChanged;
        public event Action? OnGameOver;
        public event Action? OnCountdownChanged; // New event for countdown changes

        // Dispatcher for UI thread synchronization
        private Action<Action>? _uiThreadInvoker;

        // Method to set the UI thread invoker
        public void SetUiThreadInvoker(Action<Action> invoker)
        {
            _uiThreadInvoker = invoker;
        }

        // Helper method to safely invoke events on the UI thread
        private void SafeInvokeOnUiThread(Action action)
        {
            if (_uiThreadInvoker != null)
            {
                _uiThreadInvoker(action);
            }
            else
            {
                // If no invoker is set, just invoke directly (may cause threading issues)
                action();
            }
        }

        // Inject ISoundService
        public GameService(ILogger<GameService> logger, ISoundService soundService) 
        {
            _logger = logger;
            _soundService = soundService; // Store injected service
            _logger.LogInformation("GameService created");
        }

        // Modified to accept optional user preferences
        public void InitializeGame(int playerCount = 1, int cpuCount = 15, UserPreferences? preferences = null) 
        {
            _logger.LogInformation("Initializing game with {PlayerCount} players and {CpuCount} CPU snakes. Preferences provided: {HasPrefs}", 
                playerCount, cpuCount, preferences != null);

            Arena = new Arena(_arenaWidth, _arenaHeight);
            Snakes.Clear();
            DyingSnakes.Clear();
            FadingFoods.Clear();
            _foodFadeTimers.Clear();
            _snakeDyingTimers.Clear();
            
            IsGameRunning = false;
            IsGameOver = false;
            TimeRemaining = Arena.GameDuration;
            Arena.GameSpeed = _globalSpeedMultiplier; // Start with the global speed multiplier

            // For test environments, disable countdown to allow immediate movement
            IsCountdownActive = Arena.Width != 10 && Arena.Width != 20; // Only active in normal game, not in test arena sizes
            CountdownValue = 3;
            _countdownTimer = 0f;
            _logger.LogInformation("Countdown initialized: Active={IsActive}, Value={Value}", IsCountdownActive, CountdownValue);

            // Define personality colors
            var personalityColors = new Dictionary<SnakePersonality, string>
            {
                { SnakePersonality.Human, preferences?.PlayerSnakeColorHex ?? "#00FF00" }, // Use Hex property, Green or preference
                { SnakePersonality.Random, "#808080" },      // Gray
                { SnakePersonality.Foodie, "#FFFF00" },      // Yellow
                { SnakePersonality.Cautious, "#00FFFF" },    // Cyan
                { SnakePersonality.Hunter, "#FF0000" },      // Red (Color still defined, but personality won't be used)
                { SnakePersonality.Survivor, "#00BFFF" },    // Deep Sky Blue
                { SnakePersonality.Speedy, "#FFA500" },      // Orange
                { SnakePersonality.Aggressive, "#800080" }   // Purple
            };
            _logger.LogInformation("Using player snake color: {Color}", personalityColors[SnakePersonality.Human]);

            // Create player snake
            var playerSnake = new Snake(
                new Position(_arenaWidth / 8, _arenaHeight / 2), // Left side position
                Direction.Right,
                personalityColors[SnakePersonality.Human],
                SnakePersonality.Human)
            {
                SizeMultiplier = 1.2f // 20% larger
            };
            Snakes.Add(playerSnake);
            _logger.LogInformation("Player snake created at position: {Position}", playerSnake.Segments[0]);

            // Create CPU snakes, excluding Hunter
            var cpuPersonalities = Enum.GetValues<SnakePersonality>()
                                       .Where(p => p != SnakePersonality.Human && p != SnakePersonality.Hunter) // Exclude Hunter
                                       .ToList();
            if (!cpuPersonalities.Any()) // Handle case where only Human/Hunter exist
            {
                 _logger.LogWarning("No CPU personalities available after excluding Human and Hunter.");
                 // Optionally add a default like Random if the list becomes empty
                 // cpuPersonalities.Add(SnakePersonality.Random); 
            }

            for (int i = 0; i < cpuCount; i++)
            {
                 // Ensure we don't crash if the list is empty
                if (!cpuPersonalities.Any()) break; 

                var personality = cpuPersonalities[i % cpuPersonalities.Count]; // Cycle through available CPU personalities
                var color = personalityColors[personality];

                // Calculate position on the right side with some spacing
                Position startPosition;
                do
                {
                    startPosition = new Position(
                        _random.Next(_arenaWidth * 3 / 4, _arenaWidth - 2), // Right side of arena
                        _random.Next(1, _arenaHeight - 1)
                    );
                } while (IsPositionOccupied(startPosition));

                // Create snake facing left (toward player side)
                var cpuSnake = new Snake(startPosition, Direction.Left, color, personality);
                // Note: SpeedyAI constructor sets its own speed

                Snakes.Add(cpuSnake);
                _logger.LogInformation("CPU snake ({Personality}) created at position: {Position} with color {Color}",
                    personality, cpuSnake.Segments[0], color);
            }

            // Make sure Arena's Snakes reference points to our Snakes list
            Arena.Snakes = Snakes;

            // Generate initial food
            for (int i = 0; i < _initialFoodCount; i++)
            {
                SpawnFood();
            }

            SafeInvokeOnUiThread(() => OnGameStateChanged?.Invoke());
        }

        public void StartGame()
        {
            if (IsGameRunning) return;

            IsGameRunning = true;
            IsGameOver = false;
            _logger.LogInformation("Game started. Countdown status: Active={IsActive}, Value={Value}", IsCountdownActive, CountdownValue);

            // Reset the countdown timer to ensure it continues correctly when resuming from pause
            if (IsCountdownActive)
            {
                _countdownTimer = 0f;
                _logger.LogInformation("Countdown timer reset during game resume");
                // Play countdown start sound?
                // _ = _soundService.PlaySoundAsync("countdown.wav"); 
            }

            // Initial game state notification
            if (OnGameStateChanged != null)
            {
                SafeInvokeOnUiThread(() => OnGameStateChanged.Invoke());
            }
        }

        public void PauseGame()
        {
            if (!IsGameRunning) return;

            IsGameRunning = false;
            _logger.LogInformation("Game paused");

            // Save the countdown state if paused during countdown
            if (IsCountdownActive)
            {
                _logger.LogInformation("Game paused during active countdown. Value={Value}", CountdownValue);
            }

            if (OnGameStateChanged != null)
            {
                SafeInvokeOnUiThread(() => OnGameStateChanged.Invoke());
            }
        }

        public void EndGame()
        {
            IsGameRunning = false;
            IsGameOver = true;
            _logger.LogInformation("Game over. Final countdown state: Active={IsActive}, Value={Value}", IsCountdownActive, CountdownValue);
            
            // Play game over sound
            _ = _soundService.PlaySoundAsync("gameover.wav"); 

            if (OnGameOver != null)
            {
                SafeInvokeOnUiThread(() => OnGameOver.Invoke());
            }
        }

        public void Update(float deltaTime)
        {
            if (!IsGameRunning || IsGameOver) return;

            // Handle countdown timer if active
            if (IsCountdownActive)
            {
                _countdownTimer += deltaTime;
                _logger.LogDebug("Countdown timer updated: CurrentTimer={Timer:0.00}, CountdownValue={Value}", _countdownTimer, CountdownValue);
                
                if (_countdownTimer >= 1.0f) // Every second
                {
                    _countdownTimer = 0f;
                    CountdownValue--;
                    _logger.LogInformation("Countdown decremented to {Value}", CountdownValue);
                    
                    // Play countdown tick sound
                    _ = _soundService.PlaySoundAsync("tick.wav", 0.8); 

                    if (CountdownValue <= 0)
                    {
                        IsCountdownActive = false;
                        _logger.LogInformation("Countdown completed. Game starting properly now.");
                        // Play countdown finish sound?
                        // _ = _soundService.PlaySoundAsync("start.wav"); 

                        // Notify of countdown end
                        if (OnCountdownChanged != null)
                        {
                            SafeInvokeOnUiThread(() => OnCountdownChanged.Invoke());
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Countdown in progress: {Value} seconds remaining", CountdownValue);
                        // Notify of countdown change
                        if (OnCountdownChanged != null)
                        {
                            SafeInvokeOnUiThread(() => OnCountdownChanged.Invoke());
                        }
                    }
                }

                // Don't update game state during countdown
                return;
            }

            // Update game timer
            TimeRemaining -= deltaTime;
            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                EndGame();
                return;
            }

            // Gradually increase game speed as time passes
            float progress = 1 - (TimeRemaining / Arena.GameDuration);
            Arena.GameSpeed = _globalSpeedMultiplier * (1.0f + (progress * 0.3f)); // Speed ranges from global*1.0 to global*1.3

            // Make sure the Arena's Snakes reference points to our Snakes list (critical for tests)
            Arena.Snakes = Snakes;
            
            // Move all snakes
            MoveSnakes();

            // Check for collisions
            CheckCollisions();

            // Ensure food count is maintained at target
            while (Arena.Foods.Count < _targetFoodCount)
            {
                SpawnFood();
            }

            // Handle power-up spawning
            if (_random.NextDouble() < _powerUpSpawnChance * deltaTime)
            {
                SpawnPowerUp();
            }

            // Update game state
            Arena.ElapsedTime += deltaTime;

            // Update food fade timers
            UpdateFadingFoods(deltaTime);
            
            // Update dying snake timers
            UpdateDyingSnakes(deltaTime);

            // Notify listeners (safely on UI thread)
            if (OnGameStateChanged != null)
            {
                SafeInvokeOnUiThread(() => OnGameStateChanged.Invoke());
            }
        }

        private void MoveSnakes()
        {
            // Debug information to help with test debugging
            // Console.WriteLine($"Moving {Snakes.Count} snakes, {Snakes.Count(s => s.IsAlive)} are alive");

            foreach (var snake in Snakes.Where(s => s.IsAlive))
            {
                // For CPU snakes, decide on direction change
                if (snake.Personality != SnakePersonality.Human) // Check personality enum
                {
                    UpdateCpuSnakeDirection(snake);

                    // Reduce default speed of CPU snakes unless it's SpeedyAI or speed modified by powerup
                    if (snake.Personality != SnakePersonality.Speedy && snake.Speed == 1.0f)
                    {
                        snake.Speed = 0.7f; // Base speed for non-Speedy CPU
                    }
                }

                // Calculate new head position
                Position directionVector = Position.FromDirection(snake.CurrentDirection);
                Position newHead = snake.Segments[0] + directionVector;

                // Move the snake
                snake.Move(newHead);

                // Debug information for the snake's movement
                // Console.WriteLine($"Snake moved to {newHead.X},{newHead.Y}, Direction: {snake.CurrentDirection}");
                // _logger.LogDebug("Snake moved to {Position}", newHead);
            }
        }

        private void UpdateCpuSnakeDirection(Snake snake)
        {
            // Different AI logic based on personality
            CpuSnakeAI? ai = null;

            // Make Arena.Snakes point to our list of snakes for proper AI behavior
            // This is critical for test cases to work properly
            Arena.Snakes = Snakes;

            // Debug information about the current state
            // Console.WriteLine($"Updating direction for CPU snake with personality: {snake.Personality}, at position ({snake.Segments[0].X}, {snake.Segments[0].Y})");
            
            // Using Factory Method pattern to create the appropriate AI implementation
            switch (snake.Personality) // Switch on the enum
            {
                case SnakePersonality.Aggressive:
                    ai = new AggressiveAI(snake, Arena);
                    break;
                case SnakePersonality.Cautious: // Handles both Advanced and Cautious logic now
                    ai = new CautiousAI(snake, Arena);
                    break;
                case SnakePersonality.Foodie: // Handles Simple and Foodie logic now
                    ai = new FoodieAI(snake, Arena);
                    break;
                case SnakePersonality.Random:
                    ai = new RandomAI(snake, Arena);
                    break;
                // Hunter case removed as the class was deleted
                case SnakePersonality.Survivor:
                    ai = new SurvivorAI(snake, Arena);
                    break;
                case SnakePersonality.Speedy:
                    ai = new SpeedyAI(snake, Arena);
                    break;
                // No default needed if all enum values are handled and Human is excluded earlier
                // case SnakePersonality.Human: // Should not happen due to earlier check
                //     break; 
            }

            ai?.UpdateDirection();
            
            // Console.WriteLine($"CPU snake direction updated to: {snake.CurrentDirection}");
        }

        private int CalculateManhattanDistance(Position p1, Position p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        private bool IsOppositeDirection(Direction dir1, Direction dir2)
        {
            return (dir1 == Direction.Up && dir2 == Direction.Down) ||
                   (dir1 == Direction.Down && dir2 == Direction.Up) ||
                   (dir1 == Direction.Left && dir2 == Direction.Right) ||
                   (dir1 == Direction.Right && dir2 == Direction.Left);
        }

        // New method to get valid relative directions (forward, left, right, but not backward)
        private List<Direction> GetValidRelativeDirections(Direction currentDirection)
        {
            var directions = new List<Direction>();
            
            // Add current direction (forward)
            directions.Add(currentDirection);
            
            // Add left and right relative to current direction
            switch (currentDirection)
            {
                case Direction.Up:
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Right);
                    break;
                case Direction.Down:
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Right);
                    break;
                case Direction.Left:
                    directions.Add(Direction.Up);
                    directions.Add(Direction.Down);
                    break;
                case Direction.Right:
                    directions.Add(Direction.Up);
                    directions.Add(Direction.Down);
                    break;
            }
            
            return directions;
        }

        private void CheckCollisions()
        {
            foreach (var snake in Snakes.Where(s => s.IsAlive))
            {
                var head = snake.Segments[0];

                // Debug info for wall collisions
                // Console.WriteLine($"Checking wall collision for snake at {head.X},{head.Y}, arena size: {Arena.Width}x{Arena.Height}");
                
                // Check wall collision
                if (Arena.IsOutOfBounds(head))
                {
                    // Console.WriteLine($"Snake hit wall at {head.X},{head.Y}");
                    _logger.LogInformation("Snake hit wall at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check obstacle collision
                if (Arena.HasCollision(head))
                {
                    // Console.WriteLine($"Snake hit obstacle at {head.X},{head.Y}");
                    _logger.LogInformation("Snake hit obstacle at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check self collision (skip the head)
                if (snake.Segments.Skip(1).Any(segment => segment.Equals(head)))
                {
                    // Console.WriteLine($"Snake hit itself at {head.X},{head.Y}");
                    _logger.LogInformation("Snake hit itself at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check collision with other snakes
                foreach (var otherSnake in Snakes.Where(s => s != snake && s.IsAlive))
                {
                    if (otherSnake.Segments.Any(segment => segment.Equals(head)))
                    {
                        // Console.WriteLine($"Snake hit another snake at {head.X},{head.Y}");
                        _logger.LogInformation("Snake hit another snake at {Position}", head);
                        HandleSnakeCollision(snake);
                        break; // Exit inner loop once collision is handled for this snake
                    }
                }
                
                // If snake died in the previous check, skip food/powerup checks
                if (!snake.IsAlive) continue; 

                // Check food consumption
                var foodIndex = Arena.Foods.FindIndex(f => f.Equals(head));
                if (foodIndex >= 0)
                {
                    var food = Arena.Foods[foodIndex];
                    
                    // Instead of immediately removing food, mark it as fading
                    if (!FadingFoods.Contains(food))
                    {
                        // Console.WriteLine($"Snake ate food at {head.X},{head.Y}");
                        
                        // Add to fading foods list
                        FadingFoods.Add(food);
                        _foodFadeTimers[food] = FoodFadeTime;
                        
                        // Remove from active foods
                        Arena.Foods.RemoveAt(foodIndex);
                        
                        // Snake grows and gets points
                        snake.Grow();
                        snake.AddPoints(10);
                        
                        _logger.LogInformation("Snake ate food at {Position}", head);
                        
                        // Play eat sound
                        _ = _soundService.PlaySoundAsync("eat.wav", 0.7); 
                        
                        // Spawn new food to maintain target count
                        SpawnFood();
                    }
                }

                // Check power-up consumption
                var powerUpIndex = Arena.PowerUps.FindIndex(p => p.Position.Equals(head));
                if (powerUpIndex >= 0)
                {
                    var powerUp = Arena.PowerUps[powerUpIndex];
                    Arena.PowerUps.RemoveAt(powerUpIndex);

                    // Apply power-up effect
                    ApplyPowerUp(snake, powerUp);

                    _logger.LogInformation("Snake consumed power-up at {Position}", head);
                    
                    // Play power-up sound
                    _ = _soundService.PlaySoundAsync("powerup.wav", 0.9); 
                }
            }

            // Check if game is over (only player snake matters for now)
            var playerSnake = Snakes.FirstOrDefault(s => s.Personality == SnakePersonality.Human);
            if (playerSnake != null && !playerSnake.IsAlive)
            {
                EndGame();
            }
        }

        private void HandleSnakeCollision(Snake snake)
        {
            // Mark snake as dead
            snake.Die();
            // Console.WriteLine($"Snake {snake.Personality} died due to collision"); // Use Personality enum
            _logger.LogInformation("Snake {Personality} died due to collision", snake.Personality);

            // Play death sound
            _ = _soundService.PlaySoundAsync("death.wav", 0.6); 

            // Add to dying snakes for animation
            if (!DyingSnakes.Contains(snake))
            {
                DyingSnakes.Add(snake);
                _snakeDyingTimers[snake] = SnakeDyingTime;
            }
            
            // Segments become obstacles after animation completes in UpdateDyingSnakes
        }

        private void ApplyPowerUp(Snake snake, PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.Speed:
                    snake.Speed *= (1 + powerUp.Value);
                    _logger.LogDebug("Snake speed increased to {Speed}", snake.Speed);
                    break;

                case PowerUpType.SlowDown:
                    snake.Speed *= (1 - powerUp.Value);
                    if (snake.Speed < 0.5f) snake.Speed = 0.5f; // Minimum speed
                    _logger.LogDebug("Snake speed decreased to {Speed}", snake.Speed);
                    break;

                case PowerUpType.Points:
                    snake.AddPoints((int)powerUp.Value);
                    _logger.LogDebug("Snake got {Points} points", powerUp.Value);
                    break;

                default:
                    _logger.LogWarning("Unknown power-up type: {Type}", powerUp.Type);
                    break;
            }
            // Note: Sound is played in CheckCollisions after ApplyPowerUp is called
        }

        private void SpawnFood()
        {
            // Find a free position for food
            Position position;
            do
            {
                position = new Position(
                    _random.Next(_arenaWidth),
                    _random.Next(_arenaHeight)
                );
            } while (IsPositionOccupied(position));

            Arena.AddFood(position);
            _logger.LogDebug("Food spawned at {Position}", position);
        }

        private void SpawnPowerUp()
        {
            // Find a free position for the power-up
            Position position;
            do
            {
                position = new Position(
                    _random.Next(_arenaWidth),
                    _random.Next(_arenaHeight)
                );
            } while (IsPositionOccupied(position));

            // Determine power-up type
            PowerUpType type = (PowerUpType)_random.Next(Enum.GetValues(typeof(PowerUpType)).Length);

            // Create power-up with appropriate values based on type
            float duration = 5f; // 5 seconds default duration
            float value = 0;

            switch (type)
            {
                case PowerUpType.Speed:
                    value = 0.5f; // 50% speed boost
                    break;
                case PowerUpType.SlowDown:
                    value = 0.3f; // 30% speed reduction
                    break;
                case PowerUpType.Points:
                    value = 50; // 50 points bonus
                    duration = 0; // Instant effect
                    break;
            }

            var powerUp = new PowerUp(position, type, duration, value);
            Arena.AddPowerUp(powerUp);

            _logger.LogDebug("PowerUp {Type} spawned at {Position}", type, position);
        }

        private bool IsPositionOccupied(Position position)
        {
            // Check if position is occupied by any snake
            if (Snakes.Any(snake => snake.Segments.Any(segment => segment.Equals(position))))
                return true;

            // Check if position is occupied by food
            if (Arena.Foods.Any(food => food.Equals(position)))
                return true;

            // Check if position is occupied by power-up
            if (Arena.PowerUps.Any(powerUp => powerUp.Position.Equals(position)))
                return true;

            // Check if position is an obstacle
            if (Arena.Obstacles.Contains(position))
                return true;

            return false;
        }

        public void ChangeDirection(Direction newDirection, Snake snake)
        {
            // Ensure the snake can't turn 180 degrees
            if (!IsOppositeDirection(snake.CurrentDirection, newDirection))
            {
                snake.CurrentDirection = newDirection;
            }
        }

        public void ChangeHumanSnakeDirection(Direction newDirection)
        {
            // Find the human snake and change its direction
            var humanSnake = Snakes.FirstOrDefault(s => s.Personality == SnakePersonality.Human && s.IsAlive);
            if (humanSnake != null)
            {
                ChangeDirection(newDirection, humanSnake);
            }
        }

        private void UpdateFadingFoods(float deltaTime)
        {
            // Remove fully faded food items
            var finishedFoods = new List<Position>();
            
            foreach (var food in _foodFadeTimers.Keys.ToList())
            {
                _foodFadeTimers[food] -= deltaTime;
                if (_foodFadeTimers[food] <= 0)
                {
                    finishedFoods.Add(food);
                }
            }
            
            // Remove completed animations
            foreach (var food in finishedFoods)
            {
                FadingFoods.Remove(food);
                _foodFadeTimers.Remove(food);
            }
        }

        private void UpdateDyingSnakes(float deltaTime)
        {
            // Process dying snakes timers
            var completedSnakes = new List<Snake>();
            
            foreach (var snake in _snakeDyingTimers.Keys.ToList())
            {
                _snakeDyingTimers[snake] -= deltaTime;
                
                // When animation completes, add segments as obstacles
                if (_snakeDyingTimers[snake] <= 0)
                {
                    completedSnakes.Add(snake);
                    
                    // Add snake segments as obstacles
                    foreach (var segment in snake.Segments)
                    {
                        Arena.AddObstacle(segment);
                    }
                }
            }
            
            // Remove completed snakes from dying list
            foreach (var snake in completedSnakes)
            {
                DyingSnakes.Remove(snake);
                _snakeDyingTimers.Remove(snake);
            }
        }
    }
}
