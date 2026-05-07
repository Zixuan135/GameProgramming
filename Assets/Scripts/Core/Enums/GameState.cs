namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Global game flow states for scene-level and runtime-level control.
    /// </summary>
    public enum GameState
    {
        None = 0,
        MainMenu = 1,
        ModeSelect = 2,
        MapSelect = 3,
        BattlePreparing = 4,
        BattleRunning = 5,
        BattleFinished = 6,
        Result = 7,
        Paused = 8,
        CharacterSelect = 9
    }
}
