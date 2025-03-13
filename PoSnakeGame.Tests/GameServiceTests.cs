using Microsoft.Extensions.Logging;
using Moq;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Core.Services;
using System.Drawing;
using Xunit;

namespace PoSnakeGame.Tests
{
    public class GameServiceTests
    {
        private readonly Mock<ILogger<GameService>> _loggerMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            // Setup mock logger
            _loggerMock = new Mock<ILogger<GameService>>();
            _gameService = new GameService(_loggerMock.Object) { Arena = new Arena(20, 20) };
        }

        [Fact]
        public void InitializeGame_ShouldCreateSnakesAndFood()
        {
            // Arrange & Act
            _gameService.InitializeGame(playerCount: 1, cpuCount: 3);

            // Assert
            Assert.NotNull(_gameService.Arena);
            Assert.Equal(4, _gameService.Snakes.Count); // 1 player + 3 CPU
            Assert.True(_gameService.Snakes[0].Type == SnakeType.Human); // First snake should be human
            Assert.All(_gameService.Snakes.Skip(1), snake => Assert.Equal(SnakeType.CPU, snake.Type)); // Rest should be CPU
            Assert.NotEmpty(_gameService.Arena.Foods); // Should have some food
            Assert.True(_gameService.TimeRemaining > 0); // Time should be set
        }

        [Fact]
        public void Update_ShouldMoveSnakes()
        {
            // Arrange
            _gameService.InitializeGame(playerCount: 1, cpuCount: 0);
            var snake = _gameService.Snakes[0];
            var initialPosition = snake.Segments[0];
            _gameService.StartGame();

            // Act
            _gameService.Update(0.1f); // Update with small time step

            // Assert
            Assert.NotEqual(initialPosition, snake.Segments[0]); // Snake should have moved
        }

        [Fact]
        public void CheckCollisions_ShouldDetectWallCollision()
        {
            // Arrange
            _gameService.InitializeGame(playerCount: 1, cpuCount: 0);
            var snake = _gameService.Snakes[0];
            
            // Force snake position to be at the edge
            Direction edgeDirection = Direction.Right;
            Position edgePosition = new Position(_gameService.Arena.Width - 1, 5);
            
            // Use reflection to access private fields to modify snake position
            var segments = new List<Position> { edgePosition };
            typeof(Snake).GetProperty("Segments")!.SetValue(snake, segments);
            typeof(Snake).GetProperty("CurrentDirection")!.SetValue(snake, edgeDirection);
            
            _gameService.StartGame();

            // Act
            _gameService.Update(0.1f); // This should cause the snake to hit the wall

            // Assert
            Assert.False(snake.IsAlive); // Snake should be dead
        }

        [Fact]
        public void EatingFood_ShouldGrowSnakeAndAddPoints()
        {
            // Arrange
            _gameService.InitializeGame(playerCount: 1, cpuCount: 0);
            var snake = _gameService.Snakes[0];
            int initialLength = snake.Length;
            int initialScore = snake.Score;
            
            // Place food directly in front of the snake's head
            Position headPosition = snake.Segments[0];
            Position foodPosition = Position.FromDirection(snake.CurrentDirection) + headPosition;
            
            // Clear existing foods and add our test food
            _gameService.Arena.Foods.Clear();
            _gameService.Arena.AddFood(foodPosition);
            
            _gameService.StartGame();

            // Act
            _gameService.Update(0.1f); // Snake should eat the food

            // Assert
            Assert.True(snake.Length > initialLength); // Snake should have grown
            Assert.True(snake.Score > initialScore); // Score should have increased
        }

        [Fact]
        public void InputResponse_ShouldBeImmediate()
        {
            // Arrange
            _gameService.InitializeGame(playerCount: 1, cpuCount: 0);
            var snake = _gameService.Snakes[0];
            var initialDirection = snake.CurrentDirection;
            Direction newDirection = (initialDirection == Direction.Right) ? Direction.Up : Direction.Right;
            
            // Act
            _gameService.ChangeHumanSnakeDirection(newDirection);
            
            // Assert
            Assert.Equal(newDirection, snake.CurrentDirection); // Direction should change immediately
        }

        [Fact]
        public void SnakeMovement_ShouldBeSmooth()
        {
            // Arrange
            _gameService.InitializeGame(playerCount: 1, cpuCount: 0);
            var snake = _gameService.Snakes[0];
            var initialPosition = snake.Segments[0];
            _gameService.StartGame();
            
            // Act & Assert
            // Test multiple frames to ensure smooth movement
            for (int i = 0; i < 5; i++)
            {
                var previousPosition = snake.Segments[0];
                _gameService.Update(0.033f); // ~30 FPS
                var newPosition = snake.Segments[0];
                
                // Movement should be exactly one cell per frame at normal speed
                var distance = Math.Abs(newPosition.X - previousPosition.X) + Math.Abs(newPosition.Y - previousPosition.Y);
                Assert.Equal(1, distance); // Snake should move exactly one cell per frame
            }
        }

        [Fact]
        public void AggressiveAI_ShouldMoveRandomlyForNow()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Red, SnakeType.CPU) { Personality = "Aggressive" };
            var arena = new Arena(10, 10);
            var ai = new AggressiveAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.InRange((int)snake.CurrentDirection, 0, 3); // Direction should be one of the valid directions
        }

        [Fact]
        public void CautiousAI_ShouldMoveRandomlyForNow()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Blue, SnakeType.CPU) { Personality = "Cautious" };
            var arena = new Arena(10, 10);
            var ai = new CautiousAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.InRange((int)snake.CurrentDirection, 0, 3); // Direction should be one of the valid directions
        }

        [Fact]
        public void FoodieAI_ShouldMoveTowardsFood()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Yellow, SnakeType.CPU) { Personality = "Foodie" };
            var arena = new Arena(10, 10);
            arena.AddFood(new Position(7, 5)); // Place food to the right of the snake
            var ai = new FoodieAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.Equal(Direction.Right, snake.CurrentDirection); // Should move towards the food
        }

        [Fact]
        public void RandomAI_ShouldMoveRandomly()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Purple, SnakeType.CPU) { Personality = "Random" };
            var arena = new Arena(10, 10);
            var ai = new RandomAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.InRange((int)snake.CurrentDirection, 0, 3); // Direction should be one of the valid directions
        }

        [Fact]
        public void HunterAI_ShouldMoveTowardsPlayerSnake()
        {
            // Arrange
            var playerSnake = new Snake(new Position(7, 5), Direction.Right, Color.Green, SnakeType.Human);
            var cpuSnake = new Snake(new Position(5, 5), Direction.Right, Color.Orange, SnakeType.CPU) { Personality = "Hunter" };
            var arena = new Arena(10, 10);
            arena.Snakes.Add(playerSnake);
            var ai = new HunterAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection); // Should move towards the player snake
        }

        [Fact]
        public void SurvivorAI_ShouldMoveRandomlyForNow()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Cyan, SnakeType.CPU) { Personality = "Survivor" };
            var arena = new Arena(10, 10);
            var ai = new SurvivorAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.InRange((int)snake.CurrentDirection, 0, 3); // Direction should be one of the valid directions
        }

        [Fact]
        public void SpeedyAI_ShouldMoveRandomly()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Cyan, SnakeType.CPU) { Personality = "Speedy" };
            var arena = new Arena(10, 10);
            var ai = new SpeedyAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.InRange((int)snake.CurrentDirection, 0, 3); // Direction should be one of the valid directions
        }
    }
}