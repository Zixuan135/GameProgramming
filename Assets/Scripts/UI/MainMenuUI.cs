using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Main menu button callbacks and chibi-style placeholder layout.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private void OnGUI()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(680f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.LabelPill("CANDY ARENA TEST BUILD");
            SimpleUIFactory.Title("BubbleTown");
            SimpleUIFactory.Body("A cute 3D grid battle with bubble bombs, toy blocks, and bright power-ups.");
            SimpleUIFactory.FeatureRow("GRID ARENA", "BUBBLE BOMBS", "POWER UPS");
            SimpleUIFactory.FlexibleSpace();

            if (SimpleUIFactory.PrimaryButton("START GAME"))
            {
                OnClickStart();
            }

            if (SimpleUIFactory.SecondaryButton("QUIT"))
            {
                OnClickQuit();
            }

            SimpleUIFactory.SmallBody("Placeholder UI pass: IMGUI now, Canvas polish later.");
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
