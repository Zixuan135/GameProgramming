namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Purpose: Lists the player-facing game modes available from the mode select screen.
    /// Inputs: selected by UI or restored from current session data.
    /// Output: enum values used by GameManager to decide which characters and rules to enable.
    /// </summary>
    public enum GameMode
    {
        /// <summary>One player clears the route objective alone.</summary>
        SinglePlayer = 0,

        /// <summary>One player fights an AI opponent.</summary>
        AIBattle = 1,

        /// <summary>Two local players share one keyboard for versus play.</summary>
        LocalVS = 2
    }
}
