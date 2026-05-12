using System;
using System.Collections;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Gameplay;
using BubbleTown.Managers;
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
        [SerializeField, Min(0)] protected int shieldCharges;
        [SerializeField, Min(1)] protected int maxShieldCharges = GameConstants.DefaultMaxShieldCharges;

        [Header("Bomb")]
        [SerializeField] protected BombController bombPrefab;
        [SerializeField] protected Transform bombSpawnRoot;
        [SerializeField, Min(0)] protected int activeBombCount;

        [Header("Life State")]
        [SerializeField] protected bool isAlive = true;
        [SerializeField] protected bool hideRenderersOnDeath = true;
        [SerializeField] protected bool disableCollidersOnDeath = true;
        [SerializeField] protected bool clearMapOccupationOnDeath = true;

        [Header("Opening Protection")]
        [SerializeField] protected bool isInvincible;
        [SerializeField, Min(0f)] protected float invincibleSecondsRemaining;
        [SerializeField] protected bool logProtectedExplosionHits = true;

        [Header("Death Feedback Timing")]
        [SerializeField] protected bool delayDeathPresentationForFeedback = true;
        [SerializeField, Min(0f)] protected float deathPresentationDelay = 0.55f;
        [SerializeField] protected bool disableCollidersImmediatelyOnDeath = true;

        [Header("Visual Facing")]
        [SerializeField] protected bool faceMoveDirection = true;
        [SerializeField] protected Transform visualRoot;

        private Vector3 moveTargetWorldPosition;
        private Coroutine delayedDeathPresentationRoutine;

        public event Action<CharacterBase> Died;
        public event Action<CharacterBase> StatsChanged;
        public event Action<CharacterBase> BombPlaced;
        public event Action<CharacterBase> ExplosionHit;
        public event Action<CharacterBase> DeathFeedbackStarted;

        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector3 CurrentWorldPosition => currentWorldPosition;
        public Vector3 MoveTargetWorldPosition => moveTargetWorldPosition;
        public float MoveSpeed => moveSpeed;
        public bool IsMoving => isMoving;
        public bool IsAlive => isAlive;
        public bool IsInvincible => isInvincible;
        public float InvincibleSecondsRemaining => invincibleSecondsRemaining;
        public int MaxBombCount => maxBombCount;
        public int ActiveBombCount => activeBombCount;
        public int CurrentBombCount => activeBombCount;
        public int RemainingBombCount => Mathf.Max(0, maxBombCount - activeBombCount);
        public int BombRange => bombRange;
        public int ShieldCharges => shieldCharges;
        public int MaxShieldCharges => maxShieldCharges;
        public bool HasShield => shieldCharges > 0;
        public MapManager MapManager => mapManager;
        public BombController BombPrefab => bombPrefab;
        public Transform BombSpawnRoot => bombSpawnRoot;

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void Awake()
        {
            isAlive = true;
            activeBombCount = 0;
            currentWorldPosition = transform.position;
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void Start()
        {
            InitializeGridPosition();
        }

        /// <summary>
        /// Purpose: Runs this component's per-frame logic.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void Update()
        {
            if (!isAlive)
            {
                return;
            }

            TickInvincibility();
            UpdateGridMovement();
        }

        /// <summary>
        /// Purpose: Performs initialize grid position for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Converts a world-space direction into a grid direction and requests movement.
        /// Inputs: worldDirection is usually a player or AI input vector on the XZ plane.
        /// Output: no return value; movement succeeds or fails through TryMoveGridDirection.
        /// </summary>
        /// <param name="worldDirection">World-space direction where X is horizontal movement and Z is vertical grid movement.</param>
        public virtual void Move(Vector3 worldDirection)
        {
            Vector2Int gridDirection = WorldDirectionToGridDirection(worldDirection);
            TryMoveGridDirection(gridDirection);
        }

        /// <summary>
        /// Purpose: Attempts to reserve a neighboring grid cell and starts smooth movement if it is valid.
        /// Inputs: gridDirection should be one cardinal grid step such as up, down, left, or right.
        /// Output: returns true when the move starts; returns false when blocked by state, walls, bombs, or another character.
        /// </summary>
        /// <param name="gridDirection">The requested one-cell movement direction in grid coordinates.</param>
        /// <returns>True if the character reserved the target cell and began moving; otherwise false.</returns>
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
            // Reserve the destination before the visual movement finishes so other actors cannot enter it.
            mapManager?.ClearCharacter(previousGridPosition);
            currentGridPosition = targetGridPosition;

            if (mapManager != null && !mapManager.SetCharacter(currentGridPosition))
            {
                // Roll back the logical position if reservation fails after clearing the old cell.
                currentGridPosition = previousGridPosition;
                mapManager.SetCharacter(previousGridPosition);
                return false;
            }

            FaceGridDirection(gridDirection);
            BeginMoveToCurrentGridPosition();
            AudioManager.Instance?.PlayMoveSFX();
            return true;
        }

        /// <summary>
        /// Purpose: Returns whether this object can start move.
        /// Inputs: `gridDirection`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="gridDirection">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        protected virtual bool CanStartMove(Vector2Int gridDirection)
        {
            return isAlive && !isMoving && gridDirection != Vector2Int.zero;
        }

        /// <summary>
        /// Purpose: Begins move to current grid position.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void BeginMoveToCurrentGridPosition()
        {
            moveTargetWorldPosition = GridToWorld(currentGridPosition);
            isMoving = true;
        }

        /// <summary>
        /// Purpose: Performs face grid direction for this component.
        /// Inputs: `gridDirection`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="gridDirection">Input value used by this method.</param>
        protected virtual void FaceGridDirection(Vector2Int gridDirection)
        {
            if (!faceMoveDirection || gridDirection == Vector2Int.zero)
            {
                return;
            }

            Transform facingTarget = visualRoot != null ? visualRoot : transform;
            Vector3 worldDirection = new Vector3(gridDirection.x, 0f, gridDirection.y);
            facingTarget.rotation = Quaternion.LookRotation(worldDirection, Vector3.up);
        }

        /// <summary>
        /// Purpose: Moves the Transform toward the reserved grid cell over time.
        /// Inputs: no direct parameters; reads current target, speed, and Time.deltaTime.
        /// Output: no return value; updates world position and clears IsMoving when the target is reached.
        /// </summary>
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

        /// <summary>
        /// Purpose: Performs complete grid movement for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void CompleteGridMovement()
        {
            transform.position = moveTargetWorldPosition;
            currentWorldPosition = transform.position;
            isMoving = false;
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
            shieldCharges = 0;
            ClearInvincibility();
            RestoreAlivePresentation();

            currentGridPosition = spawnGridPosition;
            currentWorldPosition = GridToWorld(currentGridPosition);
            moveTargetWorldPosition = currentWorldPosition;
            transform.position = currentWorldPosition;
            mapManager?.SetCharacter(currentGridPosition);
        }

        /// <summary>
        /// Purpose: Applies character data to the current object or scene.
        /// Inputs: `characterData`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        public virtual void ApplyCharacterData(CharacterData characterData)
        {
            if (characterData == null)
            {
                return;
            }

            SetMoveSpeed(characterData.MoveSpeed);
            SetMaxBombCount(characterData.MaxBombCount);
            SetBombRange(characterData.ExplosionRange);
            ApplyVisualTheme(characterData);
        }

        /// <summary>
        /// Purpose: Performs replace visual for this component.
        /// Inputs: `visualPrefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="visualPrefab">Input value used by this method.</param>
        public virtual void ReplaceVisual(GameObject visualPrefab)
        {
            if (visualPrefab == null)
            {
                return;
            }

            if (visualRoot != null && visualRoot != transform)
            {
                Destroy(visualRoot.gameObject);
            }

            UnityEngine.Object instantiatedVisual = Instantiate((UnityEngine.Object)visualPrefab, transform);
            GameObject visualInstance = instantiatedVisual as GameObject;
            if (visualInstance == null)
            {
                Debug.LogWarning($"[CharacterBase] Could not instantiate visual prefab '{visualPrefab.name}' as a GameObject.");
                return;
            }

            visualInstance.name = visualPrefab.name;
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;
            visualRoot = visualInstance.transform;
        }

        /// <summary>
        /// Purpose: Applies visual theme to the current object or scene.
        /// Inputs: `characterData`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="characterData">Input value used by this method.</param>
        private void ApplyVisualTheme(CharacterData characterData)
        {
            if (characterData == null || visualRoot == null)
            {
                return;
            }

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                ApplyThemeToRenderer(renderers[i], characterData.ThemeColor);
            }
        }

        /// <summary>
        /// Purpose: Applies theme to renderer to the current object or scene.
        /// Inputs: `targetRenderer`, `themeColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="targetRenderer">Input value used by this method.</param>
        /// <param name="themeColor">Input value used by this method.</param>
        private void ApplyThemeToRenderer(Renderer targetRenderer, Color themeColor)
        {
            if (targetRenderer == null || targetRenderer.sharedMaterial == null)
            {
                return;
            }

            string materialName = targetRenderer.sharedMaterial.name;
            if (materialName.Contains("Skin") ||
                materialName.Contains("Face") ||
                materialName.Contains("Glass") ||
                materialName.Contains("Fixed") ||
                materialName.Contains("Star"))
            {
                return;
            }

            // Only recolor outfit/accent materials; fixed face, skin, glass, and star details stay readable.
            Color resolvedColor = materialName.Contains("Accent")
                ? Color.Lerp(themeColor, Color.white, 0.28f)
                : themeColor;

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", resolvedColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        /// <summary>
        /// Purpose: Returns world to grid for the current state.
        /// Inputs: `worldPosition`; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <param name="worldPosition">Input value used by this method.</param>
        /// <returns>a `Vector2Int` value.</returns>
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

        /// <summary>
        /// Purpose: Returns grid to world for the current state.
        /// Inputs: `gridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="gridPosition">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
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

        /// <summary>
        /// Purpose: Returns world direction to grid direction for the current state.
        /// Inputs: `worldDirection`; may also read serialized fields and current runtime state.
        /// Output: a `Vector2Int` value.
        /// </summary>
        /// <param name="worldDirection">Input value used by this method.</param>
        /// <returns>a `Vector2Int` value.</returns>
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

        /// <summary>
        /// Purpose: Places a bomb on the character's current grid cell if capacity and map rules allow it.
        /// Inputs: no direct parameters; reads bomb prefab, bomb range, map occupancy, and active bomb count.
        /// Output: returns true if a bomb prefab was spawned and registered; otherwise false.
        /// </summary>
        /// <returns>True when the bomb is successfully placed; otherwise false.</returns>
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

            // The map data is marked before spawning the prefab to prevent duplicate bombs on one cell.
            Vector3 spawnPosition = GridToWorld(currentGridPosition);
            BombController bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity, bombSpawnRoot);
            bomb.Initialize(this, mapManager, currentGridPosition, bombRange);
            RegisterPlacedBomb();
            OnBombPlaced();
            AudioManager.Instance?.PlayPlaceBombSFX();
            return true;
        }

        /// <summary>
        /// Purpose: Handles the bomb exploded event or callback.
        /// Inputs: `bomb`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bomb">Input value used by this method.</param>
        public virtual void OnBombExploded(BombController bomb)
        {
            ReleaseBombSlot();
        }

        /// <summary>
        /// Purpose: Returns whether this object can place more bombs.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        public virtual bool CanPlaceMoreBombs()
        {
            return activeBombCount < maxBombCount;
        }

        /// <summary>
        /// Purpose: Registers placed bomb in the relevant runtime system.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void RegisterPlacedBomb()
        {
            activeBombCount = Mathf.Min(maxBombCount, activeBombCount + 1);
        }

        /// <summary>
        /// Purpose: Handles the bomb placed event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void OnBombPlaced()
        {
            BombPlaced?.Invoke(this);
        }

        /// <summary>
        /// Purpose: Performs release bomb slot for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void ReleaseBombSlot()
        {
            activeBombCount = Mathf.Max(0, activeBombCount - 1);
        }

        /// <summary>
        /// Purpose: Sets max bomb count.
        /// Inputs: `newMaxBombCount`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newMaxBombCount">Input value used by this method.</param>
        public virtual void SetMaxBombCount(int newMaxBombCount)
        {
            maxBombCount = Mathf.Max(1, newMaxBombCount);
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Sets bomb range.
        /// Inputs: `newBombRange`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newBombRange">Input value used by this method.</param>
        public virtual void SetBombRange(int newBombRange)
        {
            bombRange = Mathf.Max(1, newBombRange);
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Sets move speed.
        /// Inputs: `newMoveSpeed`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newMoveSpeed">Input value used by this method.</param>
        public virtual void SetMoveSpeed(float newMoveSpeed)
        {
            moveSpeed = Mathf.Max(1f, newMoveSpeed);
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Applies move speed modifier to the current object or scene.
        /// Inputs: `delta`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="delta">Input value used by this method.</param>
        public virtual void ApplyMoveSpeedModifier(float delta)
        {
            SetMoveSpeed(moveSpeed + delta);
        }

        /// <summary>
        /// Purpose: Applies bomb count modifier to the current object or scene.
        /// Inputs: `delta`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="delta">Input value used by this method.</param>
        public virtual void ApplyBombCountModifier(int delta)
        {
            SetMaxBombCount(maxBombCount + delta);
        }

        /// <summary>
        /// Purpose: Applies bomb range modifier to the current object or scene.
        /// Inputs: `delta`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="delta">Input value used by this method.</param>
        public virtual void ApplyBombRangeModifier(int delta)
        {
            SetBombRange(bombRange + delta);
        }

        /// <summary>
        /// Purpose: Sets shield charges.
        /// Inputs: `newShieldCharges`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="newShieldCharges">Input value used by this method.</param>
        public virtual void SetShieldCharges(int newShieldCharges)
        {
            shieldCharges = Mathf.Clamp(newShieldCharges, 0, Mathf.Max(1, maxShieldCharges));
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Applies shield charges modifier to the current object or scene.
        /// Inputs: `delta`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="delta">Input value used by this method.</param>
        public virtual void ApplyShieldChargesModifier(int delta)
        {
            SetShieldCharges(shieldCharges + delta);
        }

        /// <summary>
        /// Purpose: Applies item effect to the current object or scene.
        /// Inputs: `itemType`, `bombCountDelta`, `explosionRangeDelta`, `moveSpeedDelta`, `shieldChargesDelta`, `invincibleSeconds`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="itemType">Input value used by this method.</param>
        /// <param name="bombCountDelta">Input value used by this method.</param>
        /// <param name="explosionRangeDelta">Input value used by this method.</param>
        /// <param name="moveSpeedDelta">Input value used by this method.</param>
        /// <param name="shieldChargesDelta">Input value used by this method.</param>
        /// <param name="invincibleSeconds">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public virtual bool ApplyItemEffect(
            ItemType itemType,
            int bombCountDelta,
            int explosionRangeDelta,
            float moveSpeedDelta,
            int shieldChargesDelta = GameConstants.DefaultItemShieldChargesDelta,
            float invincibleSeconds = GameConstants.DefaultItemInvincibleSeconds)
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
                case ItemType.Shield:
                    ApplyShieldChargesModifier(shieldChargesDelta);
                    return true;
                case ItemType.TemporaryInvincible:
                    SetInvincible(invincibleSeconds);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Purpose: Performs notify stats changed for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void NotifyStatsChanged()
        {
            StatsChanged?.Invoke(this);
        }

        /// <summary>
        /// Purpose: Handles the hit by explosion event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public virtual void OnHitByExplosion()
        {
            if (!isAlive)
            {
                return;
            }

            if (isInvincible)
            {
                if (logProtectedExplosionHits)
                {
                    Debug.Log($"[CharacterBase] {name} ignored explosion hit during opening protection.");
                }

                return;
            }

            if (shieldCharges > 0)
            {
                shieldCharges = Mathf.Max(0, shieldCharges - 1);
                NotifyStatsChanged();
                Debug.Log($"[CharacterBase] {name} blocked explosion hit with shield. Remaining shields: {shieldCharges}");
                ExplosionHit?.Invoke(this);
                return;
            }

            Debug.Log($"[CharacterBase] {name} was hit by explosion.");
            ExplosionHit?.Invoke(this);
            Die();
        }

        /// <summary>
        /// Purpose: Sets invincible.
        /// Inputs: `seconds`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="seconds">Input value used by this method.</param>
        public virtual void SetInvincible(float seconds)
        {
            invincibleSecondsRemaining = Mathf.Max(0f, seconds);
            isInvincible = invincibleSecondsRemaining > 0f;
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Clears invincibility.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public virtual void ClearInvincibility()
        {
            invincibleSecondsRemaining = 0f;
            isInvincible = false;
            NotifyStatsChanged();
        }

        /// <summary>
        /// Purpose: Advances invincibility by one update step.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void TickInvincibility()
        {
            if (!isInvincible)
            {
                return;
            }

            invincibleSecondsRemaining = Mathf.Max(0f, invincibleSecondsRemaining - Time.deltaTime);
            if (invincibleSecondsRemaining <= 0f)
            {
                isInvincible = false;
                NotifyStatsChanged();
            }
        }

        /// <summary>
        /// Purpose: Performs die for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

            BeginDeathPresentation();
            OnDied();
        }

        /// <summary>
        /// Purpose: Begins death presentation.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void BeginDeathPresentation()
        {
            DeathFeedbackStarted?.Invoke(this);

            if (disableCollidersOnDeath && disableCollidersImmediatelyOnDeath)
            {
                ApplyDeathColliderPresentation();
            }

            if (delayDeathPresentationForFeedback && deathPresentationDelay > 0f)
            {
                if (delayedDeathPresentationRoutine != null)
                {
                    StopCoroutine(delayedDeathPresentationRoutine);
                }

                delayedDeathPresentationRoutine = StartCoroutine(ApplyDeathPresentationAfterDelay());
                return;
            }

            ApplyDeathPresentation();
        }

        /// <summary>
        /// Purpose: Applies death presentation after delay to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `IEnumerator` value.
        /// </summary>
        /// <returns>a `IEnumerator` value.</returns>
        private IEnumerator ApplyDeathPresentationAfterDelay()
        {
            yield return new WaitForSeconds(deathPresentationDelay);
            delayedDeathPresentationRoutine = null;
            ApplyDeathPresentation();
        }

        /// <summary>
        /// Purpose: Applies death presentation to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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
                ApplyDeathColliderPresentation();
            }
        }

        /// <summary>
        /// Purpose: Applies death collider presentation to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void ApplyDeathColliderPresentation()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        /// <summary>
        /// Purpose: Performs restore alive presentation for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void RestoreAlivePresentation()
        {
            if (delayedDeathPresentationRoutine != null)
            {
                StopCoroutine(delayedDeathPresentationRoutine);
                delayedDeathPresentationRoutine = null;
            }

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

        /// <summary>
        /// Purpose: Handles the died event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        protected virtual void OnDied()
        {
            Debug.Log($"[CharacterBase] {name} died.");
            AudioManager.Instance?.PlayCharacterDeathSFX();
            Died?.Invoke(this);
        }
    }
}
