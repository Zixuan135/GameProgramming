using UnityEngine;
using UnityEngine.SceneManagement;

public static class AssignmentAutoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateAssignmentController()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.path.Contains("Imported Custom Asset Packages"))
        {
            return;
        }

        if (Object.FindObjectOfType<AssignmentSceneController>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("AssignmentSceneController");
        bootstrapObject.AddComponent<AssignmentSceneController>();
    }
}
