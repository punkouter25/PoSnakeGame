namespace PoSnakeGame.Infrastructure.Configuration;

public class TableStorageConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string HighScoresTableName { get; set; } = "HighScores";
    public string GameStatisticsTableName { get; set; } = "GameStatistics";
}