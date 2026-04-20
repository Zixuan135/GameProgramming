using System.Collections;
using UnityEngine;

namespace BubbleTown
{
    public class ExplosionController : MonoBehaviour
    {
        [SerializeField] private float lifetime = GameConstants.DefaultExplosionLifetime;
        [SerializeField] private LayerMask hitMask;

        private CharacterBase owner;
        private int range;
        private MapManager mapManager;

        public void Initialize(CharacterBase ownerCharacter, int explosionRange)
        {
            owner = ownerCharacter;
            range = Mathf.Max(1, explosionRange);
            mapManager = FindObjectOfType<MapManager>();

            ApplyExplosion();
            StartCoroutine(DestroyAfterLifetime());
        }

        private void ApplyExplosion()
        {
            if (mapManager == null)
            {
                HitAt(transform.position);
                return;
            }

            Vector2Int centerCell = mapManager.WorldToCell(transform.position);
            HitAt(mapManager.CellToWorld(centerCell));

            foreach (Vector2Int direction in GameConstants.CardinalDirections)
            {
                for (int step = 1; step <= range; step++)
                {
                    Vector2Int cell = centerCell + direction * step;
                    if (!mapManager.IsInsideMap(cell))
                    {
                        break;
                    }

                    Vector3 worldPos = mapManager.CellToWorld(cell);
                    if (HitAt(worldPos))
                    {
                        // Placeholder stop rule: if something blocks explosion, stop here in this direction.
                        break;
                    }
                }
            }
        }

        private bool HitAt(Vector3 worldPos)
        {
            bool stopPropagation = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.3f, hitMask);

            foreach (Collider2D hit in hits)
            {
                CharacterBase character = hit.GetComponent<CharacterBase>();
                if (character != null)
                {
                    character.TakeDamage();
                    continue;
                }

                BombController bomb = hit.GetComponent<BombController>();
                if (bomb != null)
                {
                    bomb.TriggerNow();
                    continue;
                }

                if (hit.CompareTag(GameConstants.HardWallTag) || hit.CompareTag(GameConstants.SoftWallTag))
                {
                    stopPropagation = true;
                }
            }

            return stopPropagation;
        }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(gameObject);
        }
    }
}
