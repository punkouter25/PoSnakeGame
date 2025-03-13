namespace PoSnakeGame.Core.Models;

public class PowerUp
{
    public Position Position { get; set; }
    public PowerUpType Type { get; set; }
    public float Duration { get; set; }
    public float Value { get; set; }

    public PowerUp(Position position, PowerUpType type, float duration, float value)
    {
        Position = position;
        Type = type;
        Duration = duration;
        Value = value;
    }
}

public enum PowerUpType
{
    Speed,
    SlowDown,
    Points
}