using System.Collections;
using UnityEngine;

namespace BubbleTown.Map
{
    /// <summary>
    /// Low-cost placeholder feedback for Phase 2 wall interactions.
    /// Hard walls punch/shake when blocking explosions; soft walls shrink, shake, and spawn simple shards before disappearing.
    /// </summary>
    public class WallFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;

        [Header("Hard Wall Block Feedback")]
        [SerializeField, Min(0.01f)] private float blockFeedbackSeconds = 0.18f;
        [SerializeField, Min(0f)] private float blockShakeDistance = 0.035f;
        [SerializeField, Min(0f)] private float blockScalePunch = 0.06f;

        [Header("Soft Wall Destroy Feedback")]
        [SerializeField, Min(0.01f)] private float destroyFeedbackSeconds = 0.32f;
        [SerializeField, Min(0f)] private float destroyShakeDistance = 0.06f;
        [SerializeField, Range(0f, 1f)] private float finalShrinkScale = 0.08f;
        [SerializeField, Min(0)] private int shardCount = 6;
        [SerializeField, Min(0.01f)] private float shardLifetimeSeconds = 0.55f;
        [SerializeField, Min(0.01f)] private float shardSize = 0.14f;
        [SerializeField, Min(0f)] private float shardScatterForce = 2.2f;

        private Vector3 originalLocalPosition;
        private Vector3 originalLocalScale;
        private Coroutine feedbackRoutine;
        private bool isDestroying;

        private void Awake()
        {
            CacheOriginalTransform();
        }

        private void OnEnable()
        {
            CacheOriginalTransform();
        }

        public void PlayHardWallBlockedFeedback(Vector3 explosionWorldPosition)
        {
            if (!isActiveAndEnabled || isDestroying)
            {
                return;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                RestoreVisualTransform();
            }

            feedbackRoutine = StartCoroutine(PlayHardWallBlockedRoutine(explosionWorldPosition));
        }

        public void PlaySoftWallDestroyedFeedback()
        {
            if (!isActiveAndEnabled || isDestroying)
            {
                return;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(PlaySoftWallDestroyedRoutine());
        }

        private IEnumerator PlayHardWallBlockedRoutine(Vector3 explosionWorldPosition)
        {
            CacheOriginalTransform();

            Vector3 shakeDirection = ResolveShakeDirection(explosionWorldPosition);
            float elapsed = 0f;
            while (elapsed < blockFeedbackSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / blockFeedbackSeconds);
                float fade = 1f - t;
                float shake = Mathf.Sin(t * Mathf.PI * 5f) * blockShakeDistance * fade;
                float scalePunch = Mathf.Sin(t * Mathf.PI) * blockScalePunch;

                visualRoot.localPosition = originalLocalPosition + shakeDirection * shake;
                visualRoot.localScale = originalLocalScale * (1f + scalePunch);
                yield return null;
            }

            RestoreVisualTransform();
            feedbackRoutine = null;
        }

        private IEnumerator PlaySoftWallDestroyedRoutine()
        {
            isDestroying = true;
            CacheOriginalTransform();
            DisableColliders();
            SpawnSimpleShards();

            float elapsed = 0f;
            while (elapsed < destroyFeedbackSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / destroyFeedbackSeconds);
                float easeOut = 1f - Mathf.Pow(1f - t, 2f);
                float shakeFade = 1f - t;
                Vector3 shakeOffset = new Vector3(
                    Mathf.Sin(t * Mathf.PI * 10f) * destroyShakeDistance * shakeFade,
                    Mathf.Sin(t * Mathf.PI * 7f) * destroyShakeDistance * 0.45f * shakeFade,
                    Mathf.Cos(t * Mathf.PI * 9f) * destroyShakeDistance * shakeFade);

                visualRoot.localPosition = originalLocalPosition + shakeOffset;
                visualRoot.localScale = Vector3.Lerp(originalLocalScale, originalLocalScale * finalShrinkScale, easeOut);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void SpawnSimpleShards()
        {
            if (shardCount <= 0)
            {
                return;
            }

            Material shardMaterial = ResolveShardMaterial();
            for (int i = 0; i < shardCount; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "SoftWallShard";
                shard.transform.position = transform.position + Vector3.up * 0.45f;
                shard.transform.localScale = Vector3.one * shardSize;

                MeshRenderer renderer = shard.GetComponent<MeshRenderer>();
                if (renderer != null && shardMaterial != null)
                {
                    renderer.sharedMaterial = shardMaterial;
                }

                Rigidbody rigidbody = shard.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;
                rigidbody.mass = 0.12f;

                Vector3 scatterDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.6f, 1.25f),
                    Random.Range(-1f, 1f)).normalized;
                rigidbody.AddForce(scatterDirection * shardScatterForce, ForceMode.Impulse);
                rigidbody.AddTorque(Random.insideUnitSphere * shardScatterForce, ForceMode.Impulse);

                Destroy(shard, shardLifetimeSeconds);
            }
        }

        private Material ResolveShardMaterial()
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return null;
            }

            return renderer.sharedMaterial;
        }

        private Vector3 ResolveShakeDirection(Vector3 explosionWorldPosition)
        {
            Vector3 direction = transform.position - explosionWorldPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.right;
            }

            return visualRoot.parent == null
                ? direction.normalized
                : visualRoot.parent.InverseTransformDirection(direction.normalized);
        }

        private void DisableColliders()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void CacheOriginalTransform()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            originalLocalPosition = visualRoot.localPosition;
            originalLocalScale = visualRoot.localScale;
        }

        private void RestoreVisualTransform()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = originalLocalPosition;
            visualRoot.localScale = originalLocalScale;
        }
    }
}
