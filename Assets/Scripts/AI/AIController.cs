using System;
using System.Collections.Generic;
using BubbleTown.Characters;
using BubbleTown.Core.Enums;
using BubbleTown.Gameplay;
using BubbleTown.Managers;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.AI
{
    /// <summary>
    /// First-pass AI controller for grid movement, danger avoidance, and simple bomb placement.
    /// It reuses CharacterBase movement and bomb APIs so AI follows the same rules as players.
    /// </summary>
    public class AIController : CharacterBase
    {
        private static readonly Vector2Int[] MoveDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        [Header("AI Movement")]
        [SerializeField, Min(0.05f)] private float moveDecisionInterval = 0.35f;
        [SerializeField, Min(0.05f)] private float failedMoveRetryDelay = 0.15f;
        [SerializeField, Range(0f, 1f)] private float idleChance = 0.12f;
        [SerializeField, Range(0f, 1f)] private float avoidImmediateReverseChance = 0.7f;
        [SerializeField, Min(1)] private int maxFailedMoveAttemptsBeforePause = 3;

        [Header("AI Danger Avoidance")]
        [SerializeField] private bool enableDangerAvoidance = true;
        [SerializeField, Min(1)] private int escapeSearchDepth = 6;
        [SerializeField, Min(0.01f)] private float dangerMoveRetryDelay = 0.05f;

        [Header("AI Strategy")]
        [SerializeField] private bool enableStrategicMovement = true;
        [SerializeField, Min(1)] private int playerChaseDistance = 7;
        [SerializeField, Min(1)] private int softWallTargetSearchDepth = 8;
        [SerializeField, Min(0.1f)] private float strategicMoveRetryDelay = 0.18f;
        [SerializeField, Min(0f)] private float postBombEscapeSeconds = 1.4f;

        [Header("AI Safety Scoring")]
        [SerializeField, Min(0)] private int minimumSafeNeighborsAfterBomb = 1;
        [SerializeField, Min(0f)] private float safeNeighborScoreWeight = 4f;
        [SerializeField, Min(0f)] private float bombDistanceScoreWeight = 1.2f;
        [SerializeField, Min(0f)] private float escapeDepthPenalty = 0.35f;

        [Header("AI Bomb Logic")]
        [SerializeField] private bool enableBombPlacement = true;
        [SerializeField, Min(0.25f)] private float bombDecisionInterval = 2f;
        [SerializeField, Min(0.01f)] private float urgentBombDecisionDelay = 0.12f;
        [SerializeField, Range(0f, 1f)] private float bombPlacementChance = 0.45f;
        [SerializeField, Range(0f, 1f)] private float softWallBombPlacementChance = 0.72f;
        [SerializeField, Range(0f, 1f)] private float adjacentSoftWallBombPlacementChance = 0.9f;
        [SerializeField, Range(0f, 1f)] private float playerAttackBombPlacementChance = 0.85f;
        [SerializeField, Min(1)] private int softWallCheckDistance = 1;
        [SerializeField] private bool requireEscapeRouteBeforeBomb = true;
        [SerializeField, Min(1)] private int bombEscapeSearchDepth = 6;

        [Header("AI Difficulty")]
        [SerializeField] private AIDifficulty currentDifficulty = AIDifficulty.Normal;

        [Header("AI Runtime")]
        [SerializeField] private Vector2Int currentMoveDirection = Vector2Int.zero;
        [SerializeField] private int failedMoveAttempts;
        [SerializeField] private bool currentCellDangerous;
        [SerializeField] private float postBombEscapeTimer;
        [SerializeField] private Vector2Int currentStrategicTarget = Vector2Int.zero;
        [SerializeField] private string currentIntent = "Idle";

        [Header("AI Debug")]
        [SerializeField] private bool logDecisionDebug;

        private float moveDecisionTimer;
        private float bombTimer;

        public string CurrentIntent => currentIntent;
        public Vector2Int CurrentStrategicTarget => currentStrategicTarget;
        public AIDifficulty CurrentDifficulty => currentDifficulty;

        /// <summary>
        /// Bundles one difficulty profile so AI tuning can be copied into serialized behavior fields together.
        /// </summary>
        private struct DifficultyPreset
        {
            public float MoveDecisionInterval;
            public float FailedMoveRetryDelay;
            public float IdleChance;
            public float AvoidReverseChance;
            public int EscapeSearchDepth;
            public int PlayerChaseDistance;
            public int SoftWallSearchDepth;
            public float StrategicRetryDelay;
            public float PostBombEscapeSeconds;
            public float BombDecisionInterval;
            public float UrgentBombDecisionDelay;
            public float BombPlacementChance;
            public float SoftWallBombChance;
            public float AdjacentSoftWallBombChance;
            public float PlayerAttackBombChance;
            public int SoftWallCheckDistance;
            public int BombEscapeSearchDepth;
            public int MinimumSafeNeighbors;

            /// <summary>
            /// Purpose: Packages all tunable AI behavior numbers for one difficulty preset.
            /// Inputs: every argument maps directly to a serialized AIController behavior field.
            /// Output: returns a DifficultyPreset value that can be applied to the controller.
            /// </summary>
            public DifficultyPreset(
                float moveDecisionInterval,
                float failedMoveRetryDelay,
                float idleChance,
                float avoidReverseChance,
                int escapeSearchDepth,
                int playerChaseDistance,
                int softWallSearchDepth,
                float strategicRetryDelay,
                float postBombEscapeSeconds,
                float bombDecisionInterval,
                float urgentBombDecisionDelay,
                float bombPlacementChance,
                float softWallBombChance,
                float adjacentSoftWallBombChance,
                float playerAttackBombChance,
                int softWallCheckDistance,
                int bombEscapeSearchDepth,
                int minimumSafeNeighbors)
            {
                MoveDecisionInterval = moveDecisionInterval;
                FailedMoveRetryDelay = failedMoveRetryDelay;
                IdleChance = idleChance;
                AvoidReverseChance = avoidReverseChance;
                EscapeSearchDepth = escapeSearchDepth;
                PlayerChaseDistance = playerChaseDistance;
                SoftWallSearchDepth = softWallSearchDepth;
                StrategicRetryDelay = strategicRetryDelay;
                PostBombEscapeSeconds = postBombEscapeSeconds;
                BombDecisionInterval = bombDecisionInterval;
                UrgentBombDecisionDelay = urgentBombDecisionDelay;
                BombPlacementChance = bombPlacementChance;
                SoftWallBombChance = softWallBombChance;
                AdjacentSoftWallBombChance = adjacentSoftWallBombChance;
                PlayerAttackBombChance = playerAttackBombChance;
                SoftWallCheckDistance = softWallCheckDistance;
                BombEscapeSearchDepth = bombEscapeSearchDepth;
                MinimumSafeNeighbors = minimumSafeNeighbors;
            }
        }

        /// <summary>
        /// Stores one breadth-first-search node while the AI looks for a safe movement route.
        /// </summary>
        private struct EscapeSearchNode
        {
            public Vector2Int Position;
            public Vector2Int FirstDirection;
            public int Depth;

            /// <summary>
            /// Purpose: Stores one node used by AI breadth-first searches.
            /// Inputs: position is the searched cell, firstDirection is the first step from the AI, and depth is distance from start.
            /// Output: returns an EscapeSearchNode value used only inside the AI search queue.
            /// </summary>
            /// <param name="position">Grid cell represented by this search node.</param>
            /// <param name="firstDirection">First movement step from the AI's start cell toward this node.</param>
            /// <param name="depth">Number of grid steps from the AI's start cell.</param>
            public EscapeSearchNode(Vector2Int position, Vector2Int firstDirection, int depth)
            {
                Position = position;
                FirstDirection = firstDirection;
                Depth = depth;
            }
        }

        /// <summary>
        /// Purpose: Runs this component's per-frame logic.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (!IsAlive)
            {
                return;
            }

            if (!CanActDuringBattle())
            {
                return;
            }

            currentCellDangerous = enableDangerAvoidance && IsGridDangerous(CurrentGridPosition);
            TickPostBombEscapeTimer();
            UpdateBombDecision();
            UpdateMovementDecision();
        }

        /// <summary>
        /// Purpose: Configures for battle for the current battle or scene.
        /// Inputs: `newMapManager`, `spawnGridPosition`, `newBombSpawnRoot`, `newBombPrefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newMapManager">Input value used by this method.</param>
        /// <param name="spawnGridPosition">Input value used by this method.</param>
        /// <param name="newBombSpawnRoot">Input value used by this method.</param>
        /// <param name="newBombPrefab">Input value used by this method.</param>
        public override void ConfigureForBattle(
            MapManager newMapManager,
            Vector2Int spawnGridPosition,
            Transform newBombSpawnRoot,
            BombController newBombPrefab = null)
        {
            ApplyDifficultyPreset(currentDifficulty);
            base.ConfigureForBattle(newMapManager, spawnGridPosition, newBombSpawnRoot, newBombPrefab);
            ResetAIState();
        }

        /// <summary>
        /// Purpose: Applies the selected AI difficulty to movement, attack, and escape settings.
        /// Inputs: difficulty is the preset chosen by the player before AI Battle.
        /// Output: no return value; updates AI tuning fields and refreshes runtime decision timers.
        /// </summary>
        /// <param name="difficulty">AI difficulty preset to apply.</param>
        public void ConfigureDifficulty(AIDifficulty difficulty)
        {
            currentDifficulty = difficulty;
            ApplyDifficultyPreset(currentDifficulty);
            ResetAIState();
        }

        /// <summary>
        /// Purpose: Returns whether this object can act during battle.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool CanActDuringBattle()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null ||
                   (gameManager.CurrentGameState == GameState.BattleRunning && !gameManager.IsBattlePaused);
        }

        /// <summary>
        /// Purpose: Updates movement decision.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void UpdateMovementDecision()
        {
            if (IsMoving)
            {
                return;
            }

            if (currentCellDangerous || postBombEscapeTimer > 0f)
            {
                if (moveDecisionTimer > dangerMoveRetryDelay)
                {
                    moveDecisionTimer = 0f;
                }

                moveDecisionTimer -= Time.deltaTime;
                if (moveDecisionTimer > 0f)
                {
                    return;
                }

                TryMoveTowardSafety();
                return;
            }

            moveDecisionTimer -= Time.deltaTime;
            if (moveDecisionTimer > 0f)
            {
                return;
            }

            if (enableStrategicMovement && TryMoveStrategicDirection())
            {
                return;
            }

            TryMoveRandomDirection();
        }

        /// <summary>
        /// Purpose: Attempts to move toward safety.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void TryMoveTowardSafety()
        {
            if (TryFindEscapeDirection(IsGridDangerous, escapeSearchDepth, false, Vector2Int.zero, out Vector2Int escapeDirection))
            {
                TryMoveOrRegisterFailure(escapeDirection, dangerMoveRetryDelay);
                return;
            }

            if (TryPickWalkableDirection(out Vector2Int fallbackDirection, false))
            {
                TryMoveOrRegisterFailure(fallbackDirection, dangerMoveRetryDelay);
                return;
            }

            RegisterFailedMove(dangerMoveRetryDelay);
        }

        /// <summary>
        /// Purpose: Attempts to move strategic direction.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool TryMoveStrategicDirection()
        {
            if (TryFindPlayerAttackPositionDirection(out Vector2Int playerDirection, out Vector2Int playerTarget))
            {
                currentStrategicTarget = playerTarget;
                currentIntent = "Set Player Trap";
                LogDecision($"Moving toward safe player attack position via {playerDirection}. Target: {playerTarget}");
                TryMoveOrRegisterFailure(playerDirection, strategicMoveRetryDelay);
                return true;
            }

            if (TryFindPlayerChaseDirection(out Vector2Int chaseDirection, out Vector2Int chaseTarget))
            {
                currentStrategicTarget = chaseTarget;
                currentIntent = "Chase Player";
                LogDecision($"Closing distance to player via {chaseDirection}. Target: {chaseTarget}");
                TryMoveOrRegisterFailure(chaseDirection, strategicMoveRetryDelay);
                return true;
            }

            if (TryFindSoftWallApproachDirection(out Vector2Int softWallDirection, out Vector2Int softWallTarget))
            {
                currentStrategicTarget = softWallTarget;
                currentIntent = "Seek Soft Wall";
                LogDecision($"Moving toward soft wall approach via {softWallDirection}. Target: {softWallTarget}");
                TryMoveOrRegisterFailure(softWallDirection, strategicMoveRetryDelay);
                return true;
            }

            currentIntent = "Patrol";
            currentStrategicTarget = Vector2Int.zero;
            return false;
        }

        /// <summary>
        /// Purpose: Attempts to move random direction.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void TryMoveRandomDirection()
        {
            if (UnityEngine.Random.value < idleChance)
            {
                currentIntent = "Idle";
                ScheduleNextMoveDecision(moveDecisionInterval);
                return;
            }

            if (!TryPickWalkableDirection(out Vector2Int nextDirection, true))
            {
                RegisterFailedMove(failedMoveRetryDelay);
                return;
            }

            currentIntent = "Patrol";
            TryMoveOrRegisterFailure(nextDirection, moveDecisionInterval);
        }

        /// <summary>
        /// Purpose: Attempts to move or register failure.
        /// Inputs: `direction`, `nextDecisionDelay`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="direction">Input value used by this method.</param>
        /// <param name="nextDecisionDelay">Input value used by this method.</param>
        private void TryMoveOrRegisterFailure(Vector2Int direction, float nextDecisionDelay)
        {
            bool moved = TryMoveGridDirection(direction);
            if (!moved)
            {
                RegisterFailedMove(failedMoveRetryDelay);
                return;
            }

            currentMoveDirection = direction;
            failedMoveAttempts = 0;
            ScheduleNextMoveDecision(nextDecisionDelay);
        }

        /// <summary>
        /// Purpose: Attempts to pick walkable direction.
        /// Inputs: `direction`, `avoidDangerousCells`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="direction">Input value used by this method.</param>
        /// <param name="avoidDangerousCells">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool TryPickWalkableDirection(out Vector2Int direction, bool avoidDangerousCells)
        {
            direction = Vector2Int.zero;
            Vector2Int[] candidates = new Vector2Int[MoveDirections.Length];
            int candidateCount = 0;

            int startIndex = UnityEngine.Random.Range(0, MoveDirections.Length);
            for (int i = 0; i < MoveDirections.Length; i++)
            {
                Vector2Int candidateDirection = MoveDirections[(startIndex + i) % MoveDirections.Length];
                if (ShouldSkipImmediateReverse(candidateDirection))
                {
                    continue;
                }

                Vector2Int candidateGridPosition = CurrentGridPosition + candidateDirection;
                if (!IsGridWalkable(candidateGridPosition))
                {
                    continue;
                }

                if (avoidDangerousCells && IsGridDangerous(candidateGridPosition))
                {
                    continue;
                }

                candidates[candidateCount] = candidateDirection;
                candidateCount++;
            }

            if (candidateCount == 0 && currentMoveDirection != Vector2Int.zero)
            {
                Vector2Int reverseDirection = -currentMoveDirection;
                Vector2Int reverseGridPosition = CurrentGridPosition + reverseDirection;
                if (IsGridWalkable(reverseGridPosition) && (!avoidDangerousCells || !IsGridDangerous(reverseGridPosition)))
                {
                    direction = reverseDirection;
                    return true;
                }
            }

            if (candidateCount == 0)
            {
                return false;
            }

            direction = candidates[UnityEngine.Random.Range(0, candidateCount)];
            return true;
        }

        /// <summary>
        /// Purpose: Returns whether this object should skip immediate reverse.
        /// Inputs: `candidateDirection`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="candidateDirection">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool ShouldSkipImmediateReverse(Vector2Int candidateDirection)
        {
            if (currentMoveDirection == Vector2Int.zero)
            {
                return false;
            }

            if (candidateDirection != -currentMoveDirection)
            {
                return false;
            }

            return UnityEngine.Random.value < avoidImmediateReverseChance;
        }

        /// <summary>
        /// Purpose: Returns whether this object is grid walkable.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsGridWalkable(Vector2Int gridPosition)
        {
            return MapManager != null && MapManager.IsWalkable(gridPosition);
        }

        /// <summary>
        /// Purpose: Registers failed move in the relevant runtime system.
        /// Inputs: `retryDelay`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="retryDelay">Input value used by this method.</param>
        private void RegisterFailedMove(float retryDelay)
        {
            failedMoveAttempts++;
            if (failedMoveAttempts >= maxFailedMoveAttemptsBeforePause)
            {
                currentMoveDirection = Vector2Int.zero;
                failedMoveAttempts = 0;
                ScheduleNextMoveDecision(moveDecisionInterval);
                return;
            }

            ScheduleNextMoveDecision(retryDelay);
        }

        /// <summary>
        /// Purpose: Performs schedule next move decision for this component.
        /// Inputs: `delay`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="delay">Input value used by this method.</param>
        private void ScheduleNextMoveDecision(float delay)
        {
            moveDecisionTimer = Mathf.Max(0.01f, delay);
        }

        /// <summary>
        /// Purpose: Updates bomb decision.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void UpdateBombDecision()
        {
            if (!enableBombPlacement || IsMoving || postBombEscapeTimer > 0f)
            {
                return;
            }

            bombTimer -= Time.deltaTime;
            if (bombTimer > 0f)
            {
                if (HasImmediateBombOpportunity())
                {
                    bombTimer = Mathf.Min(bombTimer, urgentBombDecisionDelay);
                }

                return;
            }

            bombTimer = bombDecisionInterval;
            if (!CanPlaceMoreBombs() || currentCellDangerous)
            {
                return;
            }

            if (!TryEvaluateBombPlacementOpportunity(out float placementChance, out string reason))
            {
                return;
            }

            if (UnityEngine.Random.value > placementChance)
            {
                LogDecision($"Skipped bomb chance. Reason: {reason}, Chance: {placementChance:0.00}");
                return;
            }

            if (requireEscapeRouteBeforeBomb && !CanReachSafetyAfterPlacingBomb())
            {
                LogDecision($"Skipped bomb because no escape route was found. Reason: {reason}");
                return;
            }

            if (TryPlaceBomb())
            {
                currentCellDangerous = true;
                postBombEscapeTimer = postBombEscapeSeconds;
                moveDecisionTimer = 0f;
                currentIntent = "Escape After Bomb";
                LogDecision($"Placed bomb. Reason: {reason}. Escape timer: {postBombEscapeTimer:0.00}s");
            }
        }

        /// <summary>
        /// Purpose: Returns whether this object has immediate bomb opportunity.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool HasImmediateBombOpportunity()
        {
            return HasAdjacentSoftWall(CurrentGridPosition) || IsPlayerInBombLine();
        }

        /// <summary>
        /// Purpose: Attempts to evaluate bomb placement opportunity.
        /// Inputs: `placementChance`, `reason`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="placementChance">Input value used by this method.</param>
        /// <param name="reason">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool TryEvaluateBombPlacementOpportunity(out float placementChance, out string reason)
        {
            placementChance = bombPlacementChance;
            reason = string.Empty;

            if (IsPlayerInBombLine())
            {
                placementChance = Mathf.Max(placementChance, playerAttackBombPlacementChance);
                reason = "Player in blast line";
                return true;
            }

            if (HasAdjacentSoftWall(CurrentGridPosition))
            {
                placementChance = Mathf.Max(placementChance, adjacentSoftWallBombPlacementChance);
                reason = "Adjacent soft wall";
                return true;
            }

            if (IsNearSoftWall())
            {
                placementChance = Mathf.Max(placementChance, softWallBombPlacementChance);
                reason = "Soft wall nearby";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Purpose: Returns whether this object is near soft wall.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool IsNearSoftWall()
        {
            if (MapManager == null)
            {
                return false;
            }

            int checkDistance = Mathf.Min(softWallCheckDistance, BombRange);
            for (int i = 0; i < MoveDirections.Length; i++)
            {
                Vector2Int direction = MoveDirections[i];
                for (int distance = 1; distance <= checkDistance; distance++)
                {
                    Vector2Int targetGridPosition = CurrentGridPosition + direction * distance;
                    GridCell cell = MapManager.GetCell(targetGridPosition);
                    if (cell == null || cell.IsHardWall)
                    {
                        break;
                    }

                    if (cell.IsSoftWall)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Returns whether this object has adjacent soft wall.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool HasAdjacentSoftWall(Vector2Int gridPosition)
        {
            if (MapManager == null)
            {
                return false;
            }

            for (int i = 0; i < MoveDirections.Length; i++)
            {
                GridCell cell = MapManager.GetCell(gridPosition + MoveDirections[i]);
                if (cell != null && cell.IsSoftWall)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Returns whether this object is player in bomb line.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool IsPlayerInBombLine()
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController player = players[i];
                if (player == null || !player.IsAlive || !player.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (IsGridInBombBlast(player.CurrentGridPosition, CurrentGridPosition, BombRange))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Finds the first step toward a cell where the AI can safely threaten the nearest player with a bomb.
        /// Inputs: no direct parameters; reads the nearest active player, map data, bomb range, and escape settings.
        /// Output: returns true when a safe attack route exists and outputs the first step plus the target attack cell.
        /// </summary>
        /// <param name="direction">First grid step toward the attack cell.</param>
        /// <param name="targetGridPosition">Reachable grid cell where a bomb could threaten the player and still leave an escape route.</param>
        /// <returns>True if a safe attack position was found; otherwise false.</returns>
        private bool TryFindPlayerAttackPositionDirection(out Vector2Int direction, out Vector2Int targetGridPosition)
        {
            direction = Vector2Int.zero;
            targetGridPosition = Vector2Int.zero;

            if (!TryFindNearestActivePlayer(out PlayerController nearestPlayer))
            {
                return false;
            }

            targetGridPosition = nearestPlayer.CurrentGridPosition;
            Vector2Int playerGridPosition = targetGridPosition;
            int distance = ManhattanDistance(CurrentGridPosition, playerGridPosition);
            if (distance > playerChaseDistance)
            {
                return false;
            }

            if (IsGridInBombBlast(playerGridPosition, CurrentGridPosition, BombRange))
            {
                return false;
            }

            return TryFindDirectionToGoal(
                position => position != CurrentGridPosition &&
                            IsGridInBombBlast(playerGridPosition, position, BombRange) &&
                            CanSafelyPlantBombAt(position),
                playerChaseDistance,
                true,
                out direction,
                out targetGridPosition);
        }

        /// <summary>
        /// Purpose: Finds a stable step that moves the AI closer to the nearest player when no safe bomb trap is ready.
        /// Inputs: no direct parameters; reads active players, map walkability, and danger information.
        /// Output: returns true and outputs a first step when the AI can reduce distance without entering obvious danger.
        /// </summary>
        /// <param name="direction">First grid step toward a closer player approach cell.</param>
        /// <param name="targetGridPosition">Reachable grid cell chosen as the current chase target.</param>
        /// <returns>True if a safer chase step was found; otherwise false.</returns>
        private bool TryFindPlayerChaseDirection(out Vector2Int direction, out Vector2Int targetGridPosition)
        {
            direction = Vector2Int.zero;
            targetGridPosition = Vector2Int.zero;

            if (!TryFindNearestActivePlayer(out PlayerController nearestPlayer))
            {
                return false;
            }

            Vector2Int playerGridPosition = nearestPlayer.CurrentGridPosition;
            int currentDistance = ManhattanDistance(CurrentGridPosition, playerGridPosition);
            if (currentDistance <= 1 || currentDistance > playerChaseDistance)
            {
                return false;
            }

            return TryFindDirectionToGoal(
                position => position != CurrentGridPosition &&
                            ManhattanDistance(position, playerGridPosition) < currentDistance &&
                            HasSafeMovementOptions(position),
                playerChaseDistance,
                true,
                out direction,
                out targetGridPosition);
        }

        /// <summary>
        /// Purpose: Attempts to find nearest active player.
        /// Inputs: `nearestPlayer`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="nearestPlayer">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool TryFindNearestActivePlayer(out PlayerController nearestPlayer)
        {
            nearestPlayer = null;
            int bestDistance = int.MaxValue;
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController player = players[i];
                if (player == null || !player.IsAlive || !player.gameObject.activeInHierarchy)
                {
                    continue;
                }

                int distance = ManhattanDistance(CurrentGridPosition, player.CurrentGridPosition);
                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                nearestPlayer = player;
            }

            return nearestPlayer != null;
        }

        /// <summary>
        /// Purpose: Finds the first step toward a soft-wall clearing position that still has a post-bomb escape route.
        /// Inputs: no direct parameters; reads map cells, bomb range, and configured search depth.
        /// Output: returns true when a safe soft-wall approach cell exists and outputs the first step plus target cell.
        /// </summary>
        /// <param name="direction">First grid step toward the soft-wall approach cell.</param>
        /// <param name="targetGridPosition">Reachable cell beside a soft wall where the AI can safely plant a bomb later.</param>
        /// <returns>True if a safe soft-wall approach was found; otherwise false.</returns>
        private bool TryFindSoftWallApproachDirection(out Vector2Int direction, out Vector2Int targetGridPosition)
        {
            direction = Vector2Int.zero;
            targetGridPosition = Vector2Int.zero;

            if (HasAdjacentSoftWall(CurrentGridPosition))
            {
                return false;
            }

            return TryFindDirectionToGoal(
                position => HasAdjacentSoftWall(position) && CanSafelyPlantBombAt(position),
                softWallTargetSearchDepth,
                true,
                out direction,
                out targetGridPosition);
        }

        /// <summary>
        /// Purpose: Checks whether the AI can escape if it plants a bomb on its current cell.
        /// Inputs: no direct parameters; reads the current grid position, bomb range, map data, and active danger.
        /// Output: returns true when a safe cell can be reached within the configured bomb escape search depth.
        /// </summary>
        /// <returns>True if the AI can escape its own bomb from the current cell; otherwise false.</returns>
        private bool CanReachSafetyAfterPlacingBomb()
        {
            return CanSafelyPlantBombAt(CurrentGridPosition);
        }

        /// <summary>
        /// Purpose: Checks whether a candidate cell is a safe place for the AI to plant a bomb later.
        /// Inputs: bombGridPosition is the candidate bomb cell; reads bomb range, map data, and current active danger.
        /// Output: returns true when the cell has a reachable escape path after the hypothetical bomb is placed.
        /// </summary>
        /// <param name="bombGridPosition">Candidate grid cell where the AI may plant a bomb.</param>
        /// <returns>True if the candidate bomb cell has a safe escape route; otherwise false.</returns>
        private bool CanSafelyPlantBombAt(Vector2Int bombGridPosition)
        {
            return TryFindEscapeDirectionFrom(
                bombGridPosition,
                gridPosition => IsGridDangerous(gridPosition) || IsGridInBombBlast(gridPosition, bombGridPosition, BombRange),
                bombEscapeSearchDepth,
                true,
                bombGridPosition,
                out _);
        }

        /// <summary>
        /// Purpose: Checks whether a movement target has enough nearby safe options to avoid obvious dead ends.
        /// Inputs: gridPosition is the candidate movement cell; reads map walkability and danger.
        /// Output: returns true when the cell is not dangerous and has at least one safe neighboring route.
        /// </summary>
        /// <param name="gridPosition">Candidate grid cell being evaluated for movement.</param>
        /// <returns>True if the cell is safe enough for strategic movement; otherwise false.</returns>
        private bool HasSafeMovementOptions(Vector2Int gridPosition)
        {
            return !IsGridDangerous(gridPosition) &&
                   CountSafeNeighborCells(gridPosition, IsGridDangerous, false, Vector2Int.zero) >= minimumSafeNeighborsAfterBomb;
        }

        /// <summary>
        /// Purpose: Searches nearby walkable cells and returns the first step toward a matching goal cell.
        /// Inputs: isGoal tests candidate cells, maxDepth limits search cost, and avoidDangerousCells skips threatened cells.
        /// Output: returns true and sets direction when a path is found; otherwise returns false and sets direction to zero.
        /// </summary>
        /// <param name="isGoal">Callback that returns true for an acceptable target grid cell.</param>
        /// <param name="maxDepth">Maximum number of grid steps the AI may search from its current cell.</param>
        /// <param name="avoidDangerousCells">When true, cells threatened by bombs or explosions are ignored.</param>
        /// <param name="direction">First grid step the AI should take toward the found goal.</param>
        /// <returns>True if a reachable goal was found within maxDepth; otherwise false.</returns>
        private bool TryFindDirectionToGoal(
            Func<Vector2Int, bool> isGoal,
            int maxDepth,
            bool avoidDangerousCells,
            out Vector2Int direction)
        {
            return TryFindDirectionToGoal(isGoal, maxDepth, avoidDangerousCells, out direction, out _);
        }

        /// <summary>
        /// Purpose: Searches nearby walkable cells and returns both the first step and matched goal cell.
        /// Inputs: isGoal tests cells, maxDepth limits search, and avoidDangerousCells skips active danger while pathing.
        /// Output: returns true with direction and targetGridPosition when a reachable goal is found.
        /// </summary>
        /// <param name="isGoal">Callback that returns true for an acceptable target grid cell.</param>
        /// <param name="maxDepth">Maximum number of grid steps the AI may search from its current cell.</param>
        /// <param name="avoidDangerousCells">When true, cells threatened by bombs or active explosions are ignored.</param>
        /// <param name="direction">First grid step the AI should take toward the found goal.</param>
        /// <param name="targetGridPosition">Actual goal cell found by the breadth-first search.</param>
        /// <returns>True if a reachable goal was found within maxDepth; otherwise false.</returns>
        private bool TryFindDirectionToGoal(
            Func<Vector2Int, bool> isGoal,
            int maxDepth,
            bool avoidDangerousCells,
            out Vector2Int direction,
            out Vector2Int targetGridPosition)
        {
            direction = Vector2Int.zero;
            targetGridPosition = Vector2Int.zero;
            if (MapManager == null || isGoal == null)
            {
                return false;
            }

            // Breadth-first search keeps the first found path short and stable for grid movement.
            Queue<EscapeSearchNode> openNodes = new Queue<EscapeSearchNode>();
            HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
            Vector2Int startPosition = CurrentGridPosition;

            openNodes.Enqueue(new EscapeSearchNode(startPosition, Vector2Int.zero, 0));
            visitedPositions.Add(startPosition);

            while (openNodes.Count > 0)
            {
                EscapeSearchNode currentNode = openNodes.Dequeue();
                if (currentNode.Depth > 0 && isGoal(currentNode.Position))
                {
                    direction = currentNode.FirstDirection;
                    targetGridPosition = currentNode.Position;
                    return true;
                }

                if (currentNode.Depth >= maxDepth)
                {
                    continue;
                }

                int startIndex = UnityEngine.Random.Range(0, MoveDirections.Length);
                for (int i = 0; i < MoveDirections.Length; i++)
                {
                    Vector2Int stepDirection = MoveDirections[(startIndex + i) % MoveDirections.Length];
                    Vector2Int nextPosition = currentNode.Position + stepDirection;
                    if (visitedPositions.Contains(nextPosition))
                    {
                        continue;
                    }

                    if (!IsGridWalkable(nextPosition))
                    {
                        continue;
                    }

                    if (avoidDangerousCells && IsGridDangerous(nextPosition))
                    {
                        continue;
                    }

                    // Store only the first step, because the AI will make a fresh decision after moving.
                    Vector2Int firstDirection = currentNode.Depth == 0 ? stepDirection : currentNode.FirstDirection;
                    openNodes.Enqueue(new EscapeSearchNode(nextPosition, firstDirection, currentNode.Depth + 1));
                    visitedPositions.Add(nextPosition);
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Finds a first movement step that leads away from dangerous cells.
        /// Inputs: isDangerous marks unsafe cells, maxDepth limits search, and blockedBombCell can reserve the newly placed bomb cell.
        /// Output: returns true and sets direction when a safe path exists; otherwise returns false and sets direction to zero.
        /// </summary>
        /// <param name="isDangerous">Callback that returns true when a grid cell is unsafe.</param>
        /// <param name="maxDepth">Maximum number of grid steps the AI may inspect.</param>
        /// <param name="hasBlockedBombCell">Whether blockedBombCell should be treated as unwalkable during the search.</param>
        /// <param name="blockedBombCell">Bomb cell to avoid, usually the AI's current cell after placing a bomb.</param>
        /// <param name="direction">First grid step toward safety.</param>
        /// <returns>True if the AI can reach a safe cell within maxDepth; otherwise false.</returns>
        private bool TryFindEscapeDirection(
            Func<Vector2Int, bool> isDangerous,
            int maxDepth,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell,
            out Vector2Int direction)
        {
            return TryFindEscapeDirectionFrom(
                CurrentGridPosition,
                isDangerous,
                maxDepth,
                hasBlockedBombCell,
                blockedBombCell,
                out direction);
        }

        /// <summary>
        /// Purpose: Searches from any start cell and chooses the safest reachable escape cell, not just the first safe one.
        /// Inputs: startPosition is the simulated AI cell, isDangerous marks unsafe cells, maxDepth limits search, and blockedBombCell can represent a newly planted bomb.
        /// Output: returns true and outputs the first step toward the best-scored safe cell; otherwise returns false.
        /// </summary>
        /// <param name="startPosition">Grid cell where the escape search begins.</param>
        /// <param name="isDangerous">Callback that returns true when a grid cell is unsafe.</param>
        /// <param name="maxDepth">Maximum number of grid steps the AI may inspect.</param>
        /// <param name="hasBlockedBombCell">Whether blockedBombCell should be treated as unwalkable after leaving it.</param>
        /// <param name="blockedBombCell">Bomb cell to avoid, usually the AI's current or simulated bomb cell.</param>
        /// <param name="direction">First grid step toward the best escape cell.</param>
        /// <returns>True if at least one safe escape cell was found; otherwise false.</returns>
        private bool TryFindEscapeDirectionFrom(
            Vector2Int startPosition,
            Func<Vector2Int, bool> isDangerous,
            int maxDepth,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell,
            out Vector2Int direction)
        {
            direction = Vector2Int.zero;
            if (MapManager == null || isDangerous == null)
            {
                return false;
            }

            // The queue explores the map in short paths; scoring below keeps the final choice less trap-prone.
            Queue<EscapeSearchNode> openNodes = new Queue<EscapeSearchNode>();
            HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
            Vector2Int bestDirection = Vector2Int.zero;
            float bestScore = float.MinValue;
            bool foundSafeCell = false;

            openNodes.Enqueue(new EscapeSearchNode(startPosition, Vector2Int.zero, 0));
            visitedPositions.Add(startPosition);

            while (openNodes.Count > 0)
            {
                EscapeSearchNode currentNode = openNodes.Dequeue();
                if (currentNode.Depth > 0 && !isDangerous(currentNode.Position))
                {
                    float score = CalculateEscapeCellScore(
                        currentNode.Position,
                        currentNode.Depth,
                        isDangerous,
                        hasBlockedBombCell,
                        blockedBombCell);

                    if (!foundSafeCell || score > bestScore)
                    {
                        foundSafeCell = true;
                        bestScore = score;
                        bestDirection = currentNode.FirstDirection;
                    }
                }

                if (currentNode.Depth >= maxDepth)
                {
                    continue;
                }

                int startIndex = UnityEngine.Random.Range(0, MoveDirections.Length);
                for (int i = 0; i < MoveDirections.Length; i++)
                {
                    Vector2Int stepDirection = MoveDirections[(startIndex + i) % MoveDirections.Length];
                    Vector2Int nextPosition = currentNode.Position + stepDirection;
                    if (visitedPositions.Contains(nextPosition))
                    {
                        continue;
                    }

                    if (!CanUseCellForEscapeSearch(nextPosition, startPosition, hasBlockedBombCell, blockedBombCell))
                    {
                        continue;
                    }

                    if (IsGridHitByActiveExplosion(nextPosition))
                    {
                        continue;
                    }

                    // Keep the original first step instead of storing the whole path.
                    Vector2Int firstDirection = currentNode.Depth == 0 ? stepDirection : currentNode.FirstDirection;
                    openNodes.Enqueue(new EscapeSearchNode(nextPosition, firstDirection, currentNode.Depth + 1));
                    visitedPositions.Add(nextPosition);
                }
            }

            direction = bestDirection;
            return foundSafeCell;
        }

        /// <summary>
        /// Purpose: Scores a safe escape candidate so the AI prefers open cells farther away from its own bomb.
        /// Inputs: gridPosition is the safe candidate, depth is path length, isDangerous checks neighbor safety, and blockedBombCell marks the bomb cell if any.
        /// Output: returns a higher score for safer, more flexible escape destinations.
        /// </summary>
        /// <param name="gridPosition">Safe candidate cell being scored.</param>
        /// <param name="depth">Number of grid steps from the escape start to this candidate.</param>
        /// <param name="isDangerous">Callback that returns true when a grid cell is unsafe.</param>
        /// <param name="hasBlockedBombCell">Whether blockedBombCell should affect the distance score.</param>
        /// <param name="blockedBombCell">Bomb cell the AI should move away from.</param>
        /// <returns>Weighted score used to choose the best escape direction.</returns>
        private float CalculateEscapeCellScore(
            Vector2Int gridPosition,
            int depth,
            Func<Vector2Int, bool> isDangerous,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell)
        {
            int safeNeighborCount = CountSafeNeighborCells(gridPosition, isDangerous, hasBlockedBombCell, blockedBombCell);
            int bombDistance = hasBlockedBombCell ? ManhattanDistance(gridPosition, blockedBombCell) : 0;
            float openSpaceScore = safeNeighborCount * safeNeighborScoreWeight;
            float distanceScore = bombDistance * bombDistanceScoreWeight;
            float pathPenalty = depth * escapeDepthPenalty;

            // Cells with no safe exits are still usable in emergencies, but the AI should prefer open routes.
            if (safeNeighborCount < minimumSafeNeighborsAfterBomb)
            {
                openSpaceScore -= safeNeighborScoreWeight;
            }

            return openSpaceScore + distanceScore - pathPenalty;
        }

        /// <summary>
        /// Purpose: Counts safe neighboring cells around a candidate position.
        /// Inputs: gridPosition is the center cell, isDangerous checks danger, and blockedBombCell represents a bomb to avoid.
        /// Output: returns the number of adjacent cells that are physically usable and not dangerous.
        /// </summary>
        /// <param name="gridPosition">Center grid cell whose neighbors are checked.</param>
        /// <param name="isDangerous">Callback that returns true when a neighboring cell is unsafe.</param>
        /// <param name="hasBlockedBombCell">Whether blockedBombCell should be treated as blocked.</param>
        /// <param name="blockedBombCell">Bomb cell to exclude from safe neighbor counting.</param>
        /// <returns>Number of safe neighboring cells around gridPosition.</returns>
        private int CountSafeNeighborCells(
            Vector2Int gridPosition,
            Func<Vector2Int, bool> isDangerous,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell)
        {
            int safeNeighborCount = 0;
            for (int i = 0; i < MoveDirections.Length; i++)
            {
                Vector2Int neighborPosition = gridPosition + MoveDirections[i];
                if (!CanUseCellForEscapeSearch(neighborPosition, gridPosition, hasBlockedBombCell, blockedBombCell))
                {
                    continue;
                }

                if (isDangerous != null && isDangerous(neighborPosition))
                {
                    continue;
                }

                safeNeighborCount++;
            }

            return safeNeighborCount;
        }

        /// <summary>
        /// Purpose: Checks whether a cell can be used by AI path searches that simulate escaping from bombs.
        /// Inputs: gridPosition is the tested cell, startPosition is always allowed, and blockedBombCell may represent a bomb.
        /// Output: returns true when the cell is physically usable for the simulated path.
        /// </summary>
        /// <param name="gridPosition">Grid cell being tested for escape search usage.</param>
        /// <param name="startPosition">Search start cell that remains usable even when it contains the AI or a just-placed bomb.</param>
        /// <param name="hasBlockedBombCell">Whether blockedBombCell should be treated as unwalkable after leaving it.</param>
        /// <param name="blockedBombCell">Bomb cell to avoid during the search.</param>
        /// <returns>True if the search may move through this cell; otherwise false.</returns>
        private bool CanUseCellForEscapeSearch(
            Vector2Int gridPosition,
            Vector2Int startPosition,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell)
        {
            if (gridPosition == startPosition)
            {
                return true;
            }

            if (hasBlockedBombCell && gridPosition == blockedBombCell)
            {
                return false;
            }

            if (gridPosition == CurrentGridPosition)
            {
                return IsGridPhysicallyWalkableIgnoringCharacters(gridPosition);
            }

            return IsGridWalkable(gridPosition);
        }

        /// <summary>
        /// Purpose: Checks wall and bomb blocking while ignoring character occupancy for AI simulations.
        /// Inputs: gridPosition is the tested map cell.
        /// Output: returns true when the cell has no wall or bomb, even if the AI currently occupies it.
        /// </summary>
        /// <param name="gridPosition">Grid cell being checked for physical blocking.</param>
        /// <returns>True if walls and bombs do not block the cell; otherwise false.</returns>
        private bool IsGridPhysicallyWalkableIgnoringCharacters(Vector2Int gridPosition)
        {
            return MapManager != null &&
                   !MapManager.IsBlockedByWall(gridPosition) &&
                   !MapManager.IsBlockedByBomb(gridPosition);
        }

        /// <summary>
        /// Purpose: Returns whether this object is grid dangerous.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsGridDangerous(Vector2Int gridPosition)
        {
            return IsGridHitByActiveExplosion(gridPosition) || IsGridThreatenedByActiveBomb(gridPosition);
        }

        /// <summary>
        /// Purpose: Returns whether this object is grid hit by active explosion.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsGridHitByActiveExplosion(Vector2Int gridPosition)
        {
            ExplosionController[] explosions = FindObjectsOfType<ExplosionController>();
            for (int i = 0; i < explosions.Length; i++)
            {
                ExplosionController explosion = explosions[i];
                if (explosion != null && explosion.IsInitialized && explosion.GridPosition == gridPosition)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Returns whether this object is grid threatened by active bomb.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsGridThreatenedByActiveBomb(Vector2Int gridPosition)
        {
            BombController[] bombs = FindObjectsOfType<BombController>();
            for (int i = 0; i < bombs.Length; i++)
            {
                BombController bomb = bombs[i];
                if (bomb == null || bomb.HasExploded)
                {
                    continue;
                }

                if (IsGridInBombBlast(gridPosition, bomb.GridPosition, bomb.Range))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Checks whether a grid cell would be hit by a bomb's cross-shaped blast.
        /// Inputs: targetGridPosition is the tested cell, bombGridPosition is the bomb cell, and blastRange is the bomb range.
        /// Output: returns true if the target is on the bomb row/column before a wall blocks propagation.
        /// </summary>
        /// <param name="targetGridPosition">Grid cell being tested for danger.</param>
        /// <param name="bombGridPosition">Grid cell occupied by the bomb.</param>
        /// <param name="blastRange">Maximum blast distance in grid cells.</param>
        /// <returns>True if the target cell is inside the bomb's effective blast path; otherwise false.</returns>
        private bool IsGridInBombBlast(Vector2Int targetGridPosition, Vector2Int bombGridPosition, int blastRange)
        {
            if (MapManager == null)
            {
                return false;
            }

            if (targetGridPosition == bombGridPosition)
            {
                return true;
            }

            Vector2Int delta = targetGridPosition - bombGridPosition;
            bool sameColumn = delta.x == 0;
            bool sameRow = delta.y == 0;
            if (!sameColumn && !sameRow)
            {
                return false;
            }

            int distance = Mathf.Abs(sameColumn ? delta.y : delta.x);
            if (distance <= 0 || distance > blastRange)
            {
                return false;
            }

            Vector2Int direction = sameColumn
                ? new Vector2Int(0, delta.y > 0 ? 1 : -1)
                : new Vector2Int(delta.x > 0 ? 1 : -1, 0);

            // Walk from the bomb to the target so walls can block danger exactly like real explosions.
            for (int step = 1; step <= distance; step++)
            {
                Vector2Int checkGridPosition = bombGridPosition + direction * step;
                GridCell cell = MapManager.GetCell(checkGridPosition);
                if (cell == null || cell.IsHardWall)
                {
                    return false;
                }

                if (checkGridPosition == targetGridPosition)
                {
                    return true;
                }

                if (cell.IsSoftWall)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Purpose: Copies one difficulty preset into the AI's live behavior fields.
        /// Inputs: difficulty selects Easy, Normal, or Hard tuning values.
        /// Output: no return value; movement, bomb, chase, and escape settings are updated in-place.
        /// </summary>
        /// <param name="difficulty">Difficulty preset that should drive this AI instance.</param>
        private void ApplyDifficultyPreset(AIDifficulty difficulty)
        {
            DifficultyPreset preset = GetDifficultyPreset(difficulty);

            moveDecisionInterval = preset.MoveDecisionInterval;
            failedMoveRetryDelay = preset.FailedMoveRetryDelay;
            idleChance = preset.IdleChance;
            avoidImmediateReverseChance = preset.AvoidReverseChance;
            escapeSearchDepth = preset.EscapeSearchDepth;
            playerChaseDistance = preset.PlayerChaseDistance;
            softWallTargetSearchDepth = preset.SoftWallSearchDepth;
            strategicMoveRetryDelay = preset.StrategicRetryDelay;
            postBombEscapeSeconds = preset.PostBombEscapeSeconds;
            bombDecisionInterval = preset.BombDecisionInterval;
            urgentBombDecisionDelay = preset.UrgentBombDecisionDelay;
            bombPlacementChance = preset.BombPlacementChance;
            softWallBombPlacementChance = preset.SoftWallBombChance;
            adjacentSoftWallBombPlacementChance = preset.AdjacentSoftWallBombChance;
            playerAttackBombPlacementChance = preset.PlayerAttackBombChance;
            softWallCheckDistance = preset.SoftWallCheckDistance;
            bombEscapeSearchDepth = preset.BombEscapeSearchDepth;
            minimumSafeNeighborsAfterBomb = preset.MinimumSafeNeighbors;
        }

        /// <summary>
        /// Purpose: Returns the numeric AI behavior preset for a selected difficulty.
        /// Inputs: difficulty is the selected AI difficulty enum.
        /// Output: returns a DifficultyPreset struct containing movement, attack, and escape parameters.
        /// </summary>
        /// <param name="difficulty">AI difficulty to resolve.</param>
        /// <returns>Resolved behavior preset.</returns>
        private DifficultyPreset GetDifficultyPreset(AIDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    return new DifficultyPreset(
                        0.56f,
                        0.22f,
                        0.24f,
                        0.45f,
                        4,
                        4,
                        5,
                        0.26f,
                        1.15f,
                        3.05f,
                        0.35f,
                        0.18f,
                        0.34f,
                        0.52f,
                        0.42f,
                        1,
                        4,
                        1);
                case AIDifficulty.Hard:
                    return new DifficultyPreset(
                        0.23f,
                        0.08f,
                        0.04f,
                        0.88f,
                        8,
                        10,
                        11,
                        0.09f,
                        1.75f,
                        0.95f,
                        0.05f,
                        0.68f,
                        0.92f,
                        1f,
                        0.95f,
                        2,
                        8,
                        2);
                default:
                    return new DifficultyPreset(
                        0.35f,
                        0.15f,
                        0.12f,
                        0.7f,
                        6,
                        7,
                        8,
                        0.18f,
                        1.4f,
                        2f,
                        0.12f,
                        0.45f,
                        0.72f,
                        0.9f,
                        0.85f,
                        1,
                        6,
                        1);
            }
        }

        /// <summary>
        /// Purpose: Resets aistate to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResetAIState()
        {
            currentMoveDirection = Vector2Int.zero;
            failedMoveAttempts = 0;
            currentCellDangerous = false;
            postBombEscapeTimer = 0f;
            currentStrategicTarget = Vector2Int.zero;
            currentIntent = "Idle";
            moveDecisionTimer = UnityEngine.Random.Range(0f, moveDecisionInterval);
            bombTimer = UnityEngine.Random.Range(0f, bombDecisionInterval);
        }

        /// <summary>
        /// Purpose: Advances post bomb escape timer by one update step.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void TickPostBombEscapeTimer()
        {
            if (postBombEscapeTimer <= 0f)
            {
                return;
            }

            postBombEscapeTimer = Mathf.Max(0f, postBombEscapeTimer - Time.deltaTime);
        }

        /// <summary>
        /// Purpose: Returns manhattan distance for the current state.
        /// Inputs: `a`, `b`; may also read serialized fields and current runtime state.
        /// Output: a `int` value.
        /// </summary>
        /// <param name="a">Input value used by this method.</param>
        /// <param name="b">Input value used by this method.</param>
        /// <returns>a `int` value.</returns>
        private int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Purpose: Performs log decision for this component.
        /// Inputs: `message`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="message">Input value used by this method.</param>
        private void LogDecision(string message)
        {
            if (!logDecisionDebug)
            {
                return;
            }

            Debug.Log($"[AIController] {name}: {message}");
        }
    }
}
