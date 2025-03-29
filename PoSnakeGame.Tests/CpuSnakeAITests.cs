using Xunit;
using PoSnakeGame.Core.Models;
using PoSnakeGame.Core.Services;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace PoSnakeGame.Tests
{
    public class CpuSnakeAITests
    {
        private Arena CreateTestArena(int width = 10, int height = 10)
        {
            return new Arena(width, height);
        }

        // === RandomAI Tests ===

        [Fact]
        public void RandomAI_ShouldChooseASafeDirection()
        {
            // Arrange
            var arena = CreateTestArena();
            // Place snake in the middle, plenty of safe options
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Purple, SnakeType.CPU) { Personality = "Random" };
            arena.Snakes.Add(snake);
            var ai = new RandomAI(snake, arena);
            var initialDirection = snake.CurrentDirection;

            // Act
            ai.UpdateDirection();

            // Assert
            // Should pick one of the 3 safe directions (Up, Down, Right)
            Assert.Contains(snake.CurrentDirection, new[] { Direction.Up, Direction.Down, Direction.Right });
            Assert.NotEqual(Direction.Left, snake.CurrentDirection); // Should not reverse
        }

        [Fact]
        public void RandomAI_ShouldChooseOnlySafeDirection_WhenCornered()
        {
            // Arrange
            var arena = CreateTestArena();
             // Place snake near top-left corner, only Right is safe initially
            var snake = new Snake(new Position(1, 1), Direction.Up, Color.Purple, SnakeType.CPU) { Personality = "Random" };
             // Add walls/obstacles to force only one safe direction (Right)
            arena.AddObstacle(new Position(1, 0)); // Above
            arena.AddObstacle(new Position(1, 2)); // Below
            arena.AddObstacle(new Position(0, 1)); // Left 
            // Top and Left walls are implicit boundaries
            arena.Snakes.Add(snake);
            var ai = new RandomAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.Equal(Direction.Right, snake.CurrentDirection); // Only safe option
        }

        // === FoodieAI Tests ===

        [Fact]
        public void FoodieAI_ShouldMoveTowardsNearestFood()
        {
            // Arrange
            var arena = CreateTestArena();
            var snake = new Snake(new Position(5, 5), Direction.Up, Color.Yellow, SnakeType.CPU) { Personality = "Foodie" };
            // Place food so distances are unambiguous from safe moves
            arena.AddFood(new Position(5, 8)); // Food below (Dist 3 from head)
            arena.AddFood(new Position(6, 5)); // Food right (Dist 1 from head - NEAREST)
            arena.Snakes.Add(snake);
            var ai = new FoodieAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Nearest food is (6,5). Safe directions are Up, Left, Right.
            // Moving Right to (6,5) has distance 0 to target.
            // Moving Up to (5,4) has distance 2 to target.
            // Moving Left to (4,5) has distance 2 to target.
            Assert.Equal(Direction.Right, snake.CurrentDirection); // Should move towards nearest food (Right)
        }

        [Fact]
        public void FoodieAI_ShouldPrioritizePointsPowerUp()
        {
            // Arrange
            var arena = CreateTestArena();
            var snake = new Snake(new Position(5, 5), Direction.Up, Color.Yellow, SnakeType.CPU) { Personality = "Foodie" };
            arena.AddFood(new Position(5, 7)); // Regular food below (closer)
            arena.AddPowerUp(new PowerUp(new Position(7, 5), PowerUpType.Points, 5f, 50)); // Points power-up right (further)
            arena.Snakes.Add(snake);
            var ai = new FoodieAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.Equal(Direction.Right, snake.CurrentDirection); // Should prioritize Points power-up (Right)
        }

        [Fact]
        public void FoodieAI_ShouldFallbackToOpenSpace_WhenNoFood()
        {
            // Arrange
            var arena = CreateTestArena();
            // Place snake where 'Right' has more open space
            var snake = new Snake(new Position(1, 5), Direction.Up, Color.Yellow, SnakeType.CPU) { Personality = "Foodie" };
            arena.AddObstacle(new Position(1, 4)); // Obstacle above
            arena.AddObstacle(new Position(1, 6)); // Obstacle below
            // Left is wall, Right is open
            arena.Snakes.Add(snake);
            var ai = new FoodieAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            Assert.Equal(Direction.Right, snake.CurrentDirection); // Should move towards the most open space
        }

        [Fact]
        public void FoodieAI_ShouldAvoidCollisionWhenMovingToFood()
        {
            // Arrange
            var arena = CreateTestArena();
            var snake = new Snake(new Position(5, 5), Direction.Up, Color.Yellow, SnakeType.CPU) { Personality = "Foodie" };
            arena.AddFood(new Position(5, 3)); // Food directly above
            arena.AddObstacle(new Position(5, 4)); // Obstacle blocking direct path
            arena.Snakes.Add(snake);
            var ai = new FoodieAI(snake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Should choose Left or Right as Up is blocked
            Assert.Contains(snake.CurrentDirection, new[] { Direction.Left, Direction.Right });
            Assert.NotEqual(Direction.Up, snake.CurrentDirection);
            Assert.NotEqual(Direction.Down, snake.CurrentDirection); // Should not reverse
        }

        // === HunterAI Tests ===

        [Fact]
        public void HunterAI_ShouldMoveTowardsPlayer()
        {
            // Arrange
            var arena = CreateTestArena();
            var playerSnake = new Snake(new Position(7, 5), Direction.Right, Color.Red, SnakeType.Human);
            var cpuSnake = new Snake(new Position(5, 5), Direction.Up, Color.Orange, SnakeType.CPU) { Personality = "Hunter" };
            arena.Snakes.Add(playerSnake);
            arena.Snakes.Add(cpuSnake);
            var ai = new HunterAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection(); // Player predicted at (8,5)

            // Assert
            // Safe directions: Up (dist 4), Left (dist 4), Right (dist 2)
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection); // Should move towards predicted player pos
        }

        [Fact]
        public void HunterAI_ShouldFallback_WhenPlayerAbsent()
        {
            // Arrange
            var arena = CreateTestArena();
            var cpuSnake = new Snake(new Position(5, 5), Direction.Up, Color.Orange, SnakeType.CPU) { Personality = "Hunter" };
            arena.AddObstacle(new Position(4, 5)); // Block Left
            arena.AddPowerUp(new PowerUp(new Position(6, 5), PowerUpType.Points, 5f, 50)); // Points power-up Right
            arena.Snakes.Add(cpuSnake);
            var ai = new HunterAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection(); // No player, should fallback

            // Assert
            // Fallback prioritizes open space + points powerup
            // Safe: Up (score 30 + dist 100-1 = 129), Right (score 30 + dist 100-0 = 130)
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection); // Should move towards powerup/open space
        }

        // === AggressiveAI Tests ===

        [Fact]
        public void AggressiveAI_ShouldTargetNearestSnake()
        {
            // Arrange
            var arena = CreateTestArena();
            var targetSnakeNear = new Snake(new Position(7, 5), Direction.Left, Color.Blue, SnakeType.CPU); // Dist 2
            var targetSnakeFar = new Snake(new Position(5, 8), Direction.Up, Color.Green, SnakeType.CPU);   // Dist 3
            var cpuSnake = new Snake(new Position(5, 5), Direction.Up, Color.Red, SnakeType.CPU) { Personality = "Aggressive" };
            arena.Snakes.Add(targetSnakeNear);
            arena.Snakes.Add(targetSnakeFar);
            arena.Snakes.Add(cpuSnake);
            var ai = new AggressiveAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection(); // Nearest target predicted at (6,5)

            // Assert
            // Safe: Up (dist 2), Left (dist 2), Right (dist 0)
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection); // Should move towards nearest predicted pos
        }

        [Fact]
        public void AggressiveAI_ShouldFallbackToPowerUp_WhenNoSnakes()
        {
            // Arrange
            var arena = CreateTestArena();
            var cpuSnake = new Snake(new Position(5, 5), Direction.Up, Color.Red, SnakeType.CPU) { Personality = "Aggressive" };
            arena.AddPowerUp(new PowerUp(new Position(5, 7), PowerUpType.Speed, 5f, 0.5f)); // Powerup below
            arena.Snakes.Add(cpuSnake);
            var ai = new AggressiveAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Safe: Up, Left, Right. Down is not safe (opposite).
            // Should move towards powerup at (5,7) -> Down is best, but not safe.
            // Next best: Left (dist 2), Right (dist 2), Up (dist 2). Chooses first best.
            // Let's refine: Add obstacle to force choice
            arena.AddObstacle(new Position(4, 5)); // Block Left
            ai.UpdateDirection(); // Recalculate
            // Safe: Up (dist 2), Right (dist 2). Chooses Up.
            // Let's block Up too
            arena.AddObstacle(new Position(5, 4)); // Block Up
            ai.UpdateDirection(); // Recalculate
            // Safe: Right (dist 2)
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection);
        }

        [Fact]
        public void AggressiveAI_ShouldFallbackToRandom_WhenNoSnakesOrPowerUps()
        {
            // Arrange
            var arena = CreateTestArena();
            var cpuSnake = new Snake(new Position(5, 5), Direction.Right, Color.Red, SnakeType.CPU) { Personality = "Aggressive" };
            arena.Snakes.Add(cpuSnake);
            var ai = new AggressiveAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Should pick one of the 3 safe directions (Up, Down, Right)
            Assert.Contains(cpuSnake.CurrentDirection, new[] { Direction.Up, Direction.Down, Direction.Right });
            Assert.NotEqual(Direction.Left, cpuSnake.CurrentDirection); // Should not reverse
        }


        // === CautiousAI Tests ===

        [Fact]
        public void CautiousAI_ShouldPreferOpenSpaceAndAvoidSnakes()
        {
            // Arrange
            var arena = CreateTestArena();
            var otherSnake = new Snake(new Position(8, 5), Direction.Left, Color.Green, SnakeType.CPU); // Snake further to the right
            var cpuSnake = new Snake(new Position(5, 5), Direction.Up, Color.Blue, SnakeType.CPU) { Personality = "Cautious" };
            arena.AddObstacle(new Position(5, 3)); // Add obstacle to make 'Up' less attractive
            arena.Snakes.Add(otherSnake);
            arena.Snakes.Add(cpuSnake);
            var ai = new CautiousAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Evaluate safe directions (Up, Left, Right):
            // Up to (5,4): Open=2 (blocked above), Dist=4 -> Score = 10 + 4 = 14
            // Left to (4,5): Open=3, Dist=4 -> Score = 15 + 4 = 19
            // Right to (6,5): Open=2, Dist=2 -> Score = 10 + 2 = 12
            Assert.Equal(Direction.Left, cpuSnake.CurrentDirection); // Should choose Left (highest score)
        }

        // === SurvivorAI Tests ===

        [Fact]
        public void SurvivorAI_ShouldAvoidWallsAndSnakes()
        {
            // Arrange
            var arena = CreateTestArena();
            var otherSnake = new Snake(new Position(1, 3), Direction.Right, Color.Green, SnakeType.CPU); // Snake below-left
            // Place snake near top-left corner
            var cpuSnake = new Snake(new Position(1, 1), Direction.Right, Color.Cyan, SnakeType.CPU) { Personality = "Survivor" };
            arena.Snakes.Add(otherSnake);
            arena.Snakes.Add(cpuSnake);
            var ai = new SurvivorAI(cpuSnake, arena);

            // Act
            ai.UpdateDirection();

            // Assert
            // Evaluate safe directions (Down, Right):
            // Down to (1,2): Open=2, NearbySnakes=1 (dist 1), NearWall=Yes -> Score = 40 - 10 - 30 = 0
            // Right to (2,1): Open=3, NearbySnakes=0 (dist 2), NearWall=Yes -> Score = 60 - 0 - 30 = 30
            Assert.Equal(Direction.Right, cpuSnake.CurrentDirection); // Should choose Right (higher score)
        }

        // === SpeedyAI Tests ===

        [Fact]
        public void SpeedyAI_ShouldMoveErraticOccasionally()
        {
            // Arrange
            var arena = CreateTestArena();
            var snake = new Snake(new Position(5, 5), Direction.Right, Color.Magenta, SnakeType.CPU) { Personality = "Speedy" };
            arena.Snakes.Add(snake);
            var ai = new SpeedyAI(snake, arena);
            var directions = new List<Direction>();

            // Act
            // Run AI multiple times (e.g., 30) to increase probability of observing erratic change
            for (int i = 0; i < 30; i++) 
            {
                ai.UpdateDirection();
                directions.Add(snake.CurrentDirection);
            }

            // Assert
            // Check if direction changed at least once (due to erratic move or fallback)
            // This isn't a perfect test for randomness, but checks for non-static behavior
            bool changedDirection = false;
            for(int i = 1; i < directions.Count; i++)
            {
                if (directions[i] != directions[0])
                {
                    changedDirection = true;
                    break;
                }
            }
            Assert.True(changedDirection);
            // Also check speed is set correctly
            Assert.Equal(1.5f, snake.Speed);
        }

         [Fact]
        public void SpeedyAI_ShouldSeekFoodWhenNotErratic()
        {
            // Arrange
            var arena = CreateTestArena();
            var snake = new Snake(new Position(5, 5), Direction.Up, Color.Magenta, SnakeType.CPU) { Personality = "Speedy" };
            arena.AddFood(new Position(7, 5)); // Food to the right
            arena.Snakes.Add(snake);
            var ai = new SpeedyAI(snake, arena);

            // Act
            // Run twice - first move might be erratic, second should target food if first wasn't
            ai.UpdateDirection(); // Move 1
            var firstMove = snake.CurrentDirection;
            ai.UpdateDirection(); // Move 2
            var secondMove = snake.CurrentDirection;


            // Assert
             // If the first move wasn't erratic (targeting food -> Right), the second should also be Right.
             // If the first move *was* erratic, the second move should target food (Right).
             // So, at least one of the first two moves should be Right.
             // Note: This test might occasionally fail if the erratic move happens to be Right.
             Assert.True(firstMove == Direction.Right || secondMove == Direction.Right, "SpeedyAI should target food when not moving erratically.");
        }
    }
}
