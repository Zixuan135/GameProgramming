using System;
using System.Collections.Generic;
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
        [SerializeField] private Transform mapVisualRoot;

        [Header("Map Config (Data Layer)")]
        [SerializeField] private int mapWidth = GameConstants.DefaultMapWidth;
        [SerializeField] private int mapHeight = GameConstants.DefaultMapHeight;
        [SerializeField] private float cellSize = GameConstants.GridCellSize;
        [SerializeField] private bool useMapGeneratorSize = true;

        [Header("Initial Blocking Cells")]
        [SerializeField] private Vector2Int[] initialHardWallCells = new Vector2Int[0];
        [SerializeField] private Vector2Int[] initialSoftWallCells = new Vector2Int[0];

        [Header("Runtime")]
        [SerializeField] private BattleMapType selectedMapType = BattleMapType.Default;

        public BattleMapType SelectedMapType => selectedMapType;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;

        public event Action<Vector2Int> SoftWallDestroyed;

        private GridCell[,] grid;
        private Vector2Int player1SpawnGrid = new Vector2Int(1, 1);
        private Vector2Int player2SpawnGrid = new Vector2Int(1, 1);
        private Vector2Int aiSpawnGrid = new Vector2Int(1, 1);
        private readonly Dictionary<Vector2Int, GameObject> softWallObjects = new Dictionary<Vector2Int, GameObject>();

        private void Awake()
        {
            InitializeGridData();
        }

        private void Start()
        {
            GenerateMap();
            RebuildSoftWallObjectLookup();
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
            ApplyInitialBlockingCells();
            ApplySelectedMapLayout();
            ResolveSpawnPointsByMode();
            ReserveSpawnArea(player1SpawnGrid);
            ReserveSpawnArea(player2SpawnGrid);
            ReserveSpawnArea(aiSpawnGrid);
        }

        private void ApplySelectedMapLayout()
        {
            switch (selectedMapType)
            {
                case BattleMapType.OpenField:
                    ApplyOpenFieldLayout();
                    break;
                case BattleMapType.Maze:
                    ApplyMazeLayout();
                    break;
                default:
                    ApplyCandyParkLayout();
                    break;
            }
        }

        private void ApplyCandyParkLayout()
        {
            ApplyClassicHardWallPillars();
            ApplyPatternedSoftWalls(3, 1);
        }

        private void ApplyOpenFieldLayout()
        {
            ApplyPatternedSoftWalls(5, 2);
        }

        private void ApplyMazeLayout()
        {
            ApplyClassicHardWallPillars();
            ApplyPatternedSoftWalls(2, 0);
        }

        private void ApplyClassicHardWallPillars()
        {
            for (int x = 2; x < mapWidth - 2; x += 2)
            {
                for (int y = 2; y < mapHeight - 2; y += 2)
                {
                    SetHardWall(new Vector2Int(x, y), true);
                }
            }
        }

        private void ApplyPatternedSoftWalls(int interval, int offset)
        {
            int safeInterval = Mathf.Max(2, interval);
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    GridCell cell = GetCell(gridPosition);
                    if (cell == null || cell.IsHardWall)
                    {
                        continue;
                    }

                    int patternValue = x * 7 + y * 11 + offset;
                    if (patternValue % safeInterval == 0)
                    {
                        SetSoftWall(gridPosition, true);
                    }
                }
            }
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

        private void ApplyInitialBlockingCells()
        {
            for (int i = 0; i < initialHardWallCells.Length; i++)
            {
                SetHardWall(initialHardWallCells[i], true);
            }

            for (int i = 0; i < initialSoftWallCells.Length; i++)
            {
                SetSoftWall(initialSoftWallCells[i], true);
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
            if (IsBorderCell(gridPos))
            {
                return;
            }

            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.IsHardWall = false;
            cell.IsSoftWall = false;
        }

        private bool IsBorderCell(Vector2Int gridPos)
        {
            return gridPos.x == 0 || gridPos.x == mapWidth - 1 ||
                   gridPos.y == 0 || gridPos.y == mapHeight - 1;
        }

        public void SetHardWall(Vector2Int gridPos, bool isHardWall)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.IsHardWall = isHardWall;
            if (isHardWall && cell.IsSoftWall)
            {
                RemoveSoftWallObject(gridPos);
                cell.IsSoftWall = false;
            }
        }

        public void SetSoftWall(Vector2Int gridPos, bool isSoftWall)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || cell.IsHardWall)
            {
                return;
            }

            bool wasSoftWall = cell.IsSoftWall;
            cell.IsSoftWall = isSoftWall;
            if (!isSoftWall && wasSoftWall)
            {
                RemoveSoftWallObject(gridPos);
            }
        }

        public bool DestroySoftWall(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || !cell.IsSoftWall)
            {
                return false;
            }

            cell.IsSoftWall = false;
            RemoveSoftWallObject(gridPos);
            NotifySoftWallDestroyed(gridPos);
            return true;
        }

        private void NotifySoftWallDestroyed(Vector2Int gridPos)
        {
            SoftWallDestroyed?.Invoke(gridPos);
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

            if (IsBlockedByWall(cell))
            {
                return false;
            }

            if (IsBlockedByBomb(cell))
            {
                return false;
            }

            if (IsOccupiedByCharacter(cell))
            {
                return false;
            }

            return true;
        }

        public bool IsBlockedByWall(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            return cell == null || IsBlockedByWall(cell);
        }

        public bool IsBlockedByBomb(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            return cell != null && IsBlockedByBomb(cell);
        }

        private bool IsBlockedByWall(GridCell cell)
        {
            return cell.IsHardWall || cell.IsSoftWall;
        }

        private bool IsBlockedByBomb(GridCell cell)
        {
            // Bombs block movement for now; later this can support owner grace rules.
            return cell.HasBomb;
        }

        private bool IsOccupiedByCharacter(GridCell cell)
        {
            return cell.HasCharacter;
        }

        public bool SetCharacter(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || IsBlockedByWall(cell) || IsBlockedByBomb(cell) || IsOccupiedByCharacter(cell))
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
            if (cell == null || IsBlockedByWall(cell) || IsBlockedByBomb(cell))
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

        public void PlayHardWallBlockedFeedback(Vector2Int gridPos, Vector3 explosionWorldPosition)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null || !cell.IsHardWall)
            {
                return;
            }

            GameObject wallObject = FindMapVisualObjectAtGrid(gridPos);
            if (wallObject == null)
            {
                return;
            }

            WallFeedback wallFeedback = wallObject.GetComponent<WallFeedback>();
            if (wallFeedback != null)
            {
                wallFeedback.PlayHardWallBlockedFeedback(explosionWorldPosition);
            }
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

        public void RegisterSoftWallObject(Vector2Int gridPos, GameObject softWallObject)
        {
            if (softWallObject == null)
            {
                return;
            }

            softWallObjects[gridPos] = softWallObject;
        }

        public void RebuildSoftWallObjectLookup()
        {
            softWallObjects.Clear();

            Transform root = mapVisualRoot != null ? mapVisualRoot : transform;
            RebuildSoftWallObjectLookupRecursive(root);
            if (mapGenerator != null && mapGenerator.GeneratedMapRoot != null && mapGenerator.GeneratedMapRoot != root)
            {
                RebuildSoftWallObjectLookupRecursive(mapGenerator.GeneratedMapRoot);
            }
        }

        private void RebuildSoftWallObjectLookupRecursive(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Vector2Int rootGridPosition = WorldToGrid(root.position);
            GridCell rootCell = GetCell(rootGridPosition);
            if (rootCell != null && rootCell.IsSoftWall && IsSoftWallVisualObject(root.gameObject))
            {
                RegisterSoftWallObject(rootGridPosition, root.gameObject);
            }

            for (int i = 0; i < root.childCount; i++)
            {
                RebuildSoftWallObjectLookupRecursive(root.GetChild(i));
            }
        }

        private void RemoveSoftWallObject(Vector2Int gridPos)
        {
            if (!softWallObjects.TryGetValue(gridPos, out GameObject softWallObject) || softWallObject == null)
            {
                RebuildSoftWallObjectLookup();
                softWallObjects.TryGetValue(gridPos, out softWallObject);
            }

            if (softWallObject == null)
            {
                softWallObject = FindMapVisualObjectAtGrid(gridPos);
            }

            softWallObjects.Remove(gridPos);

            if (softWallObject == null)
            {
                return;
            }

            WallFeedback wallFeedback = softWallObject.GetComponent<WallFeedback>();
            if (Application.isPlaying && wallFeedback != null)
            {
                wallFeedback.PlaySoftWallDestroyedFeedback();
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(softWallObject);
            }
            else
            {
                DestroyImmediate(softWallObject);
            }
        }

        private GameObject FindMapVisualObjectAtGrid(Vector2Int gridPos)
        {
            if (mapGenerator != null && mapGenerator.GeneratedMapRoot != null)
            {
                GameObject generatedObject = FindMapVisualObjectAtGridRecursive(mapGenerator.GeneratedMapRoot, gridPos);
                if (generatedObject != null)
                {
                    return generatedObject;
                }
            }

            Transform root = mapVisualRoot != null ? mapVisualRoot : transform;
            return FindMapVisualObjectAtGridRecursive(root, gridPos);
        }

        private GameObject FindMapVisualObjectAtGridRecursive(Transform root, Vector2Int gridPos)
        {
            if (root == null)
            {
                return null;
            }

            if (WorldToGrid(root.position) == gridPos && IsWallVisualObject(root.gameObject))
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject childMatch = FindMapVisualObjectAtGridRecursive(root.GetChild(i), gridPos);
                if (childMatch != null)
                {
                    return childMatch;
                }
            }

            return null;
        }

        private bool IsWallVisualObject(GameObject visualObject)
        {
            return visualObject != null &&
                   (visualObject.GetComponent<WallFeedback>() != null || visualObject.name.StartsWith("Wall_"));
        }

        private bool IsSoftWallVisualObject(GameObject visualObject)
        {
            return visualObject != null &&
                   (visualObject.name.StartsWith("Wall_Soft") || visualObject.GetComponent<WallFeedback>() != null);
        }

        public void GenerateMap()
        {
            if (mapGenerator == null)
            {
                return;
            }

            mapGenerator.Generate(selectedMapType, this);
            RebuildSoftWallObjectLookup();
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

            softWallObjects.Clear();
        }
    }
}
