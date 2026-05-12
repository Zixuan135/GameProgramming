using BubbleTown.Characters;
using BubbleTown.CameraSystem;
using BubbleTown.Core;
using BubbleTown.Managers;
using BubbleTown.Map;
using UnityEngine;

namespace BubbleTown.Gameplay
{
    /// <summary>
    /// Controls bomb timer and triggers explosion.
    /// Chain reaction support can trigger the same explosion flow early.
    /// </summary>
    public class BombController : MonoBehaviour
    {
        private static readonly Vector2Int[] ExplosionDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        [Header("Bomb")]
        [SerializeField, Min(0.1f)] private float fuseSeconds = GameConstants.DefaultBombFuseSeconds;
        [SerializeField] private ExplosionController explosionPrefab;
        [SerializeField] private ExplosionController explosionCenterPrefab;
        [SerializeField] private ExplosionController explosionHorizontalPrefab;
        [SerializeField] private ExplosionController explosionVerticalPrefab;

        [Header("Grid")]
        [SerializeField] private Vector2Int gridPosition;

        [Header("Visual Countdown Feedback")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] flashRenderers = new Renderer[0];
        [SerializeField] private Color flashEmissionColor = new Color(1f, 0.55f, 0.12f);
        [SerializeField, Min(0.01f)] private float slowFlashInterval = 0.55f;
        [SerializeField, Min(0.01f)] private float fastFlashInterval = 0.08f;
        [SerializeField, Range(0.05f, 0.95f)] private float flashOnRatio = 0.42f;
        [SerializeField, Range(0f, 0.25f)] private float flashScalePulse = 0.1f;

        [Header("Runtime")]
        [SerializeField] private float remainingFuseSeconds;
        [SerializeField] private bool isCountingDown;
        [SerializeField] private bool triggeredByChainExplosion;

        [Header("Camera Feedback")]
        [SerializeField] private bool shakeCameraOnExplosion = true;
        [SerializeField, Min(0f)] private float explosionCameraShakeDuration = 0.16f;
        [SerializeField, Min(0f)] private float explosionCameraShakeMagnitude = 0.16f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private CharacterBase owner;
        private MapManager mapManager;
        private int range;
        private bool exploded;
        private bool hasGridOccupation;
        private bool gridOccupationReleased;
        private bool ownerNotified;
        private MaterialPropertyBlock flashPropertyBlock;
        private Vector3 baseVisualScale = Vector3.one;

        public CharacterBase Owner => owner;
        public Vector2Int GridPosition => gridPosition;
        public int Range => range;
        public float FuseSeconds => fuseSeconds;
        public float RemainingFuseSeconds => remainingFuseSeconds;
        public bool IsCountingDown => isCountingDown;
        public bool HasExploded => exploded;
        public bool TriggeredByChainExplosion => triggeredByChainExplosion;

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            CacheVisualReferences();
            ResetFuseTimer();
        }

        /// <summary>
        /// Purpose: Performs initialize for this component.
        /// Inputs: `bombOwner`, `bombRange`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bombOwner">Input value used by this method.</param>
        /// <param name="bombRange">Input value used by this method.</param>
        public void Initialize(CharacterBase bombOwner, int bombRange)
        {
            MapManager ownerMapManager = bombOwner != null ? bombOwner.MapManager : null;
            Vector2Int ownerGridPosition = bombOwner != null ? bombOwner.CurrentGridPosition : Vector2Int.zero;
            Initialize(bombOwner, ownerMapManager, ownerGridPosition, bombRange);
        }

        /// <summary>
        /// Purpose: Performs initialize for this component.
        /// Inputs: `bombOwner`, `ownerMapManager`, `bombGridPosition`, `bombRange`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="bombOwner">Input value used by this method.</param>
        /// <param name="ownerMapManager">Input value used by this method.</param>
        /// <param name="bombGridPosition">Input value used by this method.</param>
        /// <param name="bombRange">Input value used by this method.</param>
        public void Initialize(CharacterBase bombOwner, MapManager ownerMapManager, Vector2Int bombGridPosition, int bombRange)
        {
            owner = bombOwner;
            mapManager = ownerMapManager;
            gridPosition = bombGridPosition;
            range = Mathf.Max(1, bombRange);
            exploded = false;
            triggeredByChainExplosion = false;
            gridOccupationReleased = false;
            ownerNotified = false;
            hasGridOccupation = HasPlacedBombOccupation();
            ResetFuseTimer();
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            BeginCountdown();
        }

