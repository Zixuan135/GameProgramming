using System.Collections;
using UnityEngine;

namespace BubbleTown.Characters
{
    /// <summary>
    /// Lightweight placeholder animation for primitive chibi characters.
    /// It animates only the replaceable visual child, leaving grid movement and colliders untouched.
    /// </summary>
    public class CharacterVisualAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBase character;
        [SerializeField] private Transform animatedRoot;
        [SerializeField] private Renderer[] flashRenderers = new Renderer[0];

        [Header("Idle Bob")]
        [SerializeField] private bool enableIdleBob = true;
        [SerializeField, Min(0f)] private float idleBobAmplitude = 0.035f;
        [SerializeField, Min(0.1f)] private float idleBobSpeed = 3.2f;

        [Header("Move Bounce")]
        [SerializeField, Min(0f)] private float moveBobAmplitude = 0.065f;
        [SerializeField, Min(0.1f)] private float moveBobSpeed = 11f;
        [SerializeField, Min(0f)] private float moveSwayDegrees = 7f;
        [SerializeField, Range(0f, 0.3f)] private float moveScalePulse = 0.07f;

        [Header("Bomb Action")]
        [SerializeField, Min(0.01f)] private float bombActionDuration = 0.28f;
        [SerializeField, Range(0f, 0.5f)] private float bombSquashAmount = 0.22f;
        [SerializeField, Min(0f)] private float bombHopHeight = 0.13f;
        [SerializeField, Min(0f)] private float bombTiltDegrees = 13f;
        [SerializeField, Min(0f)] private float bombShakeDegrees = 5f;

