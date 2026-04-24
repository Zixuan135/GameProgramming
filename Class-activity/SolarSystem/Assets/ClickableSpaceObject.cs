using UnityEngine;

public class ClickableSpaceObject : MonoBehaviour
{
    public SolarSystemManager manager;
    public string childFriendlyFact;

    public AudioSource audioSource;

    private Vector3 originalScale;
    private Renderer objectRenderer;
    private Color originalColor;

    void Start()
    {
        originalScale = transform.localScale;

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    void OnMouseDown()
    {
        if (manager != null)
        {
            manager.FocusOnObject(transform, childFriendlyFact);
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        StopAllCoroutines();
        StartCoroutine(ClickEffect());
    }

    System.Collections.IEnumerator ClickEffect()
    {
        transform.localScale = originalScale * 1.25f;

        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.yellow;
        }

        yield return new WaitForSeconds(0.3f);

        transform.localScale = originalScale;

        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
}