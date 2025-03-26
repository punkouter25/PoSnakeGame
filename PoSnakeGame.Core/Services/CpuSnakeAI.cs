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

            // Check arena bounds
            if (Arena.IsOutOfBounds(nextPos)) return false;

            // Check collision with obstacles
            if (Arena.HasCollision(nextPos)) return false;

            // Check collision with other snakes
            foreach (var snake in Arena.Snakes)
            {
                if (snake.Segments.Contains(nextPos)) return false;
            }

            return true;
        }

        protected List<Direction> GetSafeDirections()
        {
            return Enum.GetValues<Direction>()
                      .Where(IsValidMove)
                      .ToList();
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

            // Check for nearby snakes
            var nearbySnake = Arena.Snakes
                .Where(s => s != Snake && s.IsAlive)
                .Any(s => CalculateManhattanDistance(Snake.Segments[0], s.Segments[0]) < 3);

            if (nearbySnake)
            {
                // If near another snake, try to move away
                AvoidOtherSnakes(safeDirections);
            }
            else if (Arena.Foods.Any())
            {
                // If food is nearby and path is safe, go for it
                var closestFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                    .First();
                
                if (CalculateManhattanDistance(Snake.Segments[0], closestFood) < 5)
                {
                    MoveTowards(closestFood, safeDirections);
                }
                else
                {
                    // Move randomly but safely
                    Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
                }
            }
            else
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }

        private void AvoidOtherSnakes(List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var nearestSnake = Arena.Snakes
                .Where(s => s != Snake && s.IsAlive)
                .OrderBy(s => CalculateManhattanDistance(head, s.Segments[0]))
                .First();

            // Try to move in opposite direction of nearest snake
            var avoidDirection = head.X < nearestSnake.Segments[0].X ? Direction.Left : Direction.Right;
            if (safeDirections.Contains(avoidDirection))
            {
                Snake.CurrentDirection = avoidDirection;
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
        public SurvivorAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            var safeDirections = GetSafeDirections();
            if (!safeDirections.Any())
            {
                return;
            }

            // Always try to stay away from other snakes
            var nearestSnake = Arena.Snakes
                .Where(s => s != Snake && s.IsAlive)
                .OrderBy(s => CalculateManhattanDistance(Snake.Segments[0], s.Segments[0]))
                .FirstOrDefault();

            if (nearestSnake != null && CalculateManhattanDistance(Snake.Segments[0], nearestSnake.Segments[0]) < 4)
            {
                // Move away from nearest snake
                AvoidTarget(nearestSnake.Segments[0], safeDirections);
            }
            else if (Arena.Foods.Any())
            {
                // Only go for food if it's safe
                var closestFood = Arena.Foods
                    .OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f))
                    .First();
                
                if (IsSafePath(closestFood))
                {
                    MoveTowards(closestFood, safeDirections);
                }
                else
                {
                    Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
                }
            }
            else
            {
                Snake.CurrentDirection = safeDirections[Random.Next(safeDirections.Count)];
            }
        }

        private bool IsSafePath(Position target)
        {
            // Check if there are any snakes between us and the target
            var distance = CalculateManhattanDistance(Snake.Segments[0], target);
            return !Arena.Snakes.Where(s => s != Snake && s.IsAlive)
                               .Any(s => CalculateManhattanDistance(s.Segments[0], target) < distance);
        }

        private void AvoidTarget(Position target, List<Direction> safeDirections)
        {
            var head = Snake.Segments[0];
            var preferredDirection = Direction.Right;

            if (Math.Abs(target.X - head.X) > Math.Abs(target.Y - head.Y))
            {
                preferredDirection = target.X < head.X ? Direction.Right : Direction.Left;
            }
            else
            {
                preferredDirection = target.Y < head.Y ? Direction.Down : Direction.Up;
            }

            Snake.CurrentDirection = safeDirections.Contains(preferredDirection)
                ? preferredDirection
                : safeDirections[Random.Next(safeDirections.Count)];
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
