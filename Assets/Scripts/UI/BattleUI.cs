using System.Collections.Generic;
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
        private const int TextureSize = 64;

        [Header("Result Flow")]
        [SerializeField, Min(0f)] private float resultSceneDelay = 0.95f;

        [Header("Battle Timer")]
        [SerializeField] private bool countTimerOnlyWhileRunning = true;
        [SerializeField] private bool resetTimerOnEnable = true;

        [Header("Opening Prompt")]
        [SerializeField] private bool showOpeningPrompt = true;
        [SerializeField, Min(0f)] private float readyPromptSeconds = 1.05f;
        [SerializeField, Min(0f)] private float goPromptSeconds = 0.75f;
        [SerializeField] private string readyText = "READY";
        [SerializeField] private string goText = "GO!";

        [Header("Pickup Toast")]
        [SerializeField, Min(0f)] private float pickupToastSeconds = 1.45f;
        [SerializeField] private Color pickupToastColor = new Color(1f, 0.94f, 0.55f);

        [Header("HUD Colors")]
        [SerializeField] private Color player1Color = new Color(0.12f, 0.72f, 1f);
        [SerializeField] private Color player2Color = new Color(1f, 0.45f, 0.26f);
        [SerializeField] private Color aiColor = new Color(0.64f, 0.46f, 1f);
        [SerializeField] private Color neutralColor = new Color(1f, 0.82f, 0.32f);

        private static readonly Dictionary<string, Texture2D> RoundedTextureCache = new Dictionary<string, Texture2D>();
        private GUIStyle hudTextStyle;
        private GUIStyle hudSmallStyle;
        private GUIStyle hudValueStyle;
        private GUIStyle promptTitleStyle;
        private GUIStyle promptBodyStyle;
        private GUIStyle toastStyle;
        private GUIStyle buttonStyle;

        private bool resultQueued;
        private float resultTimer;
        private float battleElapsedSeconds;
        private float openingPromptTimer;
        private string hudHint = "Win/Lose MVP: defeat or get defeated by bombs. Use Force Result to test the Result scene.";
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
            TickBattleTimer();
            TickOpeningPrompt();
            TickQueuedResult();
            TickPickupToast();

            if (!resultQueued)
            {
                EvaluateBattleResult();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawBattleHud();
            DrawPickupToast();
            DrawOpeningPrompt();
            DrawResultPrompt();
            DrawActionButtons();
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
            GameManager.Instance?.ClearBattleResult();
            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickForceResult()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            ShowBattleResultPrompt(
                "Battle Finished",
                "Manual result button was pressed for MVP flow testing.",
                "Manual Test");
        }

        public void ShowBattleResultPrompt(string title, string detail, string winner)
        {
            QueueResult(title, detail, winner);
        }

        private void ResetBattleHudState()
        {
            battleElapsedSeconds = 0f;
            openingPromptTimer = showOpeningPrompt ? readyPromptSeconds + goPromptSeconds : 0f;
            resultQueued = false;
            resultTimer = 0f;
            resultPromptTitle = string.Empty;
            resultPromptDetail = string.Empty;
            pickupToastText = string.Empty;
            pickupToastTimer = 0f;
            hudHint = "Break blocks, collect power-ups, and avoid the blast lines.";
        }

        private void TickBattleTimer()
        {
            GameManager gameManager = GameManager.Instance;
            if (countTimerOnlyWhileRunning && (gameManager == null || gameManager.CurrentGameState != GameState.BattleRunning))
            {
                return;
            }

            if (resultQueued)
            {
                return;
            }

            battleElapsedSeconds += Time.deltaTime;
        }

        private void TickOpeningPrompt()
        {
            if (openingPromptTimer <= 0f)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager != null && gameManager.CurrentGameState != GameState.BattleRunning)
            {
                return;
            }

            openingPromptTimer = Mathf.Max(0f, openingPromptTimer - Time.deltaTime);
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

            resultPromptTitle = string.IsNullOrEmpty(title) ? "Battle Finished" : title;
            resultPromptDetail = string.IsNullOrEmpty(detail) ? "The battle has ended." : detail;
            GameManager.Instance?.FinishBattle(resultPromptTitle, resultPromptDetail, winner);
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

        private void DrawBattleHud()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                DrawPanel(new Rect(18f, 18f, 420f, 86f), new Color(1f, 0.96f, 0.72f, 0.94f), neutralColor);
                GUI.Label(new Rect(36f, 36f, 380f, 44f), "Battle HUD waiting for GameManager...", hudTextStyle);
                return;
            }

            DrawTopStatusBar(gameManager);
            DrawCharacterPanel(new Rect(18f, Screen.height - 176f, 318f, 152f), "PLAYER 1", gameManager.Player1, player1Color);

            CharacterBase rightCharacter = ResolveRightSideCharacter(gameManager, out string rightLabel, out Color rightColor);
            DrawCharacterPanel(new Rect(Screen.width - 336f, Screen.height - 176f, 318f, 152f), rightLabel, rightCharacter, rightColor);
        }

        private void DrawTopStatusBar(GameManager gameManager)
        {
            Rect topRect = new Rect(18f, 18f, Mathf.Min(Screen.width - 236f, 720f), 90f);
            DrawPanel(topRect, new Color(1f, 0.96f, 0.72f, 0.95f), new Color(0.18f, 0.67f, 0.95f, 1f));

            float x = topRect.x + 18f;
            float y = topRect.y + 14f;
            DrawInfoPill(new Rect(x, y, 180f, 30f), "MODE", gameManager.CurrentGameMode.ToString(), new Color(0.12f, 0.72f, 1f));
            DrawInfoPill(new Rect(x + 190f, y, 180f, 30f), "MAP", FormatMapName(gameManager.CurrentMapType), new Color(0.48f, 0.9f, 0.34f));
            DrawInfoPill(new Rect(x + 380f, y, 150f, 30f), "TIME", FormatTime(battleElapsedSeconds), new Color(1f, 0.58f, 0.18f));

            GUI.Label(new Rect(topRect.x + 20f, topRect.y + 52f, topRect.width - 40f, 28f), hudHint, hudSmallStyle);
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
            DrawPanel(rect, new Color(1f, 0.94f, 0.72f, 0.94f), accentColor);
            DrawInfoPill(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, 30f), label, FormatLifeState(character), accentColor);

            if (character == null || !character.gameObject.activeInHierarchy)
            {
                GUI.Label(new Rect(rect.x + 18f, rect.y + 58f, rect.width - 36f, 58f), "Not active in this mode", hudTextStyle);
                DrawAbilityRow(rect, 0, 0, 0, 0f, accentColor);
                return;
            }

            DrawAbilityRow(
                rect,
                character.RemainingBombCount,
                character.MaxBombCount,
                character.BombRange,
                character.MoveSpeed,
                accentColor);
        }

        private void DrawAbilityRow(Rect panelRect, int remainingBombs, int maxBombs, int range, float speed, Color accentColor)
        {
            float y = panelRect.y + 58f;
            float itemWidth = (panelRect.width - 44f) / 3f;
            DrawAbilityBox(new Rect(panelRect.x + 14f, y, itemWidth, 74f), "BOMBS", remainingBombs + "/" + maxBombs, accentColor);
            DrawAbilityBox(new Rect(panelRect.x + 22f + itemWidth, y, itemWidth, 74f), "RANGE", range.ToString(), new Color(1f, 0.58f, 0.18f));
            DrawAbilityBox(new Rect(panelRect.x + 30f + itemWidth * 2f, y, itemWidth, 74f), "SPEED", speed.ToString("0.0"), new Color(0.48f, 0.9f, 0.34f));
        }

        private void DrawAbilityBox(Rect rect, string label, string value, Color accentColor)
        {
            DrawPanel(rect, new Color(1f, 0.98f, 0.86f, 0.94f), Color.Lerp(accentColor, Color.white, 0.2f), 14, 2);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 8f, rect.width - 12f, 20f), label, hudSmallStyle);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 30f, rect.width - 12f, 34f), value, hudValueStyle);
        }

        private void DrawInfoPill(Rect rect, string label, string value, Color accentColor)
        {
            DrawPanel(rect, accentColor, Color.white, 15, 2);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 5f, rect.width * 0.36f, rect.height - 10f), label, hudSmallStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.38f, rect.y + 5f, rect.width * 0.58f, rect.height - 10f), value, hudTextStyle);
        }

        private void DrawOpeningPrompt()
        {
            if (openingPromptTimer <= 0f)
            {
                return;
            }

            bool readyPhase = openingPromptTimer > goPromptSeconds;
            string prompt = readyPhase ? readyText : goText;
            Color promptColor = readyPhase ? new Color(1f, 0.74f, 0.18f, 0.95f) : new Color(0.2f, 0.88f, 1f, 0.95f);
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.04f;
            Rect rect = new Rect(Screen.width * 0.5f - 160f * pulse, Screen.height * 0.5f - 58f * pulse, 320f * pulse, 116f * pulse);
            DrawPanel(rect, new Color(1f, 0.96f, 0.72f, 0.96f), promptColor, 24, 5);
            GUI.Label(new Rect(rect.x, rect.y + 18f, rect.width, 62f), prompt, promptTitleStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 78f, rect.width - 36f, 24f), "Bubble battle begins!", promptBodyStyle);
        }

        private void DrawResultPrompt()
        {
            if (!resultQueued)
            {
                return;
            }

            Rect rect = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 78f, 420f, 156f);
            DrawPanel(rect, new Color(1f, 0.96f, 0.72f, 0.97f), new Color(1f, 0.58f, 0.18f), 24, 5);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 22f, rect.width - 36f, 58f), resultPromptTitle, promptTitleStyle);
            GUI.Label(new Rect(rect.x + 28f, rect.y + 84f, rect.width - 56f, 54f), resultPromptDetail, promptBodyStyle);
        }

        private void DrawPickupToast()
        {
            if (pickupToastTimer <= 0f || string.IsNullOrEmpty(pickupToastText))
            {
                return;
            }

            Rect toastRect = new Rect(Screen.width * 0.5f - 190f, 116f, 380f, 56f);
            DrawPanel(toastRect, pickupToastColor, Color.white, 18, 3);
            GUI.Label(toastRect, pickupToastText, toastStyle);
        }

        private void DrawActionButtons()
        {
            Rect buttonRect = new Rect(Screen.width - 198f, 18f, 180f, 160f);
            DrawPanel(buttonRect, new Color(1f, 0.96f, 0.72f, 0.92f), new Color(1f, 0.58f, 0.18f), 18, 3);
            GUILayout.BeginArea(new Rect(buttonRect.x + 12f, buttonRect.y + 12f, buttonRect.width - 24f, buttonRect.height - 24f));

            if (GUILayout.Button("Retry", buttonStyle, GUILayout.Height(38f)))
            {
                OnClickRetry();
            }

            if (GUILayout.Button("Force Result", buttonStyle, GUILayout.Height(38f)))
            {
                OnClickForceResult();
            }

            if (GUILayout.Button("Main Menu", buttonStyle, GUILayout.Height(38f)))
            {
                OnClickBackToMenu();
            }

            GUILayout.EndArea();
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

        private string FormatLifeState(CharacterBase character)
        {
            if (character == null || !character.gameObject.activeInHierarchy)
            {
                return "OFF";
            }

            return character.IsAlive ? "ALIVE" : "DOWN";
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
            if (hudTextStyle != null)
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

            hudValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

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
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
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
