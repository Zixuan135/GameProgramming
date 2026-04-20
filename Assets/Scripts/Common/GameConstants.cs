using UnityEngine;

namespace BubbleTown
{
    public static class GameConstants
    {
        public const string SceneMainMenu = "MainMenu";
        public const string SceneModeSelect = "ModeSelect";
        public const string SceneMapSelect = "MapSelect";
        public const string SceneBattle = "Battle";
        public const string SceneResult = "Result";

        public const float DefaultGridSize = 1f;
        public const int DefaultMapWidth = 13;
        public const int DefaultMapHeight = 11;

        public const float DefaultBombFuseSeconds = 2f;
        public const float DefaultExplosionLifetime = 0.4f;

        public const float DefaultMoveSpeed = 4f;
        public const int DefaultMaxBombCount = 1;
        public const int DefaultExplosionRange = 1;

        public static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public const string HardWallTag = "HardWall";
        public const string SoftWallTag = "SoftWall";
        public const string BombTag = "Bomb";
        public const string CharacterTag = "Character";
    }
}
