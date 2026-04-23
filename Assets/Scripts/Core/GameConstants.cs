namespace BubbleTown.Core
{
    /// <summary>
    /// Project-wide constants used by the initial game skeleton.
    /// Keep values simple for now and tune later in balancing phases.
    /// </summary>
    public static class GameConstants
    {
        public const int DefaultMapWidth = 13;
        public const int DefaultMapHeight = 11;
        public const float GridCellSize = 1f;

        public const int DefaultBombCount = 1;
        public const int DefaultExplosionRange = 2;
        public const int DefaultBombRange = DefaultExplosionRange;
        public const float DefaultBombFuseSeconds = 2f;

        public const float DefaultMoveSpeed = 4f;
        public const float DefaultExplosionDuration = 0.35f;

        public const string SceneMainMenu = "MainMenu";
        public const string SceneModeSelect = "ModeSelect";
        public const string SceneMapSelect = "MapSelect";
        public const string SceneBattle = "Battle";
        public const string SceneResult = "Result";
    }
}
