using System.Collections.Generic;
using BubbleTown.CameraSystem;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Result screen display and callbacks.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        private const int TextureSize = 64;
        private const string ResultPanelResourcePath = "UI/Result/ResultUI";
        private const string RetryButtonResourcePath = "UI/Result/Retry2";
        private const string MainMenuButtonResourcePath = "UI/Result/MainMenu2";
        private const float ResultArtworkWidth = 1586f;
        private const float ResultArtworkHeight = 992f;

        private static readonly Dictionary<string, Texture2D> RoundedTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> CircleTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Rect RetryButtonCrop = new Rect(0f, 0f, 1835f, 400f);
        private static readonly Rect MainMenuButtonCrop = new Rect(0f, 0f, 1554f, 316f);

        [Header("Result Colors")]
        [SerializeField] private Color victoryColor = new Color(0.2f, 0.86f, 1f, 1f);
        [SerializeField] private Color defeatColor = new Color(1f, 0.45f, 0.3f, 1f);
        [SerializeField] private Color drawColor = new Color(1f, 0.72f, 0.22f, 1f);
        [SerializeField] private Color neutralColor = new Color(0.66f, 0.48f, 1f, 1f);

        [Header("Feedback")]
        [SerializeField, Min(0.01f)] private float panelEntranceSeconds = 0.35f;
        [SerializeField, Min(0f)] private float resultCameraShakeDuration = 0.14f;
        [SerializeField, Min(0f)] private float victoryCameraShakeMagnitude = 0.06f;
        [SerializeField, Min(0f)] private float defeatCameraShakeMagnitude = 0.09f;
        [SerializeField, Min(0f)] private float completeCameraShakeMagnitude = 0.045f;

        private GUIStyle cardTitleStyle;
        private GUIStyle cardValueStyle;
        private GUIStyle resultIconStyle;
        private GUIStyle screenTitleStyle;
        private GUIStyle pillTextStyle;
        private GUIStyle resultTitleStyle;
        private GUIStyle resultBodyStyle;
        private GUIStyle resultDetailStyle;
        private GUIStyle transparentButtonStyle;
        private Texture2D resultPanelTexture;
        private Texture2D retryButtonTexture;
        private Texture2D mainMenuButtonTexture;
        private bool resultAudioPlayed;
        private bool texturesLoaded;
        private float shownAtTime;

        private enum ResultOutcome
        {
            Pending,
            Victory,
            Defeat,
            Draw,
            Complete
        }

        private struct ResultViewModel
        {
            public bool HasResult;
            public string Title;
            public string Detail;
            public string Winner;
            public string ModeName;
            public string MapName;
            public string OutcomeLabel;
            public string MatchScoreLabel;
            public string IconText;
            public string MoodText;
            public ResultOutcome Outcome;
            public Color AccentColor;
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            resultAudioPlayed = false;
            shownAtTime = Time.unscaledTime;
        }

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            EnsureStyles();
            EnsureResultTexturesLoaded();

            ResultViewModel viewModel = BuildResultViewModel();
            PlayResultAudioOnce(viewModel);
            if (HasImageResultAssets())
            {
                DrawImageResult(viewModel);
                return;
            }

            SimpleUIFactory.DrawCandyBackground();
            Rect panel = SimpleUIFactory.CenteredRect(800f, 500f);

            Matrix4x4 previousMatrix = GUI.matrix;
            float entranceScale = ResolvePanelEntranceScale();
            GUIUtility.ScaleAroundPivot(new Vector2(entranceScale, entranceScale), panel.center);
            DrawPanel(panel, new Color(1f, 0.96f, 0.72f, 0.96f), new Color(0.2f, 0.58f, 0.82f, 1f), 24, 4);
            DrawResultDecorations(panel, viewModel);
            DrawResultContent(panel, viewModel);
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Handles the rematch button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickRematch()
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

            SceneFlowManager.Instance?.LoadBattle();
        }

        /// <summary>
        /// Purpose: Handles the back to menu button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickBackToMenu()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadMainMenu();
        }

        /// <summary>
        /// Purpose: Builds result view model.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `ResultViewModel` value.
        /// </summary>
        /// <returns>a `ResultViewModel` value.</returns>
        private ResultViewModel BuildResultViewModel()
        {
            GameManager gameManager = GameManager.Instance;
            bool hasResult = gameManager != null && gameManager.HasBattleResult;
            string title = hasResult ? gameManager.LastResultTitle : "No Result Yet";
            string detail = hasResult ? gameManager.LastResultDetail : "Play a round to see your adventure result.";
            string winner = hasResult ? gameManager.LastResultWinner : "None";
            GameMode mode = gameManager != null ? gameManager.CurrentGameMode : GameMode.SinglePlayer;
            BattleMapType mapType = gameManager != null ? gameManager.CurrentMapType : BattleMapType.Default;
            ResultOutcome outcome = hasResult ? ResolveOutcome(title, winner) : ResultOutcome.Pending;

            return new ResultViewModel
            {
                HasResult = hasResult,
                Title = title,
                Detail = detail,
                Winner = string.IsNullOrEmpty(winner) ? "None" : winner,
                ModeName = FormatModeName(mode),
                MapName = FormatMapName(mapType),
                OutcomeLabel = FormatOutcome(outcome),
                MatchScoreLabel = FormatMatchScore(gameManager, mode),
                IconText = FormatIconText(outcome),
                MoodText = FormatMoodText(outcome),
                Outcome = outcome,
                AccentColor = ResolveAccentColor(outcome)
            };
        }

        /// <summary>
        /// Purpose: Plays result audio once.
        /// Inputs: `viewModel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="viewModel">Input value used by this method.</param>
        private void PlayResultAudioOnce(ResultViewModel viewModel)
        {
            if (resultAudioPlayed || !viewModel.HasResult)
            {
                return;
            }

            resultAudioPlayed = true;
            switch (viewModel.Outcome)
            {
                case ResultOutcome.Victory:
                    AudioManager.Instance?.PlayVictorySFX();
                    CameraController.ShakeActiveCamera(resultCameraShakeDuration, victoryCameraShakeMagnitude);
                    break;
                case ResultOutcome.Defeat:
                    AudioManager.Instance?.PlayDefeatSFX();
                    CameraController.ShakeActiveCamera(resultCameraShakeDuration, defeatCameraShakeMagnitude);
                    break;
                case ResultOutcome.Draw:
                case ResultOutcome.Complete:
                    CameraController.ShakeActiveCamera(resultCameraShakeDuration, completeCameraShakeMagnitude);
                    break;
            }
        }

        /// <summary>
        /// Purpose: Resolves outcome from the current runtime state.
        /// Inputs: `title`, `winner`; may also read serialized fields and current runtime state.
        /// Output: a `ResultOutcome` value.
        /// </summary>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="winner">Input value used by this method.</param>
        /// <returns>a `ResultOutcome` value.</returns>
        private ResultOutcome ResolveOutcome(string title, string winner)
        {
            string normalizedTitle = string.IsNullOrEmpty(title) ? string.Empty : title.ToLowerInvariant();
            string normalizedWinner = string.IsNullOrEmpty(winner) ? string.Empty : winner.ToLowerInvariant();

            if (normalizedTitle.Contains("defeat") || normalizedTitle.Contains("game over"))
            {
                return ResultOutcome.Defeat;
            }

            if (normalizedTitle.Contains("draw") || normalizedWinner == "none")
            {
                return ResultOutcome.Draw;
            }

            if (normalizedTitle.Contains("victory") || normalizedTitle.Contains("wins"))
            {
                return ResultOutcome.Victory;
            }

            return ResultOutcome.Complete;
        }

        /// <summary>
        /// Purpose: Resolves accent color from the current runtime state.
        /// Inputs: `outcome`; may also read serialized fields and current runtime state.
        /// Output: a `Color` value.
        /// </summary>
        /// <param name="outcome">Input value used by this method.</param>
        /// <returns>a `Color` value.</returns>
        private Color ResolveAccentColor(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return victoryColor;
                case ResultOutcome.Defeat:
                    return defeatColor;
                case ResultOutcome.Draw:
                    return drawColor;
                default:
                    return neutralColor;
            }
        }

        /// <summary>
        /// Purpose: Formats outcome for display or logging.
        /// Inputs: `outcome`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="outcome">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatOutcome(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return "VICTORY";
                case ResultOutcome.Defeat:
                    return "DEFEAT";
                case ResultOutcome.Draw:
                    return "DRAW";
                case ResultOutcome.Complete:
                    return "ROUND COMPLETE";
                default:
                    return "NO RESULT";
            }
        }

        /// <summary>
        /// Purpose: Formats icon text for display or logging.
        /// Inputs: `outcome`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="outcome">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatIconText(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return ":D";
                case ResultOutcome.Defeat:
                    return ":(";
                case ResultOutcome.Draw:
                    return ":|";
                case ResultOutcome.Complete:
                    return ":)";
                default:
                    return "?";
            }
        }

        /// <summary>
        /// Purpose: Formats mood text for display or logging.
        /// Inputs: `outcome`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="outcome">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatMoodText(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return "Bubble champ energy!";
                case ResultOutcome.Defeat:
                    return "Pop, dust off, retry.";
                case ResultOutcome.Draw:
                    return "Both sides popped together.";
                case ResultOutcome.Complete:
                    return "Round wrapped up.";
                default:
                    return "Play a round to reveal the result.";
            }
        }

        /// <summary>
        /// Purpose: Formats mode name for display or logging.
        /// Inputs: `mode`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="mode">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatModeName(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.AIBattle:
                    return "AI Battle " + FormatAIDifficultyName(GameManager.Instance != null
                        ? GameManager.Instance.CurrentAIDifficulty
                        : AIDifficulty.Normal);
                case GameMode.LocalVS:
                    return "Local VS";
                default:
                    return "Single Player";
            }
        }

        /// <summary>
        /// Purpose: Formats AI difficulty for result screen mode details.
        /// Inputs: difficulty is the AI preset stored by GameManager.
        /// Output: returns a short player-facing difficulty name.
        /// </summary>
        /// <param name="difficulty">Current AI difficulty.</param>
        /// <returns>AI difficulty display label.</returns>
        private string FormatAIDifficultyName(AIDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    return "Easy";
                case AIDifficulty.Hard:
                    return "Hard";
                default:
                    return "Normal";
            }
        }

        /// <summary>
        /// Purpose: Formats map name for display or logging.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatMapName(BattleMapType mapType)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return "Snowfield";
                case BattleMapType.Maze:
                    return "Jelly Maze";
                default:
                    return "Candy Park";
            }
        }

        /// <summary>
        /// Purpose: Loads the illustrated result screen assets on demand.
        /// Inputs: no direct parameters; uses Resources paths without file extensions.
        /// Output: no return value; caches loaded textures and applies UI sampling settings.
        /// </summary>
        private void EnsureResultTexturesLoaded()
        {
            if (texturesLoaded)
            {
                return;
            }

            texturesLoaded = true;
            resultPanelTexture = Resources.Load<Texture2D>(ResultPanelResourcePath);
            retryButtonTexture = Resources.Load<Texture2D>(RetryButtonResourcePath);
            mainMenuButtonTexture = Resources.Load<Texture2D>(MainMenuButtonResourcePath);

            ApplyResultTextureSettings(resultPanelTexture);
            ApplyResultTextureSettings(retryButtonTexture);
            ApplyResultTextureSettings(mainMenuButtonTexture);
        }

        /// <summary>
        /// Purpose: Checks whether the new image-based result screen can be drawn.
        /// Inputs: no direct parameters; reads cached texture references.
        /// Output: true when all required result art is available.
        /// </summary>
        /// <returns>True when the illustrated result screen assets are loaded.</returns>
        private bool HasImageResultAssets()
        {
            return resultPanelTexture != null &&
                   retryButtonTexture != null &&
                   mainMenuButtonTexture != null;
        }

        /// <summary>
        /// Purpose: Applies stable sampling settings to imported result textures.
        /// Inputs: texture may be null.
        /// Output: no return value; configures texture filtering and wrapping when present.
        /// </summary>
        /// <param name="texture">Texture to configure.</param>
        private void ApplyResultTextureSettings(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Draws the illustrated result screen while preserving runtime labels and button callbacks.
        /// Inputs: viewModel supplies dynamic result text.
        /// Output: no return value; handles retry and main menu button clicks.
        /// </summary>
        /// <param name="viewModel">Runtime result content.</param>
        private void DrawImageResult(ResultViewModel viewModel)
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = PixelSnapRect(SimpleUIFactory.CenteredRect(800f, 500f));
            Matrix4x4 previousMatrix = GUI.matrix;
            float entranceScale = ResolvePanelEntranceScale();
            GUIUtility.ScaleAroundPivot(new Vector2(entranceScale, entranceScale), panel.center);
            GUI.DrawTexture(panel, resultPanelTexture, ScaleMode.StretchToFill, false);
            DrawImageResultContent(panel, viewModel);
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Places dynamic result text over the new illustrated panel without changing button behavior.
        /// Inputs: panel is the on-screen artwork rect; viewModel supplies current battle result.
        /// Output: no return value; draws labels and image buttons.
        /// </summary>
        /// <param name="panel">Drawn artwork rect.</param>
        /// <param name="viewModel">Runtime result content.</param>
        private void DrawImageResultContent(Rect panel, ResultViewModel viewModel)
        {
            DrawImageResultTitle(panel, viewModel.Title);

            Rect iconRect = RelativeArtworkRect(panel, 286f, 313f, 156f, 156f);
            GUI.DrawTexture(iconRect, GetCircleTexture(viewModel.AccentColor));
            DrawLockedLabel(iconRect, viewModel.IconText, resultIconStyle);

            DrawArtworkTextMask(RelativeArtworkRect(panel, 490f, 286f, 740f, 202f), 0);
            DrawLockedLabel(RelativeArtworkRect(panel, 502f, 297f, 700f, 56f), viewModel.OutcomeLabel, resultTitleStyle);
            DrawLockedLabel(
                RelativeArtworkRect(panel, 502f, 374f, 720f, 104f),
                viewModel.MoodText + "\n" + viewModel.Detail,
                resultDetailStyle);

            DrawArtworkValue(RelativeArtworkRect(panel, 220f, 626f, 322f, 56f), viewModel.ModeName);
            DrawArtworkValue(RelativeArtworkRect(panel, 632f, 626f, 322f, 56f), viewModel.MapName);
            DrawArtworkValue(RelativeArtworkRect(panel, 1050f, 626f, 292f, 56f), viewModel.Winner);

            DrawImageActionButtons(panel);
        }

        /// <summary>
        /// Purpose: Draws a dynamic title only when it differs from the title baked into the artwork.
        /// Inputs: panel is the on-screen artwork rect; title is the runtime battle result title.
        /// Output: no return value; avoids double-drawing the baked Game Over text.
        /// </summary>
        /// <param name="panel">Drawn artwork rect.</param>
        /// <param name="title">Runtime result title.</param>
        private void DrawImageResultTitle(Rect panel, string title)
        {
            if (string.IsNullOrEmpty(title) ||
                string.Equals(title, "Game Over", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DrawArtworkTextMask(RelativeArtworkRect(panel, 430f, 104f, 760f, 148f), 0);
            DrawLockedLabel(RelativeArtworkRect(panel, 430f, 122f, 760f, 96f), title, screenTitleStyle);
        }

        /// <summary>
        /// Purpose: Draws one dynamic value into an illustrated summary card.
        /// Inputs: valueRect is in screen space and value is the runtime text.
        /// Output: no return value; covers baked placeholder text and draws the current value.
        /// </summary>
        /// <param name="valueRect">Screen-space text slot.</param>
        /// <param name="value">Current value text.</param>
        private void DrawArtworkValue(Rect valueRect, string value)
        {
            DrawArtworkTextMask(valueRect, 18);
            DrawLockedLabel(valueRect, value, cardValueStyle);
        }

        /// <summary>
        /// Purpose: Draws a soft mask matching the result artwork so dynamic text does not overlap baked sample text.
        /// Inputs: rect describes the text slot; radius controls corner roundness.
        /// Output: no return value; draws a non-interactive background patch.
        /// </summary>
        /// <param name="rect">Screen-space patch rect.</param>
        /// <param name="radius">Rounded corner radius.</param>
        private void DrawArtworkTextMask(Rect rect, int radius)
        {
            GUI.DrawTexture(
                PixelSnapRect(rect),
                GetRoundedTexture(new Color(1f, 0.94f, 0.68f, 1f), new Color(1f, 0.94f, 0.68f, 1f), Mathf.Max(0, radius), 0));
        }

        /// <summary>
        /// Purpose: Draws and handles the new image-based retry and main-menu buttons.
        /// Inputs: panel is the drawn result artwork rect.
        /// Output: no return value; invokes existing button callbacks on click.
        /// </summary>
        /// <param name="panel">Drawn artwork rect.</param>
        private void DrawImageActionButtons(Rect panel)
        {
            Rect retryRect = RelativeArtworkRect(panel, 334f, 790f, 435f, 112f);
            Rect menuRect = RelativeArtworkRect(panel, 813f, 790f, 435f, 112f);

            if (DrawCroppedImageButton(retryRect, retryButtonTexture, RetryButtonCrop))
            {
                OnClickRematch();
            }

            if (DrawCroppedImageButton(menuRect, mainMenuButtonTexture, MainMenuButtonCrop))
            {
                OnClickBackToMenu();
            }
        }

        /// <summary>
        /// Purpose: Draws a cropped image button and keeps click hitboxes aligned with the artwork slot.
        /// Inputs: clickRect is the interactive slot; cropPixels selects the actual button from source art.
        /// Output: true when the invisible button is clicked.
        /// </summary>
        /// <param name="clickRect">Screen-space hitbox.</param>
        /// <param name="texture">Source texture.</param>
        /// <param name="cropPixels">Source crop in top-left pixel coordinates.</param>
        /// <returns>True when clicked.</returns>
        private bool DrawCroppedImageButton(Rect clickRect, Texture2D texture, Rect cropPixels)
        {
            bool isHovered = clickRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            Rect drawRect = CalculateAspectFitRect(cropPixels.width, cropPixels.height, clickRect);

            if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.035f, 1.035f);
            }

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.98f, 0.98f);
                drawRect.y += drawRect.height * 0.025f;
            }

            DrawCroppedTexture(PixelSnapRect(drawRect), texture, cropPixels);
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Draws a top-left pixel crop from a larger source texture.
        /// Inputs: drawRect is destination space; sourcePixels is crop space in the source texture.
        /// Output: no return value; clips drawing to drawRect.
        /// </summary>
        /// <param name="drawRect">Destination rect.</param>
        /// <param name="texture">Source texture.</param>
        /// <param name="sourcePixels">Top-left pixel crop.</param>
        private void DrawCroppedTexture(Rect drawRect, Texture2D texture, Rect sourcePixels)
        {
            if (texture == null || sourcePixels.width <= 0f || sourcePixels.height <= 0f)
            {
                return;
            }

            float scaleX = drawRect.width / sourcePixels.width;
            float scaleY = drawRect.height / sourcePixels.height;
            GUI.BeginGroup(drawRect);
            GUI.DrawTexture(
                new Rect(-sourcePixels.x * scaleX, -sourcePixels.y * scaleY, texture.width * scaleX, texture.height * scaleY),
                texture,
                ScaleMode.StretchToFill,
                true);
            GUI.EndGroup();
        }

        /// <summary>
        /// Purpose: Draws result content in the current GUI or scene context.
        /// Inputs: `panel`, `viewModel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
        /// <param name="viewModel">Input value used by this method.</param>
        private void DrawResultContent(Rect panel, ResultViewModel viewModel)
        {
            float margin = Mathf.Clamp(panel.width * 0.055f, 28f, 44f);
            float contentWidth = panel.width - margin * 2f;
            float y = panel.y + 12f;

            Rect pillRect = new Rect(panel.center.x - 185f, y, 370f, 30f);
            DrawPanel(pillRect, new Color(0.23f, 0.77f, 0.95f, 1f), new Color(1f, 1f, 1f, 0.75f), 17, 2);
            DrawLockedLabel(pillRect, viewModel.HasResult ? "ROUND COMPLETE" : "RESULT WAITING", pillTextStyle);

            y += 36f;
            DrawLockedLabel(new Rect(panel.x + margin, y, contentWidth, 56f), viewModel.Title, screenTitleStyle);

            y += 64f;
            DrawHeroResultCard(new Rect(panel.x + margin + 56f, y, contentWidth - 112f, 118f), viewModel);

            y += 136f;
            DrawSummaryCards(new Rect(panel.x + margin + 18f, y, contentWidth - 36f, 64f), viewModel);

            float buttonY = panel.y + panel.height - 58f;
            DrawActionButtons(new Rect(panel.x + margin + 124f, buttonY, contentWidth - 248f, 42f));
        }

        /// <summary>
        /// Purpose: Draws hero result card in the current GUI or scene context.
        /// Inputs: `cardRect`, `viewModel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="cardRect">Input value used by this method.</param>
        /// <param name="viewModel">Input value used by this method.</param>
        private void DrawHeroResultCard(Rect cardRect, ResultViewModel viewModel)
        {
            DrawPanel(cardRect, new Color(1f, 0.98f, 0.82f, 0.98f), viewModel.AccentColor, 24, 5);

            Rect iconCircle = new Rect(cardRect.x + 24f, cardRect.y + 24f, 64f, 64f);
            Matrix4x4 previousMatrix = GUI.matrix;
            float iconPulse = viewModel.HasResult ? 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.035f : 1f;
            GUIUtility.ScaleAroundPivot(new Vector2(iconPulse, iconPulse), iconCircle.center);
            GUI.DrawTexture(iconCircle, GetCircleTexture(viewModel.AccentColor));
            DrawLockedLabel(iconCircle, viewModel.IconText, resultIconStyle);
            GUI.matrix = previousMatrix;

            float textX = cardRect.x + 108f;
            float textWidth = cardRect.width - 134f;
            DrawLockedLabel(new Rect(textX, cardRect.y + 12f, textWidth, 36f), viewModel.OutcomeLabel, resultTitleStyle);
            DrawLockedLabel(new Rect(textX, cardRect.y + 50f, textWidth, 26f), viewModel.MoodText, resultBodyStyle);
            DrawLockedLabel(new Rect(textX, cardRect.y + 76f, textWidth, 36f), viewModel.Detail, resultBodyStyle);
        }

        /// <summary>
        /// Purpose: Draws summary cards in the current GUI or scene context.
        /// Inputs: `rowRect`, `viewModel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rowRect">Input value used by this method.</param>
        /// <param name="viewModel">Input value used by this method.</param>
        private void DrawSummaryCards(Rect rowRect, ResultViewModel viewModel)
        {
            int cardCount = string.IsNullOrEmpty(viewModel.MatchScoreLabel) ? 3 : 4;
            float gap = 12f;
            float cardWidth = (rowRect.width - gap * (cardCount - 1)) / cardCount;
            int index = 0;

            DrawInfoCard(new Rect(rowRect.x + index * (cardWidth + gap), rowRect.y, cardWidth, rowRect.height), "MODE", viewModel.ModeName, new Color(0.12f, 0.72f, 1f, 1f));
            index++;
            DrawInfoCard(new Rect(rowRect.x + index * (cardWidth + gap), rowRect.y, cardWidth, rowRect.height), "MAP", viewModel.MapName, new Color(0.48f, 0.9f, 0.34f, 1f));
            index++;
            DrawInfoCard(new Rect(rowRect.x + index * (cardWidth + gap), rowRect.y, cardWidth, rowRect.height), "WINNER", viewModel.Winner, viewModel.AccentColor);
            index++;
            if (!string.IsNullOrEmpty(viewModel.MatchScoreLabel))
            {
                DrawInfoCard(new Rect(rowRect.x + index * (cardWidth + gap), rowRect.y, cardWidth, rowRect.height), "VS SCORE", viewModel.MatchScoreLabel, new Color(0.52f, 0.9f, 0.35f, 1f));
            }
        }

        /// <summary>
        /// Purpose: Formats match score for display or logging.
        /// Inputs: `gameManager`, `mode`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="gameManager">Input value used by this method.</param>
        /// <param name="mode">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string FormatMatchScore(GameManager gameManager, GameMode mode)
        {
            if (gameManager == null || mode != GameMode.LocalVS)
            {
                return string.Empty;
            }

            return gameManager.LocalVsScoreLabel;
        }

        /// <summary>
        /// Purpose: Draws info card in the current GUI or scene context.
        /// Inputs: `rect`, `label`, `value`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="label">Input value used by this method.</param>
        /// <param name="value">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private void DrawInfoCard(Rect rect, string label, string value, Color accentColor)
        {
            DrawPanel(rect, new Color(1f, 0.95f, 0.72f, 0.97f), accentColor, 18, 3);
            DrawLockedLabel(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 20f), label, cardTitleStyle);
            DrawLockedLabel(new Rect(rect.x + 10f, rect.y + 29f, rect.width - 20f, 28f), value, cardValueStyle);
        }

        /// <summary>
        /// Purpose: Draws action buttons in the current GUI or scene context.
        /// Inputs: `slot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="slot">Input value used by this method.</param>
        private void DrawActionButtons(Rect slot)
        {
            float buttonWidth = Mathf.Min(250f, (slot.width - 18f) * 0.5f);
            Rect retryRect = new Rect(slot.center.x - buttonWidth - 9f, slot.y, buttonWidth, slot.height);
            Rect menuRect = new Rect(slot.center.x + 9f, slot.y, buttonWidth, slot.height);

            if (SimpleUIFactory.FixedPrimaryButton(retryRect, "RETRY"))
            {
                OnClickRematch();
            }

            if (SimpleUIFactory.FixedSecondaryButton(menuRect, "MAIN MENU"))
            {
                OnClickBackToMenu();
            }
        }

        /// <summary>
        /// Purpose: Draws result decorations in the current GUI or scene context.
        /// Inputs: `panel`, `viewModel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
        /// <param name="viewModel">Input value used by this method.</param>
        private void DrawResultDecorations(Rect panel, ResultViewModel viewModel)
        {
            float t = Time.unscaledTime;
            Color accent = viewModel.AccentColor;
            DrawFloatingBubble(new Rect(panel.x + 42f, panel.y + 72f, 46f, 46f), Color.Lerp(accent, Color.white, 0.35f), t, 0f);
            DrawFloatingBubble(new Rect(panel.x + panel.width - 92f, panel.y + 84f, 34f, 34f), new Color(0.56f, 0.9f, 1f, 0.58f), t, 1.2f);
            DrawFloatingBubble(new Rect(panel.x + 74f, panel.y + panel.height - 116f, 58f, 58f), new Color(1f, 0.74f, 0.28f, 0.34f), t, 2.1f);
            DrawFloatingBubble(new Rect(panel.x + panel.width - 126f, panel.y + panel.height - 136f, 52f, 52f), new Color(0.54f, 1f, 0.76f, 0.34f), t, 3.4f);
            DrawSparkle(new Rect(panel.x + 148f, panel.y + 36f, 18f, 18f), new Color(1f, 0.95f, 0.45f, 0.76f), t, 0.5f);
            DrawSparkle(new Rect(panel.x + panel.width - 172f, panel.y + 44f, 16f, 16f), new Color(1f, 1f, 1f, 0.76f), t, 1.8f);
            DrawSparkle(new Rect(panel.x + panel.width - 112f, panel.y + 260f, 14f, 14f), new Color(1f, 0.95f, 0.45f, 0.62f), t, 2.7f);
        }

        /// <summary>
        /// Purpose: Draws floating bubble in the current GUI or scene context.
        /// Inputs: `rect`, `color`, `time`, `phase`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        /// <param name="time">Input value used by this method.</param>
        /// <param name="phase">Input value used by this method.</param>
        private void DrawFloatingBubble(Rect rect, Color color, float time, float phase)
        {
            float bob = Mathf.Sin(time * 1.6f + phase) * 5f;
            float pulse = 1f + Mathf.Sin(time * 2.2f + phase) * 0.04f;
            Rect animatedRect = new Rect(rect.x, rect.y + bob, rect.width, rect.height);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(pulse, pulse), animatedRect.center);
            GUI.DrawTexture(animatedRect, GetCircleTexture(color));
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Draws sparkle in the current GUI or scene context.
        /// Inputs: `rect`, `color`, `time`, `phase`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        /// <param name="time">Input value used by this method.</param>
        /// <param name="phase">Input value used by this method.</param>
        private void DrawSparkle(Rect rect, Color color, float time, float phase)
        {
            float pulse = 1f + Mathf.Sin(time * 3.4f + phase) * 0.18f;
            Rect vertical = new Rect(rect.center.x - rect.width * 0.12f, rect.y, rect.width * 0.24f, rect.height);
            Rect horizontal = new Rect(rect.x, rect.center.y - rect.height * 0.12f, rect.width, rect.height * 0.24f);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(pulse, pulse), rect.center);
            GUI.DrawTexture(vertical, GetRoundedTexture(color, Color.clear, 4, 0));
            GUI.DrawTexture(horizontal, GetRoundedTexture(color, Color.clear, 4, 0));
            GUI.DrawTexture(new Rect(rect.x + rect.width * 0.28f, rect.y + rect.height * 0.28f, rect.width * 0.44f, rect.height * 0.44f), GetCircleTexture(new Color(1f, 1f, 1f, color.a * 0.45f)));
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Draws locked label in the current GUI or scene context.
        /// Inputs: `rect`, `text`, `style`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="text">Input value used by this method.</param>
        /// <param name="style">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Performs lock style text color for this component.
        /// Inputs: `style`, `textColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="style">Input value used by this method.</param>
        /// <param name="textColor">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Resolves panel entrance scale from the current runtime state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `float` value.
        /// </summary>
        /// <returns>a `float` value.</returns>
        private float ResolvePanelEntranceScale()
        {
            float progress = Mathf.Clamp01((Time.unscaledTime - shownAtTime) / Mathf.Max(0.01f, panelEntranceSeconds));
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            return Mathf.Lerp(0.92f, 1f, eased) + Mathf.Sin(eased * Mathf.PI) * 0.035f;
        }

        /// <summary>
        /// Purpose: Converts ResultUI artwork pixel coordinates into the current screen-space panel rect.
        /// Inputs: panel is the drawn artwork rect; x/y/width/height are source-art top-left pixels.
        /// Output: a screen-space rect aligned to the artwork.
        /// </summary>
        /// <param name="panel">Drawn artwork rect.</param>
        /// <param name="x">Source artwork x coordinate.</param>
        /// <param name="y">Source artwork y coordinate.</param>
        /// <param name="width">Source artwork width.</param>
        /// <param name="height">Source artwork height.</param>
        /// <returns>Screen-space rect.</returns>
        private Rect RelativeArtworkRect(Rect panel, float x, float y, float width, float height)
        {
            return PixelSnapRect(new Rect(
                panel.x + panel.width * x / ResultArtworkWidth,
                panel.y + panel.height * y / ResultArtworkHeight,
                panel.width * width / ResultArtworkWidth,
                panel.height * height / ResultArtworkHeight));
        }

        /// <summary>
        /// Purpose: Calculates an aspect-preserving rect inside a destination slot.
        /// Inputs: sourceWidth/sourceHeight define art ratio; bounds is the available screen space.
        /// Output: an aspect-fit rect centered in bounds.
        /// </summary>
        /// <param name="sourceWidth">Source art width.</param>
        /// <param name="sourceHeight">Source art height.</param>
        /// <param name="bounds">Available bounds.</param>
        /// <returns>Aspect-fit rect.</returns>
        private Rect CalculateAspectFitRect(float sourceWidth, float sourceHeight, Rect bounds)
        {
            if (sourceWidth <= 0f || sourceHeight <= 0f || bounds.width <= 0f || bounds.height <= 0f)
            {
                return bounds;
            }

            float sourceAspect = sourceWidth / sourceHeight;
            float boundsAspect = bounds.width / bounds.height;
            float width = bounds.width;
            float height = bounds.height;
            if (boundsAspect > sourceAspect)
            {
                width = height * sourceAspect;
            }
            else
            {
                height = width / sourceAspect;
            }

            return PixelSnapRect(new Rect(
                bounds.x + (bounds.width - width) * 0.5f,
                bounds.y + (bounds.height - height) * 0.5f,
                width,
                height));
        }

        /// <summary>
        /// Purpose: Scales a rect around its center for hover/press feedback.
        /// Inputs: rect is source geometry; scaleX/scaleY are multipliers.
        /// Output: scaled rect.
        /// </summary>
        /// <param name="rect">Rect to scale.</param>
        /// <param name="scaleX">Horizontal scale.</param>
        /// <param name="scaleY">Vertical scale.</param>
        /// <returns>Scaled rect.</returns>
        private Rect ScaleRectAroundCenter(Rect rect, float scaleX, float scaleY)
        {
            Vector2 center = rect.center;
            float width = rect.width * scaleX;
            float height = rect.height * scaleY;
            return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
        }

        /// <summary>
        /// Purpose: Rounds a rect to full pixels so UI art stays stable and crisp.
        /// Inputs: rect may contain fractional coordinates from scaling.
        /// Output: pixel-snapped rect.
        /// </summary>
        /// <param name="rect">Rect to snap.</param>
        /// <returns>Pixel-aligned rect.</returns>
        private Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Round(rect.width),
                Mathf.Round(rect.height));
        }

        /// <summary>
        /// Purpose: Creates or returns a style used by invisible image button hitboxes.
        /// Inputs: no direct parameters.
        /// Output: transparent GUIStyle.
        /// </summary>
        /// <returns>Transparent button style.</returns>
        private GUIStyle GetTransparentButtonStyle()
        {
            if (transparentButtonStyle == null)
            {
                transparentButtonStyle = new GUIStyle(GUIStyle.none)
                {
                    normal = { background = null },
                    hover = { background = null },
                    active = { background = null },
                    focused = { background = null },
                    onNormal = { background = null },
                    onHover = { background = null },
                    onActive = { background = null },
                    onFocused = { background = null }
                };
            }

            return transparentButtonStyle;
        }

        /// <summary>
        /// Purpose: Ensures styles exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureStyles()
        {
            if (cardTitleStyle != null &&
                cardValueStyle != null &&
                resultIconStyle != null &&
                screenTitleStyle != null &&
                pillTextStyle != null &&
                resultTitleStyle != null &&
                resultBodyStyle != null &&
                resultDetailStyle != null)
            {
                return;
            }

            Color textPrimary = new Color(0.11f, 0.28f, 0.42f, 1f);
            Color textSecondary = new Color(0.23f, 0.45f, 0.55f, 1f);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textSecondary }
            };

            cardValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textPrimary }
            };

            resultIconStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            LockStyleTextColor(resultIconStyle, Color.white);

            screenTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };
            LockStyleTextColor(screenTitleStyle, textPrimary);

            pillTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.97f, 0.78f, 1f) }
            };
            LockStyleTextColor(pillTextStyle, new Color(1f, 0.97f, 0.78f, 1f));

            resultTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Overflow,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };
            LockStyleTextColor(resultTitleStyle, textPrimary);

            resultBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Overflow,
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = textSecondary }
            };
            LockStyleTextColor(resultBodyStyle, textSecondary);

            resultDetailStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Overflow,
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = textSecondary }
            };
            LockStyleTextColor(resultDetailStyle, textSecondary);
        }

        /// <summary>
        /// Purpose: Draws panel in the current GUI or scene context.
        /// Inputs: `rect`, `fill`, `border`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="fill">Input value used by this method.</param>
        /// <param name="border">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        private void DrawPanel(Rect rect, Color fill, Color border, int radius, int borderSize)
        {
            GUI.DrawTexture(
                new Rect(rect.x + 5f, rect.y + 7f, rect.width, rect.height),
                GetRoundedTexture(new Color(0.04f, 0.22f, 0.34f, 0.28f), new Color(0.04f, 0.22f, 0.34f, 0.28f), radius, 0));
            GUI.DrawTexture(rect, GetRoundedTexture(fill, border, radius, borderSize));
        }

        /// <summary>
        /// Purpose: Gets circle texture.
        /// Inputs: `color`; may also read serialized fields and current runtime state.
        /// Output: a `Texture2D` value.
        /// </summary>
        /// <param name="color">Input value used by this method.</param>
        /// <returns>a `Texture2D` value.</returns>
        private Texture2D GetCircleTexture(Color color)
        {
            string key = ColorKey(color);
            if (CircleTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            float center = (TextureSize - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    Color pixel = color;
                    pixel.a *= alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            CircleTextureCache[key] = texture;
            return texture;
        }

        /// <summary>
        /// Purpose: Gets rounded texture.
        /// Inputs: `fill`, `border`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: a `Texture2D` value.
        /// </summary>
        /// <param name="fill">Input value used by this method.</param>
        /// <param name="border">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        /// <returns>a `Texture2D` value.</returns>
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

        /// <summary>
        /// Purpose: Returns whether this object is inside rounded rect.
        /// Inputs: `x`, `y`, `width`, `height`, `radius`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="x">Input value used by this method.</param>
        /// <param name="y">Input value used by this method.</param>
        /// <param name="width">Input value used by this method.</param>
        /// <param name="height">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Returns color key for the current state.
        /// Inputs: `color`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="color">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string ColorKey(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
