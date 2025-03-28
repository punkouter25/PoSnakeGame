using PoSnakeGame.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoSnakeGame.Infrastructure.Services
{
    public interface ITableStorageService
    {
        Task<List<HighScore>> GetTopScoresAsync(int count = 10);
        Task<bool> IsHighScore(int score);
        Task SaveHighScoreAsync(HighScore highScore);
        Task<bool> TableExistsAsync(string tableName); // Added method signature
    }
}
