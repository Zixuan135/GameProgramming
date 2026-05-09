using UnityEngine;

public class AssignmentStarfieldStar : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public float minAlpha = 0.35f;
    [HideInInspector] public float maxAlpha = 0.95f;
    [HideInInspector] public float pulseSpeed = 1f;
    [HideInInspector] public float driftSpeed = 0.04f;
    [HideInInspector] public Vector3 driftDirection = Vector3.left;

    private Vector3 startPosition;
    private float pulseOffset;

    private void Start()
    {
        startPosition = transform.localPosition;
        pulseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed + pulseOffset) + 1f) * 0.5f);
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;

        transform.localPosition = startPosition + driftDirection.normalized * Mathf.Sin(Time.time * driftSpeed + pulseOffset) * 0.15f;
    }
}
