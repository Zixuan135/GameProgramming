using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Result screen callbacks.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        public void OnClickRematch()
        {
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickBackToMenu()
        {
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
