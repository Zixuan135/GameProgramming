using BubbleTown.Core;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Coordinates map lifecycle and links map generation with battle scene setup.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;

        [Header("Map Config (Data Layer)")]
        [SerializeField] private int mapWidth = GameConstants.DefaultMapWidth;
        [SerializeField] private int mapHeight = GameConstants.DefaultMapHeight;
        [SerializeField] private float cellSize = GameConstants.GridCellSize;
        [SerializeField] private bool useMapGeneratorSize = true;

        [Header("Runtime")]
        [SerializeField] private BattleMapType selectedMapType = BattleMapType.Default;

        public BattleMapType SelectedMapType => selectedMapType;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;

        private GridCell[,] grid;

        private void Start()
        {
            InitializeGridData();
            GenerateMap();
        }

        public void SetMapType(BattleMapType mapType)
        {
            selectedMapType = mapType;
        }

        /// <summary>
        /// Creates logical grid data only. No complex generation in this phase.
        /// </summary>
        public void InitializeGridData()
        {
            if (useMapGeneratorSize && mapGenerator != null)
            {
                mapWidth = Mathf.Max(1, mapGenerator.MapWidth);
                mapHeight = Mathf.Max(1, mapGenerator.MapHeight);
                cellSize = Mathf.Max(0.1f, mapGenerator.CellSize);
            }

            grid = new GridCell[mapWidth, mapHeight];
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    grid[x, y] = new GridCell(x, y);
                }
            }
        }

        public GridCell GetCell(Vector2Int gridPos)
        {
            if (!IsInsideBounds(gridPos))
            {
                return null;
            }

            return grid[gridPos.x, gridPos.y];
        }

        public bool IsInsideBounds(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < mapWidth &&
                   gridPos.y >= 0 && gridPos.y < mapHeight;
        }

        public bool IsWalkable(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return false;
            }

            if (cell.IsHardWall || cell.IsSoftWall)
            {
                return false;
            }

            if (cell.HasBomb || cell.HasCharacter)
            {
                return false;
            }

            return true;
        }

        public bool SetCharacter(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || cell.IsHardWall || cell.IsSoftWall || cell.HasCharacter)
            {
                return false;
            }

            cell.HasCharacter = true;
            return true;
        }

        public void ClearCharacter(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.HasCharacter = false;
        }

        public bool PlaceBomb(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || cell.IsHardWall || cell.IsSoftWall || cell.HasBomb)
            {
                return false;
            }

            cell.HasBomb = true;
            return true;
        }

        public void RemoveBomb(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.HasBomb = false;
        }

        public bool SetItem(Vector2Int gridPos, bool hasItem)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return false;
            }

            cell.HasItem = hasItem;
            return true;
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / cellSize);
            int y = Mathf.RoundToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorld(Vector2Int gridPos, float y = 0f)
        {
            return new Vector3(gridPos.x * cellSize, y, gridPos.y * cellSize);
        }

        public void GenerateMap()
        {
            if (mapGenerator == null)
            {
                return;
            }

            mapGenerator.Generate(selectedMapType);
        }

        public void ClearMap()
        {
            if (mapGenerator == null)
            {
                return;
            }

            mapGenerator.Clear();

            if (grid == null)
            {
                return;
            }

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    grid[x, y].ClearDynamicFlags();
                }
            }
        }
    }
}
