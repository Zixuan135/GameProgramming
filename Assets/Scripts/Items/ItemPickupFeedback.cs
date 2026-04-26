using System;
using System.Collections;
using BubbleTown.Characters;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Visual/audio pickup feedback for item prefabs.
    /// It animates only the visual child and disables pickup colliders, leaving grid cleanup in ItemBase.
    /// </summary>
    public class ItemPickupFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] pulseRenderers = new Renderer[0];

        [Header("Disappear Animation")]
        [SerializeField, Min(0.01f)] private float pickupDuration = 0.32f;
        [SerializeField, Min(0f)] private float riseHeight = 0.45f;
        [SerializeField, Range(0f, 0.75f)] private float popScaleAmount = 0.28f;
        [SerializeField, Min(0f)] private float spinDegrees = 210f;
        [SerializeField] private bool disableCollidersOnPickup = true;

        [Header("Glow Pulse")]
        [SerializeField] private Color pickupEmissionColor = new Color(1f, 0.92f, 0.35f);
        [SerializeField, Min(0f)] private float maxEmissionIntensity = 1.8f;

        [Header("Audio Hook")]
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.85f;
        [SerializeField] private bool playClipAtWorldPosition = true;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock propertyBlock;
        private Coroutine feedbackRoutine;
        private bool isPlaying;

        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            ResolveReferences();
        }

        public void PlayPickupFeedback(CharacterBase collector, ItemType itemType, Action onComplete)
        {
            if (isPlaying)
            {
                return;
            }

            ResolveReferences();
            DisableIdleVisualAnimation();
            DisablePickupColliders();
            PlayPickupAudio();

            feedbackRoutine = StartCoroutine(PickupRoutine(onComplete));
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
        }

        private void DisableIdleVisualAnimation()
        {
            ItemVisualAnimator visualAnimator = GetComponent<ItemVisualAnimator>();
            if (visualAnimator != null)
            {
                visualAnimator.enabled = false;
            }
        }

        private void DisablePickupColliders()
        {
            if (!disableCollidersOnPickup)
            {
                return;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void PlayPickupAudio()
        {
            if (pickupClip == null)
            {
                return;
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(pickupClip, pickupVolume);
                return;
            }

            if (playClipAtWorldPosition)
            {
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
            }
        }

        private IEnumerator PickupRoutine(Action onComplete)
        {
            isPlaying = true;

            if (visualRoot == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            Vector3 startPosition = visualRoot.localPosition;
            Vector3 startScale = visualRoot.localScale;
            Quaternion startRotation = visualRoot.localRotation;
            float elapsed = 0f;

            while (elapsed < pickupDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / pickupDuration);
                float pop = Mathf.Sin(normalizedTime * Mathf.PI);
                float shrink = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((normalizedTime - 0.32f) / 0.68f));
                float scaleMultiplier = Mathf.Max(0.01f, (1f + pop * popScaleAmount) * shrink);

                visualRoot.localPosition = startPosition + Vector3.up * (riseHeight * Mathf.SmoothStep(0f, 1f, normalizedTime));
                visualRoot.localScale = startScale * scaleMultiplier;
                visualRoot.localRotation = startRotation * Quaternion.Euler(0f, spinDegrees * normalizedTime, 0f);
                ApplyEmission(pop);

                yield return null;
            }

            visualRoot.localScale = Vector3.zero;
            ApplyEmission(0f);
            isPlaying = false;
            feedbackRoutine = null;
            onComplete?.Invoke();
        }

        private void ApplyEmission(float pulse)
        {
            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                return;
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            Color emissionColor = pickupEmissionColor * pulse * maxEmissionIntensity;
            for (int i = 0; i < pulseRenderers.Length; i++)
            {
                Renderer pulseRenderer = pulseRenderers[i];
                if (pulseRenderer == null)
                {
                    continue;
                }

                pulseRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                pulseRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnDisable()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                feedbackRoutine = null;
            }

            isPlaying = false;
            ApplyEmission(0f);
        }
    }
}
