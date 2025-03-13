using PoSnakeGame.Core.Models;
using System;
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
    }

    public class AggressiveAI : CpuSnakeAI
    {
        public AggressiveAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Aggressive AI logic: Prioritizes attacking other snakes
            // Placeholder logic: Move randomly for now
            Snake.CurrentDirection = (Direction)Random.Next(4);
        }
    }

    public class CautiousAI : CpuSnakeAI
    {
        public CautiousAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Cautious AI logic: Avoids other snakes and obstacles
            // Placeholder logic: Move randomly for now
            Snake.CurrentDirection = (Direction)Random.Next(4);
        }
    }

    public class FoodieAI : CpuSnakeAI
    {
        public FoodieAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Foodie AI logic: Focuses on collecting food
            if (Arena.Foods.Any())
            {
                var closestFood = Arena.Foods.OrderBy(f => CalculateManhattanDistance(Snake.Segments[0], f)).First();
                MoveTowards(closestFood);
            }
            else
            {
                Snake.CurrentDirection = (Direction)Random.Next(4);
            }
        }

        private void MoveTowards(Position target)
        {
            var head = Snake.Segments[0];
            if (target.X < head.X) Snake.CurrentDirection = Direction.Left;
            else if (target.X > head.X) Snake.CurrentDirection = Direction.Right;
            else if (target.Y < head.Y) Snake.CurrentDirection = Direction.Up;
            else if (target.Y > head.Y) Snake.CurrentDirection = Direction.Down;
        }
    }

    public class RandomAI : CpuSnakeAI
    {
        public RandomAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Random AI logic: Moves randomly
            Snake.CurrentDirection = (Direction)Random.Next(4);
        }
    }

    public class HunterAI : CpuSnakeAI
    {
        public HunterAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Hunter AI logic: Targets the player snake
            var playerSnake = Arena.Snakes.FirstOrDefault(s => s.Type == SnakeType.Human);
            if (playerSnake != null)
            {
                MoveTowards(playerSnake.Segments[0]);
            }
            else
            {
                Snake.CurrentDirection = (Direction)Random.Next(4);
            }
        }

        private void MoveTowards(Position target)
        {
            var head = Snake.Segments[0];
            if (target.X < head.X) Snake.CurrentDirection = Direction.Left;
            else if (target.X > head.X) Snake.CurrentDirection = Direction.Right;
            else if (target.Y < head.Y) Snake.CurrentDirection = Direction.Up;
            else if (target.Y > head.Y) Snake.CurrentDirection = Direction.Down;
        }
    }

    public class SurvivorAI : CpuSnakeAI
    {
        public SurvivorAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Survivor AI logic: Prioritizes survival and avoids danger
            // Placeholder logic: Move randomly for now
            Snake.CurrentDirection = (Direction)Random.Next(4);
        }
    }

    public class SpeedyAI : CpuSnakeAI
    {
        public SpeedyAI(Snake snake, Arena arena) : base(snake, arena) { }

        public override void UpdateDirection()
        {
            // Speedy AI logic: Moves quickly and erratically
            Snake.CurrentDirection = (Direction)Random.Next(4);
        }
    }
}