        /// <summary>
        /// Purpose: Runs this component's per-frame logic.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Update()
        {
            TickFuseTimer();
            UpdateCountdownVisualFeedback();
        }

        /// <summary>
        /// Purpose: Begins countdown.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void BeginCountdown()
        {
            if (exploded)
            {
                return;
            }

            remainingFuseSeconds = Mathf.Max(0.1f, remainingFuseSeconds);
            isCountingDown = true;
        }

        /// <summary>
        /// Purpose: Stops countdown.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void StopCountdown()
        {
            isCountingDown = false;
        }

        /// <summary>
        /// Purpose: Resets fuse timer to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResetFuseTimer()
        {
            remainingFuseSeconds = Mathf.Max(0.1f, fuseSeconds);
            isCountingDown = false;
        }

        /// <summary>
        /// Purpose: Advances fuse timer by one update step.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void TickFuseTimer()
        {
            if (!isCountingDown || exploded)
            {
                return;
            }

            remainingFuseSeconds = Mathf.Max(0f, remainingFuseSeconds - Time.deltaTime);
            if (remainingFuseSeconds > 0f)
            {
                return;
            }

            TriggerExplosion();
        }

        /// <summary>
        /// Purpose: Performs trigger chain explosion for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void TriggerChainExplosion()
        {
            TryTriggerChainExplosion(null);
        }

        /// <summary>
        /// Purpose: Attempts to trigger chain explosion.
        /// Inputs: `sourceExplosion`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="sourceExplosion">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public bool TryTriggerChainExplosion(ExplosionController sourceExplosion)
        {
            if (exploded)
            {
                return false;
            }

            triggeredByChainExplosion = true;
            string sourceGrid = sourceExplosion != null ? sourceExplosion.GridPosition.ToString() : "Unknown";
            Debug.Log($"[BombController] Chain explosion triggered at {gridPosition} by explosion cell {sourceGrid}.");
            return TryTriggerExplosion();
        }

        /// <summary>
        /// Purpose: Attempts to trigger explosion.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        public bool TryTriggerExplosion()
        {
            if (exploded)
            {
                return false;
            }

            TriggerExplosion();
            return true;
        }

        /// <summary>
        /// Purpose: Runs the complete explosion flow for this bomb once, either from the fuse or a chain reaction.
        /// Inputs: no direct parameters; reads owner, map position, range, explosion prefabs, and audio/camera settings.
        /// Output: no return value; releases map occupancy, spawns explosion cells, notifies the owner, and destroys this bomb.
        /// </summary>
        public void TriggerExplosion()
        {
            if (exploded)
            {
                return;
            }

            // Set the guard flag before spawning explosions so chain reactions cannot re-enter this flow.
            exploded = true;
            isCountingDown = false;
            remainingFuseSeconds = 0f;
            ResetCountdownVisualFeedback();
            ReleaseGridOccupation();
            AudioManager.Instance?.PlayExplosionSFX();
            PlayExplosionCameraFeedback();
            SpawnExplosion();
            NotifyOwnerBombEnded();
            Destroy(gameObject);
        }

        /// <summary>
        /// Purpose: Performs explode for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void Explode()
        {
            TriggerExplosion();
        }

        /// <summary>
        /// Purpose: Spawns the center explosion cell and then expands cross-shaped blast lines.
        /// Inputs: no direct parameters; reads the bomb grid position, range, map data, and explosion prefab references.
        /// Output: no return value; creates temporary ExplosionController instances in reachable cells.
        /// </summary>
        protected virtual void SpawnExplosion()
        {
            if (!HasAnyExplosionPrefab())
            {
                return;
            }

            EnsureMapManager();
            // The center always appears, even when every direction is immediately blocked.
            SpawnExplosionCell(gridPosition, ResolveExplosionPrefab(Vector2Int.zero));

            for (int i = 0; i < ExplosionDirections.Length; i++)
            {
                SpawnExplosionLine(ExplosionDirections[i]);
            }
        }

