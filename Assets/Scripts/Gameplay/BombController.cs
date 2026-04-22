using BubbleTown.Characters;
using BubbleTown.Core;
using UnityEngine;

namespace BubbleTown.Gameplay
{
    /// <summary>
    /// Controls bomb timer and triggers explosion.
    /// Chain reaction support can call Explode() early.
    /// </summary>
    public class BombController : MonoBehaviour
    {
        [Header("Bomb")]
        [SerializeField] private float fuseSeconds = GameConstants.DefaultBombFuseSeconds;
        [SerializeField] private ExplosionController explosionPrefab;

        private CharacterBase owner;
        private int range;
        private bool exploded;

        public CharacterBase Owner => owner;
        public int Range => range;

        public void Initialize(CharacterBase bombOwner, int bombRange)
        {
            owner = bombOwner;
            range = Mathf.Max(1, bombRange);
        }

        private void Start()
        {
            Invoke(nameof(Explode), fuseSeconds);
        }

        public void TriggerChainExplosion()
        {
            Explode();
        }

        public void Explode()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;

            if (explosionPrefab != null)
            {
                ExplosionController explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                explosion.Initialize(range, owner);
            }

            owner?.OnBombExploded(this);
            Destroy(gameObject);
        }
    }
}
