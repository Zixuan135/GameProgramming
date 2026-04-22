using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Mode selection callbacks that store selected game mode.
    /// </summary>
    public class ModeSelectUI : MonoBehaviour
    {
        public void OnSelectSinglePlayer() => SelectMode(GameMode.SinglePlayer);
        public void OnSelectAIBattle() => SelectMode(GameMode.AIBattle);
        public void OnSelectLocalVS() => SelectMode(GameMode.LocalVS);

        private void SelectMode(GameMode mode)
        {
            GameManager.Instance?.SetGameMode(mode);
            SceneFlowManager.Instance?.LoadMapSelect();
        }

        public void OnClickBack()
        {
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
