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

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            CacheOriginalTransform();
            phaseOffset = Mathf.Abs(GetInstanceID() % 1000) * 0.037f;
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            CacheOriginalTransform();
        }

        /// <summary>
        /// Purpose: Runs this component's per-frame logic.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Performs configure for this component.
        /// Inputs: `root`, `bob`, `bobAmount`, `bobRate`, `spinRate`, `scalePulse`, `pulseAmount`, `pulseRate`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="root">Input value used by this method.</param>
        /// <param name="bob">Input value used by this method.</param>
        /// <param name="bobAmount">Input value used by this method.</param>
        /// <param name="bobRate">Input value used by this method.</param>
        /// <param name="spinRate">Input value used by this method.</param>
        /// <param name="scalePulse">Input value used by this method.</param>
        /// <param name="pulseAmount">Input value used by this method.</param>
        /// <param name="pulseRate">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Performs cache original transform for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies bob to the current object or scene.
        /// Inputs: `time`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="time">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Applies rotation to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Applies scale pulse to the current object or scene.
        /// Inputs: `time`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="time">Input value used by this method.</param>
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
