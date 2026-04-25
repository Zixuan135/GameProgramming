using BubbleTown.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown.Managers
{
    /// <summary>
    /// Handles scene transitions for the fixed flow:
    /// MainMenu -> ModeSelect -> MapSelect -> Battle -> Result.
    /// </summary>
    public class SceneFlowManager : MonoBehaviour
    {
        private const string RuntimeObjectName = "SceneFlowManager";

        private static SceneFlowManager instance;
        private static bool isQuitting;

        public static SceneFlowManager Instance
        {
            get
            {
                if (instance == null && !isQuitting)
                {
                    instance = FindObjectOfType<SceneFlowManager>();
                    if (instance == null)
                    {
                        GameObject sceneFlowObject = new GameObject(RuntimeObjectName);
                        instance = sceneFlowObject.AddComponent<SceneFlowManager>();
                    }
                }

                return instance;
            }
            private set => instance = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public void LoadMainMenu() => LoadScene(GameConstants.SceneMainMenu);
        public void LoadModeSelect() => LoadScene(GameConstants.SceneModeSelect);
        public void LoadMapSelect() => LoadScene(GameConstants.SceneMapSelect);
        public void LoadBattle()
        {
            GameManager.Instance?.BeginBattle();
        }

        public void LoadResult() => LoadScene(GameConstants.SceneResult);

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneFlowManager] Scene name is empty.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
