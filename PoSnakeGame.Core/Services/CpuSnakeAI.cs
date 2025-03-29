using PoSnakeGame.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoSnakeGame.Core.Services
{
    // Strategy Pattern: The CpuSnakeAI class hierarchy implements different strategies for snake movement
    public abstract class CpuSnakeAI
    {
        protected readonly Snake Snake;
        protected readonly Arena Arena;
        protected readonly Random Random;

        protected CpuSnakeAI(Snake snake, Arena arena)
        {
            Snake = snake;
            Arena = arena;
            // Use a shared Random instance if possible, but for simplicity, each AI gets its own for now.
            // Consider injecting Random if needed for better testability or seed control.
            Random = new Random(); 
        }

        /// <summary>
        /// Abstract method for AI logic to determine the snake's next direction.
        /// Must be implemented by concrete AI personality classes.
        /// </summary>
        public abstract void UpdateDirection();

        /// <summary>
        /// Calculates the Manhattan distance between two positions.
        /// Useful for simple distance heuristics.
        /// </summary>
        protected static int CalculateManhattanDistance(Position p1, Position p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        /// <summary>
        /// Checks if moving in the given direction from the snake's current head position is valid.
        /// A move is valid if it's within bounds, doesn't hit an obstacle, and doesn't hit any snake's body (including its own).
        /// </summary>
        /// <param name="direction">The direction to check.</param>
        /// <returns>True if the move is valid, false otherwise.</returns>
        protected bool IsValidMove(Direction direction)
        {
            var head = Snake.Segments[0];
            var nextPos = head + Position.FromDirection(direction);

            // 1. Check Arena Bounds
            if (Arena.IsOutOfBounds(nextPos))
            {
                // Console.WriteLine($"Invalid Move: {direction} leads to Out of Bounds at {nextPos}");
                return false;
            }

            // 2. Check Obstacles
            if (Arena.HasCollision(nextPos))
            {
                // Console.WriteLine($"Invalid Move: {direction} leads to Obstacle collision at {nextPos}");
                return false;
            }

            // 3. Check Collision with ALL snake segments (including self, except potentially the tail tip if it moves)
            if (IsCollidingWithAnySnake(nextPos))
            {
                // Console.WriteLine($"Invalid Move: {direction} leads to Snake collision at {nextPos}");
                return false;
            }

            // If all checks pass, the move is valid
            return true;
        }

        /// <summary>
        /// Counts the number of adjacent positions (up, down, left, right) from a given position 
        /// that are valid moves (within bounds, no obstacles, no snakes).
        /// Useful for evaluating the "openness" of a potential next position.
        /// </summary>
        /// <param name="pos">The position to check adjacent cells from.</param>
        /// <returns>The number of valid adjacent positions.</returns>
        protected int CountValidAdjacentPositions(Position pos)
        {
            int count = 0;
            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                var adjacentPos = pos + Position.FromDirection(dir);
                // Check if the adjacent position itself is a valid place to be (not just a valid move *to*)
                if (!Arena.IsOutOfBounds(adjacentPos) &&
                    !Arena.HasCollision(adjacentPos) &&
                    !IsCollidingWithAnySnake(adjacentPos))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Checks if a given position collides with any segment of any snake currently in the arena.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if the position overlaps with any snake segment, false otherwise.</returns>
        protected bool IsCollidingWithAnySnake(Position pos)
        {
            // Ensure Arena.Snakes is populated (important for tests)
            if (Arena.Snakes == null || Arena.Snakes.Count == 0) return false; 

            foreach (var snake in Arena.Snakes)
            {
                // Check collision against all segments of the snake
                // Note: A snake *can* move into the space its own tail just vacated, 
                // but checking against all segments simplifies collision logic and prevents weird overlaps.
                if (snake.Segments.Contains(pos)) 
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a list of directions the snake can safely move in its next step.
        /// Excludes moving directly backward and directions leading to immediate collisions.
        /// </summary>
        /// <returns>A list of safe directions.</returns>
        protected List<Direction> GetSafeDirections()
        {
            // Get all valid directions that won't cause immediate collisions
            var safeDirections = new List<Direction>();
            
            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                if (IsValidMove(dir) && !IsOppositeDirection(Snake.CurrentDirection, dir))
                {
                    safeDirections.Add(dir);
                }
            }
            
            return safeDirections;
        }
        
        // Helper method to check if two directions are opposite
        protected static bool IsOppositeDirection(Direction dir1, Direction dir2)
        {
            return (dir1 == Direction.Up && dir2 == Direction.Down) ||
                   (dir1 == Direction.Down && dir2 == Direction.Up) ||
                   (dir1 == Direction.Left && dir2 == Direction.Right) ||
                   (dir1 == Direction.Right && dir2 == Direction.Left);
        }
    }

    // Concrete Strategy: Random movement AI
    /// <summary>
    /// An AI personality that chooses a random direction from the available safe moves.
    /// It does not consider food, other snakes, or long-term survival.
    /// </summary>
    public class RandomAI : CpuSnakeAI
    {
        public RandomAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            
            // If there are safe directions available, choose one randomly
            if (safeDirections.Count > 0)
            {
                // Randomly select from available safe directions
                var randomIndex = Random.Next(safeDirections.Count);
                Snake.CurrentDirection = safeDirections[randomIndex];
                
                Console.WriteLine($"RandomAI: Changed to {Snake.CurrentDirection}");
            }
            // If no safe directions, it keeps its current direction, which will likely result in a collision.
            // This is acceptable behavior for a purely random AI.
        }
    }

    // Concrete Strategy: Simple AI that follows food (Consider removing or merging with FoodieAI if redundant)
    // NOTE: This seems very similar to FoodieAI. Let's keep FoodieAI as the primary food-seeking AI.
    public class SimpleAI : CpuSnakeAI 
    {
        public SimpleAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (safeDirections.Count == 0) return; // No safe moves available
            
            var head = Snake.Segments[0];
            Position targetFood = null;
            
            // Find nearest food
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points)
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // If no food found, move in a safe direction
            if (targetFood == null)
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
                return;
            }
            
            // Try to move toward food
            Direction bestDirection = Snake.CurrentDirection;
            int shortestDistance = int.MaxValue;
            
            foreach (var dir in safeDirections)
            {
                var newPos = head + Position.FromDirection(dir);
                var distance = CalculateManhattanDistance(newPos, targetFood);
                
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    bestDirection = dir;
                }
            }
            
            Snake.CurrentDirection = bestDirection;
            Console.WriteLine($"SimpleAI: Changed to {Snake.CurrentDirection}, distance to food: {shortestDistance}");
        }
    }

    // Concrete Strategy: Advanced AI with look-ahead and path planning
    public class AdvancedAI : CpuSnakeAI
    {
        public AdvancedAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var head = Snake.Segments[0];
            var safeDirections = GetSafeDirections();
            
            // No safe moves available
            if (safeDirections.Count == 0) return;
            
            // If only one safe direction, choose it
            if (safeDirections.Count == 1)
            {
                Snake.CurrentDirection = safeDirections[0];
                return;
            }
            
            // Evaluate each safe direction
            Direction bestDirection = Snake.CurrentDirection;
            int bestScore = int.MinValue;
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                int score = CountValidAdjacentPositions(nextPos) * 5; // Open spaces
                
                // Add score for distance from other snakes
                if (Arena.Snakes != null)
                {
                    foreach (var otherSnake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
                    {
                        var otherHead = otherSnake.Segments[0];
                        var distance = CalculateManhattanDistance(nextPos, otherHead);
                        score += distance; // Farther is better
                    }
                }
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = dir;
                }
            }
            
            Snake.CurrentDirection = bestDirection;
            Console.WriteLine($"AdvancedAI: Selected {Snake.CurrentDirection} with score {bestScore}");
        }
    }

    // Concrete Strategy: Cautious AI that avoids other snakes
    public class CautiousAI : CpuSnakeAI
    {
        public CautiousAI(Snake snake, Arena arena) : base(snake, arena) { }
        
        public override void UpdateDirection()
        {
            var head = Snake.Segments[0];
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            if (safeDirections.Count == 1)
            {
                Snake.CurrentDirection = safeDirections[0]; // Only one option
                return;
            }
            
            // Score each direction based on:
            // 1. Distance from other snakes (farther is better)
            // 2. Number of open spaces (more is better)
            Direction bestDirection = Snake.CurrentDirection;
            int bestScore = int.MinValue;
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                int score = CountValidAdjacentPositions(nextPos) * 5; // Open spaces
                
                // Add score for distance from other snakes
                if (Arena.Snakes != null)
                {
                    foreach (var otherSnake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
                    {
                        var otherHead = otherSnake.Segments[0];
                        var distance = CalculateManhattanDistance(nextPos, otherHead);
                        score += distance; // Farther is better
                    }
                }

                // Debugging log for CautiousAI scoring
                Console.WriteLine($"CautiousAI: Eval Dir={dir}, NextPos={nextPos}, Open={CountValidAdjacentPositions(nextPos)}, Score={score}");

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = dir;
                    Console.WriteLine($"CautiousAI: New best direction {bestDirection} with score {bestScore}");
                }
                else if (score == bestScore)
                {
                     // Optional: Add tie-breaking logic if needed, e.g., prefer current direction or random
                     Console.WriteLine($"CautiousAI: Tied score {score} for direction {dir}. Keeping current best {bestDirection}.");
                }
            }
            
            Snake.CurrentDirection = bestDirection;
            Console.WriteLine($"CautiousAI: Final Selected {Snake.CurrentDirection} with score {bestScore}");
        }
    }

    // Concrete Strategy: Foodie AI focuses primarily on finding and consuming food.
    /// <summary>
    /// An AI personality that prioritizes moving towards the nearest food source (Points power-ups first, then regular food).
    /// If no food is nearby or reachable safely, it falls back to moving towards the most open space.
    /// </summary>
    public class FoodieAI : CpuSnakeAI
    {
        public FoodieAI(Snake snake, Arena arena) : base(snake, arena) { }
        
        public override void UpdateDirection()
        {
            var head = Snake.Segments[0];
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            
            // Look for food with a higher priority on Points
            Position targetFood = null;
            
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points)
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // If no Points powerup, check Arena.Foods
            if (targetFood == null && Arena.Foods.Count > 0)
            {
                targetFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(head, f))
                    .FirstOrDefault();
            }
            
            // If target found, move toward it
            if (targetFood != null)
            {
                Direction bestDirection = Snake.CurrentDirection;
                int shortestDistance = int.MaxValue;
                
                foreach (var dir in safeDirections)
                {
                    var newPos = head + Position.FromDirection(dir);
                    var distance = CalculateManhattanDistance(newPos, targetFood);
                    
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestDirection = dir;
                    }
                }
                
                Snake.CurrentDirection = bestDirection;
                Console.WriteLine($"FoodieAI: Moving toward food, distance: {shortestDistance}");
                return;
            }
            
            // --- Fallback Logic ---
            // If no food target was found or reachable safely, choose the safe direction 
            // that leads to the position with the most valid adjacent positions (most open space).
            Console.WriteLine($"FoodieAI: No food target found or reachable. Falling back to finding open space.");
            Direction fallbackDir = safeDirections.Count > 0 ? safeDirections[0] : Snake.CurrentDirection; // Default to first safe or current
            int maxOpenSpace = -1; // Use -1 to ensure the first valid direction is chosen if all have 0 open space
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                int openSpace = CountValidAdjacentPositions(nextPos);
                
                if (openSpace > maxOpenSpace)
                {
                    maxOpenSpace = openSpace;
                    fallbackDir = dir;
                }
            }
            
            Snake.CurrentDirection = fallbackDir;
            Console.WriteLine($"FoodieAI: No food found, moving to open space: {fallbackDir}");
        }
    }

    // Concrete Strategy: Hunter AI specifically targets the human player snake.
    /// <summary>
    /// An AI personality that attempts to intercept the human player's snake.
    /// It uses simple prediction to estimate the player's next position.
    /// If the player snake is not found, it falls back to a balanced strategy of seeking food and open space.
    /// </summary>
    public class HunterAI : CpuSnakeAI
    {
        public HunterAI(Snake snake, Arena arena) : base(snake, arena) { }
        
        public override void UpdateDirection()
        {
            var head = Snake.Segments[0];
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            if (safeDirections.Count == 1)
            {
                Snake.CurrentDirection = safeDirections[0]; // Only one option
                return;
            }
            
            // Find the player snake
            var playerSnake = Arena.Snakes?
                .FirstOrDefault(s => s.Type == SnakeType.Human && s.IsAlive);
                
            if (playerSnake != null)
            {
                // Calculate interception point (predict where player will be)
                var playerHead = playerSnake.Segments[0];
                var playerDirection = playerSnake.CurrentDirection;
                
                // Simple prediction: assume player continues in current direction
                var predictedPlayerPos = playerHead + Position.FromDirection(playerDirection);
                
                // Move toward predicted position
                Direction bestDirection = Snake.CurrentDirection;
                int shortestDistance = int.MaxValue;
                
                foreach (var dir in safeDirections)
                {
                    var newPos = head + Position.FromDirection(dir);
                    var distance = CalculateManhattanDistance(newPos, predictedPlayerPos);
                    
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestDirection = dir;
                    }
                }
                
                Snake.CurrentDirection = bestDirection;
                Console.WriteLine($"HunterAI: Hunting player, distance: {shortestDistance}");
                return;
            }
            
            // --- Fallback Logic ---
            // If the player snake is not found (e.g., already dead or not present), 
            // fall back to a strategy similar to AdvancedAI/CautiousAI: prioritize open space, 
            // but also consider moving towards Points power-ups.
            Console.WriteLine($"HunterAI: Player snake not found or not alive. Falling back.");
            Position targetFood = null; // Specifically look for Points power-ups in fallback
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points)
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // Evaluate safe directions based on open space and proximity to Points power-ups
            Direction fallbackDir = safeDirections.Count > 0 ? safeDirections[0] : Snake.CurrentDirection;
            int bestScore = int.MinValue;
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                int score = CountValidAdjacentPositions(nextPos) * 10;
                
                if (targetFood != null)
                {
                    int distScore = 100 - CalculateManhattanDistance(nextPos, targetFood);
                    score += distScore;
                }
                
                if (score > bestScore)
                {
                    bestScore = score;
                    fallbackDir = dir;
                }
            }
            
            Snake.CurrentDirection = fallbackDir;
            Console.WriteLine($"HunterAI: Fallback selected {fallbackDir} with score {bestScore}");
        }
    }

    // Concrete Strategy: Survivor AI prioritizes survival and maximizing open space.
    /// <summary>
    /// An AI personality focused on staying alive as long as possible.
    /// It prioritizes moving into areas with the most open space and avoids getting close to other snakes or walls.
    /// </summary>
    public class SurvivorAI : CpuSnakeAI
    {
        public SurvivorAI(Snake snake, Arena arena) : base(snake, arena) { }
        
        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            if (safeDirections.Count == 1)
            {
                Snake.CurrentDirection = safeDirections[0]; // Only one option
                return;
            }
            
            var head = Snake.Segments[0];
            
            // Always choose direction with most open space and fewest nearby snakes
            Direction bestDirection = Snake.CurrentDirection;
            int bestScore = int.MinValue;
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                
                // Count open spaces (most important)
                int openSpaces = CountValidAdjacentPositions(nextPos);
                
                // Count nearby snakes (penalize directions with snakes nearby)
                int nearbySnakes = 0;
                if (Arena.Snakes != null)
                {
                    foreach (var otherSnake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
                    {
                        var otherHead = otherSnake.Segments[0];
                        if (CalculateManhattanDistance(nextPos, otherHead) < 3)
                        {
                            nearbySnakes++;
                        }
                    }
                }
                
                // Calculate score: prioritize open spaces, avoid nearby snakes
                int score = (openSpaces * 20) - (nearbySnakes * 10);
                
                // Also factor in distance to walls - avoid getting too close
                if (nextPos.X < 2 || nextPos.X >= Arena.Width - 2 || 
                    nextPos.Y < 2 || nextPos.Y >= Arena.Height - 2)
                {
                    score -= 30; // Heavy penalty for being close to walls
                }
                
                Console.WriteLine($"SurvivorAI: Direction {dir} score: {score} (Open: {openSpaces}, Snakes: {nearbySnakes})");
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = dir;
                }
            }
            
            Snake.CurrentDirection = bestDirection;
            Console.WriteLine($"SurvivorAI: Selected {Snake.CurrentDirection} with score {bestScore}");
        }
    }

    // Concrete Strategy: Speedy AI moves quickly and changes direction frequently.
    /// <summary>
    /// An AI personality characterized by high speed and somewhat erratic movement.
    /// It changes direction periodically and prioritizes moving towards food or power-ups when not changing randomly.
    /// </summary>
    public class SpeedyAI : CpuSnakeAI
    {
        private int moveCounter = 0; // Counter to trigger periodic random direction changes
        
        public SpeedyAI(Snake snake, Arena arena) : base(snake, arena) 
        {
            // Speedy AI always moves faster than normal
            snake.Speed = 1.5f;
        }
        
        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            
            moveCounter++;
            
            // Change direction frequently to be unpredictable
            if (moveCounter % 3 == 0 && safeDirections.Count > 1)
            {
                // Randomly select from available safe directions
                var randomIndex = Random.Next(safeDirections.Count);
                Snake.CurrentDirection = safeDirections[randomIndex];
                Console.WriteLine($"SpeedyAI: Erratic change to {Snake.CurrentDirection}");
                return;
            }
            
            // Otherwise, use a more intelligent approach to find food or open space
            var head = Snake.Segments[0];
            Position targetItem = null; // Can be food or powerup
            
            // Find nearest relevant item (Points/Speed PowerUp or regular Food)
            var potentialTargets = new List<(Position Pos, int Dist)>();

            // Check PowerUps first
            if (Arena.PowerUps?.Count > 0)
            {
                potentialTargets.AddRange(Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points || p.Type == PowerUpType.Speed)
                    .Select(p => (p.Position, CalculateManhattanDistance(head, p.Position))));
            }
            // Check regular Food
             if (Arena.Foods?.Count > 0)
            {
                 potentialTargets.AddRange(Arena.Foods
                    .Select(f => (f, CalculateManhattanDistance(head, f))));
            }

            // Find the closest target overall
            if(potentialTargets.Any())
            {
                targetItem = potentialTargets.OrderBy(t => t.Dist).First().Pos;
            }
            
            // If target found, move toward it
            if (targetItem != null)
            {
                Direction bestDirection = Snake.CurrentDirection;
                int shortestDistance = int.MaxValue;
                
                foreach (var dir in safeDirections)
                {
                    var newPos = head + Position.FromDirection(dir);
                    var distance = CalculateManhattanDistance(newPos, targetItem); // Use targetItem
                    
                    if (distance < shortestDistance)
                    { // Added missing opening brace
                        shortestDistance = distance;
                        bestDirection = dir;
                    } // Added missing closing brace
                }
                
                Snake.CurrentDirection = bestDirection;
                Console.WriteLine($"SpeedyAI: Moving toward item at {targetItem}, distance: {shortestDistance}");
                return;
            }
            
            // If no item found, move in direction with most open space
            Direction fallbackDir = safeDirections.Count > 0 ? safeDirections[0] : Snake.CurrentDirection; // Handle no safe directions case
            int maxOpenSpace = 0;
            
            foreach (var dir in safeDirections)
            {
                var nextPos = head + Position.FromDirection(dir);
                int openSpace = CountValidAdjacentPositions(nextPos);
                
                if (openSpace > maxOpenSpace)
                {
                    maxOpenSpace = openSpace;
                    fallbackDir = dir;
                }
            }
            
            Snake.CurrentDirection = fallbackDir;
            Console.WriteLine($"SpeedyAI: No food/power-up found, moving to open space: {fallbackDir}");
        }
    }

    // Concrete Strategy: Aggressive AI targets the nearest snake (player or CPU).
    /// <summary>
    /// An AI personality that actively seeks out and attempts to intercept the nearest snake.
    /// If no snakes are nearby, it prioritizes grabbing any available power-up.
    /// As a last resort, it moves randomly.
    /// </summary>
    public class AggressiveAI : CpuSnakeAI
    {
        public AggressiveAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var head = Snake.Segments[0];
            var safeDirections = GetSafeDirections();
            
            if (safeDirections.Count == 0) return; // No safe moves
            
            // Find nearest snake to target
            Snake targetSnake = null;
            int closestDistance = int.MaxValue;
            
            if (Arena.Snakes != null)
            {
                foreach (var otherSnake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
                {
                    var distance = CalculateManhattanDistance(head, otherSnake.Segments[0]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetSnake = otherSnake;
                    }
                }
            }
            
            if (targetSnake != null)
            {
                // Try to intercept the target snake
                var targetHead = targetSnake.Segments[0];
                var targetDirection = targetSnake.CurrentDirection;
                var predictedPos = targetHead + Position.FromDirection(targetDirection);
                
                Direction bestDirection = Snake.CurrentDirection;
                int shortestDistance = int.MaxValue;
                
                foreach (var dir in safeDirections)
                {
                    var newPos = head + Position.FromDirection(dir);
                    var distance = CalculateManhattanDistance(newPos, predictedPos);
                    
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestDirection = dir;
                    }
                }
                
                Snake.CurrentDirection = bestDirection;
                Console.WriteLine($"AggressiveAI: Targeting snake, distance: {shortestDistance}");
                return;
            }
            
            // --- Fallback 1: Target Power-ups ---
            // If no snakes are found to target, look for the nearest power-up of any type.
            Console.WriteLine($"AggressiveAI: No target snake found. Looking for power-ups.");
            Position targetPowerUp = null;
            if (Arena.PowerUps?.Count > 0)
            {
                targetPowerUp = Arena.PowerUps // Corrected variable name here
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(p => p.Position) // Select the position of the power-up
                    .FirstOrDefault();
            }
            
            // Move toward the nearest power-up if one is found
            if (targetPowerUp != null)
            {
                Direction bestDirection = Snake.CurrentDirection;
                int shortestDistance = int.MaxValue;
                
                foreach (var dir in safeDirections)
                {
                    var newPos = head + Position.FromDirection(dir);
                    var distance = CalculateManhattanDistance(newPos, targetPowerUp); // Use targetPowerUp
                    
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestDirection = dir;
                    }
                }
                
                Snake.CurrentDirection = bestDirection;
                Console.WriteLine($"AggressiveAI: Moving toward power-up, distance: {shortestDistance}");
                return;
            }

            // --- Fallback 2: Random Movement ---
            // If no snakes and no power-ups are found, move randomly among safe directions.
            Console.WriteLine($"AggressiveAI: No snakes or power-ups found. Moving randomly.");
            if (safeDirections.Count > 0) // Ensure there's at least one safe direction
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
            // If no safe directions, it keeps its current direction (likely leading to collision).
        }
    }
}
