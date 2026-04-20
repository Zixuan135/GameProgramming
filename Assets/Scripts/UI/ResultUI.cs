using UnityEngine;
using UnityEngine.UI;

namespace BubbleTown
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text resultText;

        private void Start()
        {
            RefreshResultLabel();
        }

        private void RefreshResultLabel()
        {
            if (resultText == null || GameManager.Instance == null)
            {
                return;
            }

            resultText.text = $"Result: {GameManager.Instance.LastMatchOutcome}";
        }

        public void OnClickPlayAgain()
        {
            GameManager.Instance.StartBattleFlow();
        }

        public void OnClickBackToMenu()
        {
            SceneFlowManager.Instance.LoadScene(GameScene.MainMenu);
        }
    }
}
