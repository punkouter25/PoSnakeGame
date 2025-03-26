using PoSnakeGame.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoSnakeGame.Core.Services
{
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

            // Increased buffer from walls - stay further away
            if (nextPos.X <= 2 || nextPos.X >= Arena.Width - 3 || 
                nextPos.Y <= 2 || nextPos.Y >= Arena.Height - 3) return false;

            // Check collision with obstacles
            if (Arena.HasCollision(nextPos)) return false;

            // Check collision with other snakes with improved safety buffer
            foreach (var snake in Arena.Snakes)
            {
                // Check direct collision with any snake segment
                if (snake.Segments.Contains(nextPos)) return false;
                
                // Avoid getting near other snake heads (increased buffer)
                if (snake != Snake && snake.IsAlive)
                {
                    var otherHead = snake.Segments[0];
                    if (CalculateManhattanDistance(nextPos, otherHead) <= 2) return false;
                }
            }

            // Look ahead to avoid potential traps (dead ends)
            var lookAheadPos = nextPos + Position.FromDirection(direction);
            int freedomScore = CountValidAdjacentPositions(lookAheadPos);
            if (freedomScore < 2) return false; // Avoid positions that lead to limited movement options

            return true;
        }

        // New helper method to count valid adjacent positions
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
            var safeDirections = Enum.GetValues<Direction>()
                  .Where(IsValidMove)
                  .ToList();
            
            // Filter to only allow forward, left, or right (not backward)
            var validRelativeDirections = GetValidRelativeDirections(Snake.CurrentDirection);
            return safeDirections.Where(d => validRelativeDirections.Contains(d)).ToList();
        }

        // Helper method to get valid directions relative to current direction
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
    }

    public class AggressiveAI : CpuSnakeAI
    {
        public AggressiveAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                // No safe moves, keep current direction
                return;
            }

            // Look for nearby snakes to target
            var otherSnake = Arena.Snakes
                .Where(s => s != Snake && s.IsAlive)
                .OrderBy(s => CalculateManhattanDistance(Snake.Segments[0], s.Segments[0]))
                .FirstOrDefault();

            if (otherSnake != null && CalculateManhattanDistance(Snake.Segments[0], otherSnake.Segments[0]) < 5)
            {
                // If near another snake, try to intercept it
                MoveTowards(otherSnake.Segments[0], safeDirections);
            }
            else if (Arena.Foods.Any())
            {
                // Otherwise go for food
                var closestFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                    .First();
                MoveTowards(closestFood, safeDirections);
            }
            else
            {
                // Random safe movement
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }

        private void MoveTowards(Position target, List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var preferredDirection = Direction.Right;

            if (Math.Abs(target.X - head.X) > Math.Abs(target.Y - head.Y))
            {
                preferredDirection = target.X < head.X ? Direction.Left : Direction.Right;
            }
            else
            {
                preferredDirection = target.Y < head.Y ? Direction.Up : Direction.Down;
            }

            // Use preferred direction if safe, otherwise choose a random safe direction
            Snake.CurrentDirection = safeDirections.Contains(preferredDirection) 
                ? preferredDirection 
                : safeDirections[Random.Next(safeDirections.Count)];
        }
    }

    public class CautiousAI : CpuSnakeAI
    {
        public CautiousAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            // Check for nearby walls
            var head = Snake.Segments[0];
            bool nearWall = head.X <= 3 || head.X >= Arena.Width - 4 || 
                         head.Y <= 3 || head.Y >= Arena.Height - 4;

            // Check for nearby snakes with increased distance threshold
            var nearbySnake = Arena.Snakes
                .Where(s => s != Snake && s.IsAlive)
                .Any(s => CalculateManhattanDistance(Snake.Segments[0], s.Segments[0]) < 6);

            if (nearWall || nearbySnake)
            {
                // If near wall or another snake, try to move to safety
                MoveToSafety(safeDirections, nearWall);
            }
            else if (Arena.Foods.Any())
            {
                // Find the closest food that is safe to approach
                var safeFoods = FindSafeFoods();
                
                if (safeFoods.Any())
                {
                    var closestFood = safeFoods.First();
                    MoveTowards(closestFood, safeDirections);
                }
                else
                {
                    // No safe food path, move to most open area
                    MoveToBestOpenArea(safeDirections);
                }
            }
            else
            {
                // No food, move to most open area
                MoveToBestOpenArea(safeDirections);
            }
        }

        // Find foods that have safe paths to them
        private List<Position> FindSafeFoods()
        {
            var head = Snake.Segments[0];
            
            return Arena.Foods
                .Select(food => new { 
                    Food = food, 
                    Distance = CalculateManhattanDistance(head, food),
                    OpenSpace = EvaluatePathSafety(food)
                })
                .Where(f => f.OpenSpace > 0) // Only include foods with safe paths
                .OrderByDescending(f => f.OpenSpace) // Prioritize safer paths
                .ThenBy(f => f.Distance) // Then by distance
                .Select(f => f.Food)
                .ToList();
        }
        
        // Evaluate how safe the path to a target is (higher is safer)
        private int EvaluatePathSafety(Position target)
        {
            var head = Snake.Segments[0];
            var pathPositions = GetApproximatePath(head, target);
            
            // Count open adjacent spaces for each position in the path
            int totalOpenSpaces = pathPositions.Sum(pos => CountValidAdjacentPositions(pos));
            
            // Return average open spaces per position
            return pathPositions.Count > 0 ? totalOpenSpaces / pathPositions.Count : 0;
        }
        
        // Get approximate path between two positions (simple implementation)
        private List<Position> GetApproximatePath(Position start, Position end)
        {
            List<Position> path = new List<Position>();
            Position current = start;
            
            // Generate at most 8 steps to avoid too much computation
            for (int i = 0; i < 8; i++)
            {
                if (current.Equals(end)) break;
                
                // Move in the direction of largest difference
                int dx = end.X - current.X;
                int dy = end.Y - current.Y;
                
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    current = new Position(current.X + Math.Sign(dx), current.Y);
                }
                else
                {
                    current = new Position(current.X, current.Y + Math.Sign(dy));
                }
                
                path.Add(current);
                
                // Stop if we hit an obstacle or snake
                if (Arena.IsOutOfBounds(current) || Arena.HasCollision(current) || 
                    IsTouchingSnake(current))
                    break;
            }
            
            return path;
        }
        
        // Move to the direction with most open space
        private void MoveToBestOpenArea(List<Direction> safeDirections)
        {
            Direction bestDirection = safeDirections.First();
            int bestScore = -1;
            
            foreach (var direction in safeDirections)
            {
                var nextPos = Snake.Segments[0] + Position.FromDirection(direction);
                int openSpaceScore = CountValidAdjacentPositions(nextPos) * 2;
                
                // Look two steps ahead
                var furtherPos = nextPos + Position.FromDirection(direction);
                if (!Arena.IsOutOfBounds(furtherPos) && !Arena.HasCollision(furtherPos) && 
                    !IsTouchingSnake(furtherPos))
                {
                    openSpaceScore += CountValidAdjacentPositions(furtherPos);
                }
                
                if (openSpaceScore > bestScore)
                {
                    bestScore = openSpaceScore;
                    bestDirection = direction;
                }
            }
            
            Snake.CurrentDirection = bestDirection;
        }

        private void MoveToSafety(List<Direction> safeDirections, bool avoidWall)
        {
            // Implementation remains similar but favor directions with more open space
            var head = Snake.Segments[0];
            
            // Calculate scores for each direction
            var directionScores = new Dictionary<Direction, int>();
            
            foreach (var direction in safeDirections)
            {
                var nextPos = Snake.Segments[0] + Position.FromDirection(direction);
                
                // Base score is the number of valid adjacent positions
                int score = CountValidAdjacentPositions(nextPos) * 2;
                
                if (avoidWall)
                {
                    // Add score for moving away from walls and toward center
                    var centerX = Arena.Width / 2;
                    var centerY = Arena.Height / 2;
                    
                    // Current distance to center
                    int currentDistToCenter = Math.Abs(head.X - centerX) + Math.Abs(head.Y - centerY);
                    // New position's distance to center
                    int newDistToCenter = Math.Abs(nextPos.X - centerX) + Math.Abs(nextPos.Y - centerY);
                    
                    // Add points if moving closer to center
                    if (newDistToCenter < currentDistToCenter)
                    {
                        score += 3;
                    }
                }
                
                // Check for nearby snakes
                foreach (var snake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
                {
                    // Reduce score if moving closer to another snake
                    var currentDistToSnake = CalculateManhattanDistance(head, snake.Segments[0]);
                    var newDistToSnake = CalculateManhattanDistance(nextPos, snake.Segments[0]);
                    
                    if (newDistToSnake < currentDistToSnake)
                    {
                        score -= 2;
                    }
                    else if (newDistToSnake > currentDistToSnake)
                    {
                        score += 2;
                    }
                }
                
                directionScores[direction] = score;
            }
            
            // Choose the direction with highest score
            if (directionScores.Any())
            {
                Snake.CurrentDirection = directionScores.OrderByDescending(kv => kv.Value).First().Key;
            }
            else if (safeDirections.Any())
            {
                // Fallback to any safe direction
                Snake.CurrentDirection = safeDirections.First();
            }
        }

        private void MoveTowards(Position target, List<Direction> safeDirections)
        {
            if (!safeDirections.Any()) return;
            
            var head = Snake.Segments[0];
            
            // Calculate scores for each direction
            var directionScores = new Dictionary<Direction, int>();
            
            foreach (var direction in safeDirections)
            {
                var nextPos = Snake.Segments[0] + Position.FromDirection(direction);
                
                // Base score - higher for positions closer to target
                int distanceToTarget = CalculateManhattanDistance(nextPos, target);
                int score = 20 - distanceToTarget; // Higher score for closer positions
                
                // Add score based on open space
                score += CountValidAdjacentPositions(nextPos) * 2;
                
                directionScores[direction] = score;
            }
            
            // Choose the direction with highest score
            Snake.CurrentDirection = directionScores.OrderByDescending(kv => kv.Value).First().Key;
        }
    }

    public class FoodieAI : CpuSnakeAI
    {
        public FoodieAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            if (Arena.Foods.Any())
            {
                var closestFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                    .First();

                MoveTowards(closestFood, safeDirections);
            }
            else
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }

        private void MoveTowards(Position target, List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var preferredDirection = Direction.Right;

            if (Math.Abs(target.X - head.X) > Math.Abs(target.Y - head.Y))
            {
                preferredDirection = target.X < head.X ? Direction.Left : Direction.Right;
            }
            else
            {
                preferredDirection = target.Y < head.Y ? Direction.Up : Direction.Down;
            }

            Snake.CurrentDirection = safeDirections.Contains(preferredDirection)
                ? preferredDirection
                : safeDirections[Random.Next(safeDirections.Count)];
        }
    }

    public class RandomAI : CpuSnakeAI
    {
        public RandomAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (safeDirections.Any())
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }
    }

    public class HunterAI : CpuSnakeAI
    {
        public HunterAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            var playerSnake = Arena.Snakes.FirstOrDefault(s => s.Type == SnakeType.Human && s.IsAlive);
            if (playerSnake != null)
            {
                MoveTowards(playerSnake.Segments[0], safeDirections);
            }
            else if (Arena.Foods.Any()) // If no player found, go for food
            {
                var closestFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                    .First();
                MoveTowards(closestFood, safeDirections);
            }
            else
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }

        private void MoveTowards(Position target, List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var preferredDirection = Direction.Right;

            if (Math.Abs(target.X - head.X) > Math.Abs(target.Y - head.Y))
            {
                preferredDirection = target.X < head.X ? Direction.Left : Direction.Right;
            }
            else
            {
                preferredDirection = target.Y < head.Y ? Direction.Up : Direction.Down;
            }

            Snake.CurrentDirection = safeDirections.Contains(preferredDirection)
                ? preferredDirection
                : safeDirections[Random.Next(safeDirections.Count)];
        }
    }

    public class SurvivorAI : CpuSnakeAI
    {
        private Dictionary<Direction, int> _directionScores = new();
        private int _stuckCounter = 0;
        private readonly int _lookAheadDepth = 5; // How many steps to look ahead
        
        public SurvivorAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            // Calculate scores for each direction
            _directionScores.Clear();
            
            // Initialize all possible directions with base scores
            foreach (var direction in safeDirections)
            {
                var nextPos = Snake.Segments[0] + Position.FromDirection(direction);
                _directionScores[direction] = EvaluateDirectionSafety(direction, nextPos, 1);
            }

            // Check if snake seems stuck (repeatedly choosing the same direction)
            if (_stuckCounter > 10)
            {
                // Add randomness to break potential loops
                foreach (var dir in _directionScores.Keys.ToList())
                {
                    _directionScores[dir] += Random.Next(5);
                }
                _stuckCounter = 0;
            }

            // Select the best direction based on scores
            var bestDirection = _directionScores.OrderByDescending(kv => kv.Value).First().Key;
            
            // If we're selecting the same direction again, increment stuck counter
            if (bestDirection == Snake.CurrentDirection)
            {
                _stuckCounter++;
            }
            else
            {
                _stuckCounter = 0;
            }
            
            Snake.CurrentDirection = bestDirection;
        }

        // Recursively evaluate how safe a direction is by looking ahead multiple steps
        private int EvaluateDirectionSafety(Direction initialDirection, Position position, int depth)
        {
            // Base score starts with the open adjacent positions
            int score = CountValidAdjacentPositions(position) * 5;
            
            // Reward paths that lead to food
            if (Arena.Foods.Any(f => f.Equals(position)))
            {
                score += 30;
            }
            
            // Extra points for first move having more open space
            if (depth == 1)
            {
                score *= 2;
            }
            
            // Penalize proximity to other snakes
            foreach (var snake in Arena.Snakes.Where(s => s != Snake && s.IsAlive))
            {
                int distanceToSnake = CalculateManhattanDistance(position, snake.Segments[0]);
                if (distanceToSnake < 5)
                {
                    score -= (5 - distanceToSnake) * 5;
                }
            }
            
            // Penalize proximity to walls
            if (position.X <= 3 || position.X >= Arena.Width - 4 || 
                position.Y <= 3 || position.Y >= Arena.Height - 4)
            {
                score -= 15;
            }
            
            // Stop recursion at max depth
            if (depth >= _lookAheadDepth)
            {
                return score;
            }
            
            // Recursively evaluate next positions
            int bestNextScore = -1000;
            
            foreach (Direction nextDir in Enum.GetValues<Direction>())
            {
                // Skip opposite direction
                if (IsOppositeDirection(initialDirection, nextDir))
                    continue;
                    
                var nextPos = position + Position.FromDirection(nextDir);
                
                // Skip invalid positions
                if (Arena.IsOutOfBounds(nextPos) || 
                    Arena.HasCollision(nextPos) || 
                    IsTouchingSnake(nextPos))
                    continue;
                    
                // Calculate score for this path with diminishing returns
                int pathScore = EvaluateDirectionSafety(nextDir, nextPos, depth + 1) / 2;
                
                if (pathScore > bestNextScore)
                {
                    bestNextScore = pathScore;
                }
            }
            
            // Add the best next position score to current score
            if (bestNextScore > -1000)
            {
                score += bestNextScore;
            }
            
            return score;
        }
        
        // Helper method to check if two directions are opposite
        private bool IsOppositeDirection(Direction dir1, Direction dir2)
        {
            return (dir1 == Direction.Up && dir2 == Direction.Down) ||
                   (dir1 == Direction.Down && dir2 == Direction.Up) ||
                   (dir1 == Direction.Left && dir2 == Direction.Right) ||
                   (dir1 == Direction.Right && dir2 == Direction.Left);
        }
    }

    public class SpeedyAI : CpuSnakeAI
    {
        private int _movesInDirection = 0;
        private const int DirectionChangeProbability = 20; // % chance to change direction

        public SpeedyAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            // Increase speed
            Snake.Speed = 1.2f;

            _movesInDirection++;

            // Randomly change direction or if forced to by obstacles
            if (_movesInDirection > 5 || Random.Next(100) < DirectionChangeProbability || !safeDirections.Contains(Snake.CurrentDirection))
            {
                if (Arena.Foods.Any())
                {
                    var closestFood = Arena.Foods
                        .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                        .First();
                    MoveTowards(closestFood, safeDirections);
                }
                else
                {
                    Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
                }
                _movesInDirection = 0;
            }
        }

        private void MoveTowards(Position target, List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var preferredDirection = Direction.Right;

            if (Math.Abs(target.X - head.X) > Math.Abs(target.Y - head.Y))
            {
                preferredDirection = target.X < head.X ? Direction.Left : Direction.Right;
            }
            else
            {
                preferredDirection = target.Y < head.Y ? Direction.Up : Direction.Down;
            }

            Snake.CurrentDirection = safeDirections.Contains(preferredDirection)
                ? preferredDirection
                : safeDirections[Random.Next(safeDirections.Count)];
        }
    }
}

