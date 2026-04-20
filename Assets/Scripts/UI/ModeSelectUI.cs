using UnityEngine;

namespace BubbleTown
{
    public class ModeSelectUI : MonoBehaviour
    {
        public void SelectSinglePlayer()
        {
            SelectMode(GameMode.SinglePlayer);
        }

        public void SelectAIBattle()
        {
            SelectMode(GameMode.AIBattle);
        }

        public void SelectLocalVS()
        {
            SelectMode(GameMode.LocalVS);
        }

        private void SelectMode(GameMode mode)
        {
            GameManager.Instance.SetGameMode(mode);
            SceneFlowManager.Instance.LoadScene(GameScene.MapSelect);
        }
    }
}
