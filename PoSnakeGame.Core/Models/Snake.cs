using System.Drawing;

namespace PoSnakeGame.Core.Models;

/// <summary>
/// Represents a snake in the game with its properties and state
/// </summary>
public class Snake
{
    public List<Position> Segments { get; set; } = new();
    public Direction CurrentDirection { get; set; }
    public int Length { get; private set; }
    public string Color { get; set; } // Use string for hex color representation
    public SnakePersonality Personality { get; set; }
    public float Speed { get; set; }
    public bool IsAlive { get; set; }
    public int Score { get; private set; }
    public float SizeMultiplier { get; set; } = 1.0f; // New property to control snake size

    public Snake(Position startPosition, Direction initialDirection, string color, SnakePersonality personality)
    {
        Segments.Add(startPosition);
        CurrentDirection = initialDirection;
        Color = color;
        Personality = personality;
        Length = 1;
        Speed = 1.0f; // Default speed, can be overridden by AI (e.g., SpeedyAI)
        IsAlive = true;
        Score = 0;
    }

    public void Grow()
    {
        Length++;
        // New segment will be added during next move
    }

    public void AddPoints(int points)
    {
        Score += points;
    }

    public void Move(Position newHeadPosition)
    {
        Segments.Insert(0, newHeadPosition);
        if (Segments.Count > Length)
        {
            Segments.RemoveAt(Segments.Count - 1);
        }
    }

    public void Die()
    {
        IsAlive = false;
    }
}
