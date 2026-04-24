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

        [Header("Grid")]
        [SerializeField] private Vector2Int gridPosition;

        [Header("Runtime")]
        [SerializeField] private float remainingFuseSeconds;
        [SerializeField] private bool isCountingDown;

        private CharacterBase owner;
        private MapManager mapManager;
        private int range;
        private bool exploded;
        private bool gridOccupationReleased;
        private bool ownerNotified;

        public CharacterBase Owner => owner;
        public Vector2Int GridPosition => gridPosition;
        public int Range => range;
        public float FuseSeconds => fuseSeconds;
        public float RemainingFuseSeconds => remainingFuseSeconds;
        public bool IsCountingDown => isCountingDown;
        public bool HasExploded => exploded;

        private void Awake()
        {
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
            ResetFuseTimer();
        }

        private void Start()
        {
            BeginCountdown();
        }

        private void Update()
        {
            TickFuseTimer();
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
            TriggerExplosion();
        }

        public void Explode()
        {
            TriggerExplosion();
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
            ReleaseGridOccupation();
            SpawnExplosion();
            NotifyOwnerBombEnded();
            Destroy(gameObject);
        }

        protected virtual void SpawnExplosion()
        {
            if (explosionPrefab == null)
            {
                return;
            }

            SpawnExplosionCell(gridPosition);

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
                if (!CanExplosionEnterCell(targetGridPosition))
                {
                    break;
                }

                SpawnExplosionCell(targetGridPosition);

                if (ShouldStopPropagationAfterCell(targetGridPosition))
                {
                    break;
                }
            }
        }

        private void SpawnExplosionCell(Vector2Int targetGridPosition)
        {
            Vector3 spawnPosition = GridToWorld(targetGridPosition);
            ExplosionController explosion = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
            explosion.Initialize(range, owner, targetGridPosition);
        }

        private bool CanExplosionEnterCell(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return true;
            }

            GridCell cell = mapManager.GetCell(targetGridPosition);
            return cell != null && !cell.IsHardWall;
        }

        private bool ShouldStopPropagationAfterCell(Vector2Int targetGridPosition)
        {
            if (mapManager == null)
            {
                return false;
            }

            GridCell cell = mapManager.GetCell(targetGridPosition);
            return cell == null || cell.IsSoftWall;
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

        private void OnDestroy()
        {
            ReleaseGridOccupation();
            NotifyOwnerBombEnded();
        }

        private void ReleaseGridOccupation()
        {
            if (gridOccupationReleased)
            {
                return;
            }

            mapManager?.RemoveBomb(gridPosition);
            gridOccupationReleased = true;
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
