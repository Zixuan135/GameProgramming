using BubbleTown.CameraSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Configures the Battle scene camera for the current grid-based 3D prototype.
    /// </summary>
    public static class BattleSceneCameraSetup
    {
        private const string BattleScenePath = "Assets/Scenes/Battle.unity";

        [MenuItem("BubbleTown/Setup/Ensure Battle Camera")]
        public static void EnsureBattleCamera()
        {
            var scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[BattleSceneCameraSetup] Main Camera was not found.");
                return;
            }

            Transform player1 = GameObject.Find("Player1")?.transform;
            Transform player2 = GameObject.Find("Player2")?.transform;

            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<CameraController>();
            }

            mainCamera.fieldOfView = 48f;
            mainCamera.nearClipPlane = 0.3f;
            mainCamera.farClipPlane = 1000f;
            mainCamera.transform.position = new Vector3(1f, 11f, -7f);
            mainCamera.transform.LookAt(new Vector3(1f, 0.75f, 1.25f));

            SerializedObject serializedObject = new SerializedObject(cameraController);
            serializedObject.FindProperty("primaryTarget").objectReferenceValue = player1;
            serializedObject.FindProperty("secondaryTarget").objectReferenceValue = player2;
            serializedObject.FindProperty("autoFindTargets").boolValue = true;
            serializedObject.FindProperty("shareCameraInLocalVS").boolValue = true;
            serializedObject.FindProperty("followOffset").vector3Value = new Vector3(0f, 10.5f, -8f);
            serializedObject.FindProperty("lookAtOffset").vector3Value = new Vector3(0f, 0.25f, 0.25f);
            serializedObject.FindProperty("focusSmoothTime").floatValue = 0.12f;
            serializedObject.FindProperty("followSmoothTime").floatValue = 0.18f;
            serializedObject.FindProperty("rotationLerpSpeed").floatValue = 8f;
            serializedObject.FindProperty("sharedZoomOutPerCell").floatValue = 0.22f;
            serializedObject.FindProperty("maxSharedZoomOut").floatValue = 4f;
            serializedObject.FindProperty("controlledCamera").objectReferenceValue = mainCamera;
            serializedObject.FindProperty("defaultFieldOfView").floatValue = 48f;
            serializedObject.FindProperty("sharedFieldOfViewBoost").floatValue = 0.45f;
            serializedObject.FindProperty("maxFieldOfView").floatValue = 56f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(mainCamera);
            EditorUtility.SetDirty(cameraController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[BattleSceneCameraSetup] Battle camera is ready.");
        }

        public static void EnsureBattleCameraFromBatchmode()
        {
            EnsureBattleCamera();
        }
    }
}
