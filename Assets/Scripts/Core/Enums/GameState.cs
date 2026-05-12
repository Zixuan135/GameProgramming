namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Global game flow states for scene-level and runtime-level control.
    /// </summary>
    public enum GameState
    {
        /// <summary>No active state has been selected yet.</summary>
        None = 0,

        /// <summary>The main title screen is currently active.</summary>
        MainMenu = 1,

        /// <summary>The player is choosing between Solo, AI Battle, and Local VS.</summary>
        ModeSelect = 2,

        /// <summary>The player is choosing which map layout to play.</summary>
        MapSelect = 3,

        /// <summary>The battle scene is loaded, but the Ready/Go opening flow is still running.</summary>
        BattlePreparing = 4,

        /// <summary>The battle is active and characters can move, place bombs, and collect items.</summary>
        BattleRunning = 5,

        /// <summary>The battle has ended and the game is preparing to show results.</summary>
        BattleFinished = 6,

        /// <summary>The result screen is visible.</summary>
        Result = 7,

        /// <summary>The battle is temporarily paused by the player.</summary>
        Paused = 8,

        /// <summary>The player is choosing a chibi character before map selection.</summary>
        CharacterSelect = 9
    }
}
