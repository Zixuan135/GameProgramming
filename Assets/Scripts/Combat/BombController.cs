using System;
using System.Collections;
using UnityEngine;

namespace BubbleTown
{
    public class BombController : MonoBehaviour
    {
        [SerializeField] private float fuseSeconds = GameConstants.DefaultBombFuseSeconds;
        [SerializeField] private ExplosionController explosionPrefab;

        public event Action<BombController> OnBombExploded;

        private CharacterBase owner;
        private int explosionRange;
        private bool isExploded;
        private Coroutine fuseCoroutine;

        public void Initialize(CharacterBase ownerCharacter, int range)
        {
            owner = ownerCharacter;
            explosionRange = Mathf.Max(1, range);
            fuseCoroutine = StartCoroutine(FuseRoutine());
        }

        public void TriggerNow()
        {
            if (isExploded)
            {
                return;
            }

            if (fuseCoroutine != null)
            {
                StopCoroutine(fuseCoroutine);
                fuseCoroutine = null;
            }

            Explode();
        }

        private IEnumerator FuseRoutine()
        {
            yield return new WaitForSeconds(fuseSeconds);
            Explode();
        }

        private void Explode()
        {
            if (isExploded)
            {
                return;
            }

            isExploded = true;

            if (explosionPrefab != null)
            {
                ExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                explosion.Initialize(owner, explosionRange);
            }

            OnBombExploded?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
