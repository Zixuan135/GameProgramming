using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Battle HUD callbacks for skeleton stage.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        public void OnClickBackToMenu()
        {
            SceneFlowManager.Instance?.LoadMainMenu();
        }

        public void OnClickForceResult()
        {
            SceneFlowManager.Instance?.LoadResult();
        }
    }
}
