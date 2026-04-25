using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Main menu button callbacks and MVP placeholder layout.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private void OnGUI()
        {
            Rect panel = SimpleUIFactory.CenteredRect(560f, 420f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.Title("BubbleTown");
            SimpleUIFactory.Body("A colorful 3D grid bomber MVP");
            SimpleUIFactory.FlexibleSpace();

            if (SimpleUIFactory.Button("Start Game"))
            {
                OnClickStart();
            }

            if (SimpleUIFactory.Button("Quit"))
            {
                OnClickQuit();
            }

            SimpleUIFactory.EndPanel();
        }

        public void OnClickStart()
        {
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        public void OnClickQuit()
        {
            Debug.Log("[MainMenuUI] Quit requested.");
            Application.Quit();
        }
    }
}
