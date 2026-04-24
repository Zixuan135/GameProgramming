using UnityEngine;
using TMPro;

public class SolarSystemManager : MonoBehaviour
{
    public Camera mainCamera;
    public TextMeshProUGUI factText;

    public Transform mainViewPoint;
    public Transform currentTarget;

    public float cameraMoveSpeed = 3f;
    public float cameraDistance = 3f;

    private bool isFocused = false;

    void Update()
    {
        if (isFocused && currentTarget != null)
        {
            Vector3 targetPosition = currentTarget.position + new Vector3(0, 1.2f, -cameraDistance);

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                Time.deltaTime * cameraMoveSpeed
            );

            mainCamera.transform.LookAt(currentTarget);
        }
        else if (!isFocused && mainViewPoint != null)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                mainViewPoint.position,
                Time.deltaTime * cameraMoveSpeed
            );

            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                mainViewPoint.rotation,
                Time.deltaTime * cameraMoveSpeed
            );
        }
    }

    public void FocusOnObject(Transform target, string message)
    {
        currentTarget = target;
        isFocused = true;

        if (factText != null)
        {
            factText.text = message;
        }
    }

    public void BackToMainView()
    {
        isFocused = false;
        currentTarget = null;

        if (factText != null)
        {
            factText.text = "Click a planet or moon to explore!";
        }
    }
}