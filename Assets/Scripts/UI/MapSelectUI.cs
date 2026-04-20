using UnityEngine;

namespace BubbleTown
{
    public class MapSelectUI : MonoBehaviour
    {
        public void SelectMap(int mapId)
        {
            GameManager.Instance.SetMapId(mapId);
            GameManager.Instance.StartBattleFlow();
        }

        public void BackToModeSelect()
        {
            SceneFlowManager.Instance.LoadScene(GameScene.ModeSelect);
        }
    }
}
