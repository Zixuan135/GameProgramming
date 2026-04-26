using BubbleTown.AI;
using BubbleTown.Characters;
using BubbleTown.Core.Enums;
using BubbleTown.Items;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Battle HUD, minimal result detection, and battle scene callbacks.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("Result Flow")]
        [SerializeField, Min(0f)] private float resultSceneDelay = 0.75f;

        [Header("Pickup Toast")]
        [SerializeField, Min(0f)] private float pickupToastSeconds = 1.45f;
        [SerializeField] private Color pickupToastColor = new Color(1f, 0.94f, 0.55f);

        private bool resultQueued;
        private float resultTimer;
        private string hudHint = "Win/Lose MVP: defeat or get defeated by bombs. Use Force Result to test the Result scene.";
        private string pickupToastText;
        private float pickupToastTimer;

        private void OnEnable()
        {
            ItemBase.ItemPickedUp += HandleItemPickedUp;
        }

        private void OnDisable()
        {
            ItemBase.ItemPickedUp -= HandleItemPickedUp;
        }

        private void Update()
        {
            TickQueuedResult();
            TickPickupToast();

            if (!resultQueued)
            {
                EvaluateBattleResult();
            }
        }

        private void OnGUI()
        {
            DrawHud();
            DrawPickupToast();
            DrawActionButtons();
        }

        public void OnClickBackToMenu()
        {
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadMainMenu();
        }

        public void OnClickRetry()
        {
            GameManager.Instance?.ClearBattleResult();
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickForceResult()
        {
            QueueResult(
                "Battle Finished",
                "Manual result button was pressed for MVP flow testing.",
                "Manual Test");
        }

        private void EvaluateBattleResult()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.CurrentGameState != GameState.BattleRunning)
            {
                return;
            }

            PlayerController player1 = gameManager.Player1;
            PlayerController player2 = gameManager.Player2;
            AIController aiPlayer = gameManager.AIPlayer;

            if (player1 == null)
            {
                return;
            }

            bool player1Alive = IsAliveAndActive(player1);
            if (gameManager.CurrentGameMode == GameMode.AIBattle)
            {
                if (aiPlayer == null)
                {
                    return;
                }

                bool aiAlive = IsAliveAndActive(aiPlayer);
                if (!player1Alive && !aiAlive)
                {
                    QueueResult("Draw", "Player1 and AI were defeated at the same time.", "None");
                    return;
                }

                if (!player1Alive)
                {
                    QueueResult("Defeat", "Player1 was defeated by the AI.", "AI");
                    return;
                }

                if (!aiAlive)
                {
                    QueueResult("Victory", "Player1 defeated the AI.", "Player1");
                    return;
                }
            }
            else if (gameManager.CurrentGameMode == GameMode.LocalVS)
            {
                if (player2 == null)
                {
                    return;
                }

                bool player2Alive = IsAliveAndActive(player2);
                if (!player1Alive && !player2Alive)
                {
                    QueueResult("Draw", "Both players were defeated at the same time.", "None");
                    return;
                }

                if (!player1Alive)
                {
                    QueueResult("Player 2 Wins", "Player1 was defeated.", "Player2");
                    return;
                }

                if (!player2Alive)
                {
                    QueueResult("Player 1 Wins", "Player2 was defeated.", "Player1");
                    return;
                }
            }
            else if (!player1Alive)
            {
                QueueResult("Game Over", "Player1 was defeated during the single-player test.", "None");
            }
        }

        private void QueueResult(string title, string detail, string winner)
        {
            if (resultQueued)
            {
                return;
            }

            GameManager.Instance?.FinishBattle(title, detail, winner);
            resultQueued = true;
            resultTimer = resultSceneDelay;
            hudHint = "Battle finished. Loading result...";
        }

        private void TickQueuedResult()
        {
            if (!resultQueued)
            {
                return;
            }

            resultTimer -= Time.deltaTime;
            if (resultTimer > 0f)
            {
                return;
            }

            SceneFlowManager.Instance?.LoadResult();
        }

        private void TickPickupToast()
        {
            if (pickupToastTimer <= 0f)
            {
                return;
            }

            pickupToastTimer = Mathf.Max(0f, pickupToastTimer - Time.deltaTime);
        }

        private void DrawHud()
        {
            Rect hudRect = new Rect(18f, 18f, 620f, 150f);
            GUILayout.BeginArea(hudRect, GUI.skin.box);
            GUILayout.Label(BuildStatusText());
            GUILayout.Space(8f);
            GUILayout.Label(hudHint);
            GUILayout.EndArea();
        }

        private void DrawPickupToast()
        {
            if (pickupToastTimer <= 0f || string.IsNullOrEmpty(pickupToastText))
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = pickupToastColor;

            Rect toastRect = new Rect(Screen.width * 0.5f - 180f, 24f, 360f, 58f);
            GUILayout.BeginArea(toastRect, GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUILayout.Label(pickupToastText);
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();

            GUI.color = previousColor;
        }

        private void DrawActionButtons()
        {
            Rect buttonRect = new Rect(Screen.width - 190f, 18f, 170f, 190f);
            GUILayout.BeginArea(buttonRect, GUI.skin.box);

            if (GUILayout.Button("Retry", GUILayout.Height(42f)))
            {
                OnClickRetry();
            }

            if (GUILayout.Button("Force Result", GUILayout.Height(42f)))
            {
                OnClickForceResult();
            }

            if (GUILayout.Button("Main Menu", GUILayout.Height(42f)))
            {
                OnClickBackToMenu();
            }

            GUILayout.EndArea();
        }

        private string BuildStatusText()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return "Battle HUD waiting for GameManager...";
            }

            string player1State = FormatCharacterState("P1", gameManager.Player1);
            string player2State = FormatCharacterState("P2", gameManager.Player2);
            string aiState = FormatCharacterState("AI", gameManager.AIPlayer);

            return
                $"Mode: {gameManager.CurrentGameMode} | Map: {gameManager.CurrentMapType}\n" +
                $"{player1State}    {player2State}    {aiState}\n" +
                $"State: {gameManager.CurrentGameState}";
        }

        private string FormatCharacterState(string label, CharacterBase character)
        {
            if (character == null || !character.gameObject.activeInHierarchy)
            {
                return label + ": Off";
            }

            return character.IsAlive ? label + ": Alive" : label + ": Defeated";
        }

        private bool IsAliveAndActive(CharacterBase character)
        {
            return character != null && character.gameObject.activeInHierarchy && character.IsAlive;
        }

        private void HandleItemPickedUp(CharacterBase character, ItemBase item)
        {
            if (character == null || item == null)
            {
                return;
            }

            pickupToastText = $"{FormatCharacterName(character)} picked up {FormatItemName(item.ItemType)}";
            pickupToastTimer = pickupToastSeconds;
        }

        private string FormatCharacterName(CharacterBase character)
        {
            if (character == null)
            {
                return "Character";
            }

            if (character.name.Contains("Player1"))
            {
                return "Player1";
            }

            if (character.name.Contains("Player2"))
            {
                return "Player2";
            }

            if (character.name.Contains("AI"))
            {
                return "AI";
            }

            return character.name;
        }

        private string FormatItemName(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BombCountUp:
                    return "[Bomb Slot +1]";
                case ItemType.ExplosionRangeUp:
                    return "[Range +1]";
                case ItemType.MoveSpeedUp:
                    return "[Speed Up]";
                default:
                    return "[Power-Up]";
            }
        }
    }
}
