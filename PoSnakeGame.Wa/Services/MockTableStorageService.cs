using PoSnakeGame.Core.Models;
using PoSnakeGame.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoSnakeGame.Wa.Services
{
    public class MockTableStorageService : ITableStorageService
    {
        private readonly ILogger<MockTableStorageService> _logger;
        private readonly List<HighScore> _highScores;

        public MockTableStorageService(ILogger<MockTableStorageService> logger)
        {
            _logger = logger;
            
            // Create some sample high scores
            _highScores = new List<HighScore>
            {
                new HighScore
                {
                    Initials = "AAA",
                    Score = 150,
                    Date = DateTime.Now.AddDays(-7),
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = "HighScore"
                },
                new HighScore
                {
                    Initials = "BBB",
                    Score = 120,
                    Date = DateTime.Now.AddDays(-5),
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = "HighScore"
                },
                new HighScore
                {
                    Initials = "CCC",
                    Score = 100,
                    Date = DateTime.Now.AddDays(-2),
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = "HighScore"
                }
            };
        }

        public Task<List<HighScore>> GetTopScoresAsync(int count = 10)
        {
            _logger.LogInformation("Getting top {Count} scores from mock storage", count);
            var scores = _highScores.OrderByDescending(s => s.Score).Take(count).ToList();
            return Task.FromResult(scores);
        }

        public Task<bool> IsHighScore(int score)
        {
            _logger.LogInformation("Checking if {Score} is a high score", score);
            
            // If we have fewer than 10 scores, it's automatically a high score
            if (_highScores.Count < 10)
            {
                return Task.FromResult(true);
            }
            
            // Otherwise, check if it's higher than the lowest score
            var lowestHighScore = _highScores.OrderByDescending(s => s.Score).Take(10).Min(s => s.Score);
            return Task.FromResult(score > lowestHighScore);
        }

        public Task SaveHighScoreAsync(HighScore highScore)
        {
            _logger.LogInformation("Saving high score for {Initials} with score {Score}", highScore.Initials, highScore.Score);
            
            // Generate a unique key if not provided
            if (string.IsNullOrEmpty(highScore.RowKey))
            {
                highScore.RowKey = Guid.NewGuid().ToString();
            }
            
            if (string.IsNullOrEmpty(highScore.PartitionKey))
            {
                highScore.PartitionKey = "HighScore";
            }
            
            // Add the high score to our list
            _highScores.Add(highScore);
            
            // Keep only the top scores
            var topScores = _highScores.OrderByDescending(s => s.Score).Take(50).ToList();
            _highScores.Clear();
            _highScores.AddRange(topScores);
            
            return Task.CompletedTask;
        }
    }
} 