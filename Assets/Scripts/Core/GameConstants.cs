namespace BubbleTown.Core
{
    /// <summary>
    /// Project-wide constants used by the initial game skeleton.
    /// Keep values simple for now and tune later in balancing phases.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>Default logical grid width used when no map-specific width is configured.</summary>
        public const int DefaultMapWidth = 13;

        /// <summary>Default logical grid height used when no map-specific height is configured.</summary>
        public const int DefaultMapHeight = 11;

        /// <summary>World-space size of one logical grid cell on the XZ plane.</summary>
        public const float GridCellSize = 1f;

        /// <summary>Default number of bombs a character may place at the same time.</summary>
        public const int DefaultBombCount = 1;

        /// <summary>Default number of cells a bomb blast reaches in each cardinal direction.</summary>
        public const int DefaultExplosionRange = 2;

        /// <summary>Compatibility alias for older code that referred to explosion range as bomb range.</summary>
        public const int DefaultBombRange = DefaultExplosionRange;

        /// <summary>Default delay between placing a bomb and triggering its explosion.</summary>
        public const float DefaultBombFuseSeconds = 2f;

        /// <summary>Default character movement speed in world units per second.</summary>
        public const float DefaultMoveSpeed = 4f;

        /// <summary>Default lifetime of one explosion visual before it disappears.</summary>
        public const float DefaultExplosionDuration = 0.35f;

        /// <summary>Bomb-count increase applied by the bomb slot item.</summary>
        public const int DefaultItemBombCountDelta = 1;

        /// <summary>Explosion-range increase applied by the blast range item.</summary>
        public const int DefaultItemExplosionRangeDelta = 1;

        /// <summary>Movement-speed increase applied by the speed boots item.</summary>
        public const float DefaultItemMoveSpeedDelta = 0.5f;

        /// <summary>Shield charges granted by one shield pickup.</summary>
        public const int DefaultItemShieldChargesDelta = 1;

        /// <summary>Maximum shield charges a character can hold.</summary>
        public const int DefaultMaxShieldCharges = 3;

        /// <summary>Temporary invincibility duration granted by the invincible item.</summary>
        public const float DefaultItemInvincibleSeconds = 4f;

        /// <summary>Default chance for a destroyed soft wall to spawn an item.</summary>
        public const float DefaultItemDropChance = 0.3f;

        /// <summary>Default vertical offset used when spawning item visuals above a grid cell.</summary>
        public const float DefaultItemSpawnHeight = 0.35f;

        /// <summary>Fallback number of soft walls to clear for the single-player objective.</summary>
        public const int DefaultSinglePlayerSoftWallTarget = 8;

        /// <summary>Unity scene name for the main menu.</summary>
        public const string SceneMainMenu = "MainMenu";

        /// <summary>Unity scene name for the mode selection screen.</summary>
        public const string SceneModeSelect = "ModeSelect";

        /// <summary>Unity scene name for the character selection screen.</summary>
        public const string SceneCharacterSelect = "CharacterSelect";

        /// <summary>Unity scene name for the map selection screen.</summary>
        public const string SceneMapSelect = "MapSelect";

        /// <summary>Unity scene name for choosing AI behavior before AI Battle starts.</summary>
        public const string SceneDifficultySelect = "DifficultySelect";

        /// <summary>Unity scene name for the battle scene.</summary>
        public const string SceneBattle = "Battle";

        /// <summary>Unity scene name for the result screen.</summary>
        public const string SceneResult = "Result";
    }
}
