using System.Threading.Tasks;

namespace PoSnakeGame.Core.Interfaces // Changed namespace to Core.Interfaces
{
    /// <summary>
    /// Interface for playing sound effects. The implementation will likely use JS interop.
    /// </summary>
    public interface ISoundService
    {
        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="soundFileName">The logical name or path of the sound file.</param>
        /// <param name="volume">The volume level (0.0 to 1.0). Defaults to 1.0.</param>
        Task PlaySoundAsync(string soundFileName, double volume = 1.0);
    }
}
