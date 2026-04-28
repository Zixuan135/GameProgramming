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

        [Header("AI Bomb Logic")]
        [SerializeField] private bool enableBombPlacement = true;
        [SerializeField, Min(0.25f)] private float bombDecisionInterval = 2f;
        [SerializeField, Range(0f, 1f)] private float bombPlacementChance = 0.45f;
        [SerializeField, Min(1)] private int softWallCheckDistance = 1;
        [SerializeField] private bool requireEscapeRouteBeforeBomb = true;
        [SerializeField, Min(1)] private int bombEscapeSearchDepth = 6;

        [Header("AI Runtime")]
        [SerializeField] private Vector2Int currentMoveDirection = Vector2Int.zero;
        [SerializeField] private int failedMoveAttempts;
        [SerializeField] private bool currentCellDangerous;

        private float moveDecisionTimer;
        private float bombTimer;

        private struct EscapeSearchNode
        {
            public Vector2Int Position;
            public Vector2Int FirstDirection;
            public int Depth;

            public EscapeSearchNode(Vector2Int position, Vector2Int firstDirection, int depth)
            {
                Position = position;
                FirstDirection = firstDirection;
                Depth = depth;
            }
        }

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
            UpdateBombDecision();
            UpdateMovementDecision();
        }

        public override void ConfigureForBattle(
            MapManager newMapManager,
            Vector2Int spawnGridPosition,
            Transform newBombSpawnRoot,
            BombController newBombPrefab = null)
        {
            base.ConfigureForBattle(newMapManager, spawnGridPosition, newBombSpawnRoot, newBombPrefab);
            ResetAIState();
        }

        private bool CanActDuringBattle()
        {
            GameManager gameManager = GameManager.Instance;
            return gameManager == null || gameManager.CurrentGameState == GameState.BattleRunning;
        }

        private void UpdateMovementDecision()
        {
            if (IsMoving)
            {
                return;
            }

            if (currentCellDangerous)
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

            TryMoveRandomDirection();
        }

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

        private void TryMoveRandomDirection()
        {
            if (UnityEngine.Random.value < idleChance)
            {
                ScheduleNextMoveDecision(moveDecisionInterval);
                return;
            }

            if (!TryPickWalkableDirection(out Vector2Int nextDirection, true))
            {
                RegisterFailedMove(failedMoveRetryDelay);
                return;
            }

            TryMoveOrRegisterFailure(nextDirection, moveDecisionInterval);
        }

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

        private bool IsGridWalkable(Vector2Int gridPosition)
        {
            return MapManager != null && MapManager.IsWalkable(gridPosition);
        }

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

        private void ScheduleNextMoveDecision(float delay)
        {
            moveDecisionTimer = Mathf.Max(0.01f, delay);
        }

        private void UpdateBombDecision()
        {
            if (!enableBombPlacement || IsMoving)
            {
                return;
            }

            bombTimer -= Time.deltaTime;
            if (bombTimer > 0f)
            {
                return;
            }

            bombTimer = bombDecisionInterval;
            if (!CanPlaceMoreBombs() || currentCellDangerous)
            {
                return;
            }

            if (!HasBombPlacementOpportunity())
            {
                return;
            }

            if (UnityEngine.Random.value > bombPlacementChance)
            {
                return;
            }

            if (requireEscapeRouteBeforeBomb && !CanReachSafetyAfterPlacingBomb())
            {
                return;
            }

            if (TryPlaceBomb())
            {
                currentCellDangerous = true;
                moveDecisionTimer = 0f;
            }
        }

        private bool HasBombPlacementOpportunity()
        {
            return IsNearSoftWall() || IsPlayerInBombLine();
        }

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

        private bool CanReachSafetyAfterPlacingBomb()
        {
            Vector2Int hypotheticalBombGridPosition = CurrentGridPosition;
            return TryFindEscapeDirection(
                gridPosition => IsGridDangerous(gridPosition) || IsGridInBombBlast(gridPosition, hypotheticalBombGridPosition, BombRange),
                bombEscapeSearchDepth,
                true,
                hypotheticalBombGridPosition,
                out _);
        }

        private bool TryFindEscapeDirection(
            Func<Vector2Int, bool> isDangerous,
            int maxDepth,
            bool hasBlockedBombCell,
            Vector2Int blockedBombCell,
            out Vector2Int direction)
        {
            direction = Vector2Int.zero;
            if (MapManager == null)
            {
                return false;
            }

            Queue<EscapeSearchNode> openNodes = new Queue<EscapeSearchNode>();
            HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
            Vector2Int startPosition = CurrentGridPosition;

            openNodes.Enqueue(new EscapeSearchNode(startPosition, Vector2Int.zero, 0));
            visitedPositions.Add(startPosition);

            while (openNodes.Count > 0)
            {
                EscapeSearchNode currentNode = openNodes.Dequeue();
                if (currentNode.Depth > 0 && !isDangerous(currentNode.Position))
                {
                    direction = currentNode.FirstDirection;
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

                    if (!CanUseCellForEscapeSearch(nextPosition, startPosition, hasBlockedBombCell, blockedBombCell))
                    {
                        continue;
                    }

                    Vector2Int firstDirection = currentNode.Depth == 0 ? stepDirection : currentNode.FirstDirection;
                    openNodes.Enqueue(new EscapeSearchNode(nextPosition, firstDirection, currentNode.Depth + 1));
                    visitedPositions.Add(nextPosition);
                }
            }

            return false;
        }

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

            return IsGridWalkable(gridPosition);
        }

        private bool IsGridDangerous(Vector2Int gridPosition)
        {
            return IsGridHitByActiveExplosion(gridPosition) || IsGridThreatenedByActiveBomb(gridPosition);
        }

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

        private void ResetAIState()
        {
            currentMoveDirection = Vector2Int.zero;
            failedMoveAttempts = 0;
            currentCellDangerous = false;
            moveDecisionTimer = UnityEngine.Random.Range(0f, moveDecisionInterval);
            bombTimer = UnityEngine.Random.Range(0f, bombDecisionInterval);
        }
    }
}
