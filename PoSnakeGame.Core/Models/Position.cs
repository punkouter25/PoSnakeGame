namespace PoSnakeGame.Core.Models;

/// <summary>
/// Represents a position on the game grid
/// </summary>
public record Position(int X, int Y)
{
    public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);
    
    public static Position FromDirection(Direction direction) => direction switch
    {
        Direction.Up => new Position(0, -1),
        Direction.Down => new Position(0, 1),
        Direction.Left => new Position(-1, 0),
        Direction.Right => new Position(1, 0),
        _ => throw new ArgumentException("Invalid direction", nameof(direction))
    };
}