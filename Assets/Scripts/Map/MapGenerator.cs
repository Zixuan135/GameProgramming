using BubbleTown.Core;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Generates a basic grid map placeholder.
    /// Detailed tile rules and spawn layout will be implemented later.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int mapWidth = GameConstants.DefaultMapWidth;
        [SerializeField] private int mapHeight = GameConstants.DefaultMapHeight;
        [SerializeField] private float cellSize = GameConstants.GridCellSize;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;

        public void Generate(BattleMapType mapType)
        {
            Debug.Log($"[MapGenerator] Generate placeholder map. Type: {mapType}, Size: {mapWidth}x{mapHeight}");
            // TODO: Create runtime grid cells and wall layout.
        }

        public void Clear()
        {
            // TODO: Clean generated runtime map objects.
        }
    }
}
