using BubbleTown.Characters;
using BubbleTown.Core;
using UnityEngine;

namespace BubbleTown.Gameplay
{
    /// <summary>
    /// Represents one explosion cell spawned by a bomb's grid-based cross propagation.
    /// </summary>
    public class ExplosionController : MonoBehaviour
    {
        [Header("Lifetime")]
        [SerializeField, Min(0.05f)] private float lifeSeconds = GameConstants.DefaultExplosionDuration;

        [Header("Placeholder Visual")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Vector3 startScale = new Vector3(0.35f, 0.35f, 0.35f);
        [SerializeField] private Vector3 peakScale = new Vector3(1.15f, 1.15f, 1.15f);

        [Header("Runtime")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private float elapsedSeconds;

        private int range;
        private CharacterBase owner;
        private bool initialized;

        public int Range => range;
        public CharacterBase Owner => owner;
        public Vector2Int GridPosition => gridPosition;
        public Vector2Int CenterGridPosition => gridPosition;
        public float LifeSeconds => lifeSeconds;
        public bool IsInitialized => initialized;

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            visualRoot.localScale = startScale;
        }

        public void Initialize(int explosionRange, CharacterBase explosionOwner)
        {
            Initialize(explosionRange, explosionOwner, Vector2Int.zero);
        }

        public void Initialize(int explosionRange, CharacterBase explosionOwner, Vector2Int explosionGridPosition)
        {
            range = Mathf.Max(1, explosionRange);
            owner = explosionOwner;
            gridPosition = explosionGridPosition;
            elapsedSeconds = 0f;
            initialized = true;
            Debug.Log($"[ExplosionController] Explosion cell. Grid: {gridPosition}, Range: {range}, Owner: {(owner != null ? owner.name : "None")}");
        }

        private void Update()
        {
            TickLifetime();
            UpdatePlaceholderVisual();
        }

        protected virtual void TickLifetime()
        {
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds >= lifeSeconds)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void UpdatePlaceholderVisual()
        {
            if (visualRoot == null)
            {
                return;
            }

            float normalizedTime = Mathf.Clamp01(elapsedSeconds / lifeSeconds);
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            visualRoot.localScale = Vector3.Lerp(startScale, peakScale, pulse);
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
