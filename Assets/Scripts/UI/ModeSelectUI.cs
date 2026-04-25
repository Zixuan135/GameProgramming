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
        private void OnGUI()
        {
            Rect panel = SimpleUIFactory.CenteredRect(620f, 520f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.Title("Select Mode");
            SimpleUIFactory.Body("Choose how this round should be created.");

            if (SimpleUIFactory.Button("Single Player"))
            {
                OnSelectSinglePlayer();
            }

            if (SimpleUIFactory.Button("AI Battle"))
            {
                OnSelectAIBattle();
            }

            if (SimpleUIFactory.Button("Local VS"))
            {
                OnSelectLocalVS();
            }

            if (SimpleUIFactory.Button("Back"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
        }

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
