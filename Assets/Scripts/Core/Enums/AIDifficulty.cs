namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Defines how aggressively the AI opponent should move, chase, escape, and place bombs.
    /// The enum is stored by GameManager so the selected difficulty survives scene changes.
    /// </summary>
    public enum AIDifficulty
    {
        /// <summary>
        /// Slower, safer, and less likely to place bombs. Good for learning the game.
        /// </summary>
        Easy,

        /// <summary>
        /// Balanced behavior that keeps the previous MVP AI feel.
        /// </summary>
        Normal,

        /// <summary>
        /// Faster, more aggressive, and better at searching for escape routes.
        /// </summary>
        Hard
    }
}
