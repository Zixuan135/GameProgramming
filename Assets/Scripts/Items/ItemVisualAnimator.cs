using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Lightweight placeholder animation for item visuals.
    /// It only animates the replaceable visual child so pickup colliders and grid state stay stable.
    /// </summary>
    public class ItemVisualAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] pulseRenderers = new Renderer[0];

        [Header("Float")]
        [SerializeField] private bool enableFloat = true;
        [SerializeField, Min(0f)] private float floatAmplitude = 0.08f;
        [SerializeField, Min(0.1f)] private float floatSpeed = 2.6f;

        [Header("Rotation")]
        [SerializeField] private Vector3 rotationDegreesPerSecond = new Vector3(0f, 90f, 0f);

        [Header("Pulse")]
        [SerializeField, Range(0f, 0.3f)] private float scalePulseAmount = 0.05f;
        [SerializeField, Min(0.1f)] private float scalePulseSpeed = 3.4f;
        [SerializeField] private Color pulseEmissionColor = new Color(0.45f, 1f, 0.95f);
        [SerializeField, Min(0f)] private float maxEmissionIntensity = 1.15f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private Quaternion baseLocalRotation;
        private float elapsedSeconds;
        private float timeOffset;
        private bool hasCachedBasePose;
        private MaterialPropertyBlock pulsePropertyBlock;

        private void Awake()
        {
            ResolveReferences();
            CacheBasePose();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheBasePose();
        }

        private void OnDisable()
        {
            RestoreBasePose();
            ApplyPulseEmission(0f);
        }

        private void Update()
        {
            if (visualRoot == null)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            UpdateVisualPose();
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                Transform childVisualRoot = transform.Find("VisualRoot");
                visualRoot = childVisualRoot != null ? childVisualRoot : transform;
            }

            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                pulseRenderers = GetComponentsInChildren<Renderer>();
            }

            if (Mathf.Approximately(timeOffset, 0f))
            {
                timeOffset = Random.Range(0f, 10f);
            }
        }

        private void CacheBasePose()
        {
            if (hasCachedBasePose || visualRoot == null)
            {
                return;
            }

            baseLocalPosition = visualRoot.localPosition;
            baseLocalScale = visualRoot.localScale;
            baseLocalRotation = visualRoot.localRotation;
            hasCachedBasePose = true;
        }

        private void UpdateVisualPose()
        {
            float animationTime = elapsedSeconds + timeOffset;
            float bob = enableFloat ? Mathf.Sin(animationTime * floatSpeed) * floatAmplitude : 0f;
            float pulse = (Mathf.Sin(animationTime * scalePulseSpeed) + 1f) * 0.5f;
            float scaleMultiplier = 1f + pulse * scalePulseAmount;

            visualRoot.localPosition = baseLocalPosition + Vector3.up * bob;
            visualRoot.localScale = baseLocalScale * scaleMultiplier;
            visualRoot.localRotation = baseLocalRotation * Quaternion.Euler(rotationDegreesPerSecond * elapsedSeconds);
            ApplyPulseEmission(pulse);
        }

        private void ApplyPulseEmission(float pulse)
        {
            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                return;
            }

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

        private void RestoreBasePose()
        {
            if (!hasCachedBasePose || visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = baseLocalPosition;
            visualRoot.localScale = baseLocalScale;
            visualRoot.localRotation = baseLocalRotation;
        }
    }
}
