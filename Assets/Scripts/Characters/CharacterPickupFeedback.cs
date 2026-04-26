using System.Collections;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Short visual-only character highlight when a power-up is collected.
    /// Uses MaterialPropertyBlock so placeholder materials do not need to be duplicated.
    /// </summary>
    public class CharacterPickupFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer[] flashRenderers = new Renderer[0];

        [Header("Flash")]
        [SerializeField, Min(0.05f)] private float flashDuration = 0.42f;
        [SerializeField, Min(0.1f)] private float flashFrequency = 10f;
        [SerializeField, Min(0f)] private float maxEmissionIntensity = 1.35f;

        [Header("Type Colors")]
        [SerializeField] private Color bombCountColor = new Color(0.25f, 0.9f, 1f);
        [SerializeField] private Color explosionRangeColor = new Color(1f, 0.58f, 0.12f);
        [SerializeField] private Color moveSpeedColor = new Color(0.48f, 1f, 0.28f);
        [SerializeField] private Color fallbackColor = new Color(1f, 0.94f, 0.55f);

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        public void PlayPickupFlash(ItemType itemType)
        {
            ResolveReferences();

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                ApplyEmission(Color.black);
            }

            flashRoutine = StartCoroutine(FlashRoutine(ResolveColor(itemType)));
        }

        private void ResolveReferences()
        {
            if (flashRenderers == null || flashRenderers.Length == 0)
            {
                flashRenderers = GetComponentsInChildren<Renderer>();
            }
        }

        private Color ResolveColor(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BombCountUp:
                    return bombCountColor;
                case ItemType.ExplosionRangeUp:
                    return explosionRangeColor;
                case ItemType.MoveSpeedUp:
                    return moveSpeedColor;
                default:
                    return fallbackColor;
            }
        }

        private IEnumerator FlashRoutine(Color flashColor)
        {
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / flashDuration);
                float fade = Mathf.Sin(normalizedTime * Mathf.PI);
                float blink = (Mathf.Sin(elapsed * flashFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
                ApplyEmission(flashColor * fade * (0.45f + blink * 0.55f) * maxEmissionIntensity);
                yield return null;
            }

            ApplyEmission(Color.black);
            flashRoutine = null;
        }

        private void ApplyEmission(Color emissionColor)
        {
            if (flashRenderers == null || flashRenderers.Length == 0)
            {
                return;
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                Renderer flashRenderer = flashRenderers[i];
                if (flashRenderer == null)
                {
                    continue;
                }

                flashRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                flashRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnDisable()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            ApplyEmission(Color.black);
        }
    }
}
