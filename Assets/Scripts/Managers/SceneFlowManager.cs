using BubbleTown.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown.Managers
{
    /// <summary>
    /// Handles scene transitions for the fixed flow:
    /// The scene path is MainMenu, ModeSelect, CharacterSelect, MapSelect, optional DifficultySelect, Battle, then Result.
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

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Handles application shutdown cleanup.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// Purpose: Loads main menu.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadMainMenu() => LoadScene(GameConstants.SceneMainMenu);
        /// <summary>
        /// Purpose: Loads mode select.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadModeSelect() => LoadScene(GameConstants.SceneModeSelect);
        /// <summary>
        /// Purpose: Loads character select.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadCharacterSelect() => LoadScene(GameConstants.SceneCharacterSelect);
        /// <summary>
        /// Purpose: Loads map select.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadMapSelect() => LoadScene(GameConstants.SceneMapSelect);
        /// <summary>
        /// Purpose: Loads difficulty select for AI Battle.
        /// Inputs: no direct parameters; reads the shared scene name constant.
        /// Output: no return value; Unity loads the difficulty selection scene.
        /// </summary>
        public void LoadDifficultySelect() => LoadScene(GameConstants.SceneDifficultySelect);
        /// <summary>
        /// Purpose: Loads battle.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadBattle()
        {
            GameManager.Instance?.BeginBattle();
        }

        /// <summary>
        /// Purpose: Loads result.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void LoadResult() => LoadScene(GameConstants.SceneResult);

        /// <summary>
        /// Purpose: Loads scene.
        /// Inputs: `sceneName`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sceneName">Input value used by this method.</param>
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