        /// <summary>
        /// Purpose: Propagates one arm of the cross-shaped explosion in a single grid direction.
        /// Inputs: direction is one of the four cardinal grid directions.
        /// Output: no return value; stops at bounds, hard walls, or after destroying a soft wall.
        /// </summary>
        /// <param name="direction">Cardinal grid direction used for this explosion arm.</param>
        private void SpawnExplosionLine(Vector2Int direction)
        {
            for (int distance = 1; distance <= range; distance++)
            {
                Vector2Int targetGridPosition = gridPosition + direction * distance;
                if (!TryGetExplosionCell(targetGridPosition, out GridCell targetCell))
                {
                    // Leaving the map stops only this arm; other directions keep propagating.
                    break;
                }

                if (IsHardWall(targetCell))
                {
                    NotifyHardWallBlockedExplosion(targetGridPosition);
                    break;
                }

                SpawnExplosionCell(targetGridPosition, ResolveExplosionPrefab(direction));

                if (ShouldStopPropagationAfterCell(targetCell))
                {
                    // Soft walls receive the blast, are destroyed, and block any cells behind them.
                    DestroySoftWallAt(targetGridPosition);
                    break;
                }
            }
        }

        /// <summary>
        /// Purpose: Spawns explosion cell.
        /// Inputs: `targetGridPosition`, `prefab`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="targetGridPosition">Input value used by this method.</param>
        /// <param name="prefab">Input value used by this method.</param>
        private void SpawnExplosionCell(Vector2Int targetGridPosition, ExplosionController prefab)
        {
            if (prefab == null)
            {
                return;
            }

            Vector3 spawnPosition = GridToWorld(targetGridPosition);
            ExplosionController explosion = Instantiate(prefab, spawnPosition, Quaternion.identity);
            explosion.Initialize(range, owner, targetGridPosition);
        }

        /// <summary>
        /// Purpose: Returns whether this object has any explosion prefab.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool HasAnyExplosionPrefab()
        {
            return explosionPrefab != null ||
                   explosionCenterPrefab != null ||
                   explosionHorizontalPrefab != null ||
                   explosionVerticalPrefab != null;
        }

        /// <summary>
        /// Purpose: Resolves explosion prefab from the current runtime state.
        /// Inputs: `direction`; may also read serialized fields and current runtime state.
        /// Output: a `ExplosionController` value.
        /// </summary>
        /// <param name="direction">Input value used by this method.</param>
        /// <returns>a `ExplosionController` value.</returns>
        private ExplosionController ResolveExplosionPrefab(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
            {
                return explosionCenterPrefab != null ? explosionCenterPrefab : explosionPrefab;
            }

            if (direction.x != 0)
            {
                return explosionHorizontalPrefab != null ? explosionHorizontalPrefab : explosionPrefab;
            }

            return explosionVerticalPrefab != null ? explosionVerticalPrefab : explosionPrefab;
        }

        /// <summary>
        /// Purpose: Attempts to get explosion cell.
        /// Inputs: `targetGridPosition`, `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="targetGridPosition">Input value used by this method.</param>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool TryGetExplosionCell(Vector2Int targetGridPosition, out GridCell cell)
        {
            cell = null;
            if (mapManager == null)
            {
                return false;
            }

            if (!mapManager.IsInsideBounds(targetGridPosition))
            {
                return false;
            }

            cell = mapManager.GetCell(targetGridPosition);
            return cell != null;
        }

        /// <summary>
        /// Purpose: Returns whether this object should stop propagation after cell.
        /// Inputs: `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool ShouldStopPropagationAfterCell(GridCell cell)
        {
            return cell == null || cell.IsSoftWall;
        }

        /// <summary>
        /// Purpose: Destroys soft wall at.
        /// Inputs: `targetGridPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="targetGridPosition">Input value used by this method.</param>
        private void DestroySoftWallAt(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return;
            }

