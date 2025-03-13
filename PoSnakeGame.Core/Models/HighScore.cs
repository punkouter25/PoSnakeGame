using Azure;
using Azure.Data.Tables;

namespace PoSnakeGame.Core.Models;

/// <summary>
/// Represents a high score entry
/// </summary>
public class HighScore : ITableEntity
{
    public string Initials { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime Date { get; set; }
    public float GameDuration { get; set; }
    public int SnakeLength { get; set; }
    public int FoodEaten { get; set; }

    // ITableEntity implementation
    public string PartitionKey { get; set; } = "HighScore";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}