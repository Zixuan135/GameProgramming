using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown
{
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

        public void LoadScene(GameScene scene)
        {
            SceneManager.LoadScene(GetSceneName(scene));
        }

        public void LoadBattle()
        {
            LoadScene(GameScene.Battle);
        }

        public void LoadResult()
        {
            LoadScene(GameScene.Result);
        }

        public static string GetSceneName(GameScene scene)
        {
            switch (scene)
            {
                case GameScene.MainMenu:
                    return GameConstants.SceneMainMenu;
                case GameScene.ModeSelect:
                    return GameConstants.SceneModeSelect;
                case GameScene.MapSelect:
                    return GameConstants.SceneMapSelect;
                case GameScene.Battle:
                    return GameConstants.SceneBattle;
                case GameScene.Result:
                    return GameConstants.SceneResult;
                default:
                    return GameConstants.SceneMainMenu;
            }
        }
    }
}
