using BubbleTown.Characters;
using BubbleTown.Core;
using BubbleTown.Core.Enums;
using BubbleTown.Items;
using BubbleTown.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BubbleTown.UI
{
    /// <summary>
    /// Canvas-based Battle HUD and overlay controller.
    /// This script replaces the Battle screen's old IMGUI drawing while keeping the same readable left-side layout.
    /// </summary>
    public class BattleCanvasHUD : MonoBehaviour
    {
        private const float LeftHudX = 14f;
        private const float LeftHudWidth = 286f;
        private const float ObjectivePanelBottom = 144f;
        private const float CharacterAreaTop = ObjectivePanelBottom + 14f;
        private const float CharacterSectionGap = 14f;
        private const float BottomHudMargin = 26f;
        private const float BottomControlsPanelHeight = 46f;

        private readonly Color safeAreaFill = new Color(0.74f, 0.93f, 1f, 1f);
        private readonly Color panelFill = new Color(1f, 0.96f, 0.72f, 0.9f);
        private readonly Color textPrimary = new Color(0.11f, 0.28f, 0.42f, 1f);
        private readonly Color textSecondary = new Color(0.22f, 0.45f, 0.55f, 1f);
        private readonly Color creamText = new Color(1f, 0.98f, 0.8f, 1f);
        private readonly Color player1Color = new Color(0.12f, 0.72f, 1f, 1f);
        private readonly Color player2Color = new Color(1f, 0.45f, 0.26f, 1f);
        private readonly Color aiColor = new Color(0.64f, 0.46f, 1f, 1f);
        private readonly Color neutralColor = new Color(1f, 0.82f, 0.32f, 1f);
        private readonly Color orangeColor = new Color(1f, 0.58f, 0.18f, 1f);
        private readonly Color greenColor = new Color(0.48f, 0.9f, 0.34f, 1f);
        private readonly Color purpleColor = new Color(0.72f, 0.48f, 1f, 1f);

        private BattleUI owner;
        private Font uiFont;
        private bool initialized;
        private bool suppressSettingsCallbacks;
        private string settingsFeedbackText = "Saved automatically";
        private float settingsFeedbackVisibleUntil;

        private RectTransform safeAreaPanel;
        private RectTransform topStatusPanel;
        private RectTransform objectivePanel;
        private RectTransform localVsPanel;
        private RectTransform player1Panel;
        private RectTransform opponentPanel;
        private RectTransform bottomControlsPanel;
        private Text modeValueText;
        private Text mapValueText;
        private Text timeValueText;
        private Text stateValueText;
        private Text objectiveTitleText;
        private Text objectiveValueText;
        private Image objectiveProgressFill;
        private Text localVsRoundText;
        private Text localVsScoreText;
        private CharacterPanelViews player1Views;
        private CharacterPanelViews opponentViews;
        private Button pauseButton;
        private Button guideButton;
        private Text guideButtonText;

        private RectTransform overlayRoot;
        private RectTransform overlayDimmer;
        private RectTransform pausePanel;
        private RectTransform settingsPanel;
        private RectTransform itemGuidePanel;
        private RectTransform openingPromptPanel;
        private RectTransform resultPromptPanel;
        private RectTransform pickupToastPanel;
        private Text openingTitleText;
        private Text openingBodyText;
        private Image openingAccentImage;
        private Text resultTitleText;
        private Text resultDetailText;
        private Image resultAccentImage;
        private Text pickupToastText;
        private Image pickupToastImage;
        private Text settingsFeedbackLabel;
        private Text masterPercentText;
        private Text bgmPercentText;
        private Text sfxPercentText;
        private Text muteBgmButtonText;
        private Text muteSfxButtonText;
        private Text shakeButtonText;
        private Slider masterSlider;
        private Slider bgmSlider;
        private Slider sfxSlider;
        private ItemGuideCardViews[] itemGuideCards;

        /// <summary>
        /// Groups Text references for one character stats panel.
        /// </summary>
        private struct CharacterPanelViews
        {
            public Text HeaderLabel;
            public Text HeaderValue;
            public Text BombsValue;
            public Text RangeValue;
            public Text SpeedValue;
            public Text GuardValue;
        }

        /// <summary>
        /// Groups views needed to animate and display one item guide card.
        /// </summary>
        private struct ItemGuideCardViews
        {
            public RectTransform IconRoot;
            public ItemType ItemType;
        }

        /// <summary>
        /// Purpose: Creates a runtime Canvas HUD as a child of BattleUI.
        /// Inputs: battleUI owns callbacks and battle state used by this HUD.
        /// Output: returns the created BattleCanvasHUD, or null when battleUI is missing.
        /// </summary>
        /// <param name="battleUI">Battle UI controller that drives this Canvas HUD.</param>
        /// <returns>Created BattleCanvasHUD instance.</returns>
        public static BattleCanvasHUD Create(BattleUI battleUI)
        {
            if (battleUI == null)
            {
                return null;
            }

            GameObject root = new GameObject("BattleCanvasHUD", typeof(RectTransform));
            root.transform.SetParent(battleUI.transform, false);

            BattleCanvasHUD hud = root.AddComponent<BattleCanvasHUD>();
            hud.Initialize(battleUI);
            return hud;
        }

        /// <summary>
        /// Purpose: Initializes the runtime Canvas hierarchy once.
        /// Inputs: battleUI supplies callbacks for pause, guide, retry, menu, and settings actions.
        /// Output: no return value; creates child UI objects and caches their references.
        /// </summary>
        /// <param name="battleUI">Battle UI controller that owns this HUD.</param>
        public void Initialize(BattleUI battleUI)
        {
            owner = battleUI;
            if (initialized)
            {
                return;
            }

            uiFont = ResolveFont();
            EnsureEventSystemExists();
            BuildCanvas();
            BuildHudHierarchy();
            BuildOverlayHierarchy();
            initialized = true;
        }

        /// <summary>
        /// Purpose: Updates every Canvas HUD element with current battle data.
        /// Inputs: gameManager provides gameplay state; elapsedSeconds is the visible timer.
        /// Output: no return value; updates layout, labels, overlays, and button states.
        /// </summary>
        /// <param name="gameManager">Current GameManager, or null while loading.</param>
        /// <param name="elapsedSeconds">Elapsed battle time in seconds.</param>
        public void Refresh(GameManager gameManager, float elapsedSeconds)
        {
            RefreshPersistentHud(gameManager, elapsedSeconds);
            RefreshOverlays(gameManager);
        }

        /// <summary>
        /// Purpose: Shows or hides the entire runtime Canvas HUD.
        /// Inputs: visible decides whether the GameObject is active.
        /// Output: no return value; updates active state.
        /// </summary>
        /// <param name="visible">True to show the HUD; false to hide it.</param>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Purpose: Finds a built-in font for runtime Text components.
        /// Inputs: no direct parameters; reads Unity built-in resources.
        /// Output: returns a Font reference when available.
        /// </summary>
        /// <returns>Built-in font used by the Canvas HUD.</returns>
        private Font ResolveFont()
        {
            Font resolvedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return resolvedFont != null ? resolvedFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        /// <summary>
        /// Purpose: Ensures Canvas buttons can receive pointer events.
        /// Inputs: no direct parameters; searches the active scene for an EventSystem.
        /// Output: no return value; creates EventSystem when needed.
        /// </summary>
        private void EnsureEventSystemExists()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// Purpose: Creates the root Canvas using pixel coordinates that match the old IMGUI layout.
        /// Inputs: no direct parameters; uses this GameObject as the Canvas root.
        /// Output: no return value; configures Canvas, scaler, and raycaster.
        /// </summary>
        private void BuildCanvas()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            gameObject.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Purpose: Creates persistent left-side HUD panels and buttons.
        /// Inputs: no direct parameters; uses fixed Battle HUD sizes from the previous layout.
        /// Output: no return value; caches Text, Image, and Button references.
        /// </summary>
        private void BuildHudHierarchy()
        {
            safeAreaPanel = CreatePanel("SafeAreaPanel", transform, safeAreaFill, Color.clear);
            topStatusPanel = CreatePanel("TopStatusPanel", safeAreaPanel, panelFill, player1Color);
            objectivePanel = CreatePanel("ObjectivePanel", safeAreaPanel, panelFill, orangeColor);
            localVsPanel = CreatePanel("LocalVSPanel", safeAreaPanel, panelFill, greenColor);
            player1Panel = CreatePanel("Player1Panel", safeAreaPanel, panelFill, player1Color);
            opponentPanel = CreatePanel("OpponentPanel", safeAreaPanel, panelFill, aiColor);
            bottomControlsPanel = CreatePanel("BottomControlsPanel", safeAreaPanel, panelFill, player1Color);

            BuildTopStatusPanel();
            BuildObjectivePanel();
            BuildLocalVsPanel();
            player1Views = BuildCharacterPanel(player1Panel, player1Color);
            opponentViews = BuildCharacterPanel(opponentPanel, aiColor);
            BuildBottomControls();
        }

        /// <summary>
        /// Purpose: Builds every Canvas overlay that used to be drawn with IMGUI.
        /// Inputs: no direct parameters; creates inactive overlay panels for later refresh.
        /// Output: no return value; caches overlay labels, buttons, sliders, and guide cards.
        /// </summary>
        private void BuildOverlayHierarchy()
        {
            overlayRoot = CreatePanel("OverlayRoot", transform, Color.clear, Color.clear);
            SetRaycastTarget(overlayRoot, false);
            overlayDimmer = CreatePanel("OverlayDimmer", overlayRoot, new Color(0.04f, 0.18f, 0.26f, 0.46f), Color.clear);
            pausePanel = CreatePanel("PausePanel", overlayRoot, new Color(1f, 0.96f, 0.72f, 0.98f), player1Color);
            settingsPanel = CreatePanel("SettingsPanel", overlayRoot, new Color(1f, 0.96f, 0.72f, 0.98f), player1Color);
            itemGuidePanel = CreatePanel("ItemGuidePanel", overlayRoot, new Color(1f, 0.96f, 0.74f, 0.97f), player1Color);
            openingPromptPanel = CreatePanel("OpeningPromptPanel", overlayRoot, new Color(1f, 0.96f, 0.72f, 0.98f), neutralColor);
            resultPromptPanel = CreatePanel("ResultPromptPanel", overlayRoot, new Color(1f, 0.96f, 0.72f, 0.98f), orangeColor);
            pickupToastPanel = CreatePanel("PickupToastPanel", overlayRoot, new Color(1f, 0.94f, 0.55f, 0.98f), Color.white);

            BuildPausePanel();
            BuildSettingsPanel();
            BuildItemGuidePanel();
            BuildOpeningPromptPanel();
            BuildResultPromptPanel();
            BuildPickupToastPanel();
        }

        /// <summary>
        /// Purpose: Builds the mode, map, time, and state pills.
        /// Inputs: no direct parameters; writes child objects under topStatusPanel.
        /// Output: no return value; caches value Text references for Refresh.
        /// </summary>
        private void BuildTopStatusPanel()
        {
            modeValueText = CreateStatusPill(topStatusPanel, "MODE", 10f, 9f, player1Color);
            mapValueText = CreateStatusPill(topStatusPanel, "MAP", 146f, 9f, greenColor);
            timeValueText = CreateStatusPill(topStatusPanel, "TIME", 10f, 42f, orangeColor);
            stateValueText = CreateStatusPill(topStatusPanel, "STATE", 146f, 42f, neutralColor);
        }

        /// <summary>
        /// Purpose: Builds the single-player objective panel.
        /// Inputs: no direct parameters; writes child objects under objectivePanel.
        /// Output: no return value; caches objective labels and progress Image.
        /// </summary>
        private void BuildObjectivePanel()
        {
            objectiveTitleText = CreateText("ObjectiveTitle", objectivePanel, "Reach Exit", 14, FontStyle.Bold, creamText, TextAnchor.MiddleLeft);
            SetTopLeft(objectiveTitleText.rectTransform, 12f, 5f, 130f, 18f);

            objectiveValueText = CreateText("ObjectiveValue", objectivePanel, "0 tiles", 13, FontStyle.Bold, textPrimary, TextAnchor.MiddleRight);
            SetTopLeft(objectiveValueText.rectTransform, 146f, 5f, 126f, 18f);

            RectTransform progressBack = CreatePanel("ObjectiveProgressBack", objectivePanel, new Color(0.16f, 0.34f, 0.44f, 0.24f), Color.clear);
            SetTopLeft(progressBack, 16f, 34f, 254f, 6f);

            objectiveProgressFill = CreateImage("ObjectiveProgressFill", progressBack, new Color(1f, 0.62f, 0.16f, 0.96f));
            SetTopLeft(objectiveProgressFill.rectTransform, 0f, 0f, 254f, 6f);
        }

        /// <summary>
        /// Purpose: Builds the LocalVS score panel.
        /// Inputs: no direct parameters; writes child objects under localVsPanel.
        /// Output: no return value; caches round and score labels.
        /// </summary>
        private void BuildLocalVsPanel()
        {
            localVsRoundText = CreateText("RoundLabel", localVsPanel, "ROUND 1", 12, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(localVsRoundText.rectTransform, 12f, 5f, 262f, 18f);

            localVsScoreText = CreateText("ScoreLabel", localVsPanel, "0 - 0", 15, FontStyle.Bold, creamText, TextAnchor.MiddleCenter);
            SetTopLeft(localVsScoreText.rectTransform, 12f, 22f, 262f, 20f);
        }

        /// <summary>
        /// Purpose: Builds a character stats panel.
        /// Inputs: parent is the panel root; accentColor styles the header and bomb box.
        /// Output: returns Text references used for live stat updates.
        /// </summary>
        /// <param name="parent">Character panel root.</param>
        /// <param name="accentColor">Accent color for this character.</param>
        /// <returns>Text references for one character panel.</returns>
        private CharacterPanelViews BuildCharacterPanel(RectTransform parent, Color accentColor)
        {
            RectTransform header = CreatePanel("Header", parent, accentColor, Color.white);
            SetTopLeft(header, 10f, 8f, 266f, 26f);

            CharacterPanelViews views = new CharacterPanelViews
            {
                HeaderLabel = CreateText("HeaderLabel", header, "PLAYER", 12, FontStyle.Bold, textPrimary, TextAnchor.MiddleLeft),
                HeaderValue = CreateText("HeaderValue", header, "ALIVE", 12, FontStyle.Bold, creamText, TextAnchor.MiddleRight)
            };
            SetTopLeft(views.HeaderLabel.rectTransform, 12f, 4f, 116f, 18f);
            SetTopLeft(views.HeaderValue.rectTransform, 138f, 4f, 116f, 18f);

            views.BombsValue = BuildAbilityBox(parent, "Bombs", 10f, accentColor);
            views.RangeValue = BuildAbilityBox(parent, "Range", 77f, orangeColor);
            views.SpeedValue = BuildAbilityBox(parent, "Speed", 144f, greenColor);
            views.GuardValue = BuildAbilityBox(parent, "Guard", 211f, new Color(0.35f, 0.78f, 1f, 1f));
            return views;
        }

        /// <summary>
        /// Purpose: Builds the bottom Pause and Item Guide controls.
        /// Inputs: no direct parameters; writes child buttons under bottomControlsPanel.
        /// Output: no return value; connects Canvas buttons to BattleUI callbacks.
        /// </summary>
        private void BuildBottomControls()
        {
            pauseButton = CreateButton("PauseButton", bottomControlsPanel, "Pause", 10f, 10f, 128f, 26f, player1Color);
            pauseButton.onClick.AddListener(() => owner?.OnCanvasPauseRequested());

            guideButton = CreateButton("ItemGuideButton", bottomControlsPanel, "Item Guide", 148f, 10f, 128f, 26f, greenColor);
            guideButtonText = guideButton.GetComponentInChildren<Text>();
            guideButton.onClick.AddListener(() => owner?.OnCanvasItemGuideRequested());
        }

        /// <summary>
        /// Purpose: Builds the pause overlay panel and its action buttons.
        /// Inputs: no direct parameters; writes child objects under pausePanel.
        /// Output: no return value; connects buttons to BattleUI callbacks.
        /// </summary>
        private void BuildPausePanel()
        {
            Text pauseTitle = CreateText("PauseTitle", pausePanel, "PAUSED", 34, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(pauseTitle.rectTransform, 30f, 24f, 370f, 52f);

            Text body = CreateText("PauseBody", pausePanel, "Take a breath. The arena is frozen until you resume.", 14, FontStyle.Normal, textSecondary, TextAnchor.MiddleCenter);
            SetTopLeft(body.rectTransform, 48f, 84f, 334f, 42f);

            Button resumeButton = CreateButton("ResumeButton", pausePanel, "RESUME", 46f, 146f, 160f, 42f, player1Color);
            resumeButton.onClick.AddListener(() => owner?.OnCanvasResumeRequested());

            Button settingsButton = CreateButton("SettingsButton", pausePanel, "SETTINGS", 224f, 146f, 160f, 42f, greenColor);
            settingsButton.onClick.AddListener(() => owner?.OnCanvasSettingsRequested());

            Button retryButton = CreateButton("RetryButton", pausePanel, "RETRY", 46f, 204f, 160f, 42f, orangeColor);
            retryButton.onClick.AddListener(() => owner?.OnClickRetry());

            Button menuButton = CreateButton("MainMenuButton", pausePanel, "MAIN MENU", 224f, 204f, 160f, 42f, player2Color);
            menuButton.onClick.AddListener(() => owner?.OnClickBackToMenu());

            Text hint = CreateText("PauseHint", pausePanel, "Press Escape or P to resume.", 12, FontStyle.Bold, textSecondary, TextAnchor.MiddleCenter);
            SetTopLeft(hint.rectTransform, 48f, 280f, 334f, 24f);
        }

        /// <summary>
        /// Purpose: Builds the pause settings overlay with sliders and quick toggle buttons.
        /// Inputs: no direct parameters; reads AudioManager and GameSettings when controls change.
        /// Output: no return value; connects settings controls to persistent game settings.
        /// </summary>
        private void BuildSettingsPanel()
        {
            Text title = CreateText("SettingsTitle", settingsPanel, "Settings", 28, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(title.rectTransform, 40f, 18f, 560f, 40f);

            Text body = CreateText("SettingsBody", settingsPanel, "Tune sound, feedback, and saved preferences.", 13, FontStyle.Normal, textSecondary, TextAnchor.MiddleCenter);
            SetTopLeft(body.rectTransform, 60f, 58f, 520f, 24f);

            Button topClose = CreateButton("TopCloseButton", settingsPanel, "X", 588f, 18f, 34f, 30f, player2Color);
            topClose.onClick.AddListener(() => owner?.OnCanvasCloseSettingsRequested());

            settingsFeedbackLabel = CreateText("SettingsFeedback", settingsPanel, "Saved automatically", 13, FontStyle.Bold, greenColor, TextAnchor.MiddleCenter);
            SetTopLeft(settingsFeedbackLabel.rectTransform, 55f, 92f, 530f, 34f);

            masterSlider = BuildSettingsSlider("Master", settingsPanel, "Master Volume", 55f, 142f, player1Color, OnMasterVolumeChanged, out masterPercentText);
            bgmSlider = BuildSettingsSlider("BGM", settingsPanel, "BGM Volume", 55f, 190f, greenColor, OnBgmVolumeChanged, out bgmPercentText);
            sfxSlider = BuildSettingsSlider("SFX", settingsPanel, "SFX Volume", 55f, 238f, orangeColor, OnSfxVolumeChanged, out sfxPercentText);

            Button muteBgm = CreateButton("MuteBGMButton", settingsPanel, "BGM ON", 80f, 298f, 140f, 34f, greenColor);
            muteBgmButtonText = muteBgm.GetComponentInChildren<Text>();
            muteBgm.onClick.AddListener(ToggleBgmMute);

            Button muteSfx = CreateButton("MuteSFXButton", settingsPanel, "SFX ON", 250f, 298f, 140f, 34f, orangeColor);
            muteSfxButtonText = muteSfx.GetComponentInChildren<Text>();
            muteSfx.onClick.AddListener(ToggleSfxMute);

            Button shake = CreateButton("ShakeButton", settingsPanel, "SHAKE ON", 420f, 298f, 140f, 34f, player1Color);
            shakeButtonText = shake.GetComponentInChildren<Text>();
            shake.onClick.AddListener(ToggleScreenShake);

            Button previewSfx = CreateButton("PreviewSFXButton", settingsPanel, "TEST SFX", 95f, 378f, 190f, 34f, purpleColor);
            previewSfx.onClick.AddListener(() => AudioManager.Instance?.PlaySettingsPreviewSFX());

            Button previewBgm = CreateButton("PreviewBGMButton", settingsPanel, "TEST BGM", 355f, 378f, 190f, 34f, greenColor);
            previewBgm.onClick.AddListener(() => AudioManager.Instance?.PlayCurrentSceneBGMPreview());

            Button restore = CreateButton("RestoreDefaultsButton", settingsPanel, "RESTORE DEFAULTS", 105f, 438f, 260f, 34f, orangeColor);
            restore.onClick.AddListener(RestoreSettingsDefaults);

            Button close = CreateButton("CloseButton", settingsPanel, "CLOSE", 386f, 438f, 150f, 34f, player1Color);
            close.onClick.AddListener(() => owner?.OnCanvasCloseSettingsRequested());
        }

        /// <summary>
        /// Purpose: Builds the item guide overlay cards.
        /// Inputs: no direct parameters; writes card views under itemGuidePanel.
        /// Output: no return value; caches card icon transforms for lightweight animation.
        /// </summary>
        private void BuildItemGuidePanel()
        {
            RectTransform titleBack = CreatePanel("GuideTitleBack", itemGuidePanel, player1Color, Color.white);
            SetTopLeft(titleBack, 192f, 14f, 236f, 34f);
            Text title = CreateText("GuideTitle", titleBack, "ITEM GUIDE", 20, FontStyle.Bold, creamText, TextAnchor.MiddleCenter);
            SetTopLeft(title.rectTransform, 0f, 0f, 236f, 34f);

            Button close = CreateButton("CloseButton", itemGuidePanel, "X", 566f, 14f, 34f, 28f, player2Color);
            close.onClick.AddListener(() => owner?.OnCanvasCloseItemGuideRequested());

            itemGuideCards = new ItemGuideCardViews[5];
            float contentX = 24f;
            float contentY = 66f;
            float gap = 10f;
            float cardHeight = 56f;
            float cardWidth = (620f - 48f - gap) * 0.5f;
            itemGuideCards[0] = BuildItemGuideCard("BombSlot", new Vector2(contentX, contentY), new Vector2(cardWidth, cardHeight), ItemType.BombCountUp, "B+", "Bomb Slot", "Extra bomb slot.", player1Color);
            itemGuideCards[1] = BuildItemGuideCard("BlastRange", new Vector2(contentX + cardWidth + gap, contentY), new Vector2(cardWidth, cardHeight), ItemType.ExplosionRangeUp, "R+", "Blast Range", "Longer blast lines.", orangeColor);
            itemGuideCards[2] = BuildItemGuideCard("SpeedBoots", new Vector2(contentX, contentY + cardHeight + gap), new Vector2(cardWidth, cardHeight), ItemType.MoveSpeedUp, "S", "Speed Boots", "Move faster.", greenColor);
            itemGuideCards[3] = BuildItemGuideCard("Shield", new Vector2(contentX + cardWidth + gap, contentY + cardHeight + gap), new Vector2(cardWidth, cardHeight), ItemType.Shield, "SH", "Shield", "Blocks one hit.", new Color(0.35f, 0.78f, 1f, 1f));
            itemGuideCards[4] = BuildItemGuideCard("Invincible", new Vector2(contentX, contentY + (cardHeight + gap) * 2f), new Vector2(620f - 48f, cardHeight), ItemType.TemporaryInvincible, "*", "Invincible", "Brief safety time.", purpleColor);

            RectTransform tipBack = CreatePanel("GuideTipBack", itemGuidePanel, new Color(0.48f, 0.9f, 0.34f, 0.72f), Color.white);
            SetTopLeft(tipBack, 140f, 290f, 340f, 24f);
            Text tip = CreateText("GuideTip", tipBack, "Soft blocks hide power-ups.", 12, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(tip.rectTransform, 0f, 0f, 340f, 24f);
        }

        /// <summary>
        /// Purpose: Builds the Ready/Go prompt overlay.
        /// Inputs: no direct parameters; creates prompt labels and accent frame.
        /// Output: no return value; caches prompt text and accent Image references.
        /// </summary>
        private void BuildOpeningPromptPanel()
        {
            openingAccentImage = openingPromptPanel.GetComponent<Image>();
            openingTitleText = CreateText("OpeningTitle", openingPromptPanel, "READY", 40, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(openingTitleText.rectTransform, 0f, 18f, 320f, 62f);

            openingBodyText = CreateText("OpeningBody", openingPromptPanel, "Controls unlock on GO", 14, FontStyle.Bold, textSecondary, TextAnchor.MiddleCenter);
            SetTopLeft(openingBodyText.rectTransform, 18f, 78f, 284f, 24f);
        }

        /// <summary>
        /// Purpose: Builds the in-battle result prompt overlay.
        /// Inputs: no direct parameters; creates title and detail labels.
        /// Output: no return value; caches labels and accent Image references.
        /// </summary>
        private void BuildResultPromptPanel()
        {
            resultAccentImage = resultPromptPanel.GetComponent<Image>();
            resultTitleText = CreateText("ResultTitle", resultPromptPanel, "Victory", 30, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(resultTitleText.rectTransform, 18f, 22f, 424f, 58f);

            resultDetailText = CreateText("ResultDetail", resultPromptPanel, "The battle has ended.", 15, FontStyle.Bold, textSecondary, TextAnchor.MiddleCenter);
            SetTopLeft(resultDetailText.rectTransform, 28f, 84f, 404f, 82f);
        }

        /// <summary>
        /// Purpose: Builds the power-up pickup toast overlay.
        /// Inputs: no direct parameters; creates one compact toast label.
        /// Output: no return value; caches text and Image references.
        /// </summary>
        private void BuildPickupToastPanel()
        {
            pickupToastImage = pickupToastPanel.GetComponent<Image>();
            pickupToastText = CreateText("PickupToastText", pickupToastPanel, "Picked up power-up", 14, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(pickupToastText.rectTransform, 12f, 5f, 276f, 30f);
        }

        /// <summary>
        /// Purpose: Updates persistent left-side HUD data and layout.
        /// Inputs: gameManager provides gameplay state; elapsedSeconds provides the timer value.
        /// Output: no return value; refreshes persistent HUD panels.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <param name="elapsedSeconds">Elapsed battle time in seconds.</param>
        private void RefreshPersistentHud(GameManager gameManager, float elapsedSeconds)
        {
            RefreshLayout(gameManager);
            if (gameManager == null)
            {
                SetText(modeValueText, "...");
                SetText(mapValueText, "...");
                SetText(timeValueText, FormatTime(elapsedSeconds));
                SetText(stateValueText, "WAIT");
                RefreshCharacterPanel(player1Views, "PLAYER 1", null);
                SetPanelActive(opponentPanel, false);
                SetPanelActive(objectivePanel, false);
                SetPanelActive(localVsPanel, false);
                RefreshButtons();
                return;
            }

            SetText(modeValueText, FormatModeName(gameManager.CurrentGameMode));
            SetText(mapValueText, FormatMapName(gameManager.CurrentMapType));
            SetText(timeValueText, FormatTime(elapsedSeconds));
            SetText(stateValueText, FormatRoundState(gameManager));
            RefreshObjective(gameManager);
            RefreshLocalVs(gameManager);
            RefreshCharacterPanels(gameManager);
            RefreshButtons();
        }

        /// <summary>
        /// Purpose: Updates all non-persistent Canvas overlays.
        /// Inputs: gameManager supplies protection timers for prompt labels.
        /// Output: no return value; shows, hides, positions, and animates overlay panels.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        private void RefreshOverlays(GameManager gameManager)
        {
            SetFullScreen(overlayRoot);
            SetFullScreen(overlayDimmer);

            bool showPause = owner != null && owner.IsBattlePaused && !owner.IsPauseSettingsOpen;
            bool showSettings = owner != null && owner.IsBattlePaused && owner.IsPauseSettingsOpen;
            bool showGuide = owner != null && owner.IsItemGuideOpen;
            bool showDimmer = showPause || showSettings || showGuide;
            SetPanelActive(overlayDimmer, showDimmer);

            RefreshPausePanel(showPause);
            RefreshSettingsPanel(showSettings);
            RefreshItemGuidePanel(showGuide);
            RefreshOpeningPrompt(gameManager, !showPause && !showSettings && !showGuide);
            RefreshResultPrompt(!showPause && !showSettings);
            RefreshPickupToast(!showPause && !showSettings && !showGuide);
        }

        /// <summary>
        /// Purpose: Positions the persistent HUD so it matches the previously accepted Battle layout.
        /// Inputs: gameManager determines whether an opponent panel is needed.
        /// Output: no return value; updates RectTransform positions.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        private void RefreshLayout(GameManager gameManager)
        {
            float safeWidth = Mathf.Max(LeftHudWidth + LeftHudX * 2f, Screen.width * 0.32f);
            SetTopLeft(safeAreaPanel, 0f, 0f, safeWidth, Screen.height);
            SetTopLeft(topStatusPanel, LeftHudX, 14f, LeftHudWidth, 76f);
            SetTopLeft(objectivePanel, LeftHudX, 98f, LeftHudWidth, 46f);
            SetTopLeft(localVsPanel, LeftHudX, 98f, LeftHudWidth, 46f);

            bool hasOpponent = HasVisibleOpponent(gameManager);
            float characterPanelHeight = hasOpponent ? 104f : 116f;
            float characterPanelGap = 10f;
            float totalCharacterHeight = hasOpponent
                ? characterPanelHeight * 2f + characterPanelGap
                : characterPanelHeight;
            float characterAreaBottom = ResolveBottomControlsPanelY() - CharacterSectionGap;
            float characterAreaHeight = Mathf.Max(totalCharacterHeight, characterAreaBottom - CharacterAreaTop);
            float characterPanelY = CharacterAreaTop + Mathf.Max(0f, (characterAreaHeight - totalCharacterHeight) * 0.5f);

            SetTopLeft(player1Panel, LeftHudX, characterPanelY, LeftHudWidth, characterPanelHeight);
            SetTopLeft(opponentPanel, LeftHudX, characterPanelY + characterPanelHeight + characterPanelGap, LeftHudWidth, characterPanelHeight);
            SetTopLeft(bottomControlsPanel, LeftHudX, ResolveBottomControlsPanelY(), LeftHudWidth, BottomControlsPanelHeight);
        }

        /// <summary>
        /// Purpose: Refreshes the single-player route objective UI.
        /// Inputs: gameManager supplies objective labels and progress.
        /// Output: no return value; updates panel visibility, labels, and fill width.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        private void RefreshObjective(GameManager gameManager)
        {
            bool showObjective = gameManager.CurrentGameMode == GameMode.SinglePlayer && gameManager.IsSinglePlayerObjectiveEnabled;
            SetPanelActive(objectivePanel, showObjective);
            if (!showObjective)
            {
                return;
            }

            SetText(objectiveTitleText, gameManager.SinglePlayerObjectiveLabel);
            SetText(objectiveValueText, gameManager.SinglePlayerObjectiveProgressLabel);
            objectiveProgressFill.rectTransform.sizeDelta = new Vector2(254f * Mathf.Clamp01(gameManager.SinglePlayerRouteProgress), 6f);
        }

        /// <summary>
        /// Purpose: Refreshes the LocalVS scoreboard UI.
        /// Inputs: gameManager supplies round and score labels.
        /// Output: no return value; updates panel visibility and text.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        private void RefreshLocalVs(GameManager gameManager)
        {
            bool showScore = gameManager.CurrentGameMode == GameMode.LocalVS;
            SetPanelActive(localVsPanel, showScore);
            if (!showScore)
            {
                return;
            }

            SetText(localVsRoundText, FormatLocalVsRoundHeader(gameManager));
            SetText(localVsScoreText, gameManager.LocalVsScoreLabel);
        }

        /// <summary>
        /// Purpose: Refreshes player and opponent stat panels.
        /// Inputs: gameManager supplies current characters.
        /// Output: no return value; updates labels and opponent visibility.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        private void RefreshCharacterPanels(GameManager gameManager)
        {
            RefreshCharacterPanel(player1Views, "PLAYER 1", gameManager.Player1);

            CharacterBase opponent = ResolveOpponent(gameManager, out string opponentLabel);
            bool hasOpponent = opponent != null && opponent.gameObject.activeInHierarchy;
            SetPanelActive(opponentPanel, hasOpponent);
            if (hasOpponent)
            {
                RefreshCharacterPanel(opponentViews, opponentLabel, opponent);
            }
        }

        /// <summary>
        /// Purpose: Refreshes one character panel from live CharacterBase data.
        /// Inputs: views contains labels to update; label is the header text; character may be null.
        /// Output: no return value; updates all stat text.
        /// </summary>
        /// <param name="views">Text references for one panel.</param>
        /// <param name="label">Header label.</param>
        /// <param name="character">Character data source.</param>
        private void RefreshCharacterPanel(CharacterPanelViews views, string label, CharacterBase character)
        {
            SetText(views.HeaderLabel, label);
            SetText(views.HeaderValue, FormatLifeState(character));

            if (character == null || !character.gameObject.activeInHierarchy)
            {
                SetText(views.BombsValue, "-");
                SetText(views.RangeValue, "-");
                SetText(views.SpeedValue, "-");
                SetText(views.GuardValue, "-");
                return;
            }

            SetText(views.BombsValue, character.RemainingBombCount + "/" + character.MaxBombCount);
            SetText(views.RangeValue, character.BombRange.ToString());
            SetText(views.SpeedValue, character.MoveSpeed.ToString("0.0"));
            SetText(views.GuardValue, character.ShieldCharges.ToString());
        }

        /// <summary>
        /// Purpose: Refreshes bottom button labels and clickability.
        /// Inputs: reads owner overlay state.
        /// Output: no return value; updates button text and interactable flags.
        /// </summary>
        private void RefreshButtons()
        {
            bool overlayOpen = owner != null && (owner.IsItemGuideOpen || owner.IsBattlePaused);
            SetText(guideButtonText, owner != null && owner.IsItemGuideOpen ? "Hide Guide" : "Item Guide");
            if (pauseButton != null)
            {
                pauseButton.interactable = !overlayOpen;
            }

            if (guideButton != null)
            {
                guideButton.interactable = !overlayOpen;
            }
        }

        /// <summary>
        /// Purpose: Shows and animates the pause panel.
        /// Inputs: visible determines whether pausePanel should be active.
        /// Output: no return value; updates panel active state, position, and scale.
        /// </summary>
        /// <param name="visible">True when the battle is paused and settings are not open.</param>
        private void RefreshPausePanel(bool visible)
        {
            SetPanelActive(pausePanel, visible);
            if (!visible)
            {
                return;
            }

            float bob = Mathf.Sin(Time.unscaledTime * 2.2f) * 2.2f;
            SetCentered(pausePanel, 0f, bob, 430f, 330f);
            pausePanel.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 3.2f) * 0.006f);
        }

        /// <summary>
        /// Purpose: Shows and refreshes the Canvas settings panel.
        /// Inputs: visible determines whether settings should be active.
        /// Output: no return value; updates slider values, toggle labels, and feedback message.
        /// </summary>
        /// <param name="visible">True when the pause settings panel should be shown.</param>
        private void RefreshSettingsPanel(bool visible)
        {
            SetPanelActive(settingsPanel, visible);
            if (!visible)
            {
                return;
            }

            SetCentered(settingsPanel, 0f, 0f, 640f, 500f);
            AudioManager audioManager = AudioManager.Instance;
            suppressSettingsCallbacks = true;
            if (masterSlider != null)
            {
                masterSlider.value = audioManager != null ? audioManager.MasterVolume : GameSettings.MasterVolume;
            }

            if (bgmSlider != null)
            {
                bgmSlider.value = audioManager != null ? audioManager.BgmVolume : GameSettings.BgmVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = audioManager != null ? audioManager.SfxVolume : GameSettings.SfxVolume;
            }

            suppressSettingsCallbacks = false;
            RefreshSettingsText(audioManager);
        }

        /// <summary>
        /// Purpose: Shows and animates the item guide overlay.
        /// Inputs: visible determines whether guide cards should be active.
        /// Output: no return value; updates active state, panel position, and icon motion.
        /// </summary>
        /// <param name="visible">True when the item guide should be shown.</param>
        private void RefreshItemGuidePanel(bool visible)
        {
            SetPanelActive(itemGuidePanel, visible);
            if (!visible)
            {
                return;
            }

            SetCentered(itemGuidePanel, 0f, 0f, 620f, 330f);
            for (int i = 0; i < itemGuideCards.Length; i++)
            {
                ItemGuideCardViews card = itemGuideCards[i];
                if (card.IconRoot == null)
                {
                    continue;
                }

                float phase = (int)card.ItemType * 0.73f;
                float bob = Mathf.Sin(Time.unscaledTime * 3.1f + phase) * 1.4f;
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4.2f + phase) * 0.045f;
                card.IconRoot.localPosition = new Vector3(card.IconRoot.localPosition.x, -9f - bob, 0f);
                card.IconRoot.localScale = Vector3.one * pulse;
            }
        }

        /// <summary>
        /// Purpose: Shows Ready/Go as a Canvas prompt.
        /// Inputs: gameManager supplies protection timer; allowVisible hides prompt behind blocking overlays.
        /// Output: no return value; updates prompt text, color, position, and pulse scale.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <param name="allowVisible">False when a modal overlay should take priority.</param>
        private void RefreshOpeningPrompt(GameManager gameManager, bool allowVisible)
        {
            bool visible = allowVisible && owner != null && owner.IsOpeningPromptVisible;
            SetPanelActive(openingPromptPanel, visible);
            if (!visible)
            {
                return;
            }

            SetCentered(openingPromptPanel, 0f, 0f, 320f, 116f);
            bool readyPhase = owner.IsOpeningReadyPhase;
            Color promptColor = readyPhase ? neutralColor : new Color(0.2f, 0.88f, 1f, 1f);
            if (openingAccentImage != null)
            {
                openingAccentImage.color = promptColor;
            }

            SetText(openingTitleText, owner.OpeningPromptTitle);
            SetText(openingBodyText, owner.OpeningPromptBody);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.04f;
            openingPromptPanel.localScale = Vector3.one * pulse;
        }

        /// <summary>
        /// Purpose: Shows queued battle result as a Canvas prompt.
        /// Inputs: allowVisible hides prompt behind settings but lets it appear during normal gameplay.
        /// Output: no return value; updates result text and entrance animation.
        /// </summary>
        /// <param name="allowVisible">False when settings or pause menus should take priority.</param>
        private void RefreshResultPrompt(bool allowVisible)
        {
            bool visible = allowVisible && owner != null && owner.IsResultPromptVisible;
            SetPanelActive(resultPromptPanel, visible);
            if (!visible)
            {
                return;
            }

            SetCentered(resultPromptPanel, 0f, 0f, 460f, 184f);
            SetText(resultTitleText, owner.ResultPromptTitle);
            SetText(resultDetailText, owner.ResultPromptDetail);
            if (resultAccentImage != null)
            {
                resultAccentImage.color = owner.ResultPromptTitle.IndexOf("Defeat", System.StringComparison.OrdinalIgnoreCase) >= 0
                    ? player2Color
                    : orangeColor;
            }

            float entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(owner.ResultPromptEntrance / 0.28f));
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.025f;
            resultPromptPanel.localScale = Vector3.one * Mathf.Lerp(0.86f, 1f, entrance) * pulse;
        }

        /// <summary>
        /// Purpose: Shows the power-up pickup toast as a Canvas element.
        /// Inputs: allowVisible hides the toast while blocking overlays are open.
        /// Output: no return value; updates text, color, position, scale, and opacity.
        /// </summary>
        /// <param name="allowVisible">False when a modal overlay should take priority.</param>
        private void RefreshPickupToast(bool allowVisible)
        {
            bool visible = allowVisible && owner != null && owner.IsPickupToastVisible;
            SetPanelActive(pickupToastPanel, visible);
            if (!visible)
            {
                return;
            }

            float progress = owner.PickupToastProgress01;
            float pop = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.22f));
            float scale = Mathf.Lerp(0.9f, 1f, pop) + Mathf.Sin(pop * Mathf.PI) * 0.08f;
            float slideOffset = Mathf.Lerp(-18f, 0f, pop);
            SetCentered(pickupToastPanel, 0f, Screen.height * 0.5f - 152f + slideOffset, 300f, 40f);
            pickupToastPanel.localScale = Vector3.one * scale;
            SetText(pickupToastText, owner.PickupToastText);
            if (pickupToastImage != null)
            {
                pickupToastImage.color = owner.PickupToastColor;
            }
        }

        /// <summary>
        /// Purpose: Refreshes all settings labels after slider or toggle values change.
        /// Inputs: audioManager supplies live audio values; falls back to GameSettings if missing.
        /// Output: no return value; updates percentage labels and toggle labels.
        /// </summary>
        /// <param name="audioManager">Audio source for current values.</param>
        private void RefreshSettingsText(AudioManager audioManager)
        {
            float master = audioManager != null ? audioManager.MasterVolume : GameSettings.MasterVolume;
            float bgm = audioManager != null ? audioManager.BgmVolume : GameSettings.BgmVolume;
            float sfx = audioManager != null ? audioManager.SfxVolume : GameSettings.SfxVolume;
            bool muteBgm = audioManager != null ? audioManager.MuteBGM : GameSettings.MuteBGM;
            bool muteSfx = audioManager != null ? audioManager.MuteSFX : GameSettings.MuteSFX;

            SetText(masterPercentText, Mathf.RoundToInt(master * 100f) + "%");
            SetText(bgmPercentText, Mathf.RoundToInt(bgm * 100f) + "%");
            SetText(sfxPercentText, Mathf.RoundToInt(sfx * 100f) + "%");
            SetText(muteBgmButtonText, muteBgm ? "BGM OFF" : "BGM ON");
            SetText(muteSfxButtonText, muteSfx ? "SFX OFF" : "SFX ON");
            SetText(shakeButtonText, GameSettings.ScreenShakeEnabled ? "SHAKE ON" : "SHAKE OFF");
            SetText(settingsFeedbackLabel, Time.unscaledTime < settingsFeedbackVisibleUntil ? settingsFeedbackText : "Saved automatically");
        }

        /// <summary>
        /// Purpose: Applies master volume changes from the Canvas slider.
        /// Inputs: value is a normalized 0-1 slider value.
        /// Output: no return value; updates AudioManager, saves settings, and plays preview SFX.
        /// </summary>
        /// <param name="value">New master volume.</param>
        private void OnMasterVolumeChanged(float value)
        {
            if (suppressSettingsCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetMasterVolume(value);
            AudioManager.Instance?.PlaySettingsPreviewSFX();
            ShowSettingsFeedback($"Saved Master Volume {Mathf.RoundToInt(value * 100f)}%");
        }

        /// <summary>
        /// Purpose: Applies BGM volume changes from the Canvas slider.
        /// Inputs: value is a normalized 0-1 slider value.
        /// Output: no return value; updates AudioManager and saves settings.
        /// </summary>
        /// <param name="value">New BGM volume.</param>
        private void OnBgmVolumeChanged(float value)
        {
            if (suppressSettingsCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetBgmVolume(value);
            ShowSettingsFeedback($"Saved BGM Volume {Mathf.RoundToInt(value * 100f)}%");
        }

        /// <summary>
        /// Purpose: Applies SFX volume changes from the Canvas slider.
        /// Inputs: value is a normalized 0-1 slider value.
        /// Output: no return value; updates AudioManager, saves settings, and plays preview SFX.
        /// </summary>
        /// <param name="value">New SFX volume.</param>
        private void OnSfxVolumeChanged(float value)
        {
            if (suppressSettingsCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetSfxVolume(value);
            AudioManager.Instance?.PlaySettingsPreviewSFX();
            ShowSettingsFeedback($"Saved SFX Volume {Mathf.RoundToInt(value * 100f)}%");
        }

        /// <summary>
        /// Purpose: Toggles BGM mute from the Canvas settings panel.
        /// Inputs: no direct parameters; reads current AudioManager or GameSettings mute state.
        /// Output: no return value; saves the new mute state and refreshes settings labels.
        /// </summary>
        private void ToggleBgmMute()
        {
            AudioManager audioManager = AudioManager.Instance;
            bool current = audioManager != null ? audioManager.MuteBGM : GameSettings.MuteBGM;
            audioManager?.SetBgmMuted(!current);
            AudioManager.Instance?.PlayButtonClickSFX();
            ShowSettingsFeedback(!current ? "BGM muted and saved" : "BGM unmuted and saved");
        }

        /// <summary>
        /// Purpose: Toggles SFX mute from the Canvas settings panel.
        /// Inputs: no direct parameters; reads current AudioManager or GameSettings mute state.
        /// Output: no return value; saves the new mute state and refreshes settings labels.
        /// </summary>
        private void ToggleSfxMute()
        {
            AudioManager audioManager = AudioManager.Instance;
            bool current = audioManager != null ? audioManager.MuteSFX : GameSettings.MuteSFX;
            audioManager?.SetSfxMuted(!current);
            if (current)
            {
                AudioManager.Instance?.PlayButtonClickSFX();
            }

            ShowSettingsFeedback(!current ? "SFX muted and saved" : "SFX unmuted and saved");
        }

        /// <summary>
        /// Purpose: Toggles screen shake from the Canvas settings panel.
        /// Inputs: no direct parameters; reads GameSettings.ScreenShakeEnabled.
        /// Output: no return value; saves the new screen shake value and refreshes labels.
        /// </summary>
        private void ToggleScreenShake()
        {
            bool enabled = !GameSettings.ScreenShakeEnabled;
            GameSettings.SetScreenShakeEnabled(enabled);
            AudioManager.Instance?.PlayButtonClickSFX();
            ShowSettingsFeedback(enabled ? "Screen shake enabled" : "Screen shake disabled");
        }

        /// <summary>
        /// Purpose: Restores settings defaults from the Canvas settings panel.
        /// Inputs: no direct parameters; uses GameSettings defaults.
        /// Output: no return value; saves defaults, reloads AudioManager, and refreshes labels.
        /// </summary>
        private void RestoreSettingsDefaults()
        {
            GameSettings.ResetToDefaults();
            AudioManager.Instance?.ReloadFromGameSettings();
            AudioManager.Instance?.PlayButtonClickSFX();
            ShowSettingsFeedback("Defaults restored and saved");
        }

        /// <summary>
        /// Purpose: Shows temporary settings feedback text.
        /// Inputs: message is displayed for a short time.
        /// Output: no return value; updates local feedback state.
        /// </summary>
        /// <param name="message">Message shown in the settings panel.</param>
        private void ShowSettingsFeedback(string message)
        {
            settingsFeedbackText = message;
            settingsFeedbackVisibleUntil = Time.unscaledTime + 2.2f;
            RefreshSettingsText(AudioManager.Instance);
        }

        /// <summary>
        /// Purpose: Creates a compact status pill and returns its value text.
        /// Inputs: parent is the top status panel; label is fixed caption; x/y place the pill; accentColor is fill.
        /// Output: returns the dynamic value Text.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="label">Fixed label.</param>
        /// <param name="x">Local x position.</param>
        /// <param name="y">Local y position from top.</param>
        /// <param name="accentColor">Pill fill color.</param>
        /// <returns>Dynamic value text.</returns>
        private Text CreateStatusPill(RectTransform parent, string label, float x, float y, Color accentColor)
        {
            RectTransform pill = CreatePanel(label + "Pill", parent, accentColor, Color.white);
            SetTopLeft(pill, x, y, 130f, 25f);

            Text labelText = CreateText("Label", pill, label, 12, FontStyle.Bold, textPrimary, TextAnchor.MiddleLeft);
            SetTopLeft(labelText.rectTransform, 8f, 5f, 40f, 15f);

            Text valueText = CreateText("Value", pill, "...", 13, FontStyle.Bold, creamText, TextAnchor.MiddleLeft);
            SetTopLeft(valueText.rectTransform, 54f, 4f, 68f, 17f);
            return valueText;
        }

        /// <summary>
        /// Purpose: Creates a compact ability box.
        /// Inputs: parent is a character panel; label names the stat; x places the box; accentColor styles the border.
        /// Output: returns the value Text inside the box.
        /// </summary>
        /// <param name="parent">Character panel.</param>
        /// <param name="label">Stat name.</param>
        /// <param name="x">Local x position.</param>
        /// <param name="accentColor">Border color.</param>
        /// <returns>Dynamic value Text.</returns>
        private Text BuildAbilityBox(RectTransform parent, string label, float x, Color accentColor)
        {
            RectTransform box = CreatePanel(label + "Box", parent, new Color(1f, 0.98f, 0.86f, 0.92f), accentColor);
            SetTopLeft(box, x, 46f, 58f, 44f);

            Text labelText = CreateText("Label", box, label, 9, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(labelText.rectTransform, 4f, 4f, 50f, 15f);

            Text valueText = CreateText("Value", box, "0", 12, FontStyle.Bold, textPrimary, TextAnchor.MiddleCenter);
            SetTopLeft(valueText.rectTransform, 4f, 20f, 50f, 20f);
            return valueText;
        }

        /// <summary>
        /// Purpose: Creates one settings slider row.
        /// Inputs: row position, label, accent color, and callback define the slider.
        /// Output: returns the Slider and exposes its percent Text through an out parameter.
        /// </summary>
        /// <param name="name">Base GameObject name.</param>
        /// <param name="parent">Parent settings panel.</param>
        /// <param name="label">Visible row label.</param>
        /// <param name="x">Local x position.</param>
        /// <param name="y">Local y position.</param>
        /// <param name="accent">Accent color for fill and handle.</param>
        /// <param name="callback">Function called when slider value changes.</param>
        /// <param name="percentText">Output text used to show percentage.</param>
        /// <returns>Created Slider component.</returns>
        private Slider BuildSettingsSlider(string name, RectTransform parent, string label, float x, float y, Color accent, UnityEngine.Events.UnityAction<float> callback, out Text percentText)
        {
            RectTransform row = CreatePanel(name + "Row", parent, Color.clear, Color.clear);
            SetTopLeft(row, x, y, 530f, 34f);

            Text labelText = CreateText("Label", row, label, 13, FontStyle.Bold, textPrimary, TextAnchor.MiddleLeft);
            SetTopLeft(labelText.rectTransform, 0f, 0f, 145f, 34f);

            percentText = CreateText("Percent", row, "100%", 13, FontStyle.Bold, textPrimary, TextAnchor.MiddleRight);
            SetTopLeft(percentText.rectTransform, 450f, 0f, 80f, 34f);

            RectTransform sliderRoot = CreatePanel("Slider", row, Color.clear, Color.clear);
            SetTopLeft(sliderRoot, 158f, 8f, 270f, 18f);
            Slider slider = sliderRoot.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            RectTransform background = CreatePanel("Background", sliderRoot, new Color(0.16f, 0.34f, 0.44f, 0.25f), Color.clear);
            SetStretch(background);

            RectTransform fillArea = CreatePanel("FillArea", sliderRoot, Color.clear, Color.clear);
            SetStretch(fillArea);
            RectTransform fill = CreatePanel("Fill", fillArea, accent, Color.clear);
            SetStretch(fill);

            RectTransform handle = CreatePanel("Handle", sliderRoot, Color.Lerp(accent, Color.white, 0.15f), Color.white);
            SetCentered(handle, 0f, 0f, 22f, 26f);

            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.onValueChanged.AddListener(callback);
            return slider;
        }

        /// <summary>
        /// Purpose: Creates one item guide card.
        /// Inputs: card settings define placement, icon, title, body, and color.
        /// Output: returns cached card views for later icon animation.
        /// </summary>
        /// <param name="name">Card GameObject name.</param>
        /// <param name="position">Top-left local position.</param>
        /// <param name="size">Card size.</param>
        /// <param name="itemType">Item represented by the card.</param>
        /// <param name="icon">Short icon label.</param>
        /// <param name="title">Item name.</param>
        /// <param name="body">Short item description.</param>
        /// <param name="accent">Card accent color.</param>
        /// <returns>Created item guide card views.</returns>
        private ItemGuideCardViews BuildItemGuideCard(string name, Vector2 position, Vector2 size, ItemType itemType, string icon, string title, string body, Color accent)
        {
            RectTransform card = CreatePanel(name, itemGuidePanel, new Color(1f, 0.99f, 0.88f, 0.92f), Color.Lerp(accent, Color.white, 0.18f));
            SetTopLeft(card, position.x, position.y, size.x, size.y);

            RectTransform iconRoot = CreatePanel("Icon", card, Color.Lerp(accent, Color.white, 0.08f), Color.white);
            SetTopLeft(iconRoot, 10f, 9f, 40f, 40f);
            Text iconText = CreateText("IconText", iconRoot, icon, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetTopLeft(iconText.rectTransform, 4f, 4f, 32f, 32f);

            Text titleText = CreateText("Title", card, title, 13, FontStyle.Bold, textPrimary, TextAnchor.MiddleLeft);
            SetTopLeft(titleText.rectTransform, 60f, 8f, size.x - 70f, 20f);
            Text bodyText = CreateText("Body", card, body, 11, FontStyle.Bold, textSecondary, TextAnchor.MiddleLeft);
            SetTopLeft(bodyText.rectTransform, 60f, 30f, size.x - 70f, 18f);

            return new ItemGuideCardViews
            {
                IconRoot = iconRoot,
                ItemType = itemType
            };
        }

        /// <summary>
        /// Purpose: Creates a button styled to fit the existing cartoon UI.
        /// Inputs: parent is the target panel; label and rect define visible button.
        /// Output: returns the Button component for callback wiring.
        /// </summary>
        /// <param name="name">Button GameObject name.</param>
        /// <param name="parent">Parent panel.</param>
        /// <param name="label">Button text.</param>
        /// <param name="x">Local x position.</param>
        /// <param name="y">Local y position from top.</param>
        /// <param name="width">Button width.</param>
        /// <param name="height">Button height.</param>
        /// <param name="fill">Button fill color.</param>
        /// <returns>Created Button component.</returns>
        private Button CreateButton(string name, RectTransform parent, string label, float x, float y, float width, float height, Color fill)
        {
            RectTransform buttonRect = CreatePanel(name, parent, fill, Color.white);
            SetTopLeft(buttonRect, x, y, width, height);
            Button button = buttonRect.gameObject.AddComponent<Button>();

            ColorBlock colors = button.colors;
            colors.normalColor = fill;
            colors.highlightedColor = Color.Lerp(fill, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(fill, Color.black, 0.1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text text = CreateText("Label", buttonRect, label, Mathf.Clamp(Mathf.RoundToInt(height * 0.42f), 12, 20), FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetTopLeft(text.rectTransform, 4f, 3f, width - 8f, height - 6f);
            return button;
        }

        /// <summary>
        /// Purpose: Creates a rectangular panel with optional outline and shadow.
        /// Inputs: name, parent, fill, and border define the new visual element.
        /// Output: returns the RectTransform for later layout updates.
        /// </summary>
        /// <param name="name">GameObject name.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="fill">Image fill color.</param>
        /// <param name="border">Outline color.</param>
        /// <returns>Created panel RectTransform.</returns>
        private RectTransform CreatePanel(string name, Transform parent, Color fill, Color border)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform));
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            Image image = panelObject.AddComponent<Image>();
            image.color = fill;

            if (border.a > 0f)
            {
                Outline outline = panelObject.AddComponent<Outline>();
                outline.effectColor = border;
                outline.effectDistance = new Vector2(2f, -2f);

                Shadow shadow = panelObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.04f, 0.18f, 0.24f, 0.24f);
                shadow.effectDistance = new Vector2(5f, -5f);
            }

            return rectTransform;
        }

        /// <summary>
        /// Purpose: Creates an Image child.
        /// Inputs: name, parent, and color define the visual element.
        /// Output: returns the Image component.
        /// </summary>
        /// <param name="name">GameObject name.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="color">Image color.</param>
        /// <returns>Created Image component.</returns>
        private Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>
        /// Purpose: Creates a Text child.
        /// Inputs: label settings define content, color, and alignment.
        /// Output: returns the Text component.
        /// </summary>
        /// <param name="name">GameObject name.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="text">Initial text.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="color">Text color.</param>
        /// <param name="alignment">Text alignment.</param>
        /// <returns>Created Text component.</returns>
        private Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            Text label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = uiFont;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        /// <summary>
        /// Purpose: Places a RectTransform using top-left screen-style coordinates.
        /// Inputs: rectTransform is modified; x/y/width/height define its local rectangle.
        /// Output: no return value; updates anchors, pivot, position, and size.
        /// </summary>
        /// <param name="rectTransform">RectTransform to place.</param>
        /// <param name="x">Distance from parent's left edge.</param>
        /// <param name="y">Distance from parent's top edge.</param>
        /// <param name="width">Element width.</param>
        /// <param name="height">Element height.</param>
        private void SetTopLeft(RectTransform rectTransform, float x, float y, float width, float height)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(x, -y);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// Purpose: Places a RectTransform around the screen center.
        /// Inputs: xOffset/yOffset move from screen center; width/height define size.
        /// Output: no return value; updates anchors, pivot, position, and size.
        /// </summary>
        /// <param name="rectTransform">RectTransform to place.</param>
        /// <param name="xOffset">Horizontal offset from center.</param>
        /// <param name="yOffset">Vertical offset from center.</param>
        /// <param name="width">Element width.</param>
        /// <param name="height">Element height.</param>
        private void SetCentered(RectTransform rectTransform, float xOffset, float yOffset, float width, float height)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// Purpose: Stretches a RectTransform across its parent.
        /// Inputs: rectTransform is modified in place.
        /// Output: no return value; anchors and offsets fill the parent area.
        /// </summary>
        /// <param name="rectTransform">RectTransform to stretch.</param>
        private void SetStretch(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Purpose: Stretches a RectTransform to the full screen.
        /// Inputs: rectTransform is modified in place.
        /// Output: no return value; anchors and offsets fill the Canvas.
        /// </summary>
        /// <param name="rectTransform">RectTransform to stretch.</param>
        private void SetFullScreen(RectTransform rectTransform)
        {
            SetStretch(rectTransform);
        }

        /// <summary>
        /// Purpose: Enables or disables UI raycast blocking on a panel Image.
        /// Inputs: panel may contain an Image; enabled decides whether that Image catches pointer events.
        /// Output: no return value; updates Image.raycastTarget when an Image exists.
        /// </summary>
        /// <param name="panel">Panel whose Image should be updated.</param>
        /// <param name="enabled">True to catch pointer events; false to let clicks pass through.</param>
        private void SetRaycastTarget(RectTransform panel, bool enabled)
        {
            Image image = panel != null ? panel.GetComponent<Image>() : null;
            if (image != null)
            {
                image.raycastTarget = enabled;
            }
        }

        /// <summary>
        /// Purpose: Shows or hides a panel safely.
        /// Inputs: panel is optional; active is the desired active state.
        /// Output: no return value; updates GameObject active state.
        /// </summary>
        /// <param name="panel">Panel to update.</param>
        /// <param name="active">Desired active state.</param>
        private void SetPanelActive(RectTransform panel, bool active)
        {
            if (panel != null && panel.gameObject.activeSelf != active)
            {
                panel.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Purpose: Updates Text only when the content changes.
        /// Inputs: label is optional; value is the new content.
        /// Output: no return value; updates label.text when needed.
        /// </summary>
        /// <param name="label">Text target.</param>
        /// <param name="value">New content.</param>
        private void SetText(Text label, string value)
        {
            if (label != null && label.text != value)
            {
                label.text = value;
            }
        }

        /// <summary>
        /// Purpose: Returns the top Y of the bottom controls panel.
        /// Inputs: no direct parameters; reads current Screen height.
        /// Output: returns the screen-space Y position from the top edge.
        /// </summary>
        /// <returns>Bottom controls top Y.</returns>
        private float ResolveBottomControlsPanelY()
        {
            return Screen.height - BottomHudMargin - BottomControlsPanelHeight;
        }

        /// <summary>
        /// Purpose: Checks whether the current mode has a visible opponent panel.
        /// Inputs: gameManager may be null.
        /// Output: returns true when Player2 or AI should be shown.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <returns>True when an opponent panel is needed.</returns>
        private bool HasVisibleOpponent(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return false;
            }

            CharacterBase opponent = ResolveOpponent(gameManager, out _);
            return opponent != null && opponent.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Purpose: Resolves the opponent character for AI Battle or LocalVS.
        /// Inputs: gameManager supplies current mode and character references; label is an output display name.
        /// Output: returns the opponent character, or null in SinglePlayer.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <param name="label">Output label for the opponent panel.</param>
        /// <returns>Opponent character or null.</returns>
        private CharacterBase ResolveOpponent(GameManager gameManager, out string label)
        {
            switch (gameManager.CurrentGameMode)
            {
                case GameMode.LocalVS:
                    label = "PLAYER 2";
                    return gameManager.Player2;
                case GameMode.AIBattle:
                    label = "AI RIVAL";
                    return gameManager.AIPlayer;
                default:
                    label = "SOLO";
                    return null;
            }
        }

        /// <summary>
        /// Purpose: Formats map enum as player-facing text.
        /// Inputs: mapType is the current map enum.
        /// Output: returns display text.
        /// </summary>
        /// <param name="mapType">Current map type.</param>
        /// <returns>Map display name.</returns>
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
        /// Purpose: Formats game mode enum as compact HUD text.
        /// Inputs: gameMode is the current mode.
        /// Output: returns display text.
        /// </summary>
        /// <param name="gameMode">Current game mode.</param>
        /// <returns>Compact mode display name.</returns>
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

        /// <summary>
        /// Purpose: Formats battle state for the top status bar.
        /// Inputs: gameManager supplies pause, state, and protection data.
        /// Output: returns compact state text.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <returns>State display label.</returns>
        private string FormatRoundState(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return "WAIT";
            }

            if (gameManager.IsBattlePaused)
            {
                return "PAUSE";
            }

            if (gameManager.CurrentGameState == GameState.BattlePreparing)
            {
                return "READY";
            }

            float protectionRemaining = ResolveMaxProtectionRemaining(gameManager);
            if (protectionRemaining > 0f)
            {
                return $"SAFE {protectionRemaining:0.0}";
            }

            return gameManager.CurrentGameState == GameState.BattleRunning ? "FIGHT" : gameManager.CurrentGameState.ToString();
        }

        /// <summary>
        /// Purpose: Formats LocalVS round metadata.
        /// Inputs: gameManager supplies target score and round number.
        /// Output: returns a scoreboard header string.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <returns>LocalVS round header.</returns>
        private string FormatLocalVsRoundHeader(GameManager gameManager)
        {
            string matchLabel = gameManager.EnableLocalVsBestOf3
                ? $"BEST OF {gameManager.LocalVsTargetScore * 2 - 1}"
                : "SINGLE ROUND";
            return $"{matchLabel}  |  ROUND {gameManager.LocalVsRoundNumber}";
        }

        /// <summary>
        /// Purpose: Formats character life and protection state.
        /// Inputs: character may be null or inactive.
        /// Output: returns OFF, SAFE, SHIELD, ALIVE, or DOWN.
        /// </summary>
        /// <param name="character">Character to inspect.</param>
        /// <returns>Life state label.</returns>
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

        /// <summary>
        /// Purpose: Finds the highest active invincibility timer.
        /// Inputs: gameManager supplies all active character references.
        /// Output: returns seconds remaining, or 0 if nobody is protected.
        /// </summary>
        /// <param name="gameManager">Current game state source.</param>
        /// <returns>Maximum protection seconds remaining.</returns>
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

        /// <summary>
        /// Purpose: Reads one character's invincibility timer.
        /// Inputs: character may be null, inactive, defeated, or unprotected.
        /// Output: returns remaining seconds or 0.
        /// </summary>
        /// <param name="character">Character to inspect.</param>
        /// <returns>Protection seconds remaining.</returns>
        private float ResolveProtectionRemaining(CharacterBase character)
        {
            if (character == null || !character.gameObject.activeInHierarchy || !character.IsAlive || !character.IsInvincible)
            {
                return 0f;
            }

            return character.InvincibleSecondsRemaining;
        }

        /// <summary>
        /// Purpose: Formats seconds as mm:ss.
        /// Inputs: seconds is the elapsed timer value.
        /// Output: returns a zero-padded time string.
        /// </summary>
        /// <param name="seconds">Elapsed seconds.</param>
        /// <returns>Formatted timer text.</returns>
        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int secondsPart = totalSeconds % 60;
            return $"{minutes:00}:{secondsPart:00}";
        }
    }
}
