using UnityEngine;

namespace BubbleTown
{
    public class MainMenuUI : MonoBehaviour
    {
        public void OnClickStart()
        {
            SceneFlowManager.Instance.LoadScene(GameScene.ModeSelect);
        }

        public void OnClickQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
