using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Result screen display and callbacks.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        private void OnGUI()
        {
            Rect panel = SimpleUIFactory.CenteredRect(720f, 500f);
            SimpleUIFactory.BeginPanel(panel);

            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.HasBattleResult)
            {
                SimpleUIFactory.Title(gameManager.LastResultTitle);
                SimpleUIFactory.Body(gameManager.LastResultDetail + "\nWinner: " + gameManager.LastResultWinner);
            }
            else
            {
                SimpleUIFactory.Title("No Result Yet");
                SimpleUIFactory.Body("No completed battle result was found. Use Retry to start a battle.");
            }

            SimpleUIFactory.FlexibleSpace();
            if (SimpleUIFactory.Button("Retry"))
            {
                OnClickRematch();
            }

            if (SimpleUIFactory.Button("Main Menu"))
            {
                OnClickBackToMenu();
            }

            SimpleUIFactory.EndPanel();
        }

        public void OnClickRematch()
        {
            GameManager.Instance?.ClearBattleResult();
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickBackToMenu()
        {
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
