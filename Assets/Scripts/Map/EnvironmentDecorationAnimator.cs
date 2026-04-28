using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Tiny code-driven ambience animator for non-gameplay environment props.
    /// Use it only on decorative objects that live outside playable grid cells.
    /// </summary>
    public class EnvironmentDecorationAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform animatedRoot;

        [Header("Bob")]
        [SerializeField] private bool enableBob = true;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.04f;
        [SerializeField, Min(0.1f)] private float bobSpeed = 2.2f;

        [Header("Rotation")]
        [SerializeField] private Vector3 rotationDegreesPerSecond;

        [Header("Scale Pulse")]
        [SerializeField] private bool enableScalePulse;
        [SerializeField, Range(0f, 0.25f)] private float scalePulseAmount = 0.04f;
        [SerializeField, Min(0.1f)] private float scalePulseSpeed = 2.6f;

        private Vector3 originalLocalPosition;
        private Vector3 originalLocalScale;
        private Quaternion originalLocalRotation;
        private Vector3 spinEuler;
        private float phaseOffset;

        private void Awake()
        {
            CacheOriginalTransform();
            phaseOffset = Mathf.Abs(GetInstanceID() % 1000) * 0.037f;
        }

        private void OnEnable()
        {
            CacheOriginalTransform();
        }

        private void Update()
        {
            if (animatedRoot == null)
            {
                return;
            }

            float time = Time.time + phaseOffset;
            ApplyBob(time);
            ApplyRotation();
            ApplyScalePulse(time);
        }

        public void Configure(
            Transform root,
            bool bob,
            float bobAmount,
            float bobRate,
            Vector3 spinRate,
            bool scalePulse,
            float pulseAmount,
            float pulseRate)
        {
            animatedRoot = root != null ? root : transform;
            enableBob = bob;
            bobAmplitude = Mathf.Max(0f, bobAmount);
            bobSpeed = Mathf.Max(0.1f, bobRate);
            rotationDegreesPerSecond = spinRate;
            enableScalePulse = scalePulse;
            scalePulseAmount = Mathf.Clamp(pulseAmount, 0f, 0.25f);
            scalePulseSpeed = Mathf.Max(0.1f, pulseRate);
            CacheOriginalTransform();
        }

        private void CacheOriginalTransform()
        {
            if (animatedRoot == null)
            {
                animatedRoot = transform;
            }

            originalLocalPosition = animatedRoot.localPosition;
            originalLocalScale = animatedRoot.localScale;
            originalLocalRotation = animatedRoot.localRotation;
        }

        private void ApplyBob(float time)
        {
            if (!enableBob)
            {
                animatedRoot.localPosition = originalLocalPosition;
                return;
            }

            float bob = Mathf.Sin(time * bobSpeed) * bobAmplitude;
            animatedRoot.localPosition = originalLocalPosition + Vector3.up * bob;
        }

        private void ApplyRotation()
        {
            if (rotationDegreesPerSecond == Vector3.zero)
            {
                animatedRoot.localRotation = originalLocalRotation;
                return;
            }

            spinEuler += rotationDegreesPerSecond * Time.deltaTime;
            animatedRoot.localRotation = originalLocalRotation * Quaternion.Euler(spinEuler);
        }

        private void ApplyScalePulse(float time)
        {
            if (!enableScalePulse)
            {
                animatedRoot.localScale = originalLocalScale;
                return;
            }

            float pulse = (Mathf.Sin(time * scalePulseSpeed) + 1f) * 0.5f;
            animatedRoot.localScale = originalLocalScale * (1f + pulse * scalePulseAmount);
        }
    }
}