        [Header("Hit Feedback")]
        [SerializeField, Min(0.01f)] private float hitFeedbackDuration = 0.22f;
        [SerializeField, Min(0f)] private float hitShakeAmplitude = 0.075f;
        [SerializeField, Range(0f, 0.35f)] private float hitScalePunch = 0.14f;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.35f, 0.18f);

        [Header("Defeat Feedback")]
        [SerializeField, Min(0.01f)] private float defeatFeedbackDuration = 0.52f;
        [SerializeField, Min(0f)] private float defeatRiseHeight = 0.22f;
        [SerializeField, Min(0f)] private float defeatShakeAmplitude = 0.09f;
        [SerializeField, Min(0f)] private float defeatSpinDegrees = 180f;
        [SerializeField, Range(0f, 1f)] private float defeatShrinkStart = 0.28f;
        [SerializeField] private Color defeatFlashColor = new Color(1f, 0.72f, 0.12f);

        [Header("Defeat Puffs")]
        [SerializeField] private bool spawnDefeatPuffs = true;
        [SerializeField, Min(0)] private int defeatPuffCount = 6;
        [SerializeField, Min(0.01f)] private float defeatPuffDuration = 0.42f;
        [SerializeField, Min(0f)] private float defeatPuffDistance = 0.48f;
        [SerializeField, Min(0f)] private float defeatPuffScale = 0.12f;

        [Header("Emission")]
        [SerializeField, Min(0f)] private float maxEmissionIntensity = 1.45f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private Quaternion baseLocalRotation;
        private float timeOffset;
        private float bombActionTimer;
        private float hitFeedbackTimer;
        private float defeatFeedbackTimer;
        private bool hasCachedBasePose;
        private bool isSubscribed;
        private bool defeatPuffsSpawned;
        private MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            ResolveReferences();
            CacheBasePose();
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            ResolveReferences();
            CacheBasePose();
            SubscribeToCharacter();
            defeatPuffsSpawned = false;
        }

        /// <summary>
        /// Purpose: Cleans up subscriptions or runtime state when this component becomes inactive.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDisable()
        {
            UnsubscribeFromCharacter();
            RestoreBasePose();
            ApplyEmission(Color.black);
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

            UpdateTimers();
            ApplyPlaceholderAnimation();
        }

        /// <summary>
        /// Purpose: Resolves references from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ResolveReferences()
        {
            if (animatedRoot == null)
            {
                Transform visualRoot = transform.Find("VisualRoot");
                animatedRoot = visualRoot != null ? visualRoot : transform;
            }

            if (character == null)
            {
                character = GetComponentInParent<CharacterBase>();
            }

            if (flashRenderers == null || flashRenderers.Length == 0)
            {
                flashRenderers = GetComponentsInChildren<Renderer>();
            }

            if (Mathf.Approximately(timeOffset, 0f))
            {
                timeOffset = Random.Range(0f, 10f);
            }
        }

        /// <summary>
        /// Purpose: Performs cache base pose for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void CacheBasePose()
        {
            if (hasCachedBasePose || animatedRoot == null)
            {
                return;
            }

            baseLocalPosition = animatedRoot.localPosition;
            baseLocalScale = animatedRoot.localScale;
            baseLocalRotation = animatedRoot.localRotation;
            hasCachedBasePose = true;
        }

        /// <summary>
        /// Purpose: Performs subscribe to character for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void SubscribeToCharacter()
        {
            if (character == null || isSubscribed)
            {
                return;
            }

            character.BombPlaced += HandleBombPlaced;
            character.ExplosionHit += HandleExplosionHit;
            character.DeathFeedbackStarted += HandleDeathFeedbackStarted;
            isSubscribed = true;
        }

        /// <summary>
        /// Purpose: Performs unsubscribe from character for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void UnsubscribeFromCharacter()
        {
            if (character == null || !isSubscribed)
            {
                return;
            }

            character.BombPlaced -= HandleBombPlaced;
            character.ExplosionHit -= HandleExplosionHit;
            character.DeathFeedbackStarted -= HandleDeathFeedbackStarted;
            isSubscribed = false;
        }

        /// <summary>
        /// Purpose: Updates timers.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void UpdateTimers()
        {
            if (bombActionTimer > 0f)
            {
                bombActionTimer = Mathf.Max(0f, bombActionTimer - Time.deltaTime);
            }

            if (hitFeedbackTimer > 0f)
            {
                hitFeedbackTimer = Mathf.Max(0f, hitFeedbackTimer - Time.deltaTime);
            }

            if (defeatFeedbackTimer > 0f)
            {
                defeatFeedbackTimer = Mathf.Max(0f, defeatFeedbackTimer - Time.deltaTime);
            }
        }

        /// <summary>
        /// Purpose: Applies placeholder animation to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyPlaceholderAnimation()
        {
            float animationTime = Time.time + timeOffset;
            Vector3 targetPosition = baseLocalPosition;
            Vector3 targetScale = baseLocalScale;
            Quaternion targetRotation = baseLocalRotation;
            Color emissionColor = Color.black;

            if (defeatFeedbackTimer > 0f)
            {
                ApplyDefeatFeedback(animationTime, ref targetPosition, ref targetScale, ref targetRotation, ref emissionColor);
                ApplyPose(targetPosition, targetScale, targetRotation, emissionColor);
                return;
            }

            bool isAlive = character == null || character.IsAlive;
            if (!isAlive)
            {
                return;
            }

            bool isMoving = character != null && character.IsMoving;
            if (isMoving)
            {
                ApplyMoveAnimation(animationTime, ref targetPosition, ref targetScale, ref targetRotation);
            }
            else if (enableIdleBob)
            {
                ApplyIdleAnimation(animationTime, ref targetPosition);
            }

            if (bombActionTimer > 0f)
            {
                ApplyBombAction(animationTime, ref targetPosition, ref targetScale, ref targetRotation, ref emissionColor);
            }

            if (hitFeedbackTimer > 0f)
            {
                ApplyHitFeedback(animationTime, ref targetPosition, ref targetScale, ref targetRotation, ref emissionColor);
            }

            ApplyPose(targetPosition, targetScale, targetRotation, emissionColor);
        }

        /// <summary>
        /// Purpose: Applies pose to the current object or scene.
        /// Inputs: `targetPosition`, `targetScale`, `targetRotation`, `emissionColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="targetPosition">Input value used by this method.</param>
        /// <param name="targetScale">Input value used by this method.</param>
        /// <param name="targetRotation">Input value used by this method.</param>
        /// <param name="emissionColor">Input value used by this method.</param>
        private void ApplyPose(Vector3 targetPosition, Vector3 targetScale, Quaternion targetRotation, Color emissionColor)
        {
            animatedRoot.localPosition = targetPosition;
            animatedRoot.localScale = targetScale;
            animatedRoot.localRotation = targetRotation;
            ApplyEmission(emissionColor);
        }

        /// <summary>
        /// Purpose: Applies idle animation to the current object or scene.
        /// Inputs: `animationTime`, `targetPosition`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="animationTime">Input value used by this method.</param>
        /// <param name="targetPosition">Input value used by this method.</param>
        private void ApplyIdleAnimation(float animationTime, ref Vector3 targetPosition)
        {
            float bob = Mathf.Sin(animationTime * idleBobSpeed) * idleBobAmplitude;
            targetPosition += Vector3.up * bob;
        }

        /// <summary>
        /// Purpose: Applies move animation to the current object or scene.
        /// Inputs: `animationTime`, `targetPosition`, `targetScale`, `targetRotation`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="animationTime">Input value used by this method.</param>
        /// <param name="targetPosition">Input value used by this method.</param>
        /// <param name="targetScale">Input value used by this method.</param>
        /// <param name="targetRotation">Input value used by this method.</param>
        private void ApplyMoveAnimation(
            float animationTime,
            ref Vector3 targetPosition,
            ref Vector3 targetScale,
            ref Quaternion targetRotation)
        {
            float phase = Mathf.Sin(animationTime * moveBobSpeed);
            float bounce = Mathf.Abs(phase);
            targetPosition += Vector3.up * bounce * moveBobAmplitude;
            targetScale = Vector3.Scale(
                targetScale,
                new Vector3(1f + bounce * moveScalePulse, 1f - bounce * moveScalePulse * 0.45f, 1f + bounce * moveScalePulse));
            targetRotation *= Quaternion.Euler(0f, 0f, phase * moveSwayDegrees);
        }

        /// <summary>
        /// Purpose: Applies bomb action to the current object or scene.
        /// Inputs: `animationTime`, `targetPosition`, `targetScale`, `targetRotation`, `emissionColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="animationTime">Input value used by this method.</param>
        /// <param name="targetPosition">Input value used by this method.</param>
        /// <param name="targetScale">Input value used by this method.</param>
        /// <param name="targetRotation">Input value used by this method.</param>
        /// <param name="emissionColor">Input value used by this method.</param>
        private void ApplyBombAction(
            float animationTime,
            ref Vector3 targetPosition,
            ref Vector3 targetScale,
            ref Quaternion targetRotation,
            ref Color emissionColor)
        {
            float normalizedTime = 1f - bombActionTimer / bombActionDuration;
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            float snap = Mathf.Sin(normalizedTime * Mathf.PI * 2f);

            targetPosition += Vector3.up * pulse * bombHopHeight;
            targetScale = Vector3.Scale(
                targetScale,
                new Vector3(1f + pulse * bombSquashAmount, 1f - pulse * bombSquashAmount * 0.75f, 1f + pulse * bombSquashAmount));
            targetRotation *= Quaternion.Euler(-pulse * bombTiltDegrees, 0f, snap * bombShakeDegrees);
            emissionColor += new Color(0.35f, 0.9f, 1f) * pulse * maxEmissionIntensity;
        }

        /// <summary>
        /// Purpose: Applies hit feedback to the current object or scene.
        /// Inputs: `animationTime`, `targetPosition`, `targetScale`, `targetRotation`, `emissionColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="animationTime">Input value used by this method.</param>
        /// <param name="targetPosition">Input value used by this method.</param>
        /// <param name="targetScale">Input value used by this method.</param>
        /// <param name="targetRotation">Input value used by this method.</param>
        /// <param name="emissionColor">Input value used by this method.</param>
        private void ApplyHitFeedback(
            float animationTime,
            ref Vector3 targetPosition,
            ref Vector3 targetScale,
            ref Quaternion targetRotation,
            ref Color emissionColor)
        {
            float normalizedTime = 1f - hitFeedbackTimer / hitFeedbackDuration;
            float fade = 1f - normalizedTime;
            float shakeX = Mathf.Sin(animationTime * 72f) * hitShakeAmplitude * fade;
            float shakeZ = Mathf.Cos(animationTime * 57f) * hitShakeAmplitude * fade;
            float punch = Mathf.Sin(normalizedTime * Mathf.PI) * hitScalePunch;

            targetPosition += new Vector3(shakeX, 0f, shakeZ);
            targetScale = Vector3.Scale(targetScale, new Vector3(1f + punch, 1f - punch * 0.55f, 1f + punch));
            targetRotation *= Quaternion.Euler(0f, 0f, Mathf.Sin(animationTime * 80f) * 10f * fade);
            emissionColor += hitFlashColor * fade * maxEmissionIntensity;
        }

        /// <summary>
        /// Purpose: Applies defeat feedback to the current object or scene.
        /// Inputs: `animationTime`, `targetPosition`, `targetScale`, `targetRotation`, `emissionColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="animationTime">Input value used by this method.</param>
        /// <param name="targetPosition">Input value used by this method.</param>
        /// <param name="targetScale">Input value used by this method.</param>
        /// <param name="targetRotation">Input value used by this method.</param>
        /// <param name="emissionColor">Input value used by this method.</param>
        private void ApplyDefeatFeedback(
            float animationTime,
            ref Vector3 targetPosition,
            ref Vector3 targetScale,
            ref Quaternion targetRotation,
            ref Color emissionColor)
        {
            float normalizedTime = 1f - defeatFeedbackTimer / defeatFeedbackDuration;
            float fade = 1f - normalizedTime;
            float shrinkProgress = Mathf.Clamp01((normalizedTime - defeatShrinkStart) / Mathf.Max(0.01f, 1f - defeatShrinkStart));
            float shrink = 1f - Mathf.SmoothStep(0f, 1f, shrinkProgress);
            float pop = Mathf.Sin(normalizedTime * Mathf.PI);
            float shakeX = Mathf.Sin(animationTime * 85f) * defeatShakeAmplitude * fade;
            float shakeZ = Mathf.Cos(animationTime * 91f) * defeatShakeAmplitude * fade;

            targetPosition += new Vector3(shakeX, defeatRiseHeight * Mathf.SmoothStep(0f, 1f, normalizedTime), shakeZ);
            targetScale = targetScale * Mathf.Max(0.01f, (1f + pop * 0.18f) * shrink);
            targetRotation *= Quaternion.Euler(0f, defeatSpinDegrees * normalizedTime, Mathf.Sin(animationTime * 42f) * 14f * fade);
            emissionColor += defeatFlashColor * (0.35f + pop * 0.65f) * maxEmissionIntensity;
        }

        /// <summary>
        /// Purpose: Applies emission to the current object or scene.
        /// Inputs: `emissionColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="emissionColor">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Performs restore base pose for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void RestoreBasePose()
        {
            if (!hasCachedBasePose || animatedRoot == null)
            {
                return;
            }

            animatedRoot.localPosition = baseLocalPosition;
            animatedRoot.localScale = baseLocalScale;
            animatedRoot.localRotation = baseLocalRotation;
        }

        /// <summary>
        /// Purpose: Spawns defeat puffs.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void SpawnDefeatPuffs()
        {
            if (!spawnDefeatPuffs || defeatPuffCount <= 0)
            {
                return;
            }

            Material puffMaterial = ResolvePuffMaterial();
            Vector3 center = animatedRoot != null ? animatedRoot.position : transform.position;
            for (int i = 0; i < defeatPuffCount; i++)
            {
                float angle = i * Mathf.PI * 2f / defeatPuffCount;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0.45f, Mathf.Sin(angle)).normalized;
                GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                puff.name = "Character_DefeatPuff";
                puff.transform.position = center + Vector3.up * 0.35f;
                puff.transform.localScale = Vector3.one * defeatPuffScale;

                Renderer renderer = puff.GetComponent<Renderer>();
                if (renderer != null && puffMaterial != null)
                {
                    renderer.sharedMaterial = puffMaterial;
                }

                Collider collider = puff.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                StartCoroutine(AnimateDefeatPuff(puff.transform, direction));
            }
        }

        /// <summary>
        /// Purpose: Resolves puff material from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `Material` value.
        /// </summary>
        /// <returns>a `Material` value.</returns>
        private Material ResolvePuffMaterial()
        {
            if (flashRenderers == null || flashRenderers.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    return flashRenderers[i].sharedMaterial;
                }
            }

            return null;
        }

        /// <summary>
        /// Purpose: Animates defeat puff over time.
        /// Inputs: `puff`, `direction`; may also read serialized fields and current runtime state.
        /// Output: a `IEnumerator` value.
        /// </summary>
        /// <param name="puff">Input value used by this method.</param>
        /// <param name="direction">Input value used by this method.</param>
        /// <returns>a `IEnumerator` value.</returns>
        private IEnumerator AnimateDefeatPuff(Transform puff, Vector3 direction)
        {
            if (puff == null)
            {
                yield break;
            }

            Vector3 startPosition = puff.position;
            Vector3 startScale = puff.localScale;
            float elapsed = 0f;
            while (elapsed < defeatPuffDuration && puff != null)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / defeatPuffDuration);
                puff.position = startPosition + direction * defeatPuffDistance * Mathf.SmoothStep(0f, 1f, normalizedTime);
                puff.localScale = startScale * (1f - Mathf.SmoothStep(0f, 1f, normalizedTime));
                yield return null;
            }

            if (puff != null)
            {
                Destroy(puff.gameObject);
            }
        }

        /// <summary>
        /// Purpose: Handles bomb placed.
        /// Inputs: `sourceCharacter`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sourceCharacter">Input value used by this method.</param>
        private void HandleBombPlaced(CharacterBase sourceCharacter)
        {
            bombActionTimer = bombActionDuration;
        }

        /// <summary>
        /// Purpose: Handles explosion hit.
        /// Inputs: `sourceCharacter`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sourceCharacter">Input value used by this method.</param>
        private void HandleExplosionHit(CharacterBase sourceCharacter)
        {
            hitFeedbackTimer = hitFeedbackDuration;
        }

        /// <summary>
        /// Purpose: Handles death feedback started.
        /// Inputs: `sourceCharacter`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sourceCharacter">Input value used by this method.</param>
        private void HandleDeathFeedbackStarted(CharacterBase sourceCharacter)
        {
            hitFeedbackTimer = 0f;
            bombActionTimer = 0f;
            defeatFeedbackTimer = defeatFeedbackDuration;

            if (!defeatPuffsSpawned)
            {
                defeatPuffsSpawned = true;
                SpawnDefeatPuffs();
            }
        }
    }
}
