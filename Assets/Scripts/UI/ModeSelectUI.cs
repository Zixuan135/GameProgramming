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
        private readonly Color singlePlayerColor = new Color(0.1f, 0.72f, 1f, 1f);
        private readonly Color aiBattleColor = new Color(1f, 0.55f, 0.18f, 1f);
        private readonly Color localVsColor = new Color(0.52f, 0.9f, 0.35f, 1f);

        private void OnGUI()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(900f, 610f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.LabelPill("CHOOSE YOUR ROUND");
            SimpleUIFactory.Title("Select Mode");
            SimpleUIFactory.Body("Pick a quick test mode. Each card keeps the current scene flow and battle setup intact.");

            if (Screen.width >= 820f)
            {
                DrawModeCardsHorizontal();
            }
            else
            {
                DrawModeCardsVertical();
            }

            SimpleUIFactory.FlexibleSpace();
            if (SimpleUIFactory.SecondaryButton("BACK"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
        }

        private void DrawModeCardsHorizontal()
        {
            GUILayout.BeginHorizontal();

            if (SimpleUIFactory.ModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.ModeCard(
                "AI Battle",
                "AI",
                "Fight a simple toy opponent that can move, dodge, and bomb.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.ModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle test.",
                localVsColor))
            {
                OnSelectLocalVS();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        private void DrawModeCardsVertical()
        {
            if (SimpleUIFactory.ModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            if (SimpleUIFactory.ModeCard(
                "AI Battle",
                "AI",
                "Fight a simple toy opponent that can move, dodge, and bomb.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            if (SimpleUIFactory.ModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle test.",
                localVsColor))
            {
                OnSelectLocalVS();
            }
        }

        public void OnSelectSinglePlayer() => SelectMode(GameMode.SinglePlayer);
        public void OnSelectAIBattle() => SelectMode(GameMode.AIBattle);
        public void OnSelectLocalVS() => SelectMode(GameMode.LocalVS);

        private void SelectMode(GameMode mode)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetGameMode(mode);
            SceneFlowManager.Instance?.LoadMapSelect();
        }

        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
