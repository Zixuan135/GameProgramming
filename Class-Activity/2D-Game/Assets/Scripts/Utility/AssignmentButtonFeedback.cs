using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AssignmentButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [HideInInspector] public Image targetImage;
    [HideInInspector] public AssignmentAudioManager audioManager;
    [HideInInspector] public Vector3 normalScale = Vector3.one;
    [HideInInspector] public Vector3 hoverScale = new Vector3(1.04f, 1.04f, 1f);
    [HideInInspector] public Vector3 pressedScale = new Vector3(0.98f, 0.98f, 1f);
    [HideInInspector] public Color normalColor = new Color(0.18f, 0.27f, 0.4f, 1f);
    [HideInInspector] public Color hoverColor = new Color(0.28f, 0.42f, 0.62f, 1f);
    [HideInInspector] public Color pressedColor = new Color(0.12f, 0.19f, 0.3f, 1f);

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyState(hoverScale, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyState(normalScale, normalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyState(pressedScale, pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ApplyState(hoverScale, hoverColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    private void ApplyState(Vector3 targetScale, Color color)
    {
        transform.localScale = targetScale;
        if (targetImage != null)
        {
            targetImage.color = color;
        }
    }
}
