using System.Collections.Generic;
using BubbleTown.Core;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Lightweight IMGUI helper for colorful chibi-style game screens.
    /// </summary>
    public static class SimpleUIFactory
    {
        /// <summary>
        /// Small map preview patterns used by menu cards and guide panels.
        /// </summary>
        public enum MapPreviewPattern
        {
            Balanced,
            Open,
            Maze
        }

        /// <summary>
        /// Icon variants used by main-menu buttons.
        /// </summary>
        public enum MenuButtonIcon
        {
            Play,
            Guide,
            Settings,
            Quit
        }

        private const int TextureSize = 64;
        private const string GuideModalResourcePath = "UI/Guide/GuideUI";
        private const string GuideCloseButtonResourcePath = "UI/Guide/Close";
        private const string SettingsModalResourcePath = "UI/Settings/SettingUI";
        private const string SettingsCloseButtonResourcePath = "UI/Settings/Close2";
        private const string SettingsMusicPreviewButtonResourcePath = "UI/Settings/MusicPreview";
        private const string SettingsRestoreDefaultsButtonResourcePath = "UI/Settings/RestoreDefaults";
        private const string SettingsSfxPreviewButtonResourcePath = "UI/Settings/SFXPreview";
        private const float GuideCloseButtonFloatSpeed = 2.8f;
        private const float GuideCloseButtonFloatAmount = 0.006f;
        private const float SettingsImageButtonFloatSpeed = 2.8f;
        private const float SettingsImageButtonFloatAmount = 0.006f;

        private static GUIStyle titleStyle;
        private static GUIStyle titleShadowStyle;
        private static GUIStyle titleHighlightStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle smallBodyStyle;
        private static GUIStyle panelStyle;
        private static GUIStyle pillStyle;
        private static GUIStyle cardTitleStyle;
        private static GUIStyle cardTagStyle;
        private static GUIStyle cardBodyStyle;
        private static GUIStyle menuButtonTextStyle;
        private static GUIStyle modalTitleStyle;
        private static GUIStyle modalBodyStyle;
        private static GUIStyle guideTitleStyle;
        private static GUIStyle guideTextStyle;
        private static GUIStyle settingsLabelStyle;
        private static GUIStyle settingsValueStyle;
        private static GUIStyle settingsFeedbackStyle;
        private static GUIStyle invisibleButtonStyle;

        /// <summary>
        /// Decorative icon variants used inside menu cards.
        /// </summary>
        private enum MenuDecorationIcon
        {
            Bomb,
            Blocks,
            PowerUp
        }

        /// <summary>
        /// Icon variants used by guide modal rows.
        /// </summary>
        private enum GuideRowIcon
        {
            PlayerOne,
            PlayerTwo,
            Bomb,
            Goal
        }

        private static readonly Dictionary<string, Texture2D> RoundedTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> CircleTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> SolidTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, GUIStyle> ButtonStyleCache = new Dictionary<string, GUIStyle>();
        private static Texture2D guideModalTexture;
        private static Texture2D guideCloseButtonTexture;
        private static bool guideTexturesLoaded;
        private static Texture2D settingsModalTexture;
        private static Texture2D settingsCloseButtonTexture;
        private static Texture2D settingsMusicPreviewButtonTexture;
        private static Texture2D settingsRestoreDefaultsButtonTexture;
        private static Texture2D settingsSfxPreviewButtonTexture;
        private static bool settingsTexturesLoaded;

        private static readonly Color PanelFill = new Color(1f, 0.96f, 0.72f, 0.96f);
        private static readonly Color PanelBorder = new Color(0.2f, 0.58f, 0.82f, 1f);
        private static readonly Color PanelShadow = new Color(0.04f, 0.22f, 0.34f, 0.32f);
        private static readonly Color TextPrimary = new Color(0.11f, 0.28f, 0.42f, 1f);
        private static readonly Color TextSecondary = new Color(0.23f, 0.45f, 0.55f, 1f);
        private static readonly Color CreamText = new Color(1f, 0.97f, 0.78f, 1f);
        private const float SettingsFeedbackSeconds = 2.2f;

        private static string settingsFeedbackMessage = "Saved automatically";
        private static float settingsFeedbackVisibleUntil;
        private static Color settingsFeedbackColor = new Color(0.42f, 0.88f, 0.38f, 1f);

        /// <summary>
        /// Purpose: Returns centered rect for the current state.
        /// Inputs: `width`, `height`; may also read serialized fields and current runtime state.
        /// Output: a `Rect` value.
        /// </summary>
        /// <param name="width">Input value used by this method.</param>
        /// <param name="height">Input value used by this method.</param>
        /// <returns>a `Rect` value.</returns>
        public static Rect CenteredRect(float width, float height)
        {
            float resolvedWidth = Mathf.Min(width, Screen.width - 40f);
            float resolvedHeight = Mathf.Min(height, Screen.height - 40f);
            return new Rect(
                (Screen.width - resolvedWidth) * 0.5f,
                (Screen.height - resolvedHeight) * 0.5f,
                resolvedWidth,
                resolvedHeight);
        }

        /// <summary>
        /// Purpose: Draws candy background in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public static void DrawCandyBackground()
        {
            float t = Time.unscaledTime;
            DrawVerticalGradient(
                new Rect(0f, 0f, Screen.width, Screen.height),
                new Color(0.45f, 0.86f, 1f, 1f),
                new Color(0.98f, 0.73f, 0.52f, 1f));

            DrawBubble(new Rect(Screen.width * 0.08f + Mathf.Sin(t * 0.7f) * 8f, Screen.height * 0.12f + Mathf.Cos(t * 0.6f) * 6f, 96f, 96f), new Color(1f, 1f, 1f, 0.22f));
            DrawBubble(new Rect(Screen.width * 0.78f + Mathf.Sin(t * 0.45f + 2f) * 10f, Screen.height * 0.08f + Mathf.Cos(t * 0.55f) * 7f, 132f, 132f), new Color(0.45f, 1f, 0.9f, 0.22f));
            DrawBubble(new Rect(Screen.width * 0.12f + Mathf.Sin(t * 0.5f + 4f) * 9f, Screen.height * 0.72f + Mathf.Cos(t * 0.48f) * 5f, 150f, 150f), new Color(1f, 0.95f, 0.45f, 0.2f));
            DrawBubble(new Rect(Screen.width * 0.72f + Mathf.Sin(t * 0.62f + 1f) * 7f, Screen.height * 0.68f + Mathf.Cos(t * 0.44f + 3f) * 8f, 110f, 110f), new Color(1f, 0.42f, 0.68f, 0.18f));
            DrawBubble(new Rect(Screen.width * 0.5f - 42f + Mathf.Sin(t * 0.58f + 5f) * 6f, Screen.height * 0.1f + Mathf.Cos(t * 0.5f + 2f) * 5f, 84f, 84f), new Color(1f, 1f, 1f, 0.18f));

            DrawRoundedRect(
                new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.88f, 420f, 12f),
                new Color(1f, 0.98f, 0.62f, 0.24f),
                Color.clear,
                6,
                0);
        }

        /// <summary>
        /// Purpose: Begins panel.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        public static void BeginPanel(Rect rect)
        {
            EnsureStyles();
            DrawRoundedRect(new Rect(rect.x - 6f, rect.y - 6f, rect.width + 12f, rect.height + 12f), new Color(0.35f, 0.9f, 1f, 0.14f), Color.clear, 30, 0);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Space(18f);
        }

        /// <summary>
        /// Purpose: Performs end panel for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public static void EndPanel()
        {
            GUILayout.EndArea();
        }

        /// <summary>
        /// Purpose: Performs title for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void Title(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(240f, 68f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y + 5f, rect.width, rect.height), text, titleShadowStyle);
            GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, rect.height), text, titleHighlightStyle);
            GUI.Label(rect, text, titleStyle);
            GUILayout.Space(6f);
        }

        /// <summary>
        /// Purpose: Performs compact title for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void CompactTitle(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(240f, 54f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y + 4f, rect.width, rect.height), text, titleShadowStyle);
            GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, rect.height), text, titleHighlightStyle);
            GUI.Label(rect, text, titleStyle);
            GUILayout.Space(2f);
        }

        /// <summary>
        /// Purpose: Performs body for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void Body(string text)
        {
            EnsureStyles();
            GUILayout.Label(text, bodyStyle);
            GUILayout.Space(12f);
        }

        /// <summary>
        /// Purpose: Performs small body for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void SmallBody(string text)
        {
            EnsureStyles();
            GUILayout.Label(text, smallBodyStyle);
            GUILayout.Space(8f);
        }

        /// <summary>
        /// Purpose: Returns button for the current state.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool Button(string text)
        {
            return PrimaryButton(text);
        }

        /// <summary>
        /// Purpose: Returns primary button for the current state.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool PrimaryButton(string text)
        {
            GUILayout.Space(8f);
            Rect rect = GUILayoutUtility.GetRect(220f, 58f, GUILayout.ExpandWidth(true));
            return CartoonButton(
                rect,
                text,
                new Color(0.09f, 0.72f, 1f, 1f),
                new Color(0.2f, 0.86f, 1f, 1f),
                new Color(0.05f, 0.58f, 0.85f, 1f),
                Color.white);
        }

        /// <summary>
        /// Purpose: Returns fixed primary button for the current state.
        /// Inputs: `rect`, `text`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="text">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool FixedPrimaryButton(Rect rect, string text)
        {
            return CartoonButton(
                rect,
                text,
                new Color(0.09f, 0.72f, 1f, 1f),
                new Color(0.2f, 0.86f, 1f, 1f),
                new Color(0.05f, 0.58f, 0.85f, 1f),
                Color.white);
        }

        /// <summary>
        /// Purpose: Returns secondary button for the current state.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool SecondaryButton(string text)
        {
            GUILayout.Space(8f);
            Rect rect = GUILayoutUtility.GetRect(220f, 52f, GUILayout.ExpandWidth(true));
            return CartoonButton(
                rect,
                text,
                new Color(1f, 0.6f, 0.28f, 1f),
                new Color(1f, 0.72f, 0.36f, 1f),
                new Color(0.88f, 0.45f, 0.18f, 1f),
                Color.white);
        }

        /// <summary>
        /// Purpose: Returns fixed secondary button for the current state.
        /// Inputs: `rect`, `text`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="text">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool FixedSecondaryButton(Rect rect, string text)
        {
            return CartoonButton(
                rect,
                text,
                new Color(1f, 0.6f, 0.28f, 1f),
                new Color(1f, 0.72f, 0.36f, 1f),
                new Color(0.88f, 0.45f, 0.18f, 1f),
                Color.white);
        }

        /// <summary>
        /// Purpose: Draws a compact selectable pill for option groups such as AI difficulty.
        /// Inputs: rect defines the button position, text is shown to the player, isSelected controls highlight state, and accentColor tints the option.
        /// Output: returns true only on the frame the player clicks the pill.
        /// </summary>
        /// <param name="rect">Screen-space area for the selectable pill.</param>
        /// <param name="text">Button label shown to the player.</param>
        /// <param name="isSelected">Whether this option is currently selected.</param>
        /// <param name="accentColor">Color used to brand this option.</param>
        /// <returns>True when the pill is clicked; otherwise false.</returns>
        public static bool ChoicePill(Rect rect, string text, bool isSelected, Color accentColor)
        {
            Color normal = isSelected ? accentColor : Color.Lerp(PanelFill, accentColor, 0.42f);
            Color hover = Color.Lerp(normal, Color.white, 0.16f);
            Color active = Color.Lerp(accentColor, Color.black, 0.16f);
            Color textColor = isSelected ? Color.white : TextPrimary;
            return CartoonButton(rect, text, normal, hover, active, textColor);
        }

        /// <summary>
        /// Purpose: Returns menu tile button for the current state.
        /// Inputs: `text`, `icon`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool MenuTileButton(string text, MenuButtonIcon icon, Color accentColor)
        {
            EnsureStyles();
            GUILayout.Space(7f);
            Rect rect = GUILayoutUtility.GetRect(190f, 78f, GUILayout.ExpandWidth(true));
            bool clicked = DrawMenuTileButton(rect, text, icon, accentColor);
            GUILayout.Space(7f);
            return clicked;
        }

        /// <summary>
        /// Purpose: Performs label pill for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void LabelPill(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 34f, GUILayout.ExpandWidth(true));
            Rect pillRect = new Rect(rect.x + rect.width * 0.18f, rect.y, rect.width * 0.64f, rect.height);
            DrawRoundedRect(pillRect, new Color(0.23f, 0.77f, 0.95f, 1f), new Color(1f, 1f, 1f, 0.75f), 18, 2);
            GUI.Label(pillRect, text, pillStyle);
            GUILayout.Space(6f);
        }

        /// <summary>
        /// Purpose: Performs compact label pill for this component.
        /// Inputs: `text`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        public static void CompactLabelPill(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 28f, GUILayout.ExpandWidth(true));
            Rect pillRect = new Rect(rect.x + rect.width * 0.22f, rect.y, rect.width * 0.56f, rect.height);
            DrawRoundedRect(pillRect, new Color(0.23f, 0.77f, 0.95f, 1f), new Color(1f, 1f, 1f, 0.75f), 16, 2);
            GUI.Label(pillRect, text, pillStyle);
            GUILayout.Space(3f);
        }

        /// <summary>
        /// Purpose: Performs feature row for this component.
        /// Inputs: `stringlabels`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="stringlabels">Input value used by this method.</param>
        public static void FeatureRow(params string[] labels)
        {
            EnsureStyles();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < labels.Length; i++)
            {
                DrawFeaturePill(labels[i], i);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        /// <summary>
        /// Purpose: Performs main menu decorations for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public static void MainMenuDecorations()
        {
            EnsureStyles();
            Rect rowRect = GUILayoutUtility.GetRect(560f, 132f, GUILayout.ExpandWidth(true));
            float contentWidth = Mathf.Min(rowRect.width, 580f);
            Rect contentRect = new Rect(rowRect.x + (rowRect.width - contentWidth) * 0.5f, rowRect.y, contentWidth, rowRect.height);

            float t = Time.unscaledTime;
            DrawRoundedRect(
                new Rect(contentRect.x + 20f, contentRect.y + 24f, contentRect.width - 40f, contentRect.height - 30f),
                new Color(0.04f, 0.22f, 0.34f, 0.16f),
                Color.clear,
                44,
                0);
            DrawRoundedRect(
                new Rect(contentRect.x + 12f, contentRect.y + 8f, contentRect.width - 24f, contentRect.height - 18f),
                new Color(0.68f, 0.92f, 1f, 0.5f),
                new Color(1f, 1f, 1f, 0.58f),
                42,
                2);

            DrawCloud(new Rect(contentRect.x + 28f + Mathf.Sin(t * 0.7f) * 4f, contentRect.y + 22f, 100f, 38f), new Color(1f, 1f, 1f, 0.74f));
            DrawCloud(new Rect(contentRect.x + contentRect.width - 135f + Mathf.Cos(t * 0.6f) * 4f, contentRect.y + 30f, 112f, 42f), new Color(1f, 1f, 1f, 0.68f));
            DrawCloud(new Rect(contentRect.x + contentRect.width * 0.36f, contentRect.y + 18f + Mathf.Sin(t * 0.5f) * 3f, 92f, 34f), new Color(1f, 1f, 1f, 0.48f));

            DrawRoundedRect(
                new Rect(contentRect.x + 32f, contentRect.y + 98f, contentRect.width - 64f, 26f),
                new Color(0.42f, 0.86f, 0.42f, 0.8f),
                new Color(0.78f, 1f, 0.65f, 0.65f),
                18,
                1);
            DrawRoundedRect(
                new Rect(contentRect.x + 88f, contentRect.y + 108f, contentRect.width - 176f, 12f),
                new Color(1f, 0.86f, 0.48f, 0.68f),
                Color.clear,
                7,
                0);

            DrawCastle(new Rect(contentRect.x + contentRect.width - 172f, contentRect.y + 42f, 126f, 82f));
            DrawHouse(new Rect(contentRect.x + 36f, contentRect.y + 68f, 70f, 56f), new Color(0.98f, 0.57f, 0.38f, 1f));
            DrawHouse(new Rect(contentRect.x + 112f, contentRect.y + 78f, 58f, 46f), new Color(0.33f, 0.78f, 1f, 1f));
            DrawTree(new Rect(contentRect.x + 190f, contentRect.y + 70f, 42f, 58f), new Color(0.34f, 0.82f, 0.36f, 1f));
            DrawTree(new Rect(contentRect.x + contentRect.width - 232f, contentRect.y + 76f, 38f, 52f), new Color(0.22f, 0.74f, 0.44f, 1f));

            DrawFloatingMenuIcon(
                new Rect(contentRect.x + contentRect.width * 0.46f, contentRect.y + 52f, 45f, 45f),
                new Color(0.1f, 0.54f, 0.92f, 1f),
                MenuDecorationIcon.Bomb,
                -9f,
                0f);
            DrawSparkle(new Rect(contentRect.x + contentRect.width * 0.22f, contentRect.y + 36f + Mathf.Sin(t * 2.3f) * 3f, 18f, 18f), new Color(1f, 0.97f, 0.58f, 0.85f));
            DrawSparkle(new Rect(contentRect.x + contentRect.width * 0.72f, contentRect.y + 24f + Mathf.Cos(t * 1.9f) * 4f, 20f, 20f), new Color(1f, 0.55f, 0.78f, 0.8f));

            GUILayout.Space(4f);
        }

        /// <summary>
        /// Purpose: Performs map select decorations for this component.
        /// Inputs: `panelWidth`, `panelHeight`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panelWidth">Input value used by this method.</param>
        /// <param name="panelHeight">Input value used by this method.</param>
        public static void MapSelectDecorations(float panelWidth, float panelHeight)
        {
            EnsureStyles();
            float t = Time.unscaledTime;
            Rect rowRect = GUILayoutUtility.GetRect(520f, 50f, GUILayout.ExpandWidth(true));
            float contentWidth = Mathf.Min(rowRect.width, 640f);
            Rect contentRect = new Rect(rowRect.x + (rowRect.width - contentWidth) * 0.5f, rowRect.y, contentWidth, rowRect.height);

            DrawRoundedRect(
                new Rect(contentRect.x + 22f, contentRect.y + 21f + Mathf.Sin(t * 0.8f) * 1.5f, contentRect.width - 44f, 18f),
                new Color(0.45f, 0.88f, 0.42f, 0.38f),
                new Color(0.85f, 1f, 0.7f, 0.45f),
                14,
                1);
            DrawRoundedRect(
                new Rect(contentRect.x + 96f, contentRect.y + 27f + Mathf.Sin(t * 0.8f) * 1.5f, contentRect.width - 192f, 7f),
                new Color(1f, 0.86f, 0.48f, 0.58f),
                Color.clear,
                5,
                0);

            DrawCloud(new Rect(contentRect.x + 18f + Mathf.Sin(t * 0.6f) * 4f, contentRect.y + 5f, 72f, 24f), new Color(1f, 1f, 1f, 0.58f));
            DrawCloud(new Rect(contentRect.x + contentRect.width - 94f + Mathf.Cos(t * 0.55f) * 4f, contentRect.y + 8f, 78f, 25f), new Color(1f, 1f, 1f, 0.52f));
            DrawTree(new Rect(contentRect.x + 128f, contentRect.y + 14f + Mathf.Sin(t * 1.1f) * 2f, 25f, 32f), new Color(0.32f, 0.82f, 0.42f, 0.82f));
            DrawHouse(new Rect(contentRect.x + contentRect.width - 170f, contentRect.y + 15f + Mathf.Cos(t * 0.9f) * 2f, 36f, 29f), new Color(0.35f, 0.76f, 1f, 0.78f));
            DrawFloatingMenuIcon(
                new Rect(contentRect.x + contentRect.width * 0.5f - 12f, contentRect.y + 9f, 24f, 24f),
                new Color(1f, 0.62f, 0.22f, 0.82f),
                MenuDecorationIcon.Blocks,
                -6f,
                1.4f);
            DrawSparkle(new Rect(contentRect.x + contentRect.width * 0.25f, contentRect.y + 10f + Mathf.Sin(t * 2.2f) * 3f, 14f, 14f), new Color(1f, 0.96f, 0.45f, 0.72f));
            DrawSparkle(new Rect(contentRect.x + contentRect.width * 0.74f, contentRect.y + 12f + Mathf.Cos(t * 1.8f) * 3f, 15f, 15f), new Color(1f, 0.55f, 0.78f, 0.66f));
            GUILayout.Space(1f);
        }

        /// <summary>
        /// Purpose: Returns menu modal for the current state.
        /// Inputs: `title`, `stringlines`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="stringlines">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool MenuModal(string title, string[] lines)
        {
            EnsureStyles();
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.42f)));

            Rect rect = CenteredRect(520f, 310f);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Space(12f);
            GUILayout.Label(title, modalTitleStyle);
            GUILayout.Space(12f);
            foreach (string line in lines)
            {
                GUILayout.Label(line, modalBodyStyle);
                GUILayout.Space(8f);
            }

            GUILayout.FlexibleSpace();
            bool shouldClose = PrimaryButton("CLOSE");
            GUILayout.EndArea();
            return shouldClose;
        }

        /// <summary>
        /// Purpose: Draws the player settings modal and applies changes immediately.
        /// Inputs: audioManager provides live audio values and preview playback; falls back to AudioManager.Instance when null.
        /// Output: returns true when the caller should close the modal; otherwise false.
        /// </summary>
        /// <param name="audioManager">Audio manager used to read/apply audio settings and play previews.</param>
        /// <returns>True if the settings modal requested closing; otherwise false.</returns>
        public static bool SettingsModal(AudioManager audioManager)
        {
            EnsureStyles();
            EnsureSettingsTexturesLoaded();
            if (audioManager == null)
            {
                audioManager = AudioManager.Instance;
            }

            if (HasSettingsModalAssets())
            {
                return DrawImageSettingsModal(audioManager);
            }

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.42f)));

            Rect rect = CenteredRect(640f, 500f);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            DrawRoundedRect(rect, PanelFill, PanelBorder, 24, 4);

            GUI.Label(new Rect(rect.x + 40f, rect.y + 18f, rect.width - 80f, 42f), "Settings", modalTitleStyle);
            GUI.Label(new Rect(rect.x + 60f, rect.y + 58f, rect.width - 120f, 24f), "Tune sound, feedback, and saved preferences.", modalBodyStyle);

            bool shouldClose = false;
            Rect topCloseRect = new Rect(rect.x + rect.width - 52f, rect.y + 18f, 34f, 30f);
            if (CartoonButton(
                topCloseRect,
                "X",
                new Color(1f, 0.45f, 0.32f, 1f),
                new Color(1f, 0.58f, 0.42f, 1f),
                new Color(0.82f, 0.28f, 0.18f, 1f),
                Color.white))
            {
                shouldClose = true;
            }

            Rect feedbackRect = new Rect(rect.x + 55f, rect.y + 92f, rect.width - 110f, 34f);
            DrawSettingsFeedback(feedbackRect);

            Rect controlsRect = new Rect(rect.x + 55f, rect.y + 138f, rect.width - 110f, 238f);
            DrawSettingsControls(controlsRect, audioManager);

            Rect previewSlot = new Rect(rect.x + 95f, rect.y + rect.height - 82f, rect.width - 190f, 34f);
            DrawSettingsPreviewButtons(previewSlot, audioManager);

            Rect buttonSlot = new Rect(rect.x + 90f, rect.y + rect.height - 42f, rect.width - 180f, 34f);
            float resetWidth = Mathf.Min(250f, buttonSlot.width * 0.58f);
            float closeWidth = Mathf.Min(170f, buttonSlot.width - resetWidth - 18f);
            Rect resetRect = new Rect(buttonSlot.center.x - (resetWidth + closeWidth + 18f) * 0.5f, buttonSlot.y, resetWidth, buttonSlot.height);
            Rect closeRect = new Rect(resetRect.xMax + 18f, buttonSlot.y, closeWidth, buttonSlot.height);

            if (CartoonButton(
                resetRect,
                "RESTORE DEFAULTS",
                new Color(1f, 0.58f, 0.28f, 1f),
                new Color(1f, 0.72f, 0.38f, 1f),
                new Color(0.88f, 0.42f, 0.18f, 1f),
                Color.white))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.ResetToDefaults();
                audioManager?.ReloadFromGameSettings();
                ShowSettingsFeedback("Defaults restored and saved", new Color(1f, 0.62f, 0.22f, 1f));
            }

            if (CartoonButton(
                closeRect,
                "CLOSE",
                new Color(0.09f, 0.72f, 1f, 1f),
                new Color(0.2f, 0.86f, 1f, 1f),
                new Color(0.05f, 0.58f, 0.85f, 1f),
                Color.white))
            {
                shouldClose = true;
            }

            return shouldClose;
        }

        /// <summary>
        /// Purpose: Loads the illustrated settings panel and button textures from Resources once.
        /// Inputs: no direct parameters; uses Resources paths without file extensions.
        /// Output: no return value; caches Texture2D references for the image-based settings popup.
        /// </summary>
        private static void EnsureSettingsTexturesLoaded()
        {
            if (settingsTexturesLoaded)
            {
                return;
            }

            settingsModalTexture = Resources.Load<Texture2D>(SettingsModalResourcePath);
            settingsCloseButtonTexture = Resources.Load<Texture2D>(SettingsCloseButtonResourcePath);
            settingsMusicPreviewButtonTexture = Resources.Load<Texture2D>(SettingsMusicPreviewButtonResourcePath);
            settingsRestoreDefaultsButtonTexture = Resources.Load<Texture2D>(SettingsRestoreDefaultsButtonResourcePath);
            settingsSfxPreviewButtonTexture = Resources.Load<Texture2D>(SettingsSfxPreviewButtonResourcePath);

            ApplyCrispUiTextureSettings(settingsModalTexture);
            ApplyCrispUiTextureSettings(settingsCloseButtonTexture);
            ApplyCrispUiTextureSettings(settingsMusicPreviewButtonTexture);
            ApplyCrispUiTextureSettings(settingsRestoreDefaultsButtonTexture);
            ApplyCrispUiTextureSettings(settingsSfxPreviewButtonTexture);
            settingsTexturesLoaded = true;
        }

        /// <summary>
        /// Purpose: Forces imported UI art to draw with sharper sampling inside IMGUI.
        /// Inputs: texture is the loaded UI image that may otherwise use soft bilinear filtering.
        /// Output: no return value; updates runtime texture sampling settings only.
        /// </summary>
        /// <param name="texture">Loaded UI texture that should look crisp on screen.</param>
        private static void ApplyCrispUiTextureSettings(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            // Point filtering avoids Unity softening rasterized UI text when the IMGUI panel is scaled.
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Checks whether the illustrated settings popup can be drawn.
        /// Inputs: no direct parameters; reads cached Texture2D fields.
        /// Output: true when the full settings background is available, otherwise false.
        /// </summary>
        /// <returns>True if the settings background texture is loaded; otherwise false.</returns>
        private static bool HasSettingsModalAssets()
        {
            return settingsModalTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the illustrated settings popup while preserving all existing settings behavior.
        /// Inputs: audioManager provides live audio values and preview playback.
        /// Output: true when the player clicks the Close button, otherwise false.
        /// </summary>
        /// <param name="audioManager">Audio manager used to read/apply audio settings and play previews.</param>
        /// <returns>True when the settings popup should close; otherwise false.</returns>
        private static bool DrawImageSettingsModal(AudioManager audioManager)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.48f)));

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect modalBounds = new Rect(
                screenRect.x + 18f,
                screenRect.y + 10f,
                Mathf.Max(1f, screenRect.width - 36f),
                Mathf.Max(1f, screenRect.height - 20f));
            Rect settingsRect = CalculateAspectFitRect(settingsModalTexture, modalBounds);
            settingsRect = PixelSnapRect(settingsRect);

            GUI.DrawTexture(settingsRect, settingsModalTexture, ScaleMode.StretchToFill, false);

            Rect feedbackRect = GetNormalizedRect(settingsRect, 0.164f, 0.214f, 0.668f, 0.067f);
            DrawSettingsFeedback(feedbackRect);
            DrawImageSettingsControls(settingsRect, audioManager);

            Rect sfxPreviewRect = GetNormalizedRect(settingsRect, 0.175f, 0.748f, 0.305f, 0.074f);
            Rect musicPreviewRect = GetNormalizedRect(settingsRect, 0.520f, 0.748f, 0.305f, 0.074f);
            Rect restoreDefaultsRect = GetNormalizedRect(settingsRect, 0.175f, 0.846f, 0.305f, 0.074f);
            Rect closeRect = GetNormalizedRect(settingsRect, 0.520f, 0.846f, 0.305f, 0.074f);

            if (DrawSettingsImageButton(
                sfxPreviewRect,
                settingsSfxPreviewButtonTexture,
                "SFX PREVIEW",
                new Color(1f, 0.62f, 0.22f, 1f),
                0.35f))
            {
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback("Playing SFX preview", new Color(1f, 0.62f, 0.22f, 1f));
            }

            if (DrawSettingsImageButton(
                musicPreviewRect,
                settingsMusicPreviewButtonTexture,
                "MUSIC PREVIEW",
                new Color(0.42f, 0.88f, 0.38f, 1f),
                1.15f))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                audioManager?.PlayCurrentSceneBGMPreview();
                ShowSettingsFeedback("Playing music preview", new Color(0.42f, 0.88f, 0.38f, 1f));
            }

            if (DrawSettingsImageButton(
                restoreDefaultsRect,
                settingsRestoreDefaultsButtonTexture,
                "RESTORE DEFAULTS",
                new Color(1f, 0.62f, 0.22f, 1f),
                1.85f))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.ResetToDefaults();
                audioManager?.ReloadFromGameSettings();
                ShowSettingsFeedback("Defaults restored and saved", new Color(1f, 0.62f, 0.22f, 1f));
            }

            return DrawSettingsImageButton(
                closeRect,
                settingsCloseButtonTexture,
                "CLOSE",
                new Color(0.09f, 0.72f, 1f, 1f),
                2.55f);
        }

        /// <summary>
        /// Purpose: Draws interactive overlays on top of the illustrated settings rows.
        /// Inputs: settingsRect is the full popup rectangle and audioManager reads/applies audio values.
        /// Output: no return value; updates GameSettings and AudioManager when the player changes a control.
        /// </summary>
        /// <param name="settingsRect">Screen-space rectangle of the illustrated settings popup.</param>
        /// <param name="audioManager">Audio manager used to read and apply audio settings.</param>
        private static void DrawImageSettingsControls(Rect settingsRect, AudioManager audioManager)
        {
            Color blue = new Color(0.1f, 0.72f, 1f, 1f);
            Color green = new Color(0.42f, 0.88f, 0.38f, 1f);
            Color orange = new Color(1f, 0.62f, 0.22f, 1f);

            float masterVolume = audioManager != null ? audioManager.MasterVolume : GameSettings.MasterVolume;
            float bgmVolume = audioManager != null ? audioManager.BgmVolume : GameSettings.BgmVolume;
            float sfxVolume = audioManager != null ? audioManager.SfxVolume : GameSettings.SfxVolume;
            bool muteBGM = audioManager != null ? audioManager.MuteBGM : GameSettings.MuteBGM;
            bool muteSFX = audioManager != null ? audioManager.MuteSFX : GameSettings.MuteSFX;

            float newMasterVolume = DrawImageSettingsSlider(settingsRect, 0.300f, masterVolume, blue);
            if (!Mathf.Approximately(newMasterVolume, masterVolume))
            {
                audioManager?.SetMasterVolume(newMasterVolume);
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback($"Saved Master Volume {Mathf.RoundToInt(newMasterVolume * 100f)}%", blue);
            }

            float newBgmVolume = DrawImageSettingsSlider(settingsRect, 0.376f, bgmVolume, green);
            if (!Mathf.Approximately(newBgmVolume, bgmVolume))
            {
                audioManager?.SetBgmVolume(newBgmVolume);
                ShowSettingsFeedback($"Saved BGM Volume {Mathf.RoundToInt(newBgmVolume * 100f)}%", green);
            }

            float newSfxVolume = DrawImageSettingsSlider(settingsRect, 0.452f, sfxVolume, orange);
            if (!Mathf.Approximately(newSfxVolume, sfxVolume))
            {
                audioManager?.SetSfxVolume(newSfxVolume);
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback($"Saved SFX Volume {Mathf.RoundToInt(newSfxVolume * 100f)}%", orange);
            }

            bool newMuteBGM = DrawImageSettingsToggle(settingsRect, 0.527f, muteBGM, green);
            if (newMuteBGM != muteBGM)
            {
                audioManager?.SetBgmMuted(newMuteBGM);
                AudioManager.Instance?.PlayButtonClickSFX();
                ShowSettingsFeedback(newMuteBGM ? "BGM muted and saved" : "BGM unmuted and saved", green);
            }

            bool newMuteSFX = DrawImageSettingsToggle(settingsRect, 0.602f, muteSFX, orange);
            if (newMuteSFX != muteSFX)
            {
                audioManager?.SetSfxMuted(newMuteSFX);
                if (!newMuteSFX)
                {
                    AudioManager.Instance?.PlayButtonClickSFX();
                }

                ShowSettingsFeedback(newMuteSFX ? "SFX muted and saved" : "SFX unmuted and saved", orange);
            }

            bool shakeEnabled = GameSettings.ScreenShakeEnabled;
            bool newShakeEnabled = DrawImageSettingsToggle(settingsRect, 0.677f, shakeEnabled, blue);
            if (newShakeEnabled != shakeEnabled)
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.SetScreenShakeEnabled(newShakeEnabled);
                ShowSettingsFeedback(newShakeEnabled ? "Screen shake enabled" : "Screen shake disabled", blue);
            }
        }

        /// <summary>
        /// Purpose: Draws a clickable slider on top of the illustrated settings art.
        /// Inputs: settingsRect is the popup, rowYNormalized locates the row, value is 0-1, and accentColor themes the slider.
        /// Output: returns the updated 0-1 value after mouse input.
        /// </summary>
        /// <param name="settingsRect">Screen-space rectangle of the settings popup.</param>
        /// <param name="rowYNormalized">Normalized Y position of the source row inside the settings art.</param>
        /// <param name="value">Current setting value in the 0-1 range.</param>
        /// <param name="accentColor">Theme color for the slider fill and knob.</param>
        /// <returns>The clamped slider value after processing the current mouse event.</returns>
        private static float DrawImageSettingsSlider(Rect settingsRect, float rowYNormalized, float value, Color accentColor)
        {
            float resolvedValue = Mathf.Clamp01(value);
            Rect trackRect = GetNormalizedRect(settingsRect, 0.374f, rowYNormalized + 0.020f, 0.376f, 0.020f);
            Rect valueRect = GetNormalizedRect(settingsRect, 0.760f, rowYNormalized + 0.006f, 0.070f, 0.048f);

            // The background art already contains labels; this small overlay only updates the live track and percentage.
            Rect trackCoverRect = new Rect(
                trackRect.x - settingsRect.width * 0.006f,
                trackRect.y - settingsRect.height * 0.004f,
                trackRect.width + settingsRect.width * 0.012f,
                trackRect.height + settingsRect.height * 0.008f);
            DrawRoundedRect(trackCoverRect, new Color(1f, 0.95f, 0.76f, 0.92f), new Color(1f, 1f, 1f, 0.58f), 9, 1);
            DrawRoundedRect(trackRect, new Color(0.94f, 0.9f, 0.68f, 1f), new Color(1f, 1f, 1f, 0.65f), 7, 1);
            DrawRoundedRect(new Rect(trackRect.x, trackRect.y, trackRect.width * resolvedValue, trackRect.height), accentColor, Color.clear, 7, 0);

            float knobSize = Mathf.Max(20f, settingsRect.height * 0.038f);
            Rect knobRect = new Rect(
                trackRect.x + trackRect.width * resolvedValue - knobSize * 0.5f,
                trackRect.center.y - knobSize * 0.5f,
                knobSize,
                knobSize);
            DrawBubble(knobRect, Color.Lerp(accentColor, Color.white, 0.16f));

            DrawRoundedRect(valueRect, new Color(1f, 0.96f, 0.74f, 0.88f), Color.clear, 12, 0);
            GUI.Label(valueRect, $"{Mathf.RoundToInt(resolvedValue * 100f)}%", settingsValueStyle);

            Event currentEvent = Event.current;
            Rect hitRect = new Rect(
                trackRect.x - knobSize * 0.5f,
                trackRect.y - knobSize * 0.45f,
                trackRect.width + knobSize,
                trackRect.height + knobSize * 0.9f);
            if (currentEvent != null &&
                (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
                hitRect.Contains(currentEvent.mousePosition))
            {
                resolvedValue = Mathf.Clamp01(Mathf.InverseLerp(trackRect.x, trackRect.xMax, currentEvent.mousePosition.x));
                currentEvent.Use();
            }

            return resolvedValue;
        }

        /// <summary>
        /// Purpose: Draws an interactive ON/OFF switch on top of the illustrated settings art.
        /// Inputs: settingsRect is the popup, rowYNormalized locates the row, value is the current state, and accentColor themes ON.
        /// Output: returns the toggled value when the player clicks the row; otherwise returns the original value.
        /// </summary>
        /// <param name="settingsRect">Screen-space rectangle of the settings popup.</param>
        /// <param name="rowYNormalized">Normalized Y position of the source row inside the settings art.</param>
        /// <param name="value">Current toggle state.</param>
        /// <param name="accentColor">Theme color used when the toggle is enabled.</param>
        /// <returns>The updated toggle state after processing button input.</returns>
        private static bool DrawImageSettingsToggle(Rect settingsRect, float rowYNormalized, bool value, Color accentColor)
        {
            Rect hitRect = GetNormalizedRect(settingsRect, 0.137f, rowYNormalized, 0.724f, 0.061f);
            Rect toggleRect = GetNormalizedRect(settingsRect, 0.728f, rowYNormalized + 0.008f, 0.098f, 0.044f);

            Color fill = value ? accentColor : new Color(0.76f, 0.8f, 0.83f, 1f);
            DrawRoundedRect(toggleRect, fill, Color.white, 18, 2);

            float knobSize = toggleRect.height * 0.86f;
            Rect knobRect = value
                ? new Rect(toggleRect.xMax - knobSize - toggleRect.height * 0.07f, toggleRect.y + toggleRect.height * 0.07f, knobSize, knobSize)
                : new Rect(toggleRect.x + toggleRect.height * 0.07f, toggleRect.y + toggleRect.height * 0.07f, knobSize, knobSize);
            DrawBubble(knobRect, Color.white);
            GUI.Label(toggleRect, value ? "ON" : "OFF", settingsValueStyle);

            // The whole illustrated row is clickable so players do not need pixel-perfect switch clicks.
            if (GUI.Button(hitRect, GUIContent.none, invisibleButtonStyle))
            {
                return !value;
            }

            return value;
        }

        /// <summary>
        /// Purpose: Draws one of the imported settings button images with hover, press, and idle motion.
        /// Inputs: slot defines the clickable area, texture is the imported art, fallbackText is used if art is missing, and accentColor themes fallback art.
        /// Output: true when the player clicks the button; otherwise false.
        /// </summary>
        /// <param name="slot">Screen-space rectangle reserved for the button.</param>
        /// <param name="texture">Imported button image to draw.</param>
        /// <param name="fallbackText">Text drawn by the generated fallback button if texture is missing.</param>
        /// <param name="accentColor">Fallback button color.</param>
        /// <param name="phaseOffset">Animation phase offset so multiple buttons do not float identically.</param>
        /// <returns>True if the button was clicked; otherwise false.</returns>
        private static bool DrawSettingsImageButton(Rect slot, Texture2D texture, string fallbackText, Color accentColor, float phaseOffset)
        {
            if (texture == null)
            {
                return CartoonButton(
                    slot,
                    fallbackText,
                    accentColor,
                    Color.Lerp(accentColor, Color.white, 0.18f),
                    Color.Lerp(accentColor, Color.black, 0.16f),
                    Color.white);
            }

            Event currentEvent = Event.current;
            bool isHovered = currentEvent != null && slot.Contains(currentEvent.mousePosition);
            bool isPressed = isHovered &&
                (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
                currentEvent.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * SettingsImageButtonFloatSpeed + phaseOffset) *
                slot.height *
                SettingsImageButtonFloatAmount;

            Rect drawSlot = slot;
            drawSlot.y += idleOffset;
            Rect drawRect = CalculateAspectFitRect(texture, drawSlot);
            drawRect = ScaleRectAroundCenter(drawRect, isHovered ? 1.035f : 1f, isHovered ? 1.035f : 1f);
            drawRect = PixelSnapRect(drawRect);

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.97f, 0.97f);
                drawRect.y += slot.height * 0.02f;
                drawRect = PixelSnapRect(drawRect);
            }

            GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);
            return GUI.Button(slot, GUIContent.none, invisibleButtonStyle);
        }

        /// <summary>
        /// Purpose: Returns guide modal for the current state.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <returns>a `bool` value.</returns>
        public static bool GuideModal()
        {
            EnsureStyles();
            EnsureGuideTexturesLoaded();

            if (HasGuideModalAssets())
            {
                return DrawImageGuideModal();
            }

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.42f)));

            Rect rect = CenteredRect(560f, 340f);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Space(4f);
            GUILayout.Label("Guide", modalTitleStyle);
            GUILayout.Space(6f);

            Rect gridRect = GUILayoutUtility.GetRect(480f, 178f, GUILayout.ExpandWidth(true));
            DrawGuideCompactGrid(gridRect);

            GUILayout.Space(6f);
            Rect buttonSlot = GUILayoutUtility.GetRect(180f, 48f, GUILayout.ExpandWidth(true));
            Rect buttonRect = new Rect(buttonSlot.center.x - 110f, buttonSlot.y, 220f, buttonSlot.height);
            bool shouldClose = CartoonButton(
                buttonRect,
                "CLOSE",
                new Color(0.09f, 0.72f, 1f, 1f),
                new Color(0.2f, 0.86f, 1f, 1f),
                new Color(0.05f, 0.58f, 0.85f, 1f),
                Color.white);
            GUILayout.EndArea();
            return shouldClose;
        }

        /// <summary>
        /// Purpose: Loads the guide modal art from Resources the first time the guide popup is opened.
        /// Inputs: no direct parameters; uses Resources paths without file extensions.
        /// Output: no return value; caches Texture2D references for the image-based guide popup.
        /// </summary>
        private static void EnsureGuideTexturesLoaded()
        {
            if (guideTexturesLoaded)
            {
                return;
            }

            guideModalTexture = Resources.Load<Texture2D>(GuideModalResourcePath);
            guideCloseButtonTexture = Resources.Load<Texture2D>(GuideCloseButtonResourcePath);
            guideTexturesLoaded = true;
        }

        /// <summary>
        /// Purpose: Checks whether the new illustrated guide popup can be drawn.
        /// Inputs: no direct parameters; reads cached Texture2D fields.
        /// Output: true when the full guide background is available, otherwise false.
        /// </summary>
        /// <returns>True if the guide image asset is loaded; otherwise false.</returns>
        private static bool HasGuideModalAssets()
        {
            return guideModalTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the full illustrated guide popup and handles its close button hitbox.
        /// Inputs: no direct parameters; reads cached guide textures and current mouse state.
        /// Output: true when the player clicks Close, otherwise false.
        /// </summary>
        /// <returns>True when the guide should close; otherwise false.</returns>
        private static bool DrawImageGuideModal()
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.48f)));

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect modalBounds = new Rect(
                screenRect.x + 18f,
                screenRect.y + 14f,
                Mathf.Max(1f, screenRect.width - 36f),
                Mathf.Max(1f, screenRect.height - 28f));
            Rect guideRect = CalculateAspectFitRect(guideModalTexture, modalBounds);

            GUI.DrawTexture(guideRect, guideModalTexture, ScaleMode.StretchToFill, false);

            Rect closeSlot = GetNormalizedRect(guideRect, 0.314f, 0.81f, 0.372f, 0.126f);
            DrawGuideCloseButton(closeSlot);
            return GUI.Button(closeSlot, GUIContent.none, invisibleButtonStyle);
        }

        /// <summary>
        /// Purpose: Draws the animated close button art in the empty lower area of the guide panel.
        /// Inputs: closeSlot is the clickable area reserved for the Close action in GuideUI.png.
        /// Output: no return value; renders only visual feedback, while DrawImageGuideModal owns the click.
        /// </summary>
        /// <param name="closeSlot">Screen-space rectangle for the guide close button.</param>
        private static void DrawGuideCloseButton(Rect closeSlot)
        {
            if (guideCloseButtonTexture == null)
            {
                return;
            }

            bool isHovered = Event.current != null && closeSlot.Contains(Event.current.mousePosition);
            bool isPressed = isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * GuideCloseButtonFloatSpeed) * closeSlot.height * GuideCloseButtonFloatAmount;

            Rect drawRect = CalculateAspectFitRect(guideCloseButtonTexture, closeSlot);
            drawRect = ScaleRectAroundCenter(drawRect, isHovered ? 1.04f : 1f, isHovered ? 1.04f : 1f);
            drawRect.y += idleOffset;

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.97f, 0.97f);
                drawRect.y += closeSlot.height * 0.02f;
            }

            GUI.DrawTexture(drawRect, guideCloseButtonTexture, ScaleMode.ScaleToFit, true);
        }

        /// <summary>
        /// Purpose: Returns mode card for the current state.
        /// Inputs: `title`, `tag`, `description`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool ModeCard(string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 210f, GUILayout.ExpandWidth(true));
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

        /// <summary>
        /// Purpose: Returns mode card for the current state.
        /// Inputs: `rect`, `title`, `tag`, `description`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool ModeCard(Rect rect, string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

        /// <summary>
        /// Purpose: Returns compact mode card for the current state.
        /// Inputs: `title`, `tag`, `description`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool CompactModeCard(string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 176f, GUILayout.ExpandWidth(true));
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

        /// <summary>
        /// Purpose: Returns map card for the current state.
        /// Inputs: `title`, `tag`, `description`, `accentColor`, `groundColor`, `blockColor`, `pathColor`, `isSelected`, `previewPattern`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="groundColor">Input value used by this method.</param>
        /// <param name="blockColor">Input value used by this method.</param>
        /// <param name="pathColor">Input value used by this method.</param>
        /// <param name="isSelected">Input value used by this method.</param>
        /// <param name="previewPattern">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        public static bool MapCard(
            string title,
            string tag,
            string description,
            Color accentColor,
            Color groundColor,
            Color blockColor,
            Color pathColor,
            bool isSelected,
            MapPreviewPattern previewPattern = MapPreviewPattern.Balanced)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(190f, 170f, GUILayout.ExpandWidth(true));
            return DrawMapCard(rect, title, tag, description, accentColor, groundColor, blockColor, pathColor, isSelected, previewPattern, false);
        }

        /// <summary>
        /// Purpose: Returns a shorter map card for screens that need extra controls below the map choices.
        /// Inputs: same visual data as MapCard; isSelected controls the READY badge and highlight.
        /// Output: returns true only on the frame the card is clicked.
        /// </summary>
        /// <param name="title">Map display name.</param>
        /// <param name="tag">Short map style tag.</param>
        /// <param name="description">One-line map description.</param>
        /// <param name="accentColor">Theme color for borders and tag.</param>
        /// <param name="groundColor">Preview ground color.</param>
        /// <param name="blockColor">Preview block color.</param>
        /// <param name="pathColor">Preview path color.</param>
        /// <param name="isSelected">True when this map is currently selected.</param>
        /// <param name="previewPattern">Preview pattern used by the mini-map art.</param>
        /// <returns>True when the card is clicked; otherwise false.</returns>
        public static bool CompactMapCard(
            string title,
            string tag,
            string description,
            Color accentColor,
            Color groundColor,
            Color blockColor,
            Color pathColor,
            bool isSelected,
            MapPreviewPattern previewPattern = MapPreviewPattern.Balanced)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(190f, 134f, GUILayout.ExpandWidth(true));
            return DrawMapCard(rect, title, tag, description, accentColor, groundColor, blockColor, pathColor, isSelected, previewPattern, true);
        }

        /// <summary>
        /// Purpose: Performs flexible space for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Purpose: Returns cartoon button for the current state.
        /// Inputs: `rect`, `text`, `normalColor`, `hoverColor`, `activeColor`, `textColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="text">Input value used by this method.</param>
        /// <param name="normalColor">Input value used by this method.</param>
        /// <param name="hoverColor">Input value used by this method.</param>
        /// <param name="activeColor">Input value used by this method.</param>
        /// <param name="textColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool CartoonButton(Rect rect, string text, Color normalColor, Color hoverColor, Color activeColor, Color textColor)
        {
            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = ResolveInteractiveScale(rect, 1.026f, 0.965f);
            ApplyScaleAround(rect, scale);

            Rect shadowRect = new Rect(rect.x + 4f, rect.y + 6f, rect.width, rect.height);
            DrawRoundedRect(shadowRect, new Color(0.05f, 0.28f, 0.38f, 0.35f), new Color(0.05f, 0.28f, 0.38f, 0.35f), 18, 0);

            GUIStyle style = GetButtonStyle(normalColor, hoverColor, activeColor, textColor);
            bool clicked = GUI.Button(rect, text, style);
            GUI.matrix = previousMatrix;
            return clicked;
        }

        /// <summary>
        /// Purpose: Draws menu tile button in the current GUI or scene context.
        /// Inputs: `rect`, `text`, `icon`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="text">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool DrawMenuTileButton(Rect rect, string text, MenuButtonIcon icon, Color accentColor)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = ResolveInteractiveScale(rect, 1.025f, 0.965f);
            ApplyScaleAround(rect, scale);

            Event currentEvent = Event.current;
            bool isHovering = currentEvent != null && rect.Contains(currentEvent.mousePosition);
            bool isPressing = isHovering && (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag);
            Color fill = isPressing
                ? Color.Lerp(accentColor, new Color(0.05f, 0.18f, 0.28f, 1f), 0.2f)
                : isHovering ? Color.Lerp(accentColor, Color.white, 0.22f) : accentColor;
            Color border = isHovering ? Color.white : new Color(1f, 1f, 1f, 0.75f);

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 7f, rect.width, rect.height), PanelShadow, PanelShadow, 22, 0);
            DrawRoundedRect(rect, fill, border, 22, 3);
            DrawRoundedRect(new Rect(rect.x + 12f, rect.y + 9f, rect.width - 24f, 7f), new Color(1f, 1f, 1f, 0.42f), Color.clear, 5, 0);

            Rect iconRect = new Rect(rect.x + 20f, rect.y + 19f, 40f, 40f);
            DrawMenuButtonIcon(iconRect, icon);
            GUI.Label(new Rect(rect.x + 66f, rect.y + 20f, rect.width - 82f, rect.height - 36f), text, menuButtonTextStyle);

            bool clicked = GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
            GUI.matrix = previousMatrix;
            return clicked;
        }

        /// <summary>
        /// Purpose: Draws mode card in the current GUI or scene context.
        /// Inputs: `rect`, `title`, `tag`, `description`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool DrawModeCard(Rect rect, string title, string tag, string description, Color accentColor)
        {
            Event currentEvent = Event.current;
            bool isHovering = currentEvent != null && rect.Contains(currentEvent.mousePosition);
            Color cardFill = isHovering ? new Color(1f, 0.98f, 0.83f, 1f) : new Color(1f, 0.94f, 0.72f, 1f);
            Color border = Color.Lerp(accentColor, Color.white, 0.25f);
            Matrix4x4 previousMatrix = GUI.matrix;
            ApplyScaleAround(rect, ResolveInteractiveScale(rect, 1.018f, 0.972f));

            DrawRoundedRect(new Rect(rect.x + 4f, rect.y + 7f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, 3);
            DrawModeCardScene(rect, accentColor, tag);

            float iconSize = Mathf.Clamp(rect.height * 0.33f, 46f, 68f);
            float iconBob = Mathf.Sin(Time.unscaledTime * 2f + rect.x * 0.01f) * 4f;
            Rect iconRect = new Rect(rect.x + rect.width * 0.5f - iconSize * 0.5f, rect.y + rect.height * 0.18f + iconBob, iconSize, iconSize);
            DrawModeIcon(iconRect, tag, accentColor);

            GUI.Label(new Rect(rect.x + 14f, rect.y + rect.height * 0.58f, rect.width - 28f, 30f), title, cardTitleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + rect.height * 0.75f, rect.width - 40f, rect.height * 0.22f), description, cardBodyStyle);

            GUI.matrix = previousMatrix;
            return GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
        }

        /// <summary>
        /// Purpose: Draws map card in the current GUI or scene context.
        /// Inputs: `rect`, `title`, `tag`, `description`, `accentColor`, `groundColor`, `blockColor`, `pathColor`, `isSelected`, `previewPattern`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="description">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="groundColor">Input value used by this method.</param>
        /// <param name="blockColor">Input value used by this method.</param>
        /// <param name="pathColor">Input value used by this method.</param>
        /// <param name="isSelected">Input value used by this method.</param>
        /// <param name="previewPattern">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool DrawMapCard(
            Rect rect,
            string title,
            string tag,
            string description,
            Color accentColor,
            Color groundColor,
            Color blockColor,
            Color pathColor,
            bool isSelected,
            MapPreviewPattern previewPattern,
            bool compact)
        {
            bool isHovering = rect.Contains(Event.current.mousePosition);
            Color cardFill = isSelected
                ? new Color(1f, 0.98f, 0.8f, 1f)
                : isHovering ? new Color(1f, 0.96f, 0.78f, 1f) : new Color(1f, 0.92f, 0.66f, 1f);
            float selectedPulse = (Mathf.Sin(Time.unscaledTime * 3f + rect.x * 0.01f) + 1f) * 0.5f;
            Color border = isSelected
                ? Color.Lerp(accentColor, Color.white, 0.18f + selectedPulse * 0.16f)
                : Color.Lerp(accentColor, Color.white, 0.35f);
            int borderSize = isSelected ? 5 : 3;
            Matrix4x4 previousMatrix = GUI.matrix;
            ApplyScaleAround(rect, ResolveInteractiveScale(rect, 1.015f, 0.974f));

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, borderSize);

            float previewHeight = compact ? 38f : 60f;
            float previewY = compact ? 10f : 12f;
            float titleY = compact ? 51f : 76f;
            float tagY = compact ? 80f : 106f;
            float descriptionY = compact ? 108f : 135f;
            float descriptionHeight = compact ? 18f : 24f;
            Rect previewRect = new Rect(rect.x + 16f, rect.y + previewY, rect.width - 32f, previewHeight);
            DrawMapPreview(previewRect, groundColor, blockColor, pathColor, accentColor, previewPattern);

            if (isSelected)
            {
                float readyY = compact ? previewRect.y + 5f : previewRect.y + 6f;
                Rect readyRect = new Rect(previewRect.x + previewRect.width - 64f, readyY, 54f, 20f);
                DrawRoundedRect(readyRect, accentColor, Color.white, 11, 2);
                GUI.Label(readyRect, "READY", pillStyle);
            }

            GUI.Label(new Rect(rect.x + 16f, rect.y + titleY, rect.width - 32f, 25f), title, cardTitleStyle);
            float tagWidth = Mathf.Min(rect.width - 42f, Mathf.Max(92f, 58f + tag.Length * 8f));
            Rect tagRect = new Rect(rect.x + rect.width * 0.5f - tagWidth * 0.5f, rect.y + tagY, tagWidth, 23f);
            DrawRoundedRect(tagRect, accentColor, Color.white, 12, 2);
            GUI.Label(tagRect, tag, cardTagStyle);

            GUI.Label(new Rect(rect.x + 18f, rect.y + descriptionY, rect.width - 36f, descriptionHeight), description, cardBodyStyle);

            GUI.matrix = previousMatrix;
            return GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
        }

        /// <summary>
        /// Purpose: Resolves interactive scale from the current runtime state.
        /// Inputs: `rect`, `hoverScale`, `pressScale`; may also read serialized fields and current runtime state.
        /// Output: a `float` value.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="hoverScale">Input value used by this method.</param>
        /// <param name="pressScale">Input value used by this method.</param>
        /// <returns>a `float` value.</returns>
        private static float ResolveInteractiveScale(Rect rect, float hoverScale, float pressScale)
        {
            Event currentEvent = Event.current;
            bool isHovering = currentEvent != null && rect.Contains(currentEvent.mousePosition);
            if (!isHovering)
            {
                return 1f;
            }

            bool isPressing = currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag;
            if (isPressing)
            {
                return pressScale;
            }

            float pulse = Mathf.Sin(Time.unscaledTime * 11f) * 0.004f;
            return hoverScale + pulse;
        }

        /// <summary>
        /// Purpose: Applies scale around to the current object or scene.
        /// Inputs: `rect`, `scale`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="scale">Input value used by this method.</param>
        private static void ApplyScaleAround(Rect rect, float scale)
        {
            if (Mathf.Approximately(scale, 1f))
            {
                return;
            }

            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
        }

        /// <summary>
        /// Purpose: Fits a texture inside a target rectangle without changing the texture aspect ratio.
        /// Inputs: texture is the image to draw; targetRect is the largest allowed screen-space area.
        /// Output: a centered Rect that preserves the source texture proportions.
        /// </summary>
        /// <param name="texture">Texture whose width and height define the desired aspect ratio.</param>
        /// <param name="targetRect">Maximum screen-space rectangle available for drawing.</param>
        /// <returns>A centered rectangle that fits inside targetRect.</returns>
        private static Rect CalculateAspectFitRect(Texture2D texture, Rect targetRect)
        {
            if (texture == null || texture.height == 0 || targetRect.height <= 0f || targetRect.width <= 0f)
            {
                return targetRect;
            }

            float textureAspect = (float)texture.width / texture.height;
            float targetAspect = targetRect.width / targetRect.height;

            if (targetAspect > textureAspect)
            {
                float fittedWidth = targetRect.height * textureAspect;
                return new Rect(
                    targetRect.x + (targetRect.width - fittedWidth) * 0.5f,
                    targetRect.y,
                    fittedWidth,
                    targetRect.height);
            }

            float fittedHeight = targetRect.width / textureAspect;
            return new Rect(
                targetRect.x,
                targetRect.y + (targetRect.height - fittedHeight) * 0.5f,
                targetRect.width,
                fittedHeight);
        }

        /// <summary>
        /// Purpose: Converts normalized coordinates within a parent rectangle into screen-space pixels.
        /// Inputs: parent is the reference rectangle; x, y, width, and height are values from 0 to 1.
        /// Output: a Rect positioned and sized relative to parent.
        /// </summary>
        /// <param name="parent">Screen-space rectangle used as the coordinate reference.</param>
        /// <param name="x">Normalized horizontal position inside parent.</param>
        /// <param name="y">Normalized vertical position inside parent.</param>
        /// <param name="width">Normalized width inside parent.</param>
        /// <param name="height">Normalized height inside parent.</param>
        /// <returns>A screen-space rectangle based on the normalized input values.</returns>
        private static Rect GetNormalizedRect(Rect parent, float x, float y, float width, float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        /// <summary>
        /// Purpose: Aligns a rectangle to whole screen pixels before drawing imported UI art.
        /// Inputs: rect is the floating-point IMGUI rectangle produced by aspect fitting or animation.
        /// Output: a Rect with rounded position and size to reduce sub-pixel texture blur.
        /// </summary>
        /// <param name="rect">Source rectangle in screen-space pixels.</param>
        /// <returns>A pixel-aligned rectangle with the same approximate bounds.</returns>
        private static Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Round(rect.width),
                Mathf.Round(rect.height));
        }

        /// <summary>
        /// Purpose: Scales a rectangle around its center point without moving the center.
        /// Inputs: rect is the source rectangle; scaleX and scaleY are independent width/height multipliers.
        /// Output: a new Rect with the requested scale applied.
        /// </summary>
        /// <param name="rect">Source rectangle to scale.</param>
        /// <param name="scaleX">Width multiplier.</param>
        /// <param name="scaleY">Height multiplier.</param>
        /// <returns>A scaled rectangle that keeps the same center as rect.</returns>
        private static Rect ScaleRectAroundCenter(Rect rect, float scaleX, float scaleY)
        {
            float scaledWidth = rect.width * scaleX;
            float scaledHeight = rect.height * scaleY;
            return new Rect(
                rect.center.x - scaledWidth * 0.5f,
                rect.center.y - scaledHeight * 0.5f,
                scaledWidth,
                scaledHeight);
        }

        /// <summary>
        /// Purpose: Draws map preview in the current GUI or scene context.
        /// Inputs: `rect`, `groundColor`, `blockColor`, `pathColor`, `accentColor`, `previewPattern`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="groundColor">Input value used by this method.</param>
        /// <param name="blockColor">Input value used by this method.</param>
        /// <param name="pathColor">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="previewPattern">Input value used by this method.</param>
        private static void DrawMapPreview(Rect rect, Color groundColor, Color blockColor, Color pathColor, Color accentColor, MapPreviewPattern previewPattern)
        {
            DrawRoundedRect(rect, groundColor, Color.white, 14, 2);

            int columns = 5;
            int rows = 4;
            float cellGap = 5f;
            float cellWidth = (rect.width - 24f - cellGap * (columns - 1)) / columns;
            float cellHeight = (rect.height - 22f - cellGap * (rows - 1)) / rows;
            Vector2 start = new Vector2(rect.x + 12f, rect.y + 11f);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    bool blocked = IsPreviewBlockCell(x, y, columns, rows, previewPattern);
                    bool path = IsPreviewPathCell(x, y, columns, rows, previewPattern);
                    Color cellColor = blocked ? blockColor : path ? pathColor : groundColor;
                    Rect cellRect = new Rect(
                        start.x + x * (cellWidth + cellGap),
                        start.y + y * (cellHeight + cellGap),
                        cellWidth,
                        cellHeight);
                    DrawRoundedRect(cellRect, cellColor, new Color(1f, 1f, 1f, 0.5f), 7, 1);
                }
            }

            if (previewPattern == MapPreviewPattern.Maze)
            {
                Rect glowPath = new Rect(rect.x + 18f, rect.y + rect.height * 0.5f - 4f, rect.width - 36f, 8f);
                DrawRoundedRect(glowPath, new Color(accentColor.r, accentColor.g, accentColor.b, 0.45f), Color.clear, 5, 0);
            }

            DrawBubble(new Rect(rect.x + rect.width - 34f, rect.y + rect.height - 34f, 26f, 26f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.55f));
        }

        /// <summary>
        /// Purpose: Returns whether this object is preview block cell.
        /// Inputs: `x`, `y`, `columns`, `rows`, `previewPattern`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="x">Input value used by this method.</param>
        /// <param name="y">Input value used by this method.</param>
        /// <param name="columns">Input value used by this method.</param>
        /// <param name="rows">Input value used by this method.</param>
        /// <param name="previewPattern">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool IsPreviewBlockCell(int x, int y, int columns, int rows, MapPreviewPattern previewPattern)
        {
            bool edge = x == 0 || y == 0 || x == columns - 1 || y == rows - 1;
            switch (previewPattern)
            {
                case MapPreviewPattern.Open:
                    return edge && (x + y) % 2 == 0;
                case MapPreviewPattern.Maze:
                    bool doorway = (x == 1 && y == 0) || (x == 3 && y == rows - 1);
                    bool mazeDivider = (x == 2 && y == 1) || (x == 1 && y == 2) || (x == 3 && y == 2);
                    return (edge && !doorway) || mazeDivider;
                default:
                    return edge;
            }
        }

        /// <summary>
        /// Purpose: Returns whether this object is preview path cell.
        /// Inputs: `x`, `y`, `columns`, `rows`, `previewPattern`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="x">Input value used by this method.</param>
        /// <param name="y">Input value used by this method.</param>
        /// <param name="columns">Input value used by this method.</param>
        /// <param name="rows">Input value used by this method.</param>
        /// <param name="previewPattern">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private static bool IsPreviewPathCell(int x, int y, int columns, int rows, MapPreviewPattern previewPattern)
        {
            switch (previewPattern)
            {
                case MapPreviewPattern.Open:
                    return !IsPreviewBlockCell(x, y, columns, rows, previewPattern);
                case MapPreviewPattern.Maze:
                    return (x == 1 && y <= 1) || (x == 3 && y >= 2) || (x == 2 && y == 2);
                default:
                    return (x + y) % 2 == 0;
            }
        }

        /// <summary>
        /// Purpose: Draws feature pill in the current GUI or scene context.
        /// Inputs: `text`, `index`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="text">Input value used by this method.</param>
        /// <param name="index">Input value used by this method.</param>
        private static void DrawFeaturePill(string text, int index)
        {
            Color[] colors =
            {
                new Color(0.17f, 0.82f, 1f, 1f),
                new Color(1f, 0.74f, 0.22f, 1f),
                new Color(0.45f, 0.95f, 0.35f, 1f)
            };

            Rect rect = GUILayoutUtility.GetRect(100f, 36f, GUILayout.ExpandWidth(true));
            Color color = colors[index % colors.Length];
            DrawRoundedRect(rect, color, Color.white, 16, 2);
            GUI.Label(rect, text, pillStyle);
            GUILayout.Space(6f);
        }

        /// <summary>
        /// Purpose: Draws settings controls in the current GUI or scene context.
        /// Inputs: `rect`, `audioManager`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="audioManager">Input value used by this method.</param>
        private static void DrawSettingsControls(Rect rect, AudioManager audioManager)
        {
            float contentWidth = Mathf.Min(rect.width, 500f);
            Rect contentRect = new Rect(rect.x + (rect.width - contentWidth) * 0.5f, rect.y, contentWidth, rect.height);
            float rowHeight = 34f;
            float rowGap = 6f;
            Color blue = new Color(0.1f, 0.72f, 1f, 1f);
            Color green = new Color(0.42f, 0.88f, 0.38f, 1f);
            Color orange = new Color(1f, 0.62f, 0.22f, 1f);

            float masterVolume = audioManager != null ? audioManager.MasterVolume : GameSettings.MasterVolume;
            float bgmVolume = audioManager != null ? audioManager.BgmVolume : GameSettings.BgmVolume;
            float sfxVolume = audioManager != null ? audioManager.SfxVolume : GameSettings.SfxVolume;
            bool muteBGM = audioManager != null ? audioManager.MuteBGM : GameSettings.MuteBGM;
            bool muteSFX = audioManager != null ? audioManager.MuteSFX : GameSettings.MuteSFX;

            Rect row = new Rect(contentRect.x, contentRect.y, contentRect.width, rowHeight);
            float newMasterVolume = DrawSettingsSlider(row, "Master Volume", masterVolume, blue);
            if (!Mathf.Approximately(newMasterVolume, masterVolume))
            {
                audioManager?.SetMasterVolume(newMasterVolume);
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback($"Saved Master Volume {Mathf.RoundToInt(newMasterVolume * 100f)}%", blue);
            }

            row.y += rowHeight + rowGap;
            float newBgmVolume = DrawSettingsSlider(row, "BGM Volume", bgmVolume, green);
            if (!Mathf.Approximately(newBgmVolume, bgmVolume))
            {
                audioManager?.SetBgmVolume(newBgmVolume);
                ShowSettingsFeedback($"Saved BGM Volume {Mathf.RoundToInt(newBgmVolume * 100f)}%", green);
            }

            row.y += rowHeight + rowGap;
            float newSfxVolume = DrawSettingsSlider(row, "SFX Volume", sfxVolume, orange);
            if (!Mathf.Approximately(newSfxVolume, sfxVolume))
            {
                audioManager?.SetSfxVolume(newSfxVolume);
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback($"Saved SFX Volume {Mathf.RoundToInt(newSfxVolume * 100f)}%", orange);
            }

            row.y += rowHeight + rowGap + 4f;
            bool newMuteBGM = DrawSettingsToggle(row, "Mute BGM", muteBGM, green);
            if (newMuteBGM != muteBGM)
            {
                audioManager?.SetBgmMuted(newMuteBGM);
                AudioManager.Instance?.PlayButtonClickSFX();
                ShowSettingsFeedback(newMuteBGM ? "BGM muted and saved" : "BGM unmuted and saved", green);
            }

            row.y += rowHeight + rowGap;
            bool newMuteSFX = DrawSettingsToggle(row, "Mute SFX", muteSFX, orange);
            if (newMuteSFX != muteSFX)
            {
                audioManager?.SetSfxMuted(newMuteSFX);
                if (!newMuteSFX)
                {
                    AudioManager.Instance?.PlayButtonClickSFX();
                }

                ShowSettingsFeedback(newMuteSFX ? "SFX muted and saved" : "SFX unmuted and saved", orange);
            }

            row.y += rowHeight + rowGap;
            bool shakeEnabled = GameSettings.ScreenShakeEnabled;
            bool newShakeEnabled = DrawSettingsToggle(row, "Screen Shake", shakeEnabled, blue);
            if (newShakeEnabled != shakeEnabled)
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.SetScreenShakeEnabled(newShakeEnabled);
                ShowSettingsFeedback(newShakeEnabled ? "Screen shake enabled" : "Screen shake disabled", blue);
            }
        }

        /// <summary>
        /// Purpose: Draws the temporary settings save message near the top of the settings modal.
        /// Inputs: rect defines where the message should appear; reads the latest feedback message and timer.
        /// Output: no return value; renders a saved/default/preview status line for the player.
        /// </summary>
        /// <param name="rect">Screen-space rectangle used for the feedback message.</param>
        private static void DrawSettingsFeedback(Rect rect)
        {
            float remainingSeconds = settingsFeedbackVisibleUntil - Time.unscaledTime;
            bool hasFreshMessage = remainingSeconds > 0f && !string.IsNullOrEmpty(settingsFeedbackMessage);
            string message = hasFreshMessage
                ? settingsFeedbackMessage
                : "Changes are saved automatically on this device.";
            Color accentColor = hasFreshMessage
                ? settingsFeedbackColor
                : new Color(0.16f, 0.72f, 1f, 1f);
            float alpha = hasFreshMessage
                ? Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(remainingSeconds / SettingsFeedbackSeconds))
                : 0.72f;

            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            DrawRoundedRect(rect, new Color(1f, 0.99f, 0.84f, 0.86f), Color.Lerp(accentColor, Color.white, 0.22f), 12, 1);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 2f, rect.width - 24f, rect.height - 4f), message, settingsFeedbackStyle);
            GUI.color = previousColor;
        }

        /// <summary>
        /// Purpose: Stores a short settings feedback message shown in the modal.
        /// Inputs: message is the player-facing status text and accentColor controls the message border color.
        /// Output: no return value; updates static UI feedback state used by DrawSettingsFeedback.
        /// </summary>
        /// <param name="message">Player-facing message describing what just happened.</param>
        /// <param name="accentColor">Color used to visually connect the message to the changed setting.</param>
        private static void ShowSettingsFeedback(string message, Color accentColor)
        {
            settingsFeedbackMessage = string.IsNullOrEmpty(message) ? "Saved automatically" : message;
            settingsFeedbackColor = accentColor;
            settingsFeedbackVisibleUntil = Time.unscaledTime + SettingsFeedbackSeconds;
        }

        /// <summary>
        /// Purpose: Draws preview buttons so players can test current SFX and music settings immediately.
        /// Inputs: rect defines the preview button area and audioManager plays the requested preview sounds.
        /// Output: no return value; may trigger audio preview playback and a short feedback message.
        /// </summary>
        /// <param name="rect">Screen-space rectangle used to lay out the preview buttons.</param>
        /// <param name="audioManager">Audio manager used to play SFX and BGM preview clips.</param>
        private static void DrawSettingsPreviewButtons(Rect rect, AudioManager audioManager)
        {
            float contentWidth = Mathf.Min(rect.width, 420f);
            Rect contentRect = new Rect(rect.x + (rect.width - contentWidth) * 0.5f, rect.y, contentWidth, rect.height);
            float gap = 14f;
            float buttonWidth = (contentWidth - gap) * 0.5f;
            Rect sfxRect = new Rect(contentRect.x, contentRect.y, buttonWidth, contentRect.height);
            Rect bgmRect = new Rect(contentRect.x + buttonWidth + gap, contentRect.y, buttonWidth, contentRect.height);

            if (CartoonButton(
                sfxRect,
                "SFX PREVIEW",
                new Color(1f, 0.62f, 0.22f, 1f),
                new Color(1f, 0.72f, 0.38f, 1f),
                new Color(0.88f, 0.42f, 0.18f, 1f),
                Color.white))
            {
                audioManager?.PlaySettingsPreviewSFX();
                ShowSettingsFeedback("Playing SFX preview", new Color(1f, 0.62f, 0.22f, 1f));
            }

            if (CartoonButton(
                bgmRect,
                "MUSIC PREVIEW",
                new Color(0.42f, 0.88f, 0.38f, 1f),
                new Color(0.55f, 0.96f, 0.46f, 1f),
                new Color(0.3f, 0.68f, 0.24f, 1f),
                Color.white))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                audioManager?.PlayCurrentSceneBGMPreview();
                ShowSettingsFeedback("Playing music preview", new Color(0.42f, 0.88f, 0.38f, 1f));
            }
        }

        /// <summary>
        /// Purpose: Draws an interactive volume slider row.
        /// Inputs: rect is the row area, label is the row name, value is the current 0-1 setting, and accentColor themes the row.
        /// Output: returns the updated 0-1 value after mouse input; returns the original value when the player did not drag or click.
        /// </summary>
        /// <param name="rect">Screen-space rectangle for the entire slider row.</param>
        /// <param name="label">Player-facing name shown on the left side of the row.</param>
        /// <param name="value">Current slider value, expected in the 0-1 range.</param>
        /// <param name="accentColor">Color used for the filled slider track and row border.</param>
        /// <returns>The clamped slider value after handling current mouse input.</returns>
        private static float DrawSettingsSlider(Rect rect, string label, float value, Color accentColor)
        {
            float resolvedValue = Mathf.Clamp01(value);
            DrawSettingsRowBackground(rect, accentColor);

            Rect labelRect = new Rect(rect.x + 18f, rect.y + 5f, 150f, rect.height - 10f);
            Rect valueRect = new Rect(rect.x + rect.width - 72f, rect.y + 5f, 54f, rect.height - 10f);
            Rect trackRect = new Rect(labelRect.xMax + 8f, rect.y + rect.height * 0.5f - 6f, rect.width - 258f, 12f);

            GUI.Label(labelRect, label, settingsLabelStyle);
            GUI.Label(valueRect, $"{Mathf.RoundToInt(resolvedValue * 100f)}%", settingsValueStyle);

            DrawRoundedRect(trackRect, new Color(0.93f, 0.9f, 0.68f, 1f), new Color(1f, 1f, 1f, 0.62f), 6, 1);
            DrawRoundedRect(new Rect(trackRect.x, trackRect.y, trackRect.width * resolvedValue, trackRect.height), accentColor, Color.clear, 6, 0);

            float knobSize = 24f;
            Rect knobRect = new Rect(trackRect.x + trackRect.width * resolvedValue - knobSize * 0.5f, trackRect.center.y - knobSize * 0.5f, knobSize, knobSize);
            DrawBubble(knobRect, Color.Lerp(accentColor, Color.white, 0.18f));

            Event currentEvent = Event.current;
            Rect hitRect = new Rect(trackRect.x - knobSize * 0.5f, trackRect.y - 10f, trackRect.width + knobSize, trackRect.height + 20f);
            if (currentEvent != null &&
                (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
                hitRect.Contains(currentEvent.mousePosition))
            {
                resolvedValue = Mathf.Clamp01(Mathf.InverseLerp(trackRect.x, trackRect.xMax, currentEvent.mousePosition.x));
                currentEvent.Use();
            }

            return resolvedValue;
        }

        /// <summary>
        /// Purpose: Draws an interactive ON/OFF settings row.
        /// Inputs: rect is the row area, label is the row name, value is the current state, and accentColor themes the active state.
        /// Output: returns the toggled value when the player clicks the row; otherwise returns the original value.
        /// </summary>
        /// <param name="rect">Screen-space rectangle for the entire toggle row.</param>
        /// <param name="label">Player-facing name shown on the left side of the row.</param>
        /// <param name="value">Current toggle state.</param>
        /// <param name="accentColor">Color used when the toggle is enabled.</param>
        /// <returns>The new toggle state after processing the current GUI event.</returns>
        private static bool DrawSettingsToggle(Rect rect, string label, bool value, Color accentColor)
        {
            DrawSettingsRowBackground(rect, accentColor);

            Rect labelRect = new Rect(rect.x + 18f, rect.y + 5f, 220f, rect.height - 10f);
            Rect toggleRect = new Rect(rect.x + rect.width - 92f, rect.y + 5f, 74f, rect.height - 10f);

            GUI.Label(labelRect, label, settingsLabelStyle);

            Color fill = value ? accentColor : new Color(0.76f, 0.8f, 0.83f, 1f);
            DrawRoundedRect(toggleRect, fill, Color.white, 15, 2);
            Rect knobRect = value
                ? new Rect(toggleRect.xMax - 29f, toggleRect.y + 3f, 24f, 24f)
                : new Rect(toggleRect.x + 5f, toggleRect.y + 3f, 24f, 24f);
            DrawBubble(knobRect, Color.white);
            GUI.Label(toggleRect, value ? "ON" : "OFF", settingsValueStyle);

            // Make the full row clickable so the toggle feels forgiving on mouse and trackpad.
            if (GUI.Button(rect, GUIContent.none, invisibleButtonStyle))
            {
                return !value;
            }

            return value;
        }

        /// <summary>
        /// Purpose: Draws the rounded card background shared by slider and toggle rows.
        /// Inputs: rect defines the row bounds and accentColor defines the colored left stripe/border.
        /// Output: no return value; renders a hover-highlighted row background.
        /// </summary>
        /// <param name="rect">Screen-space rectangle for the row background.</param>
        /// <param name="accentColor">Theme color for the row accent stripe and hover border.</param>
        private static void DrawSettingsRowBackground(Rect rect, Color accentColor)
        {
            bool isHovered = Event.current != null && rect.Contains(Event.current.mousePosition);
            Color fillColor = isHovered
                ? new Color(1f, 0.99f, 0.82f, 0.98f)
                : new Color(1f, 0.96f, 0.76f, 0.96f);
            Color borderColor = isHovered
                ? Color.Lerp(accentColor, Color.white, 0.05f)
                : Color.Lerp(accentColor, Color.white, 0.2f);

            DrawRoundedRect(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), PanelShadow, PanelShadow, 14, 0);
            DrawRoundedRect(rect, fillColor, borderColor, 14, isHovered ? 3 : 2);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 7f, 5f, rect.height - 14f), accentColor, Color.clear, 4, 0);
        }

        /// <summary>
        /// Purpose: Draws guide compact grid in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawGuideCompactGrid(Rect rect)
        {
            float contentWidth = Mathf.Min(rect.width, 480f);
            Rect contentRect = new Rect(rect.x + (rect.width - contentWidth) * 0.5f, rect.y, contentWidth, rect.height);
            float gap = 10f;
            float cardWidth = (contentRect.width - gap) * 0.5f;
            float cardHeight = (contentRect.height - gap) * 0.5f;

            DrawGuideCompactCard(
                new Rect(contentRect.x, contentRect.y, cardWidth, cardHeight),
                "Player 1",
                "WASD  +  Space",
                "Grid movement and bombs.",
                new Color(0.1f, 0.72f, 1f, 1f),
                GuideRowIcon.PlayerOne);
            DrawGuideCompactCard(
                new Rect(contentRect.x + cardWidth + gap, contentRect.y, cardWidth, cardHeight),
                "Player 2",
                "Arrows  +  Enter / RCtrl",
                "Second local player.",
                new Color(1f, 0.48f, 0.3f, 1f),
                GuideRowIcon.PlayerTwo);
            DrawGuideCompactCard(
                new Rect(contentRect.x, contentRect.y + cardHeight + gap, cardWidth, cardHeight),
                "Bombs",
                "Cross blasts",
                "Walls stop. Blocks pop.",
                new Color(1f, 0.62f, 0.2f, 1f),
                GuideRowIcon.Bomb);
            DrawGuideCompactCard(
                new Rect(contentRect.x + cardWidth + gap, contentRect.y + cardHeight + gap, cardWidth, cardHeight),
                "Goal",
                "Clear blocks or beat rivals",
                "Grab items for upgrades.",
                new Color(0.42f, 0.88f, 0.38f, 1f),
                GuideRowIcon.Goal);
        }

        /// <summary>
        /// Purpose: Draws guide compact card in the current GUI or scene context.
        /// Inputs: `rect`, `title`, `keyText`, `detail`, `accentColor`, `icon`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="title">Input value used by this method.</param>
        /// <param name="keyText">Input value used by this method.</param>
        /// <param name="detail">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        private static void DrawGuideCompactCard(Rect rect, string title, string keyText, string detail, Color accentColor, GuideRowIcon icon)
        {
            DrawRoundedRect(new Rect(rect.x + 3f, rect.y + 5f, rect.width, rect.height), PanelShadow, PanelShadow, 16, 0);
            DrawRoundedRect(rect, new Color(1f, 0.96f, 0.76f, 0.96f), Color.Lerp(accentColor, Color.white, 0.18f), 16, 2);
            DrawRoundedRect(new Rect(rect.x + 9f, rect.y + 10f, 6f, rect.height - 20f), accentColor, Color.clear, 4, 0);

            Rect iconRect = new Rect(rect.x + 24f, rect.y + rect.height * 0.5f - 16f, 32f, 32f);
            DrawGuideRowIcon(iconRect, accentColor, icon);

            GUI.Label(new Rect(rect.x + 64f, rect.y + 7f, rect.width - 76f, 24f), title, guideTitleStyle);
            GUI.Label(new Rect(rect.x + 64f, rect.y + 34f, rect.width - 76f, 18f), keyText, guideTextStyle);
            GUI.Label(new Rect(rect.x + 64f, rect.y + 54f, rect.width - 76f, rect.height - 59f), detail, guideTextStyle);
        }

        /// <summary>
        /// Purpose: Draws guide row icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`, `icon`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        private static void DrawGuideRowIcon(Rect rect, Color accentColor, GuideRowIcon icon)
        {
            float phase = rect.x * 0.04f + rect.y * 0.03f;
            float bob = Mathf.Sin(Time.unscaledTime * 2.8f + phase) * 2.4f;
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 3.1f + phase) * 0.04f;
            Rect animatedRect = new Rect(rect.x, rect.y + bob, rect.width, rect.height);

            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), animatedRect.center);
            DrawBubble(
                new Rect(animatedRect.x - 3f, animatedRect.y + 4f, animatedRect.width + 6f, animatedRect.height + 6f),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.18f));

            switch (icon)
            {
                case GuideRowIcon.PlayerTwo:
                    DrawTinyCharacter(new Rect(animatedRect.x - 2f, animatedRect.y + 1f, animatedRect.width * 0.82f, animatedRect.height * 0.82f), accentColor);
                    DrawTinyCharacter(new Rect(animatedRect.x + animatedRect.width * 0.42f, animatedRect.y + animatedRect.height * 0.16f, animatedRect.width * 0.72f, animatedRect.height * 0.72f), new Color(0.1f, 0.72f, 1f, 1f));
                    break;
                case GuideRowIcon.Bomb:
                    GUIUtility.RotateAroundPivot(Mathf.Sin(Time.unscaledTime * 4f + phase) * 4f, animatedRect.center);
                    DrawGuideBombIcon(animatedRect, accentColor);
                    break;
                case GuideRowIcon.Goal:
                    DrawPowerUpIcon(animatedRect, accentColor);
                    DrawSparkle(new Rect(animatedRect.x + animatedRect.width * 0.58f, animatedRect.y - 1f, 12f, 12f), new Color(1f, 0.94f, 0.48f, 0.92f));
                    break;
                default:
                    DrawTinyCharacter(animatedRect, accentColor);
                    break;
            }

            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Draws mode card scene in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`, `tag`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        private static void DrawModeCardScene(Rect rect, Color accentColor, string tag)
        {
            Rect sky = new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height * 0.42f);
            DrawRoundedRect(sky, new Color(0.63f, 0.9f, 1f, 0.44f), new Color(1f, 1f, 1f, 0.45f), 15, 1);
            DrawCloud(new Rect(sky.x + sky.width * 0.08f, sky.y + 8f, 50f, 20f), new Color(1f, 1f, 1f, 0.5f));
            DrawCloud(new Rect(sky.x + sky.width * 0.68f, sky.y + 14f, 58f, 22f), new Color(1f, 1f, 1f, 0.44f));
            DrawRoundedRect(
                new Rect(rect.x + 16f, rect.y + rect.height * 0.48f, rect.width - 32f, 12f),
                Color.Lerp(accentColor, Color.white, 0.35f),
                Color.clear,
                8,
                0);

            if (tag == "AI")
            {
                DrawSparkle(new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.25f, 16f, 16f), new Color(1f, 0.96f, 0.48f, 0.8f));
                DrawSparkle(new Rect(rect.x + rect.width * 0.72f, rect.y + rect.height * 0.18f, 18f, 18f), new Color(0.65f, 1f, 0.9f, 0.75f));
                return;
            }

            DrawTree(new Rect(rect.x + 18f, rect.y + rect.height * 0.31f, 28f, 38f), new Color(0.35f, 0.84f, 0.44f, 0.92f));
            if (tag == "2P")
            {
                DrawTree(new Rect(rect.x + rect.width - 48f, rect.y + rect.height * 0.29f, 30f, 40f), new Color(0.28f, 0.75f, 0.5f, 0.92f));
            }
            else
            {
                DrawHouse(new Rect(rect.x + rect.width - 52f, rect.y + rect.height * 0.34f, 38f, 30f), accentColor);
            }
        }

        /// <summary>
        /// Purpose: Draws mode icon in the current GUI or scene context.
        /// Inputs: `rect`, `tag`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="tag">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawModeIcon(Rect rect, string tag, Color accentColor)
        {
            if (tag == "AI")
            {
                DrawAiModeIcon(rect, accentColor);
                return;
            }

            if (tag == "2P")
            {
                Rect left = new Rect(rect.x - rect.width * 0.2f, rect.y + rect.height * 0.05f, rect.width * 0.75f, rect.height * 0.75f);
                Rect right = new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.15f, rect.width * 0.75f, rect.height * 0.75f);
                DrawTinyCharacter(left, new Color(0.1f, 0.72f, 1f, 1f));
                DrawTinyCharacter(right, new Color(1f, 0.5f, 0.3f, 1f));
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.36f, rect.y + rect.height * 0.48f, rect.width * 0.28f, 5f), new Color(1f, 0.92f, 0.56f, 1f), Color.white, 3, 1);
                return;
            }

            DrawTinyCharacter(rect, accentColor);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.72f, rect.y + rect.height * 0.1f, 3f, rect.height * 0.32f), new Color(0.15f, 0.32f, 0.42f, 1f), Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.76f, rect.y + rect.height * 0.1f, rect.width * 0.22f, rect.height * 0.12f), new Color(1f, 0.56f, 0.72f, 1f), Color.white, 4, 1);
        }

        /// <summary>
        /// Purpose: Draws tiny character in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawTinyCharacter(Rect rect, Color accentColor)
        {
            DrawBubble(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.02f, rect.width * 0.68f, rect.height * 0.68f), new Color(1f, 0.9f, 0.7f, 1f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.28f, rect.y + rect.height * 0.58f, rect.width * 0.44f, rect.height * 0.32f), accentColor, Color.white, 10, 2);
            DrawBubble(new Rect(rect.x + rect.width * 0.3f, rect.y + rect.height * 0.22f, rect.width * 0.11f, rect.height * 0.11f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawBubble(new Rect(rect.x + rect.width * 0.59f, rect.y + rect.height * 0.22f, rect.width * 0.11f, rect.height * 0.11f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.39f, rect.y + rect.height * 0.41f, rect.width * 0.22f, 3f), new Color(0.1f, 0.25f, 0.32f, 0.75f), Color.clear, 2, 0);
        }

        /// <summary>
        /// Purpose: Draws ai mode icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawAiModeIcon(Rect rect, Color accentColor)
        {
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.18f, rect.width * 0.68f, rect.height * 0.55f), accentColor, Color.white, 16, 3);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.04f, rect.width * 0.1f, rect.height * 0.2f), new Color(0.15f, 0.32f, 0.42f, 1f), Color.clear, 3, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.42f, rect.y, rect.width * 0.16f, rect.width * 0.16f), new Color(1f, 0.95f, 0.48f, 1f));
            DrawBubble(new Rect(rect.x + rect.width * 0.3f, rect.y + rect.height * 0.34f, rect.width * 0.12f, rect.width * 0.12f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawBubble(new Rect(rect.x + rect.width * 0.58f, rect.y + rect.height * 0.34f, rect.width * 0.12f, rect.width * 0.12f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.36f, rect.y + rect.height * 0.56f, rect.width * 0.28f, 4f), new Color(0.1f, 0.25f, 0.32f, 0.78f), Color.clear, 2, 0);
        }

        /// <summary>
        /// Purpose: Draws menu button icon in the current GUI or scene context.
        /// Inputs: `rect`, `icon`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        private static void DrawMenuButtonIcon(Rect rect, MenuButtonIcon icon)
        {
            switch (icon)
            {
                case MenuButtonIcon.Guide:
                    DrawGuideIcon(rect);
                    break;
                case MenuButtonIcon.Settings:
                    DrawSettingsIcon(rect);
                    break;
                case MenuButtonIcon.Quit:
                    DrawQuitIcon(rect);
                    break;
                default:
                    DrawPlayIcon(rect);
                    break;
            }
        }

        /// <summary>
        /// Purpose: Draws play icon in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawPlayIcon(Rect rect)
        {
            Color iconColor = new Color(1f, 0.96f, 0.74f, 1f);
            DrawBubble(rect, new Color(1f, 1f, 1f, 0.28f));
            DrawRoundedRect(new Rect(rect.x + 9f, rect.y + 8f, 5f, 24f), iconColor, Color.white, 3, 1);
            DrawRoundedRect(new Rect(rect.x + 13f, rect.y + 8f, 18f, 6f), iconColor, Color.white, 3, 1);
            DrawRoundedRect(new Rect(rect.x + 22f, rect.y + 14f, 5f, 18f), iconColor, Color.white, 3, 1);
            DrawRotatedRoundedRect(new Rect(rect.x + 18f, rect.y + 19f, 24f, 5f), iconColor, 26f, 3);
            DrawRotatedRoundedRect(new Rect(rect.x + 18f, rect.y + 23f, 24f, 5f), iconColor, -26f, 3);
        }

        /// <summary>
        /// Purpose: Draws guide icon in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawGuideIcon(Rect rect)
        {
            Color page = new Color(1f, 0.98f, 0.78f, 1f);
            Color line = new Color(0.18f, 0.38f, 0.48f, 0.78f);
            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 6f, rect.width * 0.44f, rect.height - 12f), page, Color.white, 8, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.5f, rect.y + 6f, rect.width * 0.44f, rect.height - 12f), page, Color.white, 8, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.48f, rect.y + 8f, 3f, rect.height - 16f), line, Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + 11f, rect.y + 17f, 12f, 3f), line, Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + 11f, rect.y + 25f, 16f, 3f), line, Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + 27f, rect.y + 17f, 12f, 3f), line, Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + 27f, rect.y + 25f, 16f, 3f), line, Color.clear, 2, 0);
        }

        /// <summary>
        /// Purpose: Draws settings icon in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawSettingsIcon(Rect rect)
        {
            Color gear = new Color(1f, 0.96f, 0.74f, 1f);
            Vector2 center = rect.center;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                DrawRotatedRoundedRect(new Rect(center.x - 3f, center.y - 21f, 6f, 13f), gear, angle, 3, center);
            }

            DrawBubble(new Rect(rect.x + 7f, rect.y + 7f, rect.width - 14f, rect.height - 14f), gear);
            DrawBubble(new Rect(rect.x + 15f, rect.y + 15f, rect.width - 30f, rect.height - 30f), new Color(0.12f, 0.36f, 0.48f, 0.9f));
        }

        /// <summary>
        /// Purpose: Draws quit icon in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawQuitIcon(Rect rect)
        {
            Color iconColor = new Color(1f, 0.96f, 0.74f, 1f);
            DrawBubble(rect, new Color(1f, 1f, 1f, 0.2f));
            DrawRotatedRoundedRect(new Rect(rect.x + 7f, rect.y + 17f, rect.width - 14f, 8f), iconColor, 45f, 4);
            DrawRotatedRoundedRect(new Rect(rect.x + 7f, rect.y + 17f, rect.width - 14f, 8f), iconColor, -45f, 4);
        }

        /// <summary>
        /// Purpose: Draws castle in the current GUI or scene context.
        /// Inputs: `rect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        private static void DrawCastle(Rect rect)
        {
            Color wall = new Color(1f, 0.86f, 0.62f, 1f);
            Color roof = new Color(0.38f, 0.72f, 1f, 1f);
            Color trim = new Color(0.16f, 0.42f, 0.62f, 0.75f);

            DrawRoundedRect(new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.34f, rect.width * 0.56f, rect.height * 0.56f), wall, Color.white, 10, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.04f, rect.y + rect.height * 0.42f, rect.width * 0.22f, rect.height * 0.5f), wall, Color.white, 9, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.74f, rect.y + rect.height * 0.42f, rect.width * 0.22f, rect.height * 0.5f), wall, Color.white, 9, 2);
            DrawBubble(new Rect(rect.x + rect.width * 0.37f, rect.y + rect.height * 0.12f, rect.width * 0.26f, rect.width * 0.26f), wall);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.28f, rect.width * 0.14f, rect.height * 0.16f), roof, Color.white, 6, 1);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.76f, rect.y + rect.height * 0.28f, rect.width * 0.14f, rect.height * 0.16f), roof, Color.white, 6, 1);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.42f, rect.y + rect.height * 0.26f, rect.width * 0.16f, rect.height * 0.15f), roof, Color.white, 6, 1);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.63f, rect.width * 0.1f, rect.height * 0.27f), trim, Color.clear, 7, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.43f, rect.y + rect.height * 0.55f, rect.width * 0.14f, rect.width * 0.14f), trim);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.52f, rect.y + rect.height * 0.06f, 3f, rect.height * 0.22f), trim, Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.54f, rect.y + rect.height * 0.06f, 18f, 8f), new Color(1f, 0.52f, 0.66f, 1f), Color.white, 4, 1);
        }

        /// <summary>
        /// Purpose: Draws house in the current GUI or scene context.
        /// Inputs: `rect`, `roof`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="roof">Input value used by this method.</param>
        private static void DrawHouse(Rect rect, Color roof)
        {
            Color wall = new Color(1f, 0.95f, 0.72f, 1f);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.13f, rect.y + rect.height * 0.34f, rect.width * 0.74f, rect.height * 0.55f), wall, Color.white, 9, 2);
            DrawRotatedRoundedRect(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.22f, rect.width * 0.84f, rect.height * 0.18f), roof, -10f, 8);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.44f, rect.y + rect.height * 0.58f, rect.width * 0.16f, rect.height * 0.31f), new Color(0.18f, 0.42f, 0.55f, 0.74f), Color.clear, 6, 0);
        }

        /// <summary>
        /// Purpose: Draws tree in the current GUI or scene context.
        /// Inputs: `rect`, `leaves`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="leaves">Input value used by this method.</param>
        private static void DrawTree(Rect rect, Color leaves)
        {
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.43f, rect.y + rect.height * 0.48f, rect.width * 0.14f, rect.height * 0.42f), new Color(0.56f, 0.34f, 0.18f, 1f), Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.1f, rect.width * 0.58f, rect.width * 0.58f), leaves);
            DrawBubble(new Rect(rect.x + rect.width * 0.34f, rect.y, rect.width * 0.58f, rect.width * 0.58f), leaves);
            DrawBubble(new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.22f, rect.width * 0.62f, rect.width * 0.62f), Color.Lerp(leaves, Color.white, 0.12f));
        }

        /// <summary>
        /// Purpose: Draws cloud in the current GUI or scene context.
        /// Inputs: `rect`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private static void DrawCloud(Rect rect, Color color)
        {
            DrawBubble(new Rect(rect.x, rect.y + rect.height * 0.28f, rect.width * 0.36f, rect.height * 0.58f), color);
            DrawBubble(new Rect(rect.x + rect.width * 0.22f, rect.y, rect.width * 0.42f, rect.height * 0.76f), color);
            DrawBubble(new Rect(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.22f, rect.width * 0.42f, rect.height * 0.64f), color);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.12f, rect.y + rect.height * 0.48f, rect.width * 0.76f, rect.height * 0.28f), color, Color.clear, 10, 0);
        }

        /// <summary>
        /// Purpose: Draws floating menu icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`, `icon`, `baseRotation`, `phase`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        /// <param name="icon">Input value used by this method.</param>
        /// <param name="baseRotation">Input value used by this method.</param>
        /// <param name="phase">Input value used by this method.</param>
        private static void DrawFloatingMenuIcon(Rect rect, Color accentColor, MenuDecorationIcon icon, float baseRotation, float phase)
        {
            float bob = Mathf.Sin(Time.unscaledTime * 2.3f + phase) * 7f;
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 2f + phase) * 0.035f;
            Rect animatedRect = new Rect(rect.x, rect.y + bob, rect.width, rect.height);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(baseRotation + Mathf.Sin(Time.unscaledTime * 1.3f + phase) * 2.6f, animatedRect.center);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), animatedRect.center);

            DrawBubble(
                new Rect(animatedRect.x - 7f, animatedRect.y + 7f, animatedRect.width + 14f, animatedRect.height + 14f),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.22f));

            switch (icon)
            {
                case MenuDecorationIcon.Blocks:
                    DrawBlockIcon(animatedRect, accentColor);
                    break;
                case MenuDecorationIcon.PowerUp:
                    DrawPowerUpIcon(animatedRect, accentColor);
                    break;
                default:
                    DrawBombIcon(animatedRect, accentColor);
                    break;
            }

            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Draws bomb icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawBombIcon(Rect rect, Color accentColor)
        {
            DrawBubble(rect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.95f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.58f, rect.y + 4f, 16f, 12f), new Color(1f, 0.92f, 0.48f, 1f), Color.white, 5, 1);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.72f, rect.y - 4f, 24f, 7f), new Color(0.12f, 0.28f, 0.36f, 1f), Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.9f, rect.y - 11f, 15f, 15f), new Color(1f, 0.65f, 0.2f, 0.9f));
        }

        /// <summary>
        /// Purpose: Draws guide bomb icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawGuideBombIcon(Rect rect, Color accentColor)
        {
            Rect body = new Rect(rect.x + 2f, rect.y + 6f, rect.width * 0.72f, rect.height * 0.72f);
            DrawBubble(body, new Color(accentColor.r, accentColor.g, accentColor.b, 0.95f));
            DrawBubble(new Rect(body.x + body.width * 0.18f, body.y + body.height * 0.14f, body.width * 0.18f, body.height * 0.18f), new Color(1f, 1f, 1f, 0.4f));

            Rect cap = new Rect(body.x + body.width * 0.58f, body.y + body.height * 0.15f, rect.width * 0.26f, rect.height * 0.2f);
            DrawRoundedRect(cap, new Color(1f, 0.92f, 0.48f, 1f), Color.white, 4, 1);

            Rect fuse = new Rect(body.x + body.width * 0.72f, body.y - 1f, rect.width * 0.32f, 5f);
            DrawRotatedRoundedRect(fuse, new Color(0.12f, 0.28f, 0.36f, 1f), -8f, 3);

            DrawBubble(new Rect(body.x + body.width * 0.97f, body.y - 6f, 10f, 10f), new Color(1f, 0.66f, 0.18f, 0.9f));
            DrawSparkle(new Rect(body.x + body.width * 1.12f, body.y - 8f, 9f, 9f), new Color(1f, 0.96f, 0.45f, 0.8f));
        }

        /// <summary>
        /// Purpose: Draws block icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawBlockIcon(Rect rect, Color accentColor)
        {
            float blockSize = rect.width * 0.36f;
            Color secondColor = new Color(0.96f, 0.34f, 0.52f, 1f);
            Color thirdColor = new Color(0.25f, 0.76f, 1f, 1f);
            float bottomY = rect.y + rect.height * 0.58f;

            DrawRoundedRect(new Rect(rect.x + 4f, bottomY, blockSize, blockSize), accentColor, Color.white, 8, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width - blockSize - 4f, bottomY, blockSize, blockSize), secondColor, Color.white, 8, 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.5f - blockSize * 0.5f, rect.y + 4f, blockSize, blockSize), thirdColor, Color.white, 8, 2);
        }

        /// <summary>
        /// Purpose: Draws power up icon in the current GUI or scene context.
        /// Inputs: `rect`, `accentColor`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="accentColor">Input value used by this method.</param>
        private static void DrawPowerUpIcon(Rect rect, Color accentColor)
        {
            DrawBubble(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.82f));
            Rect vertical = new Rect(rect.x + rect.width * 0.5f - 7f, rect.y + 12f, 14f, rect.height - 24f);
            Rect horizontal = new Rect(rect.x + 12f, rect.y + rect.height * 0.5f - 7f, rect.width - 24f, 14f);
            DrawRoundedRect(vertical, Color.white, new Color(1f, 0.97f, 0.56f, 1f), 7, 2);
            DrawRoundedRect(horizontal, Color.white, new Color(1f, 0.97f, 0.56f, 1f), 7, 2);
            DrawBubble(new Rect(rect.x + rect.width - 18f, rect.y + 4f, 16f, 16f), new Color(1f, 0.48f, 0.72f, 0.86f));
        }

        /// <summary>
        /// Purpose: Draws sparkle in the current GUI or scene context.
        /// Inputs: `rect`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private static void DrawSparkle(Rect rect, Color color)
        {
            Rect vertical = new Rect(rect.x + rect.width * 0.5f - rect.width * 0.12f, rect.y, rect.width * 0.24f, rect.height);
            Rect horizontal = new Rect(rect.x, rect.y + rect.height * 0.5f - rect.height * 0.12f, rect.width, rect.height * 0.24f);
            DrawRoundedRect(vertical, color, Color.clear, 4, 0);
            DrawRoundedRect(horizontal, color, Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.25f, rect.y + rect.height * 0.25f, rect.width * 0.5f, rect.height * 0.5f), new Color(1f, 1f, 1f, color.a * 0.4f));
        }

        /// <summary>
        /// Purpose: Draws vertical gradient in the current GUI or scene context.
        /// Inputs: `rect`, `top`, `bottom`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="top">Input value used by this method.</param>
        /// <param name="bottom">Input value used by this method.</param>
        private static void DrawVerticalGradient(Rect rect, Color top, Color bottom)
        {
            const int steps = 40;
            float stepHeight = rect.height / steps;
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                Color color = Color.Lerp(top, bottom, t);
                GUI.DrawTexture(new Rect(rect.x, rect.y + stepHeight * i, rect.width, stepHeight + 1f), GetSolidTexture(color));
            }
        }

        /// <summary>
        /// Purpose: Draws bubble in the current GUI or scene context.
        /// Inputs: `rect`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private static void DrawBubble(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, GetCircleTexture(color));
            Rect highlight = new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.18f, rect.width * 0.26f, rect.height * 0.26f);
            GUI.DrawTexture(highlight, GetCircleTexture(new Color(1f, 1f, 1f, color.a * 0.55f)));
        }

        /// <summary>
        /// Purpose: Draws rounded rect in the current GUI or scene context.
        /// Inputs: `rect`, `fill`, `border`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="fill">Input value used by this method.</param>
        /// <param name="border">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        private static void DrawRoundedRect(Rect rect, Color fill, Color border, int radius, int borderSize)
        {
            GUI.DrawTexture(rect, GetRoundedTexture(fill, border, radius, borderSize));
        }

        /// <summary>
        /// Purpose: Draws rotated rounded rect in the current GUI or scene context.
        /// Inputs: `rect`, `fill`, `angle`, `radius`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="fill">Input value used by this method.</param>
        /// <param name="angle">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        private static void DrawRotatedRoundedRect(Rect rect, Color fill, float angle, int radius)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, rect.center);
            DrawRoundedRect(rect, fill, Color.clear, radius, 0);
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Draws rotated rounded rect in the current GUI or scene context.
        /// Inputs: `rect`, `fill`, `angle`, `radius`, `pivot`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="fill">Input value used by this method.</param>
        /// <param name="angle">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="pivot">Input value used by this method.</param>
        private static void DrawRotatedRoundedRect(Rect rect, Color fill, float angle, int radius, Vector2 pivot)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            DrawRoundedRect(rect, fill, Color.clear, radius, 0);
            GUI.matrix = previousMatrix;
        }

        /// <summary>
        /// Purpose: Ensures styles exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private static void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 50,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(titleStyle, TextPrimary);

            titleShadowStyle = new GUIStyle(titleStyle)
            {
                normal = { textColor = new Color(0.04f, 0.18f, 0.28f, 0.22f) }
            };
            LockTextColor(titleShadowStyle, new Color(0.04f, 0.18f, 0.28f, 0.22f));

            titleHighlightStyle = new GUIStyle(titleStyle)
            {
                normal = { textColor = new Color(1f, 1f, 0.88f, 0.42f) }
            };
            LockTextColor(titleHighlightStyle, new Color(1f, 1f, 0.88f, 0.42f));

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };
            LockTextColor(bodyStyle, TextSecondary);

            smallBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };
            LockTextColor(smallBodyStyle, TextSecondary);

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(32, 32, 28, 28),
                normal = { background = GetRoundedTexture(PanelFill, PanelBorder, 24, 4) }
            };

            pillStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = CreamText }
            };
            LockTextColor(pillStyle, CreamText);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(cardTitleStyle, TextPrimary);

            cardTagStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            LockTextColor(cardTagStyle, Color.white);

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };
            LockTextColor(cardBodyStyle, TextSecondary);

            menuButtonTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            LockTextColor(menuButtonTextStyle, Color.white);

            modalTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(modalTitleStyle, TextPrimary);

            modalBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };
            LockTextColor(modalBodyStyle, TextSecondary);

            guideTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(guideTitleStyle, TextPrimary);

            guideTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };
            LockTextColor(guideTextStyle, TextSecondary);

            settingsLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(settingsLabelStyle, TextPrimary);

            settingsValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(settingsValueStyle, TextPrimary);

            settingsFeedbackStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };
            LockTextColor(settingsFeedbackStyle, TextPrimary);

            invisibleButtonStyle = new GUIStyle(GUIStyle.none);
        }

        /// <summary>
        /// Purpose: Performs lock text color for this component.
        /// Inputs: `style`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="style">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private static void LockTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }

        /// <summary>
        /// Purpose: Gets button style.
        /// Inputs: `normalColor`, `hoverColor`, `activeColor`, `textColor`; may also read serialized fields and current runtime state.
        /// Output: a `GUIStyle` value.
        /// </summary>
        /// <param name="normalColor">Input value used by this method.</param>
        /// <param name="hoverColor">Input value used by this method.</param>
        /// <param name="activeColor">Input value used by this method.</param>
        /// <param name="textColor">Input value used by this method.</param>
        /// <returns>a `GUIStyle` value.</returns>
        private static GUIStyle GetButtonStyle(Color normalColor, Color hoverColor, Color activeColor, Color textColor)
        {
            string key = ColorKey(normalColor) + ColorKey(hoverColor) + ColorKey(activeColor) + ColorKey(textColor);
            if (ButtonStyleCache.TryGetValue(key, out GUIStyle cachedStyle))
            {
                return cachedStyle;
            }

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(18, 18, 18, 18),
                normal =
                {
                    textColor = textColor,
                    background = GetRoundedTexture(normalColor, Color.white, 18, 3)
                },
                hover =
                {
                    textColor = textColor,
                    background = GetRoundedTexture(hoverColor, Color.white, 18, 3)
                },
                active =
                {
                    textColor = textColor,
                    background = GetRoundedTexture(activeColor, Color.white, 18, 3)
                }
            };

            ButtonStyleCache[key] = style;
            return style;
        }

        /// <summary>
        /// Purpose: Gets solid texture.
        /// Inputs: `color`; may also read serialized fields and current runtime state.
        /// Output: a `Texture2D` value.
        /// </summary>
        /// <param name="color">Input value used by this method.</param>
        /// <returns>a `Texture2D` value.</returns>
        private static Texture2D GetSolidTexture(Color color)
        {
            string key = ColorKey(color);
            if (SolidTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            SolidTextureCache[key] = texture;
            return texture;
        }

        /// <summary>
        /// Purpose: Gets circle texture.
        /// Inputs: `color`; may also read serialized fields and current runtime state.
        /// Output: a `Texture2D` value.
        /// </summary>
        /// <param name="color">Input value used by this method.</param>
        /// <returns>a `Texture2D` value.</returns>
        private static Texture2D GetCircleTexture(Color color)
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
        private static Texture2D GetRoundedTexture(Color fill, Color border, int radius, int borderSize)
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
        private static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
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
        private static string ColorKey(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
