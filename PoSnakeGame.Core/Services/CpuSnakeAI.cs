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
            Random = new Random();
        }

        public abstract void UpdateDirection();

        protected static int CalculateManhattanDistance(Position p1, Position p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        protected bool IsValidMove(Direction direction)
        {
            var head = Snake.Segments[0];
            var nextPos = head + Position.FromDirection(direction);

            // Check if out of bounds
            if (Arena.IsOutOfBounds(nextPos)) return false;

            // Check collision with obstacles
            if (Arena.HasCollision(nextPos)) return false;

            // Check collision with snakes
            if (IsTouchingSnake(nextPos)) return false;

            return true;
        }

        // Helper method to count valid adjacent positions
        protected int CountValidAdjacentPositions(Position pos)
        {
            int count = 0;
            
            // Check all four directions
            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                var adjacentPos = pos + Position.FromDirection(dir);
                
                // Position is valid if it's in bounds and not occupied
                if (!Arena.IsOutOfBounds(adjacentPos) && 
                    !Arena.HasCollision(adjacentPos) &&
                    !IsTouchingSnake(adjacentPos))
                {
                    count++;
                }
            }
            
            return count;
        }

        // Helper to check if a position touches any snake
        protected bool IsTouchingSnake(Position pos)
        {
            if (Arena.Snakes == null) return false;

            foreach (var snake in Arena.Snakes)
            {
                if (snake.Segments.Contains(pos))
                    return true;
            }
            return false;
        }

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
            // If no safe directions, keep current direction (likely will lead to death)
        }
    }

    // Concrete Strategy: Simple AI that follows food
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
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = dir;
                }
            }
            
            Snake.CurrentDirection = bestDirection;
            Console.WriteLine($"CautiousAI: Selected {Snake.CurrentDirection} with score {bestScore}");
        }
    }

    // Concrete Strategy: Foodie AI focuses entirely on food
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
            
            // Fallback: choose direction with most open space
            Direction fallbackDir = safeDirections[0];
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
            Console.WriteLine($"FoodieAI: No food found, moving to open space: {fallbackDir}");
        }
    }

    // Concrete Strategy: Hunter AI targets the player snake
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
            
            // Fallback to AdvancedAI if no player found
            // Look for food
            Position targetFood = null;
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points)
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // Evaluate directions
            Direction fallbackDir = Snake.CurrentDirection;
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
            Console.WriteLine($"HunterAI: No player found, fallback direction: {fallbackDir}");
        }
    }

    // Concrete Strategy: Survivor AI prioritizes survival and open space
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

    // Concrete Strategy: Speedy AI moves quickly and erratically
    public class SpeedyAI : CpuSnakeAI
    {
        private int moveCounter = 0;
        
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
            Position targetFood = null;
            
            // Find nearest food
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .Where(p => p.Type == PowerUpType.Points || p.Type == PowerUpType.Speed)
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // If food found, move toward it
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
                Console.WriteLine($"SpeedyAI: Moving toward food, distance: {shortestDistance}");
                return;
            }
            
            // If no food, move in direction with most open space
            Direction fallbackDir = safeDirections[0];
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
            Console.WriteLine($"SpeedyAI: Moving to open space: {fallbackDir}");
        }
    }

    // Concrete Strategy: Aggressive AI that targets other snakes
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
            
            // If no snakes to target, look for power-ups
            Position targetFood = null;
            if (Arena.PowerUps?.Count > 0)
            {
                targetFood = Arena.PowerUps
                    .OrderBy(f => CalculateManhattanDistance(head, f.Position))
                    .Select(f => f.Position)
                    .FirstOrDefault();
            }
            
            // Move toward food if found
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
                return;
            }
            
            // If nothing to target, move randomly
            Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
        }
    }
}

