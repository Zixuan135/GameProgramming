using System;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Gameplay;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Base stats and shared behavior for player and AI characters.
    /// </summary>
    public class CharacterBase : MonoBehaviour
    {
        private const float MoveArriveDistance = 0.001f;

        [Header("Grid Movement")]
        [SerializeField] protected MapManager mapManager;
        [SerializeField] protected Vector2Int currentGridPosition = Vector2Int.zero;
        [SerializeField] protected Vector3 currentWorldPosition;
        [SerializeField] protected bool isMoving;
        [SerializeField] protected bool snapToGridOnStart = true;

        [Header("Stats")]
        [SerializeField] protected float moveSpeed = GameConstants.DefaultMoveSpeed;
        [SerializeField, Min(1)] protected int maxBombCount = GameConstants.DefaultBombCount;
        [SerializeField] protected int bombRange = GameConstants.DefaultBombRange;

        [Header("Bomb")]
        [SerializeField] protected BombController bombPrefab;
        [SerializeField] protected Transform bombSpawnRoot;
        [SerializeField, Min(0)] protected int activeBombCount;

        [Header("Life State")]
        [SerializeField] protected bool isAlive = true;
        [SerializeField] protected bool hideRenderersOnDeath = true;
        [SerializeField] protected bool disableCollidersOnDeath = true;
        [SerializeField] protected bool clearMapOccupationOnDeath = true;

        private Vector3 moveTargetWorldPosition;

        public event Action<CharacterBase> Died;
        public event Action<CharacterBase> StatsChanged;

        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector3 CurrentWorldPosition => currentWorldPosition;
        public Vector3 MoveTargetWorldPosition => moveTargetWorldPosition;
        public float MoveSpeed => moveSpeed;
        public bool IsMoving => isMoving;
        public bool IsAlive => isAlive;
        public int MaxBombCount => maxBombCount;
        public int ActiveBombCount => activeBombCount;
        public int CurrentBombCount => activeBombCount;
        public int RemainingBombCount => Mathf.Max(0, maxBombCount - activeBombCount);
        public int BombRange => bombRange;
        public MapManager MapManager => mapManager;
        public BombController BombPrefab => bombPrefab;
        public Transform BombSpawnRoot => bombSpawnRoot;

        protected virtual void Awake()
        {
            isAlive = true;
            activeBombCount = 0;
            currentWorldPosition = transform.position;
        }

        protected virtual void Start()
        {
            InitializeGridPosition();
        }

        protected virtual void Update()
        {
            if (!isAlive)
            {
                return;
            }

            UpdateGridMovement();
        }

        protected virtual void InitializeGridPosition()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }

            currentGridPosition = WorldToGrid(transform.position);
            currentWorldPosition = GridToWorld(currentGridPosition);
            moveTargetWorldPosition = currentWorldPosition;

            if (snapToGridOnStart)
            {
                transform.position = currentWorldPosition;
            }

            mapManager?.SetCharacter(currentGridPosition);
        }

        public virtual void Move(Vector3 worldDirection)
        {
            Vector2Int gridDirection = WorldDirectionToGridDirection(worldDirection);
            TryMoveGridDirection(gridDirection);
        }

        public virtual bool TryMoveGridDirection(Vector2Int gridDirection)
        {
            if (!isAlive)
            {
                return false;
            }

            if (!CanStartMove(gridDirection))
            {
                return false;
            }

            Vector2Int targetGridPosition = currentGridPosition + gridDirection;
            if (mapManager != null && !mapManager.IsWalkable(targetGridPosition))
            {
                return false;
            }

            Vector2Int previousGridPosition = currentGridPosition;
            mapManager?.ClearCharacter(previousGridPosition);
            currentGridPosition = targetGridPosition;

            if (mapManager != null && !mapManager.SetCharacter(currentGridPosition))
            {
                currentGridPosition = previousGridPosition;
                mapManager.SetCharacter(previousGridPosition);
                return false;
            }

            BeginMoveToCurrentGridPosition();
            return true;
        }

        protected virtual bool CanStartMove(Vector2Int gridDirection)
        {
            return isAlive && !isMoving && gridDirection != Vector2Int.zero;
        }

        protected virtual void BeginMoveToCurrentGridPosition()
        {
            moveTargetWorldPosition = GridToWorld(currentGridPosition);
            isMoving = true;
        }

        protected virtual void UpdateGridMovement()
        {
            if (!isMoving)
            {
                currentWorldPosition = transform.position;
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                moveTargetWorldPosition,
                moveSpeed * Time.deltaTime);

            currentWorldPosition = transform.position;

            if (Vector3.Distance(transform.position, moveTargetWorldPosition) > MoveArriveDistance)
            {
                return;
            }

            CompleteGridMovement();
        }

        protected virtual void CompleteGridMovement()
        {
            transform.position = moveTargetWorldPosition;
            currentWorldPosition = transform.position;
            isMoving = false;
        }

        public virtual void ConfigureForBattle(
            MapManager newMapManager,
            Vector2Int spawnGridPosition,
            Transform newBombSpawnRoot,
            BombController newBombPrefab = null)
        {
            if (mapManager != null)
            {
                mapManager.ClearCharacter(currentGridPosition);
            }

            mapManager = newMapManager != null ? newMapManager : mapManager;
            if (newBombSpawnRoot != null)
            {
                bombSpawnRoot = newBombSpawnRoot;
            }

            if (newBombPrefab != null)
            {
                bombPrefab = newBombPrefab;
            }

            activeBombCount = 0;
            isAlive = true;
            isMoving = false;
            RestoreAlivePresentation();

            currentGridPosition = spawnGridPosition;
            currentWorldPosition = GridToWorld(currentGridPosition);
            moveTargetWorldPosition = currentWorldPosition;
            transform.position = currentWorldPosition;
            mapManager?.SetCharacter(currentGridPosition);
        }

        public virtual Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            if (mapManager != null)
            {
                return mapManager.WorldToGrid(worldPosition);
            }

            int x = Mathf.RoundToInt(worldPosition.x / GameConstants.GridCellSize);
            int y = Mathf.RoundToInt(worldPosition.z / GameConstants.GridCellSize);
            return new Vector2Int(x, y);
        }

        public virtual Vector3 GridToWorld(Vector2Int gridPosition)
        {
            if (mapManager != null)
            {
                return mapManager.GridToWorld(gridPosition, transform.position.y);
            }

            return new Vector3(
                gridPosition.x * GameConstants.GridCellSize,
                transform.position.y,
                gridPosition.y * GameConstants.GridCellSize);
        }

        protected virtual Vector2Int WorldDirectionToGridDirection(Vector3 worldDirection)
        {
            if (Mathf.Abs(worldDirection.x) > Mathf.Abs(worldDirection.z))
            {
                return worldDirection.x > 0f ? Vector2Int.right : Vector2Int.left;
            }

            if (Mathf.Abs(worldDirection.z) > 0f)
            {
                return worldDirection.z > 0f ? Vector2Int.up : Vector2Int.down;
            }

            return Vector2Int.zero;
        }

        public virtual bool TryPlaceBomb()
        {
            if (!isAlive)
            {
                return false;
            }

            if (bombPrefab == null)
            {
                Debug.LogWarning($"[CharacterBase] {name} cannot place a bomb because no Bomb Prefab is assigned.");
                return false;
            }

            if (!CanPlaceMoreBombs())
            {
                return false;
            }

            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }

            if (mapManager == null || !mapManager.PlaceBomb(currentGridPosition))
            {
                return false;
            }

            Vector3 spawnPosition = GridToWorld(currentGridPosition);
            BombController bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity, bombSpawnRoot);
            bomb.Initialize(this, mapManager, currentGridPosition, bombRange);
            RegisterPlacedBomb();
            return true;
        }

        public virtual void OnBombExploded(BombController bomb)
        {
            ReleaseBombSlot();
        }

        public virtual bool CanPlaceMoreBombs()
        {
            return activeBombCount < maxBombCount;
        }

        protected virtual void RegisterPlacedBomb()
        {
            activeBombCount = Mathf.Min(maxBombCount, activeBombCount + 1);
        }

        protected virtual void ReleaseBombSlot()
        {
            activeBombCount = Mathf.Max(0, activeBombCount - 1);
        }

        public virtual void SetMaxBombCount(int newMaxBombCount)
        {
            maxBombCount = Mathf.Max(1, newMaxBombCount);
            NotifyStatsChanged();
        }

        public virtual void SetBombRange(int newBombRange)
        {
            bombRange = Mathf.Max(1, newBombRange);
            NotifyStatsChanged();
        }

        public virtual void SetMoveSpeed(float newMoveSpeed)
        {
            moveSpeed = Mathf.Max(1f, newMoveSpeed);
            NotifyStatsChanged();
        }

        public virtual void ApplyMoveSpeedModifier(float delta)
        {
            SetMoveSpeed(moveSpeed + delta);
        }

        public virtual void ApplyBombCountModifier(int delta)
        {
            SetMaxBombCount(maxBombCount + delta);
        }

        public virtual void ApplyBombRangeModifier(int delta)
        {
            SetBombRange(bombRange + delta);
        }

        public virtual bool ApplyItemEffect(
            ItemType itemType,
            int bombCountDelta,
            int explosionRangeDelta,
            float moveSpeedDelta)
        {
            if (!isAlive)
            {
                return false;
            }

            switch (itemType)
            {
                case ItemType.BombCountUp:
                    ApplyBombCountModifier(bombCountDelta);
                    return true;
                case ItemType.ExplosionRangeUp:
                    ApplyBombRangeModifier(explosionRangeDelta);
                    return true;
                case ItemType.MoveSpeedUp:
                    ApplyMoveSpeedModifier(moveSpeedDelta);
                    return true;
                default:
                    return false;
            }
        }

        protected virtual void NotifyStatsChanged()
        {
            StatsChanged?.Invoke(this);
        }

        public virtual void OnHitByExplosion()
        {
            if (!isAlive)
            {
                return;
            }

            Debug.Log($"[CharacterBase] {name} was hit by explosion.");
            Die();
        }

        public virtual void Die()
        {
            if (!isAlive)
            {
                return;
            }

            isAlive = false;
            isMoving = false;

            if (clearMapOccupationOnDeath)
            {
                mapManager?.ClearCharacter(currentGridPosition);
            }

            ApplyDeathPresentation();
            OnDied();
        }

        protected virtual void ApplyDeathPresentation()
        {
            if (hideRenderersOnDeath)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                }
            }

            if (disableCollidersOnDeath)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        protected virtual void RestoreAlivePresentation()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
        }

        protected virtual void OnDied()
        {
            Debug.Log($"[CharacterBase] {name} died.");
            Died?.Invoke(this);
        }
    }
}
