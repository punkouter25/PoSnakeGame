using PoSnakeGame.Core.Models;
using System.Threading.Tasks;

namespace PoSnakeGame.Core.Interfaces
{
    /// <summary>
    /// Service for managing user preferences
    /// </summary>
    public interface IUserPreferencesService
    {
        /// <summary>
        /// Retrieves the user preferences
        /// </summary>
        Task<UserPreferences> GetUserPreferencesAsync();
        
        /// <summary>
        /// Saves user preferences
        /// </summary>
        Task SaveUserPreferencesAsync(UserPreferences preferences);
    }
} 