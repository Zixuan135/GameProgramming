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
        [SerializeField, Min(0.01f)] private float bombActionDuration = 0.22f;
        [SerializeField, Range(0f, 0.4f)] private float bombSquashAmount = 0.16f;
        [SerializeField, Min(0f)] private float bombHopHeight = 0.08f;
        [SerializeField, Min(0f)] private float bombTiltDegrees = 8f;

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale;
        private Quaternion baseLocalRotation;
        private float timeOffset;
        private float bombActionTimer;
        private bool hasCachedBasePose;
        private bool isSubscribed;

        private void Awake()
        {
            ResolveReferences();
            CacheBasePose();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheBasePose();
            SubscribeToCharacter();
        }

        private void OnDisable()
        {
            UnsubscribeFromCharacter();
            RestoreBasePose();
        }

        private void Update()
        {
            if (animatedRoot == null)
            {
                return;
            }

            UpdateTimers();
            ApplyPlaceholderAnimation();
        }

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

            if (Mathf.Approximately(timeOffset, 0f))
            {
                timeOffset = Random.Range(0f, 10f);
            }
        }

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

        private void SubscribeToCharacter()
        {
            if (character == null || isSubscribed)
            {
                return;
            }

            character.BombPlaced += HandleBombPlaced;
            isSubscribed = true;
        }

        private void UnsubscribeFromCharacter()
        {
            if (character == null || !isSubscribed)
            {
                return;
            }

            character.BombPlaced -= HandleBombPlaced;
            isSubscribed = false;
        }

        private void UpdateTimers()
        {
            if (bombActionTimer > 0f)
            {
                bombActionTimer = Mathf.Max(0f, bombActionTimer - Time.deltaTime);
            }
        }

        private void ApplyPlaceholderAnimation()
        {
            bool isAlive = character == null || character.IsAlive;
            if (!isAlive)
            {
                RestoreBasePose();
                return;
            }

            bool isMoving = character != null && character.IsMoving;
            float animationTime = Time.time + timeOffset;
            Vector3 targetPosition = baseLocalPosition;
            Vector3 targetScale = baseLocalScale;
            Quaternion targetRotation = baseLocalRotation;

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
                ApplyBombAction(ref targetPosition, ref targetScale, ref targetRotation);
            }

            animatedRoot.localPosition = targetPosition;
            animatedRoot.localScale = targetScale;
            animatedRoot.localRotation = targetRotation;
        }

        private void ApplyIdleAnimation(float animationTime, ref Vector3 targetPosition)
        {
            float bob = Mathf.Sin(animationTime * idleBobSpeed) * idleBobAmplitude;
            targetPosition += Vector3.up * bob;
        }

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

        private void ApplyBombAction(
            ref Vector3 targetPosition,
            ref Vector3 targetScale,
            ref Quaternion targetRotation)
        {
            float normalizedTime = 1f - bombActionTimer / bombActionDuration;
            float pulse = Mathf.Sin(normalizedTime * Mathf.PI);
            targetPosition += Vector3.up * pulse * bombHopHeight;
            targetScale = Vector3.Scale(
                targetScale,
                new Vector3(1f + pulse * bombSquashAmount, 1f - pulse * bombSquashAmount, 1f + pulse * bombSquashAmount));
            targetRotation *= Quaternion.Euler(-pulse * bombTiltDegrees, 0f, 0f);
        }

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

        private void HandleBombPlaced(CharacterBase sourceCharacter)
        {
            bombActionTimer = bombActionDuration;
        }
    }
}
