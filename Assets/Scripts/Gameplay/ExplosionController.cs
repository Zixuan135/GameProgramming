using BubbleTown.Characters;
using BubbleTown.Core;
using UnityEngine;

namespace BubbleTown.Gameplay
{
    /// <summary>
    /// Represents an explosion event.
    /// Full grid propagation and wall-blocking logic will be added later.
    /// </summary>
    public class ExplosionController : MonoBehaviour
    {
        [SerializeField] private float lifeSeconds = GameConstants.DefaultExplosionDuration;

        private int range;
        private CharacterBase owner;

        public void Initialize(int explosionRange, CharacterBase explosionOwner)
        {
            range = Mathf.Max(1, explosionRange);
            owner = explosionOwner;
            Debug.Log($"[ExplosionController] Placeholder explosion. Range: {range}, Owner: {(owner != null ? owner.name : "None")}");
        }

        private void Start()
        {
            Destroy(gameObject, lifeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.OnHitByExplosion();
            }

            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null)
            {
                bomb.TriggerChainExplosion();
            }
        }
    }
}
