using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Map selection callbacks that store selected map type.
    /// </summary>
    public class MapSelectUI : MonoBehaviour
    {
        private void OnGUI()
        {
            Rect panel = SimpleUIFactory.CenteredRect(700f, 560f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.Title("Select Map");
            SimpleUIFactory.Body("Map buttons currently share placeholder rules, but the selected type is stored for future generation.");

            if (SimpleUIFactory.Button("Default"))
            {
                OnSelectDefault();
            }

            if (SimpleUIFactory.Button("Open Field"))
            {
                OnSelectOpenField();
            }

            if (SimpleUIFactory.Button("Maze"))
            {
                OnSelectMaze();
            }

            if (SimpleUIFactory.Button("Back"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
        }

        public void OnSelectDefault() => SelectMap(BattleMapType.Default);
        public void OnSelectOpenField() => SelectMap(BattleMapType.OpenField);
        public void OnSelectMaze() => SelectMap(BattleMapType.Maze);

        private void SelectMap(BattleMapType mapType)
        {
            GameManager.Instance?.SetMapType(mapType);
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickBack()
        {
            SceneFlowManager.Instance?.LoadModeSelect();
        }
    }
}
