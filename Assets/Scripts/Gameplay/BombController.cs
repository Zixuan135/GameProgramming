using BubbleTown.Characters;
using BubbleTown.Core;
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

        private void Awake()
        {
            CacheVisualReferences();
            ResetFuseTimer();
        }

        public void Initialize(CharacterBase bombOwner, int bombRange)
        {
            MapManager ownerMapManager = bombOwner != null ? bombOwner.MapManager : null;
            Vector2Int ownerGridPosition = bombOwner != null ? bombOwner.CurrentGridPosition : Vector2Int.zero;
            Initialize(bombOwner, ownerMapManager, ownerGridPosition, bombRange);
        }

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

        private void Start()
        {
            BeginCountdown();
        }

        private void Update()
        {
            TickFuseTimer();
            UpdateCountdownVisualFeedback();
        }

        public void BeginCountdown()
        {
            if (exploded)
            {
                return;
            }

            remainingFuseSeconds = Mathf.Max(0.1f, remainingFuseSeconds);
            isCountingDown = true;
        }

        public void StopCountdown()
        {
            isCountingDown = false;
        }

        private void ResetFuseTimer()
        {
            remainingFuseSeconds = Mathf.Max(0.1f, fuseSeconds);
            isCountingDown = false;
        }

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

        public void TriggerChainExplosion()
        {
            TryTriggerChainExplosion(null);
        }

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

        public bool TryTriggerExplosion()
        {
            if (exploded)
            {
                return false;
            }

            TriggerExplosion();
            return true;
        }

        public void TriggerExplosion()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;
            isCountingDown = false;
            remainingFuseSeconds = 0f;
            ResetCountdownVisualFeedback();
            ReleaseGridOccupation();
            SpawnExplosion();
            NotifyOwnerBombEnded();
            Destroy(gameObject);
        }

        public void Explode()
        {
            TriggerExplosion();
        }

        protected virtual void SpawnExplosion()
        {
            if (!HasAnyExplosionPrefab())
            {
                return;
            }

            EnsureMapManager();
            SpawnExplosionCell(gridPosition, ResolveExplosionPrefab(Vector2Int.zero));

            for (int i = 0; i < ExplosionDirections.Length; i++)
            {
                SpawnExplosionLine(ExplosionDirections[i]);
            }
        }

        private void SpawnExplosionLine(Vector2Int direction)
        {
            for (int distance = 1; distance <= range; distance++)
            {
                Vector2Int targetGridPosition = gridPosition + direction * distance;
                if (!TryGetExplosionCell(targetGridPosition, out GridCell targetCell))
                {
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
                    DestroySoftWallAt(targetGridPosition);
                    break;
                }
            }
        }

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

        private bool HasAnyExplosionPrefab()
        {
            return explosionPrefab != null ||
                   explosionCenterPrefab != null ||
                   explosionHorizontalPrefab != null ||
                   explosionVerticalPrefab != null;
        }

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

        private bool ShouldStopPropagationAfterCell(GridCell cell)
        {
            return cell == null || cell.IsSoftWall;
        }

        private void DestroySoftWallAt(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return;
            }

            mapManager.DestroySoftWall(targetGridPosition);
        }

        private void NotifyHardWallBlockedExplosion(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return;
            }

            mapManager.PlayHardWallBlockedFeedback(targetGridPosition, GridToWorld(gridPosition));
        }

        private bool IsHardWall(GridCell cell)
        {
            return cell.IsHardWall;
        }

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

        private void EnsureMapManager()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }
        }

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
            float flashPulse = intervalProgress <= flashOnRatio ? 1f : 0f;
            float urgentGlow = Mathf.Lerp(0.35f, 1.25f, fuseProgress);

            ApplyCountdownFlash(flashPulse, urgentGlow);
        }

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

        private void ResetCountdownVisualFeedback()
        {
            if (flashPropertyBlock == null)
            {
                return;
            }

            ApplyCountdownFlash(0f, 0f);
        }

        private void OnDestroy()
        {
            ResetCountdownVisualFeedback();
            ReleaseGridOccupation();
            NotifyOwnerBombEnded();
        }

        private void ReleaseGridOccupation()
        {
            if (gridOccupationReleased)
            {
                return;
            }

            EnsureMapManager();
            if (hasGridOccupation && mapManager != null && mapManager.IsInsideBounds(gridPosition))
            {
                mapManager.RemoveBomb(gridPosition);
            }

            hasGridOccupation = false;
            gridOccupationReleased = true;
        }

        private bool HasPlacedBombOccupation()
        {
            if (mapManager == null || !mapManager.IsInsideBounds(gridPosition))
            {
                return false;
            }

            GridCell cell = mapManager.GetCell(gridPosition);
            return cell != null && cell.HasBomb;
        }

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
