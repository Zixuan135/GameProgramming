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
        public static SceneFlowManager Instance { get; private set; }

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

        public void LoadMainMenu() => LoadScene(GameConstants.SceneMainMenu);
        public void LoadModeSelect() => LoadScene(GameConstants.SceneModeSelect);
        public void LoadMapSelect() => LoadScene(GameConstants.SceneMapSelect);
        public void LoadBattle() => LoadScene(GameConstants.SceneBattle);
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
