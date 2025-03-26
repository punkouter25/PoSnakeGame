using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Models;

namespace PoSnakeGame.Core.Services
{
    public class GameService
    {
        private readonly ILogger<GameService> _logger;

        // Game state
        public required Arena Arena { get; set; }
        public List<Snake> Snakes { get; private set; } = new();
        public bool IsGameRunning { get; private set; }
        public bool IsGameOver { get; private set; }
        public float TimeRemaining { get; private set; }

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

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            _logger.LogInformation("GameService created");
        }

        public void InitializeGame(int playerCount = 1, int cpuCount = 7)
        {
            _logger.LogInformation("Initializing game with {PlayerCount} players and {CpuCount} CPU snakes", playerCount, cpuCount);

            Arena = new Arena(_arenaWidth, _arenaHeight);
            Snakes.Clear();
            IsGameRunning = false;
            IsGameOver = false;
            TimeRemaining = Arena.GameDuration;
            Arena.GameSpeed = _globalSpeedMultiplier; // Start with the global speed multiplier

            // Initialize countdown
            IsCountdownActive = true;
            CountdownValue = 3;
            _countdownTimer = 0f;
            _logger.LogInformation("Countdown initialized: Active={IsActive}, Value={Value}", IsCountdownActive, CountdownValue);

            // Create player snake with a more vibrant red color to make it stand out
            var playerSnake = CreateSnake(SnakeType.Human, Color.FromArgb(255, 0, 0), null, 1.2f); // Brighter red and 20% larger
            Snakes.Add(playerSnake);

            // Create CPU snakes (all in green)
            string[] personalities = {
                "Aggressive", "Cautious", "Foodie",
                "Random", "Hunter", "Survivor", "Speedy"
            };

            for (int i = 0; i < cpuCount; i++)
            {
                var personality = personalities[i % personalities.Length];
                var cpuSnake = CreateSnake(
                    SnakeType.CPU,
                    Color.Green,
                    personality
                );
                Snakes.Add(cpuSnake);
            }

            // Generate initial food
            for (int i = 0; i < _initialFoodCount; i++)
            {
                SpawnFood();
            }

            SafeInvokeOnUiThread(() => OnGameStateChanged?.Invoke());
        }

        private Snake CreateSnake(SnakeType type, Color color, string? personality = null, float sizeMultiplier = 1.0f)
        {
            Position startPosition;
            Direction initialDirection;

            if (type == SnakeType.Human)
            {
                // Place human snake in the center of the arena
                startPosition = new Position(_arenaWidth / 2, _arenaHeight / 2);
                // Start facing right for predictability
                initialDirection = Direction.Right;
                _logger.LogInformation("Player snake created at center position: {Position}", startPosition);
            }
            else
            {
                // For CPU snakes, find a random spawn position that doesn't overlap with other snakes
                // and is at least 5 units away from the player snake
                var playerSnake = Snakes.FirstOrDefault(s => s.Type == SnakeType.Human);
                do
                {
                    startPosition = new Position(
                        _random.Next(1, _arenaWidth - 1),
                        _random.Next(1, _arenaHeight - 1)
                    );
                } while (IsPositionOccupied(startPosition) || 
                        (playerSnake != null && 
                         CalculateManhattanDistance(startPosition, playerSnake.Segments[0]) < 5));

                // Random initial direction for CPU snakes
                initialDirection = (Direction)_random.Next(4);
            }

            var snake = new Snake(startPosition, initialDirection, color, type)
            {
                Personality = personality,
                SizeMultiplier = sizeMultiplier
            };

            _logger.LogInformation("Created {Type} snake at {Position} facing {Direction}", 
                type, startPosition, initialDirection);

            return snake;
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

                    if (CountdownValue <= 0)
                    {
                        IsCountdownActive = false;
                        _logger.LogInformation("Countdown completed. Game starting properly now.");

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

            // Notify listeners (safely on UI thread)
            if (OnGameStateChanged != null)
            {
                SafeInvokeOnUiThread(() => OnGameStateChanged.Invoke());
            }
        }

        private void MoveSnakes()
        {
            foreach (var snake in Snakes.Where(s => s.IsAlive))
            {
                // For CPU snakes, decide on direction change
                if (snake.Type == SnakeType.CPU)
                {
                    UpdateCpuSnakeDirection(snake);
                }

                // Calculate new head position
                Position directionVector = Position.FromDirection(snake.CurrentDirection);
                Position newHead = snake.Segments[0] + directionVector;

                // Move the snake
                snake.Move(newHead);

                _logger.LogDebug("Snake moved to {Position}", newHead);
            }
        }

        private void UpdateCpuSnakeDirection(Snake snake)
        {
            // Different AI logic based on personality
            CpuSnakeAI? ai = snake.Personality switch
            {
                "Aggressive" => new AggressiveAI(snake, Arena),
                "Cautious" => new CautiousAI(snake, Arena),
                "Foodie" => new FoodieAI(snake, Arena),
                "Random" => new RandomAI(snake, Arena),
                "Hunter" => new HunterAI(snake, Arena),
                "Survivor" => new SurvivorAI(snake, Arena),
                "Speedy" => new SpeedyAI(snake, Arena),
                _ => null
            };

            ai?.UpdateDirection();
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

        private void CheckCollisions()
        {
            foreach (var snake in Snakes.Where(s => s.IsAlive))
            {
                var head = snake.Segments[0];

                // Check wall collision
                if (Arena.IsOutOfBounds(head))
                {
                    _logger.LogInformation("Snake hit wall at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check obstacle collision
                if (Arena.HasCollision(head))
                {
                    _logger.LogInformation("Snake hit obstacle at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check self collision (skip the head)
                if (snake.Segments.Skip(1).Any(segment => segment.Equals(head)))
                {
                    _logger.LogInformation("Snake hit itself at {Position}", head);
                    HandleSnakeCollision(snake);
                    continue;
                }

                // Check collision with other snakes
                foreach (var otherSnake in Snakes.Where(s => s != snake && s.IsAlive))
                {
                    if (otherSnake.Segments.Any(segment => segment.Equals(head)))
                    {
                        _logger.LogInformation("Snake hit another snake at {Position}", head);
                        HandleSnakeCollision(snake);
                        break;
                    }
                }

                // Check food consumption
                var foodIndex = Arena.Foods.FindIndex(f => f.Equals(head));
                if (foodIndex >= 0)
                {
                    var food = Arena.Foods[foodIndex];
                    Arena.Foods.RemoveAt(foodIndex);

                    // Snake grows and gets points
                    snake.Grow();
                    snake.AddPoints(10);

                    _logger.LogInformation("Snake ate food at {Position}", head);

                    // Spawn new food to maintain target count
                    SpawnFood();
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
                }
            }

            // Check if game is over (only player snake matters for now)
            if (Snakes.Count > 0 && Snakes.First().Type == SnakeType.Human && !Snakes.First().IsAlive)
            {
                EndGame();
            }
        }

        private void HandleSnakeCollision(Snake snake)
        {
            snake.Die();

            // When a snake dies, its segments become obstacles
            foreach (var segment in snake.Segments)
            {
                Arena.AddObstacle(segment);
            }
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
            var humanSnake = Snakes.FirstOrDefault(s => s.Type == SnakeType.Human && s.IsAlive);
            if (humanSnake != null)
            {
                ChangeDirection(newDirection, humanSnake);
            }
        }
    }
}
