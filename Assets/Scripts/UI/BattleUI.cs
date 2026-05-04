using System.Collections.Generic;
using BubbleTown.AI;
using BubbleTown.CameraSystem;
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
        private const int TextureSize = 64;
        private const float LeftHudX = 14f;
        private const float LeftHudWidth = 286f;
        private const float ObjectivePanelBottom = 144f;
        private const float CharacterAreaTop = ObjectivePanelBottom + 14f;
        private const float CharacterSectionGap = 14f;
        private const float BottomHudMargin = 18f;
        private const float BottomHudGap = 10f;
        private const float ActionPanelHeight = 46f;
        private const float ItemGuidePanelHeight = 44f;

        [Header("Result Timing")]
        [SerializeField, Min(0f)] private float resultSceneDelay = 0.95f;
        [SerializeField, Min(0f)] private float localVsNextRoundDelay = 1.45f;

        [Header("Battle Timer")]
        [SerializeField] private bool countTimerOnlyWhileRunning = true;
        [SerializeField] private bool resetTimerOnEnable = true;

        [Header("Opening Prompt")]
        [SerializeField] private bool showOpeningPrompt = true;
        [SerializeField, Min(0f)] private float readyPromptSeconds = 1.05f;
        [SerializeField, Min(0f)] private float goPromptSeconds = 0.75f;
        [SerializeField, Min(0f)] private float spawnProtectionSeconds = 2.25f;
        [SerializeField] private string readyText = "READY";
        [SerializeField] private string goText = "GO!";

        [Header("Pickup Toast")]
        [SerializeField, Min(0f)] private float pickupToastSeconds = 1.45f;
        [SerializeField] private Color pickupToastColor = new Color(1f, 0.94f, 0.55f);

        [Header("Feedback")]
        [SerializeField] private bool enableHudFeedbackShake = true;
        [SerializeField, Min(0f)] private float pickupCameraShakeDuration = 0.06f;
        [SerializeField, Min(0f)] private float pickupCameraShakeMagnitude = 0.035f;
        [SerializeField, Min(0f)] private float resultCameraShakeDuration = 0.14f;
        [SerializeField, Min(0f)] private float resultCameraShakeMagnitude = 0.085f;

        [Header("HUD Colors")]
        [SerializeField] private Color player1Color = new Color(0.12f, 0.72f, 1f);
        [SerializeField] private Color player2Color = new Color(1f, 0.45f, 0.26f);
        [SerializeField] private Color aiColor = new Color(0.64f, 0.46f, 1f);
        [SerializeField] private Color neutralColor = new Color(1f, 0.82f, 0.32f);

        private static readonly Dictionary<string, Texture2D> RoundedTextureCache = new Dictionary<string, Texture2D>();
        private GUIStyle hudTextStyle;
        private GUIStyle hudSmallStyle;
        private GUIStyle hudPillLabelStyle;
        private GUIStyle hudPillValueStyle;
        private GUIStyle abilityLabelStyle;
        private GUIStyle abilityValueStyle;
        private GUIStyle promptTitleStyle;
        private GUIStyle promptBodyStyle;
        private GUIStyle toastStyle;
        private GUIStyle buttonStyle;
        private GUIStyle guideTitleStyle;
        private GUIStyle guideItemNameStyle;
        private GUIStyle guideItemBodyStyle;
        private GUIStyle guideTipStyle;

        private bool resultQueued;
        private bool localVsNextRoundQueued;
        private bool openingFlowStarted;
        private bool roundStartTriggered;
        private bool isItemGuideOpen;
        private float resultTimer;
        private float localVsNextRoundTimer;
        private float battleElapsedSeconds;
        private float openingPromptTimer;
        private string pickupToastText;
        private float pickupToastTimer;
        private string resultPromptTitle;
        private string resultPromptDetail;

        private void Awake()
        {
            ResetBattleHudState();
        }

        private void OnEnable()
        {
            ItemBase.ItemPickedUp += HandleItemPickedUp;
            if (resetTimerOnEnable)
            {
                ResetBattleHudState();
            }
        }

        private void OnDisable()
        {
            ItemBase.ItemPickedUp -= HandleItemPickedUp;
        }

        private void Update()
        {
            EnsureOpeningFlowStarted();
            TickOpeningPrompt();
            TickBattleTimer();
            TickQueuedResult();
            TickQueuedLocalVsNextRound();
            TickPickupToast();

            if (!resultQueued && !localVsNextRoundQueued)
            {
                EvaluateBattleResult();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawBattleHud();
            DrawActionButtons();
            DrawItemGuide();
            DrawPickupToast();
            DrawOpeningPrompt();
            DrawResultPrompt();
        }

        public void OnClickBackToMenu()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadMainMenu();
        }

        public void OnClickRetry()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.ClearBattleResult();
                if (gameManager.CurrentGameMode == GameMode.LocalVS)
                {
                    gameManager.ResetLocalVsMatch();
                }
            }

            ResetBattleHudState();
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void ShowBattleResultPrompt(string title, string detail, string winner)
        {
            QueueResult(title, detail, winner);
        }

        private void ResetBattleHudState()
        {
            battleElapsedSeconds = 0f;
            openingFlowStarted = false;
            roundStartTriggered = false;
            openingPromptTimer = 0f;
            resultQueued = false;
            localVsNextRoundQueued = false;
            isItemGuideOpen = false;
            resultTimer = 0f;
            localVsNextRoundTimer = 0f;
            resultPromptTitle = string.Empty;
            resultPromptDetail = string.Empty;
            pickupToastText = string.Empty;
            pickupToastTimer = 0f;
        }

        private void EnsureOpeningFlowStarted()
        {
            if (openingFlowStarted)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return;
            }

            bool canStartOpening = gameManager.CurrentGameState == GameState.BattlePreparing ||
                                   gameManager.CurrentGameState == GameState.BattleRunning;
            if (!canStartOpening)
            {
                return;
            }

            openingFlowStarted = true;
            roundStartTriggered = gameManager.CurrentGameState == GameState.BattleRunning;
            openingPromptTimer = showOpeningPrompt
                ? roundStartTriggered ? goPromptSeconds : readyPromptSeconds + goPromptSeconds
                : 0f;

            if (!showOpeningPrompt && !roundStartTriggered)
            {
                StartRoundWithProtection();
                return;
            }

        }

        private void TickBattleTimer()
        {
            GameManager gameManager = GameManager.Instance;
            if (countTimerOnlyWhileRunning && (gameManager == null || gameManager.CurrentGameState != GameState.BattleRunning))
            {
                return;
            }

            if (resultQueued || localVsNextRoundQueued)
            {
                return;
            }

            battleElapsedSeconds += Time.deltaTime;
        }

        private void TickOpeningPrompt()
        {
            if (!openingFlowStarted || openingPromptTimer <= 0f)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.CurrentGameState == GameState.BattleFinished)
            {
                return;
            }

            openingPromptTimer = Mathf.Max(0f, openingPromptTimer - Time.deltaTime);
            if (!roundStartTriggered && openingPromptTimer <= goPromptSeconds)
            {
                StartRoundWithProtection();
            }

        }

        private void StartRoundWithProtection()
        {
            if (roundStartTriggered)
            {
                return;
            }

            roundStartTriggered = true;
            GameManager.Instance?.StartBattleRound(spawnProtectionSeconds);
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
                    QueueLocalVsRoundResult("Round Draw", "Both players were defeated at the same time.", "None");
                    return;
                }

                if (!player1Alive)
                {
                    QueueLocalVsRoundResult("Player 2 Wins Round", "Player1 was defeated.", "Player2");
                    return;
                }

                if (!player2Alive)
                {
                    QueueLocalVsRoundResult("Player 1 Wins Round", "Player2 was defeated.", "Player1");
                    return;
                }
            }
            else if (!player1Alive)
            {
                QueueResult("Game Over", "Player1 was caught by an explosion.", "None");
            }
            else if (gameManager.CurrentGameMode == GameMode.SinglePlayer)
            {
                gameManager.RefreshSinglePlayerRouteObjective();
                if (gameManager.IsSinglePlayerObjectiveComplete)
                {
                    QueueResult(
                        "Objective Clear",
                        "Player1 opened a path through the soft walls and reached the exit.",
                        "Player1");
                }
            }
        }

        private void QueueResult(string title, string detail, string winner)
        {
            if (resultQueued)
            {
                return;
            }

            resultPromptTitle = string.IsNullOrEmpty(title) ? "Battle Finished" : title;
            resultPromptDetail = string.IsNullOrEmpty(detail) ? "The battle has ended." : detail;
            GameManager.Instance?.FinishBattle(resultPromptTitle, resultPromptDetail, winner);
            resultQueued = true;
            localVsNextRoundQueued = false;
            resultTimer = resultSceneDelay;
            PlayHudFeedbackShake(resultCameraShakeDuration, resultCameraShakeMagnitude);
        }

        private void QueueLocalVsRoundResult(string title, string detail, string roundWinner)
        {
            if (resultQueued || localVsNextRoundQueued)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                QueueResult(title, detail, roundWinner);
                return;
            }

            bool matchComplete = gameManager.RegisterLocalVsRoundResult(roundWinner);
            string roundDetail = $"{detail}\nScore: {gameManager.LocalVsScoreLabel}";
            if (matchComplete)
            {
                string matchWinner = gameManager.ResolveLocalVsMatchWinner();
                string matchTitle = matchWinner == "Player1" ? "Player 1 Wins Match" :
                    matchWinner == "Player2" ? "Player 2 Wins Match" : "Local VS Draw";
                QueueResult(matchTitle, $"{roundDetail}\nBest of 3 complete.", matchWinner);
                return;
            }

            resultPromptTitle = string.IsNullOrEmpty(title) ? "Round Finished" : title;
            resultPromptDetail = $"{roundDetail}\nNext round starts soon.";
            localVsNextRoundQueued = true;
            localVsNextRoundTimer = localVsNextRoundDelay;
            PlayHudFeedbackShake(resultCameraShakeDuration, resultCameraShakeMagnitude * 0.75f);
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

        private void TickQueuedLocalVsNextRound()
        {
            if (!localVsNextRoundQueued)
            {
                return;
            }

            localVsNextRoundTimer -= Time.deltaTime;
            if (localVsNextRoundTimer > 0f)
            {
                return;
            }

            GameManager.Instance?.ClearBattleResult();
            ResetBattleHudState();
            SceneFlowManager.Instance?.LoadBattle();
        }

        private void TickPickupToast()
        {
            if (pickupToastTimer <= 0f)
            {
                return;
            }

            pickupToastTimer = Mathf.Max(0f, pickupToastTimer - Time.deltaTime);
        }

        private void DrawBattleHud()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                DrawPanel(new Rect(14f, 14f, 260f, 46f), new Color(1f, 0.96f, 0.72f, 0.86f), neutralColor, 16, 2);
                GUI.Label(new Rect(28f, 22f, 232f, 28f), "Preparing arena...", hudTextStyle);
                return;
            }

            DrawTopStatusBar(gameManager);
            DrawSinglePlayerObjectivePanel(gameManager);
            DrawLocalVsScoreboard(gameManager);

            CharacterBase rightCharacter = ResolveRightSideCharacter(gameManager, out string rightLabel, out Color rightColor);
            bool hasRightCharacter = rightCharacter != null && rightCharacter.gameObject.activeInHierarchy;
            float characterPanelHeight = hasRightCharacter ? 104f : 116f;
            float characterPanelGap = 10f;
            float totalCharacterHeight = hasRightCharacter
                ? characterPanelHeight * 2f + characterPanelGap
                : characterPanelHeight;
            float characterAreaHeight = Mathf.Max(totalCharacterHeight, ResolveCharacterAreaBottom() - CharacterAreaTop);
            float characterPanelY = CharacterAreaTop + Mathf.Max(0f, (characterAreaHeight - totalCharacterHeight) * 0.5f);
            DrawCharacterPanel(new Rect(LeftHudX, characterPanelY, LeftHudWidth, characterPanelHeight), "PLAYER 1", gameManager.Player1, player1Color);

            if (hasRightCharacter)
            {
                DrawCharacterPanel(new Rect(LeftHudX, characterPanelY + characterPanelHeight + characterPanelGap, LeftHudWidth, characterPanelHeight), rightLabel, rightCharacter, rightColor);
            }
        }

        private void DrawTopStatusBar(GameManager gameManager)
        {
            Rect topRect = new Rect(14f, 14f, 286f, 76f);
            DrawPanel(topRect, new Color(1f, 0.96f, 0.72f, 0.86f), new Color(0.18f, 0.67f, 0.95f, 0.94f), 16, 2);

            float x = topRect.x + 10f;
            float y = topRect.y + 9f;
            DrawInfoPill(new Rect(x, y, 130f, 25f), "MODE", FormatModeName(gameManager.CurrentGameMode), new Color(0.12f, 0.72f, 1f));
            DrawInfoPill(new Rect(x + 136f, y, 130f, 25f), "MAP", FormatMapName(gameManager.CurrentMapType), new Color(0.48f, 0.9f, 0.34f));
            DrawInfoPill(new Rect(x, y + 33f, 130f, 25f), "TIME", FormatTime(battleElapsedSeconds), new Color(1f, 0.58f, 0.18f));
            DrawInfoPill(new Rect(x + 136f, y + 33f, 130f, 25f), "STATE", FormatRoundState(gameManager), neutralColor);
        }

        private void DrawSinglePlayerObjectivePanel(GameManager gameManager)
        {
            if (gameManager.CurrentGameMode != GameMode.SinglePlayer || !gameManager.IsSinglePlayerObjectiveEnabled)
            {
                return;
            }

            Rect rect = new Rect(14f, 98f, 286f, 46f);
            DrawPanel(rect, new Color(1f, 0.95f, 0.72f, 0.84f), new Color(1f, 0.58f, 0.18f, 0.92f), 16, 2);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 5f, rect.width * 0.46f, 18f), gameManager.SinglePlayerObjectiveLabel, hudTextStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.5f, rect.y + 5f, rect.width * 0.44f, 18f), gameManager.SinglePlayerObjectiveProgressLabel, hudSmallStyle);

            float progress = Mathf.Clamp01(gameManager.SinglePlayerRouteProgress);
            Rect progressBack = new Rect(rect.x + 16f, rect.y + rect.height - 12f, rect.width - 32f, 6f);
            GUI.DrawTexture(progressBack, GetRoundedTexture(new Color(0.16f, 0.34f, 0.44f, 0.28f), Color.clear, 3, 0));
            GUI.DrawTexture(
                new Rect(progressBack.x, progressBack.y, progressBack.width * progress, progressBack.height),
                GetRoundedTexture(new Color(1f, 0.62f, 0.16f, 0.95f), Color.clear, 3, 0));
        }

        private void DrawLocalVsScoreboard(GameManager gameManager)
        {
            if (gameManager.CurrentGameMode != GameMode.LocalVS)
            {
                return;
            }

            Rect rect = new Rect(14f, 98f, 286f, 46f);
            DrawPanel(rect, new Color(1f, 0.95f, 0.72f, 0.84f), new Color(0.52f, 0.9f, 0.35f, 0.92f), 16, 2);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 5f, rect.width - 24f, 18f), FormatLocalVsRoundHeader(gameManager), hudSmallStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 22f, rect.width - 24f, 20f), gameManager.LocalVsScoreLabel, hudTextStyle);
        }

        private CharacterBase ResolveRightSideCharacter(GameManager gameManager, out string label, out Color color)
        {
            switch (gameManager.CurrentGameMode)
            {
                case GameMode.LocalVS:
                    label = "PLAYER 2";
                    color = player2Color;
                    return gameManager.Player2;
                case GameMode.AIBattle:
                    label = "AI RIVAL";
                    color = aiColor;
                    return gameManager.AIPlayer;
                default:
                    label = "SOLO RUN";
                    color = neutralColor;
                    return null;
            }
        }

        private void DrawCharacterPanel(Rect rect, string label, CharacterBase character, Color accentColor)
        {
            DrawPanel(rect, new Color(1f, 0.94f, 0.72f, 0.82f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.92f), 16, 2);
            DrawInfoPill(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 26f), label, FormatLifeState(character), accentColor);

            if (character == null || !character.gameObject.activeInHierarchy)
            {
                DrawLockedLabel(new Rect(rect.x + 14f, rect.y + 48f, rect.width - 28f, 32f), "Not active", hudSmallStyle);
                return;
            }

            DrawAbilityRow(
                rect,
                character.RemainingBombCount,
                character.MaxBombCount,
                character.BombRange,
                character.MoveSpeed,
                character.ShieldCharges,
                accentColor);
        }

        private void DrawAbilityRow(Rect panelRect, int remainingBombs, int maxBombs, int range, float speed, int shieldCharges, Color accentColor)
        {
            float y = panelRect.y + 46f;
            float gap = 7f;
            float itemWidth = (panelRect.width - 20f - gap * 3f) / 4f;
            DrawAbilityBox(new Rect(panelRect.x + 10f, y, itemWidth, 44f), "Bombs", remainingBombs + "/" + maxBombs, accentColor);
            DrawAbilityBox(new Rect(panelRect.x + 10f + (itemWidth + gap), y, itemWidth, 44f), "Range", range.ToString(), new Color(1f, 0.58f, 0.18f));
            DrawAbilityBox(new Rect(panelRect.x + 10f + (itemWidth + gap) * 2f, y, itemWidth, 44f), "Speed", speed.ToString("0.0"), new Color(0.48f, 0.9f, 0.34f));
            DrawAbilityBox(new Rect(panelRect.x + 10f + (itemWidth + gap) * 3f, y, itemWidth, 44f), "Guard", shieldCharges.ToString(), new Color(0.35f, 0.78f, 1f));
        }

        private void DrawAbilityBox(Rect rect, string label, string value, Color accentColor)
        {
            DrawPanel(rect, new Color(1f, 0.98f, 0.86f, 0.86f), Color.Lerp(accentColor, Color.white, 0.2f), 12, 1);
            DrawLockedLabel(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 15f), label, abilityLabelStyle);
            DrawLockedLabel(new Rect(rect.x + 4f, rect.y + 20f, rect.width - 8f, 20f), value, abilityValueStyle);
        }

        private void DrawInfoPill(Rect rect, string label, string value, Color accentColor)
        {
            DrawPanel(rect, accentColor, Color.white, 15, 2);
            float labelWidth = label.Length <= 5
                ? 36f
                : Mathf.Min(rect.width * 0.58f, label.Length * 8f + 16f);
            DrawLockedLabel(new Rect(rect.x + 8f, rect.y + 5f, labelWidth, rect.height - 10f), label, hudPillLabelStyle);
            DrawLockedLabel(new Rect(rect.x + 10f + labelWidth, rect.y + 4f, rect.width - labelWidth - 16f, rect.height - 8f), value, hudPillValueStyle);
        }

        private void DrawOpeningPrompt()
        {
            if (openingPromptTimer <= 0f)
            {
                return;
            }

            bool readyPhase = !roundStartTriggered;
            string prompt = readyPhase ? readyText : goText;
            Color promptColor = readyPhase ? new Color(1f, 0.74f, 0.18f, 0.95f) : new Color(0.2f, 0.88f, 1f, 0.95f);
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.04f;
            Rect rect = new Rect(Screen.width * 0.5f - 160f * pulse, Screen.height * 0.5f - 58f * pulse, 320f * pulse, 116f * pulse);
            DrawPanel(rect, new Color(1f, 0.96f, 0.72f, 0.96f), promptColor, 24, 5);
            GUI.Label(new Rect(rect.x, rect.y + 18f, rect.width, 62f), prompt, promptTitleStyle);
            string body = readyPhase ? "Controls unlock on GO" : "Move!";
            float protectionRemaining = ResolveMaxProtectionRemaining(GameManager.Instance);
            if (!readyPhase && protectionRemaining > 0f)
            {
                body = $"Shield {protectionRemaining:0.0}s";
            }

            GUI.Label(new Rect(rect.x + 18f, rect.y + 78f, rect.width - 36f, 24f), body, promptBodyStyle);
        }

        private void DrawResultPrompt()
        {
            if (!resultQueued && !localVsNextRoundQueued)
            {
                return;
            }

            Rect rect = new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 92f, 460f, 184f);
            Matrix4x4 previousMatrix = GUI.matrix;
            float entrance = ResolveQueuedPromptEntrance();
            float easedEntrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(entrance / 0.28f));
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.025f;
            float scale = Mathf.Lerp(0.86f, 1f, easedEntrance) * pulse;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);

            DrawPanel(rect, new Color(1f, 0.96f, 0.72f, 0.97f), new Color(1f, 0.58f, 0.18f), 24, 5);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 22f, rect.width - 36f, 58f), resultPromptTitle, promptTitleStyle);
            GUI.Label(new Rect(rect.x + 28f, rect.y + 84f, rect.width - 56f, 82f), resultPromptDetail, promptBodyStyle);
            GUI.matrix = previousMatrix;
        }

        private void DrawPickupToast()
        {
            if (pickupToastTimer <= 0f || string.IsNullOrEmpty(pickupToastText))
            {
                return;
            }

            float toastY = Screen.height - 152f;
            Rect toastRect = new Rect(Screen.width * 0.5f - 150f, toastY, 300f, 40f);
            float elapsed01 = pickupToastSeconds <= 0f ? 1f : Mathf.Clamp01(1f - pickupToastTimer / pickupToastSeconds);
            float pop01 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed01 / 0.22f));
            float fadeAlpha = pickupToastSeconds <= 0f
                ? 1f
                : Mathf.Clamp01(pickupToastTimer / Mathf.Min(0.35f, Mathf.Max(0.01f, pickupToastSeconds)));
            float scale = Mathf.Lerp(0.9f, 1f, pop01) + Mathf.Sin(pop01 * Mathf.PI) * 0.08f;
            float slideOffset = Mathf.Lerp(-18f, 0f, pop01);
            toastRect.y += slideOffset;

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * fadeAlpha);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), toastRect.center);
            DrawPanel(toastRect, pickupToastColor, Color.white, 18, 3);
            GUI.Label(toastRect, pickupToastText, toastStyle);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private void DrawActionButtons()
        {
            float bob = Mathf.Sin(Time.unscaledTime * 2.1f) * 0.7f;
            Rect buttonRect = new Rect(LeftHudX, ResolveBottomActionPanelY() + bob, LeftHudWidth, ActionPanelHeight);
            DrawPanel(buttonRect, new Color(1f, 0.96f, 0.72f, 0.84f), new Color(1f, 0.58f, 0.18f, 0.92f), 16, 2);
            GUILayout.BeginArea(new Rect(buttonRect.x + 10f, buttonRect.y + 10f, buttonRect.width - 20f, buttonRect.height - 20f));
            GUILayout.BeginHorizontal();

            if (AnimatedActionButton("Retry"))
            {
                OnClickRetry();
            }

            if (AnimatedActionButton("Main Menu"))
            {
                OnClickBackToMenu();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawItemGuide()
        {
            float bob = Mathf.Sin(Time.unscaledTime * 2.1f + 1.4f) * 0.7f;
            Rect buttonPanelRect = new Rect(LeftHudX, ResolveBottomItemGuidePanelY() + bob, LeftHudWidth, ItemGuidePanelHeight);
            DrawPanel(buttonPanelRect, new Color(1f, 0.96f, 0.72f, 0.84f), new Color(0.35f, 0.78f, 1f, 0.92f), 16, 2);

            Rect buttonRect = new Rect(buttonPanelRect.x + 10f, buttonPanelRect.y + 9f, buttonPanelRect.width - 20f, 26f);
            if (AnimatedFixedButton(buttonRect, isItemGuideOpen ? "Hide Guide" : "Item Guide"))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                isItemGuideOpen = !isItemGuideOpen;
            }

            if (isItemGuideOpen)
            {
                DrawItemGuidePanel();
            }
        }

        private float ResolveBottomActionPanelY()
        {
            return Screen.height - BottomHudMargin - ActionPanelHeight;
        }

        private float ResolveBottomItemGuidePanelY()
        {
            return ResolveBottomActionPanelY() - BottomHudGap - ItemGuidePanelHeight;
        }

        private float ResolveCharacterAreaBottom()
        {
            return ResolveBottomItemGuidePanelY() - CharacterSectionGap;
        }

        private void DrawItemGuidePanel()
        {
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                GetRoundedTexture(new Color(0.07f, 0.2f, 0.28f, 0.24f), Color.clear, 1, 0));

            Rect panelRect = ResolveCenteredItemGuideRect();
            DrawPanel(panelRect, new Color(1f, 0.96f, 0.74f, 0.95f), new Color(0.18f, 0.67f, 0.95f, 0.98f), 22, 4);

            Rect titleRect = new Rect(panelRect.x + panelRect.width * 0.5f - 118f, panelRect.y + 14f, 236f, 34f);
            DrawPanel(titleRect, new Color(0.12f, 0.72f, 1f, 0.96f), Color.white, 17, 2);
            DrawLockedLabel(titleRect, "ITEM GUIDE", guideTitleStyle);

            Rect closeRect = new Rect(panelRect.x + panelRect.width - 54f, panelRect.y + 14f, 34f, 28f);
            if (AnimatedFixedButton(closeRect, "X"))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                isItemGuideOpen = false;
            }

            float contentX = panelRect.x + 24f;
            float contentY = panelRect.y + 66f;
            float gap = 10f;
            float cardHeight = 56f;
            float cardWidth = (panelRect.width - 48f - gap) * 0.5f;

            DrawItemGuideCard(new Rect(contentX, contentY, cardWidth, cardHeight), ItemType.BombCountUp, "Bomb Slot", "Extra bomb slot.", new Color(0.12f, 0.72f, 1f));
            DrawItemGuideCard(new Rect(contentX + cardWidth + gap, contentY, cardWidth, cardHeight), ItemType.ExplosionRangeUp, "Blast Range", "Longer blast lines.", new Color(1f, 0.58f, 0.18f));
            DrawItemGuideCard(new Rect(contentX, contentY + cardHeight + gap, cardWidth, cardHeight), ItemType.MoveSpeedUp, "Speed Boots", "Move faster.", new Color(0.48f, 0.9f, 0.34f));
            DrawItemGuideCard(new Rect(contentX + cardWidth + gap, contentY + cardHeight + gap, cardWidth, cardHeight), ItemType.Shield, "Shield", "Blocks one hit.", new Color(0.35f, 0.78f, 1f));
            DrawItemGuideCard(new Rect(contentX, contentY + (cardHeight + gap) * 2f, panelRect.width - 48f, cardHeight), ItemType.TemporaryInvincible, "Invincible", "Brief safety time.", new Color(0.72f, 0.48f, 1f));

            float tipWidth = Mathf.Min(340f, panelRect.width - 140f);
            Rect tipRect = new Rect(panelRect.center.x - tipWidth * 0.5f, panelRect.y + panelRect.height - 40f, tipWidth, 24f);
            DrawPanel(tipRect, new Color(0.48f, 0.9f, 0.34f, 0.72f), Color.white, 12, 1);
            DrawLockedLabel(tipRect, "Soft blocks hide power-ups.", guideTipStyle);
        }

        private Rect ResolveCenteredItemGuideRect()
        {
            float width = Mathf.Min(620f, Mathf.Max(420f, Screen.width - 70f));
            float height = Mathf.Min(330f, Mathf.Max(312f, Screen.height - 70f));
            return new Rect(
                Mathf.Max(16f, (Screen.width - width) * 0.5f),
                Mathf.Max(16f, (Screen.height - height) * 0.5f),
                width,
                height);
        }

        private void DrawItemGuideCard(Rect cardRect, ItemType itemType, string title, string body, Color accentColor)
        {
            DrawPanel(cardRect, new Color(1f, 0.99f, 0.88f, 0.9f), Color.Lerp(accentColor, Color.white, 0.18f), 15, 2);

            Rect iconRect = new Rect(cardRect.x + 10f, cardRect.y + 9f, 40f, 40f);
            DrawAnimatedItemGuideIcon(iconRect, itemType, accentColor);

            DrawLockedLabel(new Rect(cardRect.x + 60f, cardRect.y + 9f, cardRect.width - 70f, 20f), title, guideItemNameStyle);
            DrawLockedLabel(new Rect(cardRect.x + 60f, cardRect.y + 30f, cardRect.width - 70f, 18f), body, guideItemBodyStyle);
        }

        private void DrawLockedLabel(Rect rect, string text, GUIStyle style)
        {
            if (style == null)
            {
                GUI.Label(rect, text);
                return;
            }

            Color textColor = style.normal.textColor;
            LockStyleTextColor(style, textColor);

            Color previousContentColor = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUI.Label(rect, text, style);
            GUI.contentColor = previousContentColor;
        }

        private void LockStyleTextColor(GUIStyle style, Color textColor)
        {
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.active.textColor = textColor;
            style.focused.textColor = textColor;
            style.onNormal.textColor = textColor;
            style.onHover.textColor = textColor;
            style.onActive.textColor = textColor;
            style.onFocused.textColor = textColor;
        }

        private void DrawAnimatedItemGuideIcon(Rect iconRect, ItemType itemType, Color accentColor)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            float phase = (int)itemType * 0.73f;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4.2f + phase) * 0.045f;
            float bob = Mathf.Sin(Time.unscaledTime * 3.1f + phase) * 1.4f;
            float tilt = Mathf.Sin(Time.unscaledTime * 2.5f + phase) * 4.5f;
            Rect animatedRect = new Rect(iconRect.x, iconRect.y + bob, iconRect.width, iconRect.height);

            GUIUtility.RotateAroundPivot(tilt, animatedRect.center);
            GUIUtility.ScaleAroundPivot(new Vector2(pulse, pulse), animatedRect.center);
            DrawStickerIconBack(animatedRect, accentColor);
            DrawItemGuideIcon(animatedRect, itemType);
            GUI.matrix = previousMatrix;
        }

        private void DrawStickerIconBack(Rect iconRect, Color accentColor)
        {
            DrawCircle(new Rect(iconRect.x + 2f, iconRect.y + 4f, iconRect.width - 2f, iconRect.height - 2f), new Color(0.04f, 0.22f, 0.34f, 0.22f));
            DrawCircle(iconRect, Color.white);
            DrawCircle(new Rect(iconRect.x + 4f, iconRect.y + 4f, iconRect.width - 8f, iconRect.height - 8f), Color.Lerp(accentColor, Color.white, 0.08f));
            DrawCircle(new Rect(iconRect.x + 9f, iconRect.y + 8f, 8f, 8f), new Color(1f, 1f, 1f, 0.55f));
        }

        private void DrawItemGuideIcon(Rect iconRect, ItemType itemType)
        {
            Rect inner = new Rect(iconRect.x + 6f, iconRect.y + 6f, iconRect.width - 12f, iconRect.height - 12f);
            Color cream = new Color(1f, 0.98f, 0.76f, 1f);
            Color navy = new Color(0.09f, 0.25f, 0.36f, 1f);
            Color pink = new Color(1f, 0.5f, 0.78f, 1f);
            Color yellow = new Color(1f, 0.88f, 0.28f, 1f);
            Color cyan = new Color(0.26f, 0.9f, 1f, 1f);

            switch (itemType)
            {
                case ItemType.BombCountUp:
                    DrawCircle(new Rect(inner.x + 2f, inner.y + 8f, 18f, 18f), navy);
                    DrawCircle(new Rect(inner.x + 6f, inner.y + 10f, 5f, 5f), cyan);
                    DrawSolidRect(new Rect(inner.x + 17f, inner.y + 5f, 8f, 6f), yellow, 3);
                    DrawSolidRect(new Rect(inner.x + 22f, inner.y + 2f, 4f, 6f), cream, 2);
                    DrawCircle(new Rect(inner.x + 25f, inner.y, 6f, 6f), pink);
                    DrawCircle(new Rect(inner.x + 20f, inner.y - 2f, 4f, 4f), yellow);
                    break;
                case ItemType.ExplosionRangeUp:
                    DrawCircle(new Rect(inner.x + 10f, inner.y + 10f, 10f, 10f), yellow);
                    DrawSolidRect(new Rect(inner.x + 2f, inner.y + 13f, 26f, 5f), yellow, 3);
                    DrawSolidRect(new Rect(inner.x + 13f, inner.y + 2f, 5f, 26f), yellow, 3);
                    DrawCircle(new Rect(inner.x, inner.y + 10f, 10f, 10f), cream);
                    DrawCircle(new Rect(inner.x + 21f, inner.y + 10f, 10f, 10f), cream);
                    DrawCircle(new Rect(inner.x + 10f, inner.y, 10f, 10f), pink);
                    DrawCircle(new Rect(inner.x + 10f, inner.y + 21f, 10f, 10f), cyan);
                    break;
                case ItemType.MoveSpeedUp:
                    DrawSolidRect(new Rect(inner.x + 7f, inner.y + 17f, 18f, 7f), cream, 4);
                    DrawSolidRect(new Rect(inner.x + 11f, inner.y + 7f, 9f, 13f), cream, 4);
                    DrawSolidRect(new Rect(inner.x + 21f, inner.y + 21f, 9f, 4f), yellow, 2);
                    DrawCircle(new Rect(inner.x + 20f, inner.y + 7f, 6f, 6f), yellow);
                    DrawSolidRect(new Rect(inner.x + 1f, inner.y + 8f, 8f, 3f), cyan, 2);
                    DrawSolidRect(new Rect(inner.x - 1f, inner.y + 15f, 10f, 3f), cyan, 2);
                    DrawSolidRect(new Rect(inner.x + 1f, inner.y + 22f, 8f, 3f), cyan, 2);
                    break;
                case ItemType.Shield:
                    DrawCircle(new Rect(inner.x + 5f, inner.y + 3f, 20f, 20f), cream);
                    DrawSolidRect(new Rect(inner.x + 8f, inner.y + 17f, 14f, 8f), cream, 5);
                    DrawSolidRect(new Rect(inner.x + 12f, inner.y + 8f, 7f, 14f), cyan, 3);
                    DrawSolidRect(new Rect(inner.x + 8f, inner.y + 12f, 15f, 5f), cyan, 2);
                    DrawCircle(new Rect(inner.x + 4f, inner.y + 2f, 5f, 5f), Color.white);
                    break;
                case ItemType.TemporaryInvincible:
                    DrawSolidRect(new Rect(inner.x + 14f, inner.y + 2f, 5f, 27f), yellow, 3);
                    DrawSolidRect(new Rect(inner.x + 3f, inner.y + 13f, 27f, 5f), yellow, 3);
                    DrawCircle(new Rect(inner.x + 10f, inner.y + 9f, 13f, 13f), pink);
                    DrawCircle(new Rect(inner.x + 1f, inner.y + 3f, 5f, 5f), cream);
                    DrawCircle(new Rect(inner.x + 25f, inner.y + 21f, 5f, 5f), cream);
                    DrawCircle(new Rect(inner.x + 23f, inner.y + 2f, 4f, 4f), cyan);
                    break;
                default:
                    DrawCircle(new Rect(inner.x + 6f, inner.y + 5f, 17f, 17f), cream);
                    DrawSolidRect(new Rect(inner.x + 13f, inner.y + 9f, 4f, 9f), navy, 2);
                    break;
            }
        }

        private void DrawSolidRect(Rect rect, Color color, int radius = 1)
        {
            GUI.DrawTexture(rect, GetRoundedTexture(color, Color.clear, radius, 0));
        }

        private void DrawCircle(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, GetRoundedTexture(color, Color.clear, Mathf.RoundToInt(Mathf.Min(rect.width, rect.height) * 0.5f), 0));
        }

        private bool AnimatedActionButton(string text)
        {
            Rect rect = GUILayoutUtility.GetRect(118f, 26f, GUILayout.ExpandWidth(true));
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = ResolveButtonScale(rect);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
            bool clicked = GUI.Button(rect, text, buttonStyle);
            GUI.matrix = previousMatrix;
            GUILayout.Space(4f);
            return clicked;
        }

        private bool AnimatedFixedButton(Rect rect, string text)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = ResolveButtonScale(rect);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
            bool clicked = GUI.Button(rect, text, buttonStyle);
            GUI.matrix = previousMatrix;
            return clicked;
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
                case ItemType.Shield:
                    return "[Shield]";
                case ItemType.TemporaryInvincible:
                    return "[Invincible]";
                case ItemType.KickBomb:
                    return "[Kick Bomb]";
                case ItemType.PierceExplosion:
                    return "[Pierce Blast]";
                default:
                    return "[Power-Up]";
            }
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
            PlayHudFeedbackShake(pickupCameraShakeDuration, pickupCameraShakeMagnitude);
        }

        private float ResolveQueuedPromptEntrance()
        {
            float total = resultQueued ? resultSceneDelay : localVsNextRoundDelay;
            float remaining = resultQueued ? resultTimer : localVsNextRoundTimer;
            if (total <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - remaining / total);
        }

        private float ResolveButtonScale(Rect rect)
        {
            Event currentEvent = Event.current;
            bool isHovering = currentEvent != null && rect.Contains(currentEvent.mousePosition);
            if (!isHovering)
            {
                return 1f + Mathf.Sin(Time.unscaledTime * 2.7f + rect.y * 0.05f) * 0.006f;
            }

            bool isPressing = currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag;
            if (isPressing)
            {
                return 0.965f;
            }

            return 1.026f + Mathf.Sin(Time.unscaledTime * 11f) * 0.004f;
        }

        private void PlayHudFeedbackShake(float duration, float magnitude)
        {
            if (!enableHudFeedbackShake || duration <= 0f || magnitude <= 0f)
            {
                return;
            }

            CameraController.ShakeActiveCamera(duration, magnitude);
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

        private string FormatModeName(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.AIBattle:
                    return "AI";
                case GameMode.LocalVS:
                    return "VS";
                default:
                    return "Solo";
            }
        }

        private string FormatLifeState(CharacterBase character)
        {
            if (character == null || !character.gameObject.activeInHierarchy)
            {
                return "OFF";
            }

            if (character.IsAlive && character.IsInvincible)
            {
                return $"SAFE {character.InvincibleSecondsRemaining:0.0}";
            }

            if (character.IsAlive && character.HasShield)
            {
                return $"SHIELD {character.ShieldCharges}";
            }

            return character.IsAlive ? "ALIVE" : "DOWN";
        }

        private string FormatRoundState(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return "WAIT";
            }

            if (gameManager.CurrentGameState == GameState.BattlePreparing)
            {
                return "READY";
            }

            if (gameManager.CurrentGameState == GameState.BattleRunning && openingPromptTimer > 0f)
            {
                return "GO";
            }

            float protectionRemaining = ResolveMaxProtectionRemaining(gameManager);
            if (protectionRemaining > 0f)
            {
                return $"SAFE {protectionRemaining:0.0}";
            }

            if (gameManager.CurrentGameState == GameState.BattleRunning)
            {
                return "FIGHT";
            }

            return gameManager.CurrentGameState.ToString();
        }

        private string FormatLocalVsRoundHeader(GameManager gameManager)
        {
            string matchLabel = gameManager.EnableLocalVsBestOf3
                ? $"BEST OF {gameManager.LocalVsTargetScore * 2 - 1}"
                : "SINGLE ROUND";
            return $"{matchLabel}  |  ROUND {gameManager.LocalVsRoundNumber}";
        }

        private float ResolveMaxProtectionRemaining(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return 0f;
            }

            float maxRemaining = 0f;
            maxRemaining = Mathf.Max(maxRemaining, ResolveProtectionRemaining(gameManager.Player1));
            maxRemaining = Mathf.Max(maxRemaining, ResolveProtectionRemaining(gameManager.Player2));
            maxRemaining = Mathf.Max(maxRemaining, ResolveProtectionRemaining(gameManager.AIPlayer));
            return maxRemaining;
        }

        private float ResolveProtectionRemaining(CharacterBase character)
        {
            if (character == null || !character.gameObject.activeInHierarchy || !character.IsAlive || !character.IsInvincible)
            {
                return 0f;
            }

            return character.InvincibleSecondsRemaining;
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int secondsPart = totalSeconds % 60;
            return $"{minutes:00}:{secondsPart:00}";
        }

        private void EnsureStyles()
        {
            if (hudTextStyle != null &&
                hudSmallStyle != null &&
                hudPillLabelStyle != null &&
                hudPillValueStyle != null &&
                abilityLabelStyle != null &&
                abilityValueStyle != null &&
                promptTitleStyle != null &&
                promptBodyStyle != null &&
                toastStyle != null &&
                buttonStyle != null &&
                guideTitleStyle != null &&
                guideItemNameStyle != null &&
                guideItemBodyStyle != null &&
                guideTipStyle != null)
            {
                return;
            }

            Color textPrimary = new Color(0.11f, 0.28f, 0.42f);
            Color textSecondary = new Color(0.16f, 0.34f, 0.44f);

            hudTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            hudSmallStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textSecondary }
            };

            hudPillLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = textPrimary }
            };
            LockStyleTextColor(hudPillLabelStyle, textPrimary);

            hudPillValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = Color.white }
            };
            LockStyleTextColor(hudPillValueStyle, Color.white);

            abilityLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = textSecondary }
            };
            LockStyleTextColor(abilityLabelStyle, textSecondary);

            abilityValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Clip,
                normal = { textColor = textPrimary }
            };
            LockStyleTextColor(abilityValueStyle, textPrimary);

            promptTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 44,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

            promptBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textSecondary }
            };

            toastStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(0, 0, 0, 0),
                normal =
                {
                    textColor = Color.white,
                    background = GetRoundedTexture(new Color(0.12f, 0.72f, 1f, 1f), Color.white, 12, 2)
                },
                hover =
                {
                    textColor = Color.white,
                    background = GetRoundedTexture(new Color(0.25f, 0.86f, 1f, 1f), Color.white, 12, 2)
                },
                active =
                {
                    textColor = Color.white,
                    background = GetRoundedTexture(new Color(0.05f, 0.52f, 0.82f, 1f), Color.white, 12, 2)
                }
            };

            guideTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white },
                focused = { textColor = Color.white },
                onNormal = { textColor = Color.white },
                onHover = { textColor = Color.white },
                onActive = { textColor = Color.white },
                onFocused = { textColor = Color.white }
            };

            guideItemNameStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary },
                hover = { textColor = textPrimary },
                active = { textColor = textPrimary },
                focused = { textColor = textPrimary },
                onNormal = { textColor = textPrimary },
                onHover = { textColor = textPrimary },
                onActive = { textColor = textPrimary },
                onFocused = { textColor = textPrimary }
            };

            guideItemBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textSecondary },
                hover = { textColor = textSecondary },
                active = { textColor = textSecondary },
                focused = { textColor = textSecondary },
                onNormal = { textColor = textSecondary },
                onHover = { textColor = textSecondary },
                onActive = { textColor = textSecondary },
                onFocused = { textColor = textSecondary }
            };

            guideTipStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary },
                hover = { textColor = textPrimary },
                active = { textColor = textPrimary },
                focused = { textColor = textPrimary },
                onNormal = { textColor = textPrimary },
                onHover = { textColor = textPrimary },
                onActive = { textColor = textPrimary },
                onFocused = { textColor = textPrimary }
            };
        }

        private void DrawPanel(Rect rect, Color fill, Color border)
        {
            DrawPanel(rect, fill, border, 18, 3);
        }

        private void DrawPanel(Rect rect, Color fill, Color border, int radius, int borderSize)
        {
            GUI.DrawTexture(
                new Rect(rect.x + 4f, rect.y + 6f, rect.width, rect.height),
                GetRoundedTexture(new Color(0.04f, 0.22f, 0.34f, 0.28f), new Color(0.04f, 0.22f, 0.34f, 0.28f), radius, 0));
            GUI.DrawTexture(rect, GetRoundedTexture(fill, border, radius, borderSize));
        }

        private Texture2D GetRoundedTexture(Color fill, Color border, int radius, int borderSize)
        {
            string key = ColorKey(fill) + ColorKey(border) + radius + "_" + borderSize;
            if (RoundedTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    bool insideOuter = IsInsideRoundedRect(x, y, TextureSize, TextureSize, radius);
                    bool insideInner = borderSize <= 0 || IsInsideRoundedRect(
                        x - borderSize,
                        y - borderSize,
                        TextureSize - borderSize * 2,
                        TextureSize - borderSize * 2,
                        Mathf.Max(1, radius - borderSize));

                    Color pixel = Color.clear;
                    if (insideOuter)
                    {
                        pixel = insideInner ? fill : border;
                    }

                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            RoundedTextureCache[key] = texture;
            return texture;
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            int clampedRadius = Mathf.Min(radius, Mathf.Min(width, height) / 2);
            int left = clampedRadius;
            int right = width - clampedRadius - 1;
            int bottom = clampedRadius;
            int top = height - clampedRadius - 1;

            int closestX = Mathf.Clamp(x, left, right);
            int closestY = Mathf.Clamp(y, bottom, top);
            int deltaX = x - closestX;
            int deltaY = y - closestY;
            return deltaX * deltaX + deltaY * deltaY <= clampedRadius * clampedRadius;
        }

        private string ColorKey(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
