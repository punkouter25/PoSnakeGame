namespace PoSnakeGame.Core.Models;

public class Arena
{
    public int Width { get; init; }
    public int Height { get; init; }
    public List<Snake> Snakes { get; set; } = new();
    public List<Position> Foods { get; set; } = new();
    public List<PowerUp> PowerUps { get; set; } = new();
    public List<Position> Obstacles { get; set; } = new();
    public float GameSpeed { get; set; } = 1.0f;
    public float ElapsedTime { get; set; } = 0f;
    public const float GameDuration = 30f; // 30 seconds game duration

    public Arena(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool IsOutOfBounds(Position position)
    {
        return position.X < 0 || position.X >= Width || 
               position.Y < 0 || position.Y >= Height;
    }

    public bool HasCollision(Position position)
    {
        return Obstacles.Contains(position);
    }

    public void AddObstacle(Position position)
    {
        if (!Obstacles.Contains(position))
        {
            Obstacles.Add(position);
        }
    }

    public void AddFood(Position position)
    {
        if (!Foods.Contains(position) && !Obstacles.Contains(position))
        {
            Foods.Add(position);
        }
    }

    public void AddPowerUp(PowerUp powerUp)
    {
        if (!PowerUps.Any(p => p.Position == powerUp.Position) && 
            !Obstacles.Contains(powerUp.Position))
        {
            PowerUps.Add(powerUp);
        }
    }
}