            mapManager.DestroySoftWall(targetGridPosition);
        }

        /// <summary>
        /// Purpose: Performs notify hard wall blocked explosion for this component.
        /// Inputs: `targetGridPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="targetGridPosition">Input value used by this method.</param>
        private void NotifyHardWallBlockedExplosion(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return;
            }

            mapManager.PlayHardWallBlockedFeedback(targetGridPosition, GridToWorld(gridPosition));
        }

        /// <summary>
        /// Purpose: Plays explosion camera feedback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void PlayExplosionCameraFeedback()
        {
            if (!shakeCameraOnExplosion)
            {
                return;
            }

            CameraController.ShakeActiveCamera(explosionCameraShakeDuration, explosionCameraShakeMagnitude);
        }

        /// <summary>
        /// Purpose: Returns whether this object is hard wall.
        /// Inputs: `cell`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="cell">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsHardWall(GridCell cell)
        {
            return cell.IsHardWall;
        }

        /// <summary>
        /// Purpose: Returns grid to world for the current state.
        /// Inputs: `targetGridPosition`; may also read serialized fields and current runtime state.
        /// Output: a `Vector3` value.
        /// </summary>
        /// <param name="targetGridPosition">Input value used by this method.</param>
        /// <returns>a `Vector3` value.</returns>
        private Vector3 GridToWorld(Vector2Int targetGridPosition)
        {
            if (mapManager != null)
            {
                return mapManager.GridToWorld(targetGridPosition, transform.position.y);
            }

            return new Vector3(
                targetGridPosition.x * GameConstants.GridCellSize,
                transform.position.y,
                targetGridPosition.y * GameConstants.GridCellSize);
        }

        /// <summary>
        /// Purpose: Ensures map manager exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureMapManager()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }
        }

        /// <summary>
        /// Purpose: Performs cache visual references for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void CacheVisualReferences()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            baseVisualScale = visualRoot.localScale;
            if (flashRenderers == null || flashRenderers.Length == 0)
            {
                flashRenderers = GetComponentsInChildren<Renderer>();
            }

            flashPropertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Purpose: Updates bomb flash and scale feedback based on remaining fuse time.
        /// Inputs: no direct parameters; reads fuse progress, flash timing, renderers, and visual root.
        /// Output: no return value; makes the bomb blink faster as it gets closer to exploding.
        /// </summary>
        private void UpdateCountdownVisualFeedback()
        {
            if (!isCountingDown || exploded)
            {
                ResetCountdownVisualFeedback();
                return;
            }

            if (flashPropertyBlock == null)
            {
                CacheVisualReferences();
            }

            float fuseProgress = 1f - Mathf.Clamp01(remainingFuseSeconds / Mathf.Max(0.01f, fuseSeconds));
            float flashInterval = Mathf.Lerp(slowFlashInterval, fastFlashInterval, fuseProgress);
            float intervalProgress = Mathf.Repeat(Time.time, flashInterval) / flashInterval;
            // A square pulse is intentionally readable for a cartoony bomb warning.
            float flashPulse = intervalProgress <= flashOnRatio ? 1f : 0f;
            float urgentGlow = Mathf.Lerp(0.35f, 1.25f, fuseProgress);

            ApplyCountdownFlash(flashPulse, urgentGlow);
        }

        /// <summary>
        /// Purpose: Applies countdown flash to the current object or scene.
        /// Inputs: `flashPulse`, `urgentGlow`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="flashPulse">Input value used by this method.</param>
        /// <param name="urgentGlow">Input value used by this method.</param>
        private void ApplyCountdownFlash(float flashPulse, float urgentGlow)
        {
            Color emissionColor = flashEmissionColor * flashPulse * urgentGlow;
            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer flashRenderer = flashRenderers[i];
                if (flashRenderer == null)
                {
                    continue;
                }

                flashRenderer.GetPropertyBlock(flashPropertyBlock);
                flashPropertyBlock.SetColor(EmissionColorId, emissionColor);
                flashRenderer.SetPropertyBlock(flashPropertyBlock);
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = baseVisualScale * (1f + flashPulse * flashScalePulse);
            }
        }

        /// <summary>
        /// Purpose: Resets countdown visual feedback to a safe default state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResetCountdownVisualFeedback()
        {
            if (flashPropertyBlock == null)
            {
                return;
            }

            ApplyCountdownFlash(0f, 0f);
        }

        /// <summary>
        /// Purpose: Cleans up runtime state before Unity destroys this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDestroy()
        {
            ResetCountdownVisualFeedback();
            ReleaseGridOccupation();
            NotifyOwnerBombEnded();
        }

        /// <summary>
        /// Purpose: Performs release grid occupation for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ReleaseGridOccupation()
        {
            if (gridOccupationReleased)
            {
                return;
            }

            EnsureMapManager();
            if (hasGridOccupation && mapManager != null && mapManager.IsInsideBounds(gridPosition))
            {
                // Remove the blocking flag even if this bomb was destroyed by another explosion.
                mapManager.RemoveBomb(gridPosition);
            }

            hasGridOccupation = false;
            gridOccupationReleased = true;
        }

        /// <summary>
        /// Purpose: Returns whether this object has placed bomb occupation.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        private bool HasPlacedBombOccupation()
        {
            if (mapManager == null || !mapManager.IsInsideBounds(gridPosition))
            {
                return false;
            }

            GridCell cell = mapManager.GetCell(gridPosition);
            return cell != null && cell.HasBomb;
        }

        /// <summary>
        /// Purpose: Performs notify owner bomb ended for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void NotifyOwnerBombEnded()
        {
            if (ownerNotified)
            {
                return;
            }

            owner?.OnBombExploded(this);
            ownerNotified = true;
        }
    }
}
