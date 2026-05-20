using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Dedicated AI difficulty selection screen shown after map selection for AI Battle.
    /// Keeping this on its own page avoids overcrowding the map select layout.
    /// </summary>
    public class DifficultySelectUI : MonoBehaviour
    {
        private AIDifficulty selectedDifficulty = AIDifficulty.Normal;
        private bool hasInitializedSelection;

        private readonly Color easyColor = new Color(0.45f, 0.9f, 0.36f, 1f);
        private readonly Color normalColor = new Color(0.12f, 0.72f, 1f, 1f);
        private readonly Color hardColor = new Color(1f, 0.48f, 0.24f, 1f);

        /// <summary>
        /// Purpose: Draws the full difficulty selection screen.
        /// Inputs: no direct parameters; reads GameManager for current map and selected difficulty.
        /// Output: no return value; button clicks update the selected difficulty or change scenes.
        /// </summary>
        private void OnGUI()
        {
            InitializeSelectionIfNeeded();
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(900f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.CompactLabelPill("AI RIVAL SETUP");
            SimpleUIFactory.CompactTitle("Choose Difficulty");
            SimpleUIFactory.Body("Map: " + FormatMapName(GetCurrentMapType()));
            SimpleUIFactory.MapSelectDecorations(panel.width, panel.height);

            DrawDifficultyCards();
            DrawBottomButtons(panel);

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Initializes the visible selection from the current GameManager session once.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: no return value; caches the selected AI difficulty for this screen.
        /// </summary>
        private void InitializeSelectionIfNeeded()
        {
            if (hasInitializedSelection)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                selectedDifficulty = GameManager.Instance.CurrentAIDifficulty;
            }

            hasInitializedSelection = true;
        }

        /// <summary>
        /// Purpose: Draws all difficulty cards in a responsive row or column.
        /// Inputs: no direct parameters; reads Screen.width to choose layout.
        /// Output: no return value; updates selection if the player clicks a card.
        /// </summary>
        private void DrawDifficultyCards()
        {
            if (Screen.width >= 720f)
            {
                GUILayout.BeginHorizontal();
                DrawDifficultyCard(
                    AIDifficulty.Easy,
                    "Easy",
                    "Slower rival with fewer bomb attempts.",
                    easyColor);
                GUILayout.Space(14f);
                DrawDifficultyCard(
                    AIDifficulty.Normal,
                    "Normal",
                    "Balanced rival for the regular game feel.",
                    normalColor);
                GUILayout.Space(14f);
                DrawDifficultyCard(
                    AIDifficulty.Hard,
                    "Hard",
                    "Faster rival with stronger pressure.",
                    hardColor);
                GUILayout.EndHorizontal();
                GUILayout.Space(14f);
                return;
            }

            DrawDifficultyCard(AIDifficulty.Easy, "Easy", "Slower rival with fewer bomb attempts.", easyColor);
            DrawDifficultyCard(AIDifficulty.Normal, "Normal", "Balanced rival for the regular game feel.", normalColor);
            DrawDifficultyCard(AIDifficulty.Hard, "Hard", "Faster rival with stronger pressure.", hardColor);
        }

        /// <summary>
        /// Purpose: Draws one selectable AI difficulty card.
        /// Inputs: difficulty is the represented preset, title and description are visible text, accentColor styles the card.
        /// Output: no return value; clicking the card stores the selected difficulty in GameManager.
        /// </summary>
        /// <param name="difficulty">AI difficulty represented by this card.</param>
        /// <param name="title">Player-facing card title.</param>
        /// <param name="description">Short explanation of how the AI behaves.</param>
        /// <param name="accentColor">Color used to highlight this difficulty.</param>
        private void DrawDifficultyCard(AIDifficulty difficulty, string title, string description, Color accentColor)
        {
            Rect rect = GUILayoutUtility.GetRect(190f, 150f, GUILayout.ExpandWidth(true));
            bool isSelected = selectedDifficulty == difficulty;

            if (SimpleUIFactory.ChoicePill(rect, string.Empty, isSelected, accentColor))
            {
                SelectDifficulty(difficulty);
            }

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                // Keep text color stable; selection is shown through the card fill and SELECTED badge instead.
                normal = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                hover = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                active = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                focused = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) }
            };

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                hover = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                active = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                focused = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) }
            };

            GUIStyle badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                hover = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                active = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                focused = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) }
            };

            Rect titleRect = new Rect(rect.x + 16f, rect.y + 24f, rect.width - 32f, 34f);
            Rect bodyRect = new Rect(rect.x + 28f, rect.y + 66f, rect.width - 56f, 42f);
            Rect badgeRect = new Rect(rect.x + rect.width * 0.5f - 48f, rect.y + 113f, 96f, 22f);

            GUI.Label(titleRect, title.ToUpperInvariant(), titleStyle);
            GUI.Label(bodyRect, description, bodyStyle);
            GUI.Label(badgeRect, isSelected ? "SELECTED" : "PICK", badgeStyle);
        }

        /// <summary>
        /// Purpose: Draws the Start Battle and Back buttons at the bottom of the panel.
        /// Inputs: panel supplies local panel size for fixed button placement.
        /// Output: no return value; clicks start the battle or return to map selection.
        /// </summary>
        /// <param name="panel">Current menu panel rectangle used for bottom button placement.</param>
        private void DrawBottomButtons(Rect panel)
        {
            const float buttonHeight = 50f;
            const float horizontalPadding = 50f;
            const float buttonGap = 12f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.height - 78f;
            Rect startRect = new Rect(horizontalPadding, buttonY, buttonWidth, buttonHeight);
            Rect backRect = new Rect(horizontalPadding + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight);

            if (SimpleUIFactory.FixedPrimaryButton(startRect, "START BATTLE"))
            {
                OnClickStartBattle();
            }

            if (SimpleUIFactory.FixedSecondaryButton(backRect, "BACK"))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Stores the selected difficulty locally and in GameManager.
        /// Inputs: difficulty is the newly selected AI preset.
        /// Output: no return value; the choice survives the next scene load.
        /// </summary>
        /// <param name="difficulty">AI difficulty selected by the player.</param>
        private void SelectDifficulty(AIDifficulty difficulty)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            selectedDifficulty = difficulty;
            GameManager.Instance?.SetAIDifficulty(selectedDifficulty);
        }

        /// <summary>
        /// Purpose: Starts AI Battle using the currently selected difficulty.
        /// Inputs: no direct parameters; writes the selected difficulty to GameManager before loading battle.
        /// Output: no return value; triggers the battle scene flow.
        /// </summary>
        public void OnClickStartBattle()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetAIDifficulty(selectedDifficulty);
            SceneFlowManager.Instance?.LoadBattle();
        }

        /// <summary>
        /// Purpose: Returns to map selection so the player can pick a different map.
        /// Inputs: no direct parameters.
        /// Output: no return value; loads the map selection scene.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadMapSelect();
        }

        /// <summary>
        /// Purpose: Reads the selected battle map from GameManager with a safe fallback.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: returns the current battle map type.
        /// </summary>
        /// <returns>Selected map type, or Default when GameManager is unavailable.</returns>
        private BattleMapType GetCurrentMapType()
        {
            return GameManager.Instance != null
                ? GameManager.Instance.CurrentMapType
                : BattleMapType.Default;
        }

        /// <summary>
        /// Purpose: Converts a map enum into the player-facing map name used on this screen.
        /// Inputs: mapType is the selected map enum.
        /// Output: returns a readable map name.
        /// </summary>
        /// <param name="mapType">Map type to format.</param>
        /// <returns>Readable map name.</returns>
        private string FormatMapName(BattleMapType mapType)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return "Snowfield";
                case BattleMapType.Maze:
                    return "Jelly Maze";
                default:
                    return "Candy Park";
            }
        }
    }
}
