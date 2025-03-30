namespace PoSnakeGame.Core.Models;

/// <summary>
/// Represents the defined AI personalities for CPU snakes, plus the Human player.
/// </summary>
public enum SnakePersonality
{
    Human,
    Random,
    Foodie,
    Cautious, // Combines Advanced/Cautious
    Hunter,
    Survivor,
    Speedy,
    Aggressive
}
