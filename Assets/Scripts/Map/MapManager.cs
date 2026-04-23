using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
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
        private Vector2Int player1SpawnGrid = new Vector2Int(1, 1);
        private Vector2Int player2SpawnGrid = new Vector2Int(1, 1);
        private Vector2Int aiSpawnGrid = new Vector2Int(1, 1);

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

            ApplyMapRules();
        }

        /// <summary>
        /// Applies lightweight Bomberman-style logical rules:
        /// 1) hard-wall border
        /// 2) mode-based spawn positions
        /// 3) keep spawn-adjacent area empty
        /// </summary>
        private void ApplyMapRules()
        {
            ApplyHardWallBorder();
            ResolveSpawnPointsByMode();
            ReserveSpawnArea(player1SpawnGrid);
            ReserveSpawnArea(player2SpawnGrid);
            ReserveSpawnArea(aiSpawnGrid);
        }

        private void ApplyHardWallBorder()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                SetHardWall(new Vector2Int(x, 0), true);
                SetHardWall(new Vector2Int(x, mapHeight - 1), true);
            }

            for (int y = 0; y < mapHeight; y++)
            {
                SetHardWall(new Vector2Int(0, y), true);
                SetHardWall(new Vector2Int(mapWidth - 1, y), true);
            }
        }

        private void ResolveSpawnPointsByMode()
        {
            Vector2Int bottomLeft = new Vector2Int(1, 1);
            Vector2Int topRight = new Vector2Int(mapWidth - 2, mapHeight - 2);
            Vector2Int topLeft = new Vector2Int(1, mapHeight - 2);

            GameMode mode = GameManager.Instance != null
                ? GameManager.Instance.CurrentGameMode
                : GameMode.SinglePlayer;

            player1SpawnGrid = bottomLeft;
            player2SpawnGrid = topRight;
            aiSpawnGrid = topLeft;

            switch (mode)
            {
                case GameMode.SinglePlayer:
                    // Keep only player1 meaningful for now.
                    player2SpawnGrid = bottomLeft;
                    aiSpawnGrid = bottomLeft;
                    break;
                case GameMode.AIBattle:
                    // Player and AI are placed far apart for fair opening space.
                    aiSpawnGrid = topRight;
                    player2SpawnGrid = bottomLeft;
                    break;
                case GameMode.LocalVS:
                    // Two players use opposite corners to avoid early overlap.
                    player2SpawnGrid = topRight;
                    aiSpawnGrid = topLeft;
                    break;
            }
        }

        private void ReserveSpawnArea(Vector2Int center)
        {
            ClearBlockingAt(center);
            ClearBlockingAt(center + Vector2Int.right);
            ClearBlockingAt(center + Vector2Int.left);
            ClearBlockingAt(center + Vector2Int.up);
            ClearBlockingAt(center + Vector2Int.down);
        }

        private void ClearBlockingAt(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.IsHardWall = false;
            cell.IsSoftWall = false;
        }

        private void SetHardWall(Vector2Int gridPos, bool isHardWall)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.IsHardWall = isHardWall;
            if (isHardWall)
            {
                cell.IsSoftWall = false;
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

        public Vector2Int GetPlayer1SpawnGrid() => player1SpawnGrid;
        public Vector2Int GetPlayer2SpawnGrid() => player2SpawnGrid;
        public Vector2Int GetAISpawnGrid() => aiSpawnGrid;

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
