using UnityEngine;
using UnityEngine.UI;

namespace BubbleTown
{
    public class BattleUI : MonoBehaviour
    {
        [SerializeField] private Text modeText;
        [SerializeField] private Text aliveText;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBattleActorsSpawned += HandleActorsSpawned;
            }

            RefreshModeLabel();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBattleActorsSpawned -= HandleActorsSpawned;
            }
        }

        private void RefreshModeLabel()
        {
            if (modeText != null && GameManager.Instance != null)
            {
                modeText.text = $"Mode: {GameManager.Instance.CurrentMode}";
            }
        }

        private void HandleActorsSpawned(System.Collections.Generic.List<CharacterBase> actors)
        {
            if (aliveText != null)
            {
                aliveText.text = $"Alive: {actors.Count}";
            }
        }

        public void BackToMainMenu()
        {
            SceneFlowManager.Instance.LoadScene(GameScene.MainMenu);
        }
    }
}
