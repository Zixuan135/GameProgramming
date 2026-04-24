using BubbleTown.Core;
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
        private Vector3 moveTargetWorldPosition;

        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector3 CurrentWorldPosition => currentWorldPosition;
        public Vector3 MoveTargetWorldPosition => moveTargetWorldPosition;
        public float MoveSpeed => moveSpeed;
        public bool IsMoving => isMoving;
        public int MaxBombCount => maxBombCount;
        public int ActiveBombCount => activeBombCount;
        public int CurrentBombCount => activeBombCount;
        public int RemainingBombCount => Mathf.Max(0, maxBombCount - activeBombCount);
        public int BombRange => bombRange;
        public MapManager MapManager => mapManager;

        protected virtual void Awake()
        {
            activeBombCount = 0;
            currentWorldPosition = transform.position;
        }

        protected virtual void Start()
        {
            InitializeGridPosition();
        }

        protected virtual void Update()
        {
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
            return !isMoving && gridDirection != Vector2Int.zero;
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
        }

        public virtual void ApplyMoveSpeedModifier(float delta)
        {
            moveSpeed = Mathf.Max(1f, moveSpeed + delta);
        }

        public virtual void ApplyBombCountModifier(int delta)
        {
            SetMaxBombCount(maxBombCount + delta);
        }

        public virtual void ApplyBombRangeModifier(int delta)
        {
            bombRange = Mathf.Max(1, bombRange + delta);
        }

        public virtual void OnHitByExplosion()
        {
            Debug.Log($"[CharacterBase] {name} was hit by explosion.");
            // TODO: Hook HP/life and battle result flow.
        }
    }
}
