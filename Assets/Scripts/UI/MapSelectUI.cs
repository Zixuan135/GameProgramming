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

        private readonly Color defaultAccent = new Color(0.1f, 0.72f, 1f, 1f);
        private readonly Color openFieldAccent = new Color(0.45f, 0.9f, 0.34f, 1f);
        private readonly Color mazeAccent = new Color(0.66f, 0.48f, 1f, 1f);

        private void OnGUI()
        {
            InitializeSelectionIfNeeded();
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(930f, 650f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.LabelPill("PICK YOUR PLAYGROUND");
            SimpleUIFactory.Title("Select Map");
            SimpleUIFactory.Body("Choose a toy-board arena, preview the flavor, then start the battle when ready.");

            if (Screen.width >= 850f)
            {
                DrawMapCardsHorizontal();
            }
            else
            {
                DrawMapCardsVertical();
            }

            SimpleUIFactory.SmallBody("Selected: " + FormatMapName(selectedMapType));
            SimpleUIFactory.FlexibleSpace();

            if (SimpleUIFactory.PrimaryButton("START SELECTED MAP"))
            {
                OnClickStartSelectedMap();
            }

            if (SimpleUIFactory.SecondaryButton("BACK"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
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
            GUILayout.Space(12f);
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
                "DEFAULT",
                "Balanced walls and safe spawn pockets for everyday testing.",
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
                "FAST",
                "More room to move, dodge, and test long explosion chains.",
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
                "MAZE",
                "Neon jelly lanes, tighter routes, and clearer trap testing.",
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
