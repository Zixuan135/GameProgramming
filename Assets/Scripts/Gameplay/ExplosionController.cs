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

        [Header("Explosion Visual")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Vector3 startScale = new Vector3(0.35f, 0.35f, 0.35f);
        [SerializeField] private Vector3 peakScale = new Vector3(1.15f, 1.15f, 1.15f);
        [SerializeField] private Renderer[] pulseRenderers = new Renderer[0];
        [SerializeField] private Color pulseEmissionColor = new Color(1f, 0.72f, 0.18f);
        [SerializeField, Min(0f)] private float maxEmissionIntensity = 1.4f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField, Min(0f)] private float rotationDegreesPerSecond = 0f;

        [Header("Runtime")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private float elapsedSeconds;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private int range;
        private CharacterBase owner;
        private bool initialized;
        private Quaternion baseLocalRotation;
        private MaterialPropertyBlock pulsePropertyBlock;

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

            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                pulseRenderers = GetComponentsInChildren<Renderer>();
            }

            baseLocalRotation = visualRoot.localRotation;
            pulsePropertyBlock = new MaterialPropertyBlock();
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
            float easedPulse = Mathf.SmoothStep(0f, 1f, pulse);
            visualRoot.localScale = Vector3.Lerp(startScale, peakScale, easedPulse);
            visualRoot.localRotation = baseLocalRotation * Quaternion.AngleAxis(
                rotationDegreesPerSecond * elapsedSeconds,
                rotationAxis == Vector3.zero ? Vector3.up : rotationAxis.normalized);
            ApplyPulseEmission(easedPulse);
        }

        private void ApplyPulseEmission(float pulse)
        {
            if (pulsePropertyBlock == null)
            {
                pulsePropertyBlock = new MaterialPropertyBlock();
            }

            Color emissionColor = pulseEmissionColor * pulse * maxEmissionIntensity;
            for (int i = 0; i < pulseRenderers.Length; i++)
            {
                Renderer pulseRenderer = pulseRenderers[i];
                if (pulseRenderer == null)
                {
                    continue;
                }

                pulseRenderer.GetPropertyBlock(pulsePropertyBlock);
                pulsePropertyBlock.SetColor(EmissionColorId, emissionColor);
                pulseRenderer.SetPropertyBlock(pulsePropertyBlock);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTriggerHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandleTriggerHit(other);
        }

        private void HandleTriggerHit(Collider other)
        {
            CharacterBase character = other.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.OnHitByExplosion();
            }

            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null)
            {
                bomb.TryTriggerChainExplosion(this);
            }
        }
    }
}
