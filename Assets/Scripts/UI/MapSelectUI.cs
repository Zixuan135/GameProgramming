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
        private readonly Color openFieldAccent = new Color(0.45f, 0.9f, 0.34f, 1f);
        private readonly Color mazeAccent = new Color(0.66f, 0.48f, 1f, 1f);

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

        private void DrawMapCardsVertical()
        {
            DrawDefaultMapCard();
            DrawOpenFieldMapCard();
            DrawMazeMapCard();
        }

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

        private void DrawOpenFieldMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Open Field",
                "OPEN",
                "Wide lanes with wall islands.",
                openFieldAccent,
                new Color(0.62f, 0.96f, 0.58f, 1f),
                new Color(0.95f, 0.78f, 0.34f, 1f),
                new Color(0.78f, 1f, 0.74f, 1f),
                selectedMapType == BattleMapType.OpenField,
                SimpleUIFactory.MapPreviewPattern.Open))
            {
                OnSelectOpenField();
            }
        }

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

        public void OnSelectDefault() => SelectMapCard(BattleMapType.Default);
        public void OnSelectOpenField() => SelectMapCard(BattleMapType.OpenField);
        public void OnSelectMaze() => SelectMapCard(BattleMapType.Maze);

        private void SelectMapCard(BattleMapType mapType)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            selectedMapType = mapType;
            GameManager.Instance?.SetMapType(selectedMapType);
        }

        public void OnClickStartSelectedMap()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetMapType(selectedMapType);
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        private string FormatMapName(BattleMapType mapType)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return "Open Field";
                case BattleMapType.Maze:
                    return "Jelly Maze";
                default:
                    return "Candy Park";
            }
        }
    }
}
