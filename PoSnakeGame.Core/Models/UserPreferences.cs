using System.Drawing;

namespace PoSnakeGame.Core.Models
{
    /// <summary>
    /// Represents user-configurable preferences for the game.
    /// </summary>
    public class UserPreferences
    {
        /// <summary>
        /// The preferred color for the player's snake.
        /// Stored as Hex string for easy serialization (e.g., "#FF0000" for Red).
        /// Defaults to Red.
        /// </summary>
        public string PlayerSnakeColorHex { get; set; } = "#FF0000"; 

        // Potential future preferences:
        // public float SoundVolume { get; set; } = 0.8f;
        // public string DifficultyLevel { get; set; } = "Normal"; 
        // public bool ShowGridLines { get; set; } = false;

        /// <summary>
        /// Helper method to get the Color object from the hex string.
        /// Returns Color.Red if conversion fails.
        /// </summary>
        public Color GetPlayerSnakeColor()
        {
            try
            {
                // Use ColorTranslator for robust conversion from HTML hex color
                return ColorTranslator.FromHtml(PlayerSnakeColorHex);
            }
            catch (Exception)
            {
                // Fallback to default color if hex string is invalid
                return Color.Red; 
            }
        }

         /// <summary>
        /// Helper method to set the player snake color using a Color object.
        /// Converts the Color to its hex representation.
        /// </summary>
        public void SetPlayerSnakeColor(Color color)
        {
             // Use ColorTranslator for robust conversion to HTML hex color
            PlayerSnakeColorHex = ColorTranslator.ToHtml(color);
        }
    }
}
