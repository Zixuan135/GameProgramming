using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Main menu button callbacks.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        public void OnClickStart()
        {
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }
    }
}
