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

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(900f, 560f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.LabelPill("CHOOSE YOUR ROUND");
            SimpleUIFactory.Title("Select Mode");
            SimpleUIFactory.Body("Choose how you want to play today.");

            if (Screen.width >= 820f)
            {
                DrawModeCardsHorizontal();
            }
            else
            {
                DrawModeCardsVertical();
            }

            GUILayout.Space(6f);
            if (SimpleUIFactory.SecondaryButton("BACK"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Draws mode cards horizontal in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawModeCardsHorizontal()
        {
            GUILayout.BeginHorizontal();

            if (SimpleUIFactory.CompactModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.CompactModeCard(
                "AI Battle",
                "AI",
                "Fight a simple toy opponent that can move, dodge, and bomb.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.CompactModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle.",
                localVsColor))
            {
                OnSelectLocalVS();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        /// <summary>
        /// Purpose: Draws mode cards vertical in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawModeCardsVertical()
        {
            if (SimpleUIFactory.CompactModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            if (SimpleUIFactory.CompactModeCard(
                "AI Battle",
                "AI",
                "Fight a simple toy opponent that can move, dodge, and bomb.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            if (SimpleUIFactory.CompactModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle.",
                localVsColor))
            {
                OnSelectLocalVS();
            }
        }

        /// <summary>
        /// Purpose: Handles the select single player event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectSinglePlayer() => SelectMode(GameMode.SinglePlayer);
        /// <summary>
        /// Purpose: Handles the select aibattle event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectAIBattle() => SelectMode(GameMode.AIBattle);
        /// <summary>
        /// Purpose: Handles the select local vs event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectLocalVS() => SelectMode(GameMode.LocalVS);

        /// <summary>
        /// Purpose: Performs select mode for this component.
        /// Inputs: `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mode">Input value used by this method.</param>
        private void SelectMode(GameMode mode)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetGameMode(mode);
            SceneFlowManager.Instance?.LoadCharacterSelect();
        }

        /// <summary>
        /// Purpose: Handles the back button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
