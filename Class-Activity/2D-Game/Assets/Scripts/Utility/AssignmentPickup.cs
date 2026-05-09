using UnityEngine;

public class AssignmentPickup : MonoBehaviour
{
    [HideInInspector] public AssignmentPlayerFeedback playerFeedback;
    [HideInInspector] public AssignmentSceneController sceneController;

    public float duration = 7f;
    public float moveSpeedMultiplier = 1.45f;
    public float fireRateMultiplier = 0.6f;

    private Vector3 startPosition;
    private Vector3 baseScale;

    private void Start()
    {
        startPosition = transform.position;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);
        Vector3 bobPosition = startPosition;
        bobPosition.y += Mathf.Sin(Time.time * 3f) * 0.15f;
        transform.position = bobPosition;
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.localScale = baseScale * pulse;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || playerFeedback == null)
        {
            return;
        }

        playerFeedback.ApplyOverdrive(duration, moveSpeedMultiplier, fireRateMultiplier);
        if (sceneController != null)
        {
            sceneController.RegisterPickupCollected();
        }
        Destroy(gameObject);
    }
}
