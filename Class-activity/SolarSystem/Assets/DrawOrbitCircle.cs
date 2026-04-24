using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawOrbitCircle : MonoBehaviour
{
    public float radius = 6f;
    public int segments = 100;

    void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        line.widthMultiplier = 0.03f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}