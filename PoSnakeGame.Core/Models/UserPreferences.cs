using System;

namespace PoSnakeGame.Core.Models
{
    /// <summary>
    /// Model to store user preferences
    /// </summary>
    public class UserPreferences
    {
        public string Initials { get; set; } = string.Empty;
        public DateTime LastPlayed { get; set; } = DateTime.Now;
        public bool SoundEnabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;
    }
} 