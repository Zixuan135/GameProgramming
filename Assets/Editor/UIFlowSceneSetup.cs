using BubbleTown.Core;
using BubbleTown.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BubbleTown.EditorTools
{
    /// <summary>
    /// Wires the MVP UI controllers into each scene so no manual Inspector setup is required.
    /// </summary>
    public static class UIFlowSceneSetup
    {
        private const string SceneFolder = "Assets/Scenes";

        [MenuItem("BubbleTown/Setup/Ensure MVP UI Flow")]
        public static void EnsureUIFlow()
        {
            EnsureBuildSettings();
            ConfigureScene<MainMenuUI>(GameConstants.SceneMainMenu, "UI_Root_MainMenu");
            ConfigureScene<ModeSelectUI>(GameConstants.SceneModeSelect, "UI_Root_ModeSelect");
            ConfigureScene<MapSelectUI>(GameConstants.SceneMapSelect, "UI_Root_MapSelect");
            ConfigureScene<BattleUI>(GameConstants.SceneBattle, "UI_Root_Battle");
            ConfigureScene<ResultUI>(GameConstants.SceneResult, "UI_Root_Result");

            AssetDatabase.SaveAssets();
            Debug.Log("[UIFlowSceneSetup] MVP UI flow scenes are ready.");
        }

        public static void EnsureUIFlowFromBatchmode()
        {
            EnsureUIFlow();
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                CreateBuildScene(GameConstants.SceneMainMenu),
                CreateBuildScene(GameConstants.SceneModeSelect),
                CreateBuildScene(GameConstants.SceneMapSelect),
                CreateBuildScene(GameConstants.SceneBattle),
                CreateBuildScene(GameConstants.SceneResult)
            };
        }

        private static EditorBuildSettingsScene CreateBuildScene(string sceneName)
        {
            return new EditorBuildSettingsScene(GetScenePath(sceneName), true);
        }

        private static void ConfigureScene<TUI>(string sceneName, string uiRootName) where TUI : MonoBehaviour
        {
            var scene = EditorSceneManager.OpenScene(GetScenePath(sceneName), OpenSceneMode.Single);

            GameObject uiRoot = GameObject.Find(uiRootName);
            if (uiRoot == null)
            {
                uiRoot = new GameObject(uiRootName);
            }

            if (uiRoot.GetComponent<TUI>() == null)
            {
                uiRoot.AddComponent<TUI>();
            }

            EditorUtility.SetDirty(uiRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static string GetScenePath(string sceneName)
        {
            return $"{SceneFolder}/{sceneName}.unity";
        }
    }
}
