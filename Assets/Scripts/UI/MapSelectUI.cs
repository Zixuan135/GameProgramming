using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Map selection callbacks that store selected map type before starting battle.
    /// </summary>
    public class MapSelectUI : MonoBehaviour
    {
        private BattleMapType selectedMapType = BattleMapType.Default;
        private bool hasInitializedSelection;
        private Vector2 mapScrollPosition;

        private readonly Color defaultAccent = new Color(0.1f, 0.72f, 1f, 1f);
        private readonly Color snowfieldAccent = new Color(0.32f, 0.78f, 1f, 1f);
        private readonly Color mazeAccent = new Color(0.66f, 0.48f, 1f, 1f);

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            InitializeSelectionIfNeeded();
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(930f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.CompactLabelPill("PICK YOUR PLAYGROUND");
            SimpleUIFactory.CompactTitle("Select Map");
            SimpleUIFactory.MapSelectDecorations(panel.width, panel.height);

            if (Screen.width >= 720f)
            {
                DrawMapCardsHorizontal();
            }
            else
            {
                float scrollHeight = Mathf.Clamp(Screen.height - 330f, 180f, 260f);
                mapScrollPosition = GUILayout.BeginScrollView(mapScrollPosition, GUILayout.Height(scrollHeight));
                DrawMapCardsVertical();
                GUILayout.EndScrollView();
            }

            DrawBottomButtons(panel);

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Draws bottom buttons in the current GUI or scene context.
        /// Inputs: `panel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
        private void DrawBottomButtons(Rect panel)
        {
            const float buttonHeight = 50f;
            const float horizontalPadding = 50f;
            const float buttonGap = 12f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.height - 78f;
            Rect startRect = new Rect(horizontalPadding, buttonY, buttonWidth, buttonHeight);
            Rect backRect = new Rect(horizontalPadding + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight);

            if (SimpleUIFactory.FixedPrimaryButton(startRect, "START MAP"))
            {
                OnClickStartSelectedMap();
            }

            if (SimpleUIFactory.FixedSecondaryButton(backRect, "BACK"))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Performs initialize selection if needed for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void InitializeSelectionIfNeeded()
        {
            if (hasInitializedSelection)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                selectedMapType = GameManager.Instance.CurrentMapType;
            }

            hasInitializedSelection = true;
        }

        /// <summary>
        /// Purpose: Checks whether the current session is selecting a map for AI Battle.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: returns true when the AI difficulty selector should be visible.
        /// </summary>
        /// <returns>True when current mode is AI Battle; otherwise false.</returns>
        private bool IsAIBattleMode()
        {
            return GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameMode.AIBattle;
        }

        /// <summary>
        /// Purpose: Draws map cards horizontal in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMapCardsHorizontal()
        {
            GUILayout.BeginHorizontal();
            DrawDefaultMapCard();
            GUILayout.Space(14f);
            DrawOpenFieldMapCard();
            GUILayout.Space(14f);
            DrawMazeMapCard();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        /// <summary>
        /// Purpose: Draws map cards vertical in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMapCardsVertical()
        {
            DrawDefaultMapCard();
            DrawOpenFieldMapCard();
            DrawMazeMapCard();
        }

        /// <summary>
        /// Purpose: Draws default map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawDefaultMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Candy Park",
                "BALANCED",
                "Balanced candy paths.",
                defaultAccent,
                new Color(0.58f, 0.92f, 0.72f, 1f),
                new Color(1f, 0.86f, 0.48f, 1f),
                new Color(0.48f, 0.82f, 1f, 1f),
                selectedMapType == BattleMapType.Default,
                SimpleUIFactory.MapPreviewPattern.Balanced))
            {
                OnSelectDefault();
            }
        }

        /// <summary>
        /// Purpose: Draws open field map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawOpenFieldMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Snowfield",
                "SNOW",
                "Open icy lanes.",
                snowfieldAccent,
                new Color(0.88f, 0.98f, 1f, 1f),
                new Color(0.42f, 0.88f, 1f, 1f),
                new Color(1f, 0.56f, 0.78f, 1f),
                selectedMapType == BattleMapType.OpenField,
                SimpleUIFactory.MapPreviewPattern.Open))
            {
                OnSelectOpenField();
            }
        }

        /// <summary>
        /// Purpose: Draws maze map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMazeMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Jelly Maze",
                "TWISTY",
                "Tight jelly corners.",
                mazeAccent,
                new Color(0.24f, 0.18f, 0.42f, 1f),
                new Color(0.48f, 0.36f, 1f, 1f),
                new Color(0.18f, 0.9f, 1f, 1f),
                selectedMapType == BattleMapType.Maze,
                SimpleUIFactory.MapPreviewPattern.Maze))
            {
                OnSelectMaze();
            }
        }

        /// <summary>
        /// Purpose: Handles the select default event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectDefault() => SelectMapCard(BattleMapType.Default);
        /// <summary>
        /// Purpose: Handles the select open field event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectOpenField() => SelectMapCard(BattleMapType.OpenField);
        /// <summary>
        /// Purpose: Handles the select maze event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectMaze() => SelectMapCard(BattleMapType.Maze);

        /// <summary>
        /// Purpose: Performs select map card for this component.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        private void SelectMapCard(BattleMapType mapType)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            selectedMapType = mapType;
            GameManager.Instance?.SetMapType(selectedMapType);
        }

        /// <summary>
        /// Purpose: Handles the start selected map button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickStartSelectedMap()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetMapType(selectedMapType);

            if (IsAIBattleMode())
            {
                SceneFlowManager.Instance?.LoadDifficultySelect();
                return;
            }

            SceneFlowManager.Instance?.LoadBattle();
        }

        /// <summary>
        /// Purpose: Handles the back button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadCharacterSelect();
        }

        /// <summary>
        /// Purpose: Formats map name for display or logging.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
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
