namespace PoSnakeGame.Infrastructure.Configuration;

public class TableStorageConfig
{
    private const string LocalAzuriteConnectionString = "UseDevelopmentStorage=true";
    private string? _connectionString;

    public string ConnectionString 
    { 
        get => _connectionString ?? LocalAzuriteConnectionString;
        set => _connectionString = value;
    }

    public string HighScoresTableName { get; set; } = "HighScores";
    public string GameStatisticsTableName { get; set; } = "GameStatistics";

    public bool IsUsingLocalStorage => ConnectionString == LocalAzuriteConnectionString;
}