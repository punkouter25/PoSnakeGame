using PoSnakeGame.Core.Models;
using System.Threading.Tasks;

namespace PoSnakeGame.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that manages user preferences.
    /// </summary>
    public interface IUserPreferencesService
    {
        /// <summary>
        /// Loads the user preferences asynchronously.
        /// </summary>
        /// <returns>The loaded UserPreferences object, or a default object if none found or error occurs.</returns>
        Task<UserPreferences> LoadPreferencesAsync();

        /// <summary>
        /// Saves the user preferences asynchronously.
        /// </summary>
        /// <param name="preferences">The UserPreferences object to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SavePreferencesAsync(UserPreferences preferences);
    }
}
