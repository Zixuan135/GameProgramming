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
        private Vector2Int singlePlayerGoalGrid = new Vector2Int(1, 1);
        private readonly Dictionary<Vector2Int, GameObject> softWallObjects = new Dictionary<Vector2Int, GameObject>();

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            InitializeGridData();
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            GenerateMap();
            RebuildSoftWallObjectLookup();
        }

        /// <summary>
        /// Purpose: Sets map type.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        public void SetMapType(BattleMapType mapType)
        {
            selectedMapType = mapType;
        }

        /// <summary>
        /// Purpose: Builds the logical grid data used by movement, bombs, items, and map visuals.
        /// Inputs: no direct parameters; reads map size settings and optional MapGenerator dimensions.
        /// Output: no return value; creates the GridCell array and applies the active map rules.
        /// </summary>
        public void InitializeGridData()
        {
            if (useMapGeneratorSize && mapGenerator != null)
            {
                // Keep data and visuals in sync when the visual generator owns the arena dimensions.
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
        /// Purpose: Applies all lightweight Bomberman-style map rules after the empty grid is created.
        /// Inputs: no direct parameters; reads selected map type and current game mode.
        /// Output: no return value; marks hard walls, soft walls, spawns, and the Solo route objective.
        /// </summary>
        private void ApplyMapRules()
        {
            // Order matters: later rules may clear cells reserved by earlier layout rules.
            ApplyHardWallBorder();
            ApplyInitialBlockingCells();
            ApplySelectedMapLayout();
            ResolveSpawnPointsByMode();
            ReserveSpawnArea(player1SpawnGrid);
            ReserveSpawnArea(player2SpawnGrid);
            ReserveSpawnArea(aiSpawnGrid);
            ApplySinglePlayerRouteObjectiveRules();
        }

        /// <summary>
        /// Purpose: Applies selected map layout to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies candy park layout to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyCandyParkLayout()
        {
            ApplyClassicHardWallPillars();
            ApplyPatternedSoftWalls(3, 1);
        }

        /// <summary>
        /// Purpose: Applies open field layout to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyOpenFieldLayout()
        {
            ApplyOpenFieldHardWallIslands();
            ApplyPatternedSoftWalls(5, 2);
        }

        /// <summary>
        /// Purpose: Applies maze layout to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyMazeLayout()
        {
            ApplyClassicHardWallPillars();
            ApplyPatternedSoftWalls(2, 0);
        }

        /// <summary>
        /// Purpose: Applies classic hard wall pillars to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies open field hard wall islands to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyOpenFieldHardWallIslands()
        {
            if (mapWidth < 5 || mapHeight < 5)
            {
                return;
            }

            int leftX = Mathf.Clamp(Mathf.RoundToInt((mapWidth - 1) * 0.25f), 2, mapWidth - 3);
            int centerX = Mathf.Clamp((mapWidth - 1) / 2, 2, mapWidth - 3);
            int rightX = Mathf.Clamp(Mathf.RoundToInt((mapWidth - 1) * 0.75f), 2, mapWidth - 3);
            int lowerY = Mathf.Clamp(Mathf.RoundToInt((mapHeight - 1) * 0.3f), 2, mapHeight - 3);
            int centerY = Mathf.Clamp((mapHeight - 1) / 2, 2, mapHeight - 3);
            int upperY = Mathf.Clamp(Mathf.RoundToInt((mapHeight - 1) * 0.7f), 2, mapHeight - 3);

            SetInteriorHardWall(new Vector2Int(leftX, lowerY));
            SetInteriorHardWall(new Vector2Int(leftX, upperY));
            SetInteriorHardWall(new Vector2Int(centerX, centerY));
            SetInteriorHardWall(new Vector2Int(rightX, lowerY));
            SetInteriorHardWall(new Vector2Int(rightX, upperY));
        }

        /// <summary>
        /// Purpose: Sets interior hard wall.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        private void SetInteriorHardWall(Vector2Int gridPosition)
        {
            if (IsBorderCell(gridPosition))
            {
                return;
            }

            SetHardWall(gridPosition, true);
        }

        /// <summary>
        /// Purpose: Applies patterned soft walls to the current object or scene.
        /// Inputs: `interval`, `offset`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="interval">Input value used by this method.</param>
        /// <param name="offset">Input value used by this method.</param>
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
                    // Prime-like weights create a repeatable pattern without needing authored level data.
                    if (patternValue % safeInterval == 0)
                    {
                        SetSoftWall(gridPosition, true);
                    }
                }
            }
        }

        /// <summary>
        /// Purpose: Applies hard wall border to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies initial blocking cells to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Resolves spawn points by mode from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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
            singlePlayerGoalGrid = topRight;

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

        /// <summary>
        /// Purpose: Performs reserve spawn area for this component.
        /// Inputs: `center`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="center">Input value used by this method.</param>
        private void ReserveSpawnArea(Vector2Int center)
        {
            ClearBlockingAt(center);
            ClearBlockingAt(center + Vector2Int.right);
            ClearBlockingAt(center + Vector2Int.left);
            ClearBlockingAt(center + Vector2Int.up);
            ClearBlockingAt(center + Vector2Int.down);
        }

        /// <summary>
        /// Purpose: Applies single player route objective rules to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplySinglePlayerRouteObjectiveRules()
        {
            singlePlayerGoalGrid = new Vector2Int(mapWidth - 2, mapHeight - 2);
            if (ResolveCurrentGameMode() != GameMode.SinglePlayer)
            {
                return;
            }

            ClearBlockingAt(singlePlayerGoalGrid);

            Vector2Int leftGate = singlePlayerGoalGrid + Vector2Int.left;
            Vector2Int lowerGate = singlePlayerGoalGrid + Vector2Int.down;
            Vector2Int leftApproach = leftGate + Vector2Int.left;
            Vector2Int lowerApproach = lowerGate + Vector2Int.down;

            // The goal is always reachable after the player breaks at least one nearby soft-wall gate.
            ClearBlockingAt(leftApproach);
            ClearBlockingAt(lowerApproach);
            SetSoftWallGate(leftGate);
            SetSoftWallGate(lowerGate);
        }

        /// <summary>
        /// Purpose: Resolves current game mode from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `GameMode` value.
        /// </summary>
        /// <returns>a `GameMode` value.</returns>
        private GameMode ResolveCurrentGameMode()
        {
            return GameManager.Instance != null
                ? GameManager.Instance.CurrentGameMode
                : GameMode.SinglePlayer;
        }

        /// <summary>
        /// Purpose: Sets soft wall gate.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        private void SetSoftWallGate(Vector2Int gridPos)
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
            cell.IsSoftWall = true;
        }

        /// <summary>
        /// Purpose: Clears blocking at.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Returns whether this object is border cell.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsBorderCell(Vector2Int gridPos)
        {
            return gridPos.x == 0 || gridPos.x == mapWidth - 1 ||
                   gridPos.y == 0 || gridPos.y == mapHeight - 1;
        }

        /// <summary>
        /// Purpose: Sets hard wall.
        /// Inputs: `gridPos`, `isHardWall`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="isHardWall">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Sets soft wall.
        /// Inputs: `gridPos`, `isSoftWall`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="isSoftWall">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Destroys soft wall.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Performs notify soft wall destroyed for this component.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        private void NotifySoftWallDestroyed(Vector2Int gridPos)
        {
            SoftWallDestroyed?.Invoke(gridPos);
        }

        /// <summary>
        /// Purpose: Gets cell.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `GridCell` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `GridCell` value.</returns>
        public GridCell GetCell(Vector2Int gridPos)
        {
            if (!IsInsideBounds(gridPos))
            {
                return null;
            }

            return grid[gridPos.x, gridPos.y];
        }

        /// <summary>
        /// Purpose: Returns whether this object is inside bounds.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool IsInsideBounds(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < mapWidth &&
                   gridPos.y >= 0 && gridPos.y < mapHeight;
        }

        /// <summary>
        /// Purpose: Returns whether this object is walkable.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

            // Character occupancy prevents two actors from reserving the same grid cell.
            if (IsOccupiedByCharacter(cell))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Purpose: Returns whether this object is blocked by wall.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool IsBlockedByWall(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            return cell == null || IsBlockedByWall(cell);
        }

        /// <summary>
        /// Purpose: Returns whether this object is blocked by bomb.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool IsBlockedByBomb(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            return cell != null && IsBlockedByBomb(cell);
        }

        /// <summary>
        /// Purpose: Returns whether this object is blocked by wall.
        /// Inputs: `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsBlockedByWall(GridCell cell)
        {
            return cell.IsHardWall || cell.IsSoftWall;
        }

        /// <summary>
        /// Purpose: Returns whether this object is blocked by bomb.
        /// Inputs: `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsBlockedByBomb(GridCell cell)
        {
            // Bombs block movement for now; later this can support owner grace rules.
            return cell.HasBomb;
        }

        /// <summary>
        /// Purpose: Returns whether this object is occupied by character.
        /// Inputs: `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsOccupiedByCharacter(GridCell cell)
        {
            return cell.HasCharacter;
        }

        /// <summary>
        /// Purpose: Sets character.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Clears character.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        public void ClearCharacter(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.HasCharacter = false;
        }

        /// <summary>
        /// Purpose: Returns place bomb for the current state.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Performs remove bomb for this component.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        public void RemoveBomb(Vector2Int gridPos)
        {
            GridCell cell = GetCell(gridPos);
            if (cell == null)
            {
                return;
            }

            cell.HasBomb = false;
        }

        /// <summary>
        /// Purpose: Plays hard wall blocked feedback.
        /// Inputs: `gridPos`, `explosionWorldPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="explosionWorldPosition">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Sets item.
        /// Inputs: `gridPos`, `hasItem`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="hasItem">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Gets player1 spawn grid.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <returns>a `Vector2Int` value.</returns>
        public Vector2Int GetPlayer1SpawnGrid() => player1SpawnGrid;
        /// <summary>
        /// Purpose: Gets player2 spawn grid.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <returns>a `Vector2Int` value.</returns>
        public Vector2Int GetPlayer2SpawnGrid() => player2SpawnGrid;
        /// <summary>
        /// Purpose: Gets aispawn grid.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <returns>a `Vector2Int` value.</returns>
        public Vector2Int GetAISpawnGrid() => aiSpawnGrid;
        /// <summary>
        /// Purpose: Gets single player goal grid.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <returns>a `Vector2Int` value.</returns>
        public Vector2Int GetSinglePlayerGoalGrid() => singlePlayerGoalGrid;

        /// <summary>
        /// Purpose: Returns whether this object is single player goal.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool IsSinglePlayerGoal(Vector2Int gridPos)
        {
            return gridPos == singlePlayerGoalGrid;
        }

        /// <summary>
        /// Purpose: Returns count soft walls for the current state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `int` value.
        /// </summary>
        /// <returns>a `int` value.</returns>
        public int CountSoftWalls()
        {
            if (grid == null)
            {
                return 0;
            }

            int count = 0;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    GridCell cell = grid[x, y];
                    if (cell != null && cell.IsSoftWall)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Purpose: Returns world to grid for the current state.
        /// Inputs: `worldPosition`; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <param name="worldPosition">Input value used by this method.</param>
        /// <returns>a `Vector2Int` value.</returns>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / cellSize);
            int y = Mathf.RoundToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Purpose: Returns grid to world for the current state.
        /// Inputs: `gridPos`, `y`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="y">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        public Vector3 GridToWorld(Vector2Int gridPos, float y = 0f)
        {
            return new Vector3(gridPos.x * cellSize, y, gridPos.y * cellSize);
        }

        /// <summary>
        /// Purpose: Registers soft wall object in the relevant runtime system.
        /// Inputs: `gridPos`, `softWallObject`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <param name="softWallObject">Input value used by this method.</param>
        public void RegisterSoftWallObject(Vector2Int gridPos, GameObject softWallObject)
        {
            if (softWallObject == null)
            {
                return;
            }

            softWallObjects[gridPos] = softWallObject;
        }

        /// <summary>
        /// Purpose: Performs rebuild soft wall object lookup for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Performs rebuild soft wall object lookup recursive for this component.
        /// Inputs: `root`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="root">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Performs remove soft wall object for this component.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Finds map visual object at grid from scene objects or cached data.
        /// Inputs: `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Finds map visual object at grid recursive from scene objects or cached data.
        /// Inputs: `root`, `gridPos`; may also read serialized fields and current runtime state.
        /// Output: a `GameObject` value.
        /// </summary>
        /// <param name="root">Input value used by this method.</param>
        /// <param name="gridPos">Input value used by this method.</param>
        /// <returns>a `GameObject` value.</returns>
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

        /// <summary>
        /// Purpose: Returns whether this object is wall visual object.
        /// Inputs: `visualObject`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="visualObject">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsWallVisualObject(GameObject visualObject)
        {
            return visualObject != null &&
                   (visualObject.GetComponent<WallFeedback>() != null || visualObject.name.StartsWith("Wall_"));
        }

        /// <summary>
        /// Purpose: Returns whether this object is soft wall visual object.
        /// Inputs: `visualObject`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="visualObject">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsSoftWallVisualObject(GameObject visualObject)
        {
            return visualObject != null &&
                   (visualObject.name.StartsWith("Wall_Soft") || visualObject.GetComponent<WallFeedback>() != null);
        }

        /// <summary>
        /// Purpose: Performs generate map for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void GenerateMap()
        {
            if (mapGenerator == null)
            {
                return;
            }

            mapGenerator.Generate(selectedMapType, this);
            RebuildSoftWallObjectLookup();
        }

        /// <summary>
        /// Purpose: Clears map.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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
