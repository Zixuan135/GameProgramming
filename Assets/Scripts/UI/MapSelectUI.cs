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
