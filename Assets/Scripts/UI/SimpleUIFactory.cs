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
        public enum MapPreviewPattern
        {
            Balanced,
            Open,
            Maze
        }

        public enum MenuButtonIcon
        {
            Play,
            Guide,
            Settings,
            Quit
        }

        private const int TextureSize = 64;

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
        private static GUIStyle invisibleButtonStyle;

        private enum MenuDecorationIcon
        {
            Bomb,
            Blocks,
            PowerUp
        }

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

        private static readonly Color PanelFill = new Color(1f, 0.96f, 0.72f, 0.96f);
        private static readonly Color PanelBorder = new Color(0.2f, 0.58f, 0.82f, 1f);
        private static readonly Color PanelShadow = new Color(0.04f, 0.22f, 0.34f, 0.32f);
        private static readonly Color TextPrimary = new Color(0.11f, 0.28f, 0.42f, 1f);
        private static readonly Color TextSecondary = new Color(0.23f, 0.45f, 0.55f, 1f);
        private static readonly Color CreamText = new Color(1f, 0.97f, 0.78f, 1f);

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

        public static void BeginPanel(Rect rect)
        {
            EnsureStyles();
            DrawRoundedRect(new Rect(rect.x - 6f, rect.y - 6f, rect.width + 12f, rect.height + 12f), new Color(0.35f, 0.9f, 1f, 0.14f), Color.clear, 30, 0);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            GUILayout.BeginArea(rect, panelStyle);
            GUILayout.Space(18f);
        }

        public static void EndPanel()
        {
            GUILayout.EndArea();
        }

        public static void Title(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(240f, 68f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y + 5f, rect.width, rect.height), text, titleShadowStyle);
            GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, rect.height), text, titleHighlightStyle);
            GUI.Label(rect, text, titleStyle);
            GUILayout.Space(6f);
        }

        public static void CompactTitle(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(240f, 54f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y + 4f, rect.width, rect.height), text, titleShadowStyle);
            GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, rect.height), text, titleHighlightStyle);
            GUI.Label(rect, text, titleStyle);
            GUILayout.Space(2f);
        }

        public static void Body(string text)
        {
            EnsureStyles();
            GUILayout.Label(text, bodyStyle);
            GUILayout.Space(12f);
        }

        public static void SmallBody(string text)
        {
            EnsureStyles();
            GUILayout.Label(text, smallBodyStyle);
            GUILayout.Space(8f);
        }

        public static bool Button(string text)
        {
            return PrimaryButton(text);
        }

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

        public static bool MenuTileButton(string text, MenuButtonIcon icon, Color accentColor)
        {
            EnsureStyles();
            GUILayout.Space(7f);
            Rect rect = GUILayoutUtility.GetRect(190f, 78f, GUILayout.ExpandWidth(true));
            bool clicked = DrawMenuTileButton(rect, text, icon, accentColor);
            GUILayout.Space(7f);
            return clicked;
        }

        public static void LabelPill(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 34f, GUILayout.ExpandWidth(true));
            Rect pillRect = new Rect(rect.x + rect.width * 0.18f, rect.y, rect.width * 0.64f, rect.height);
            DrawRoundedRect(pillRect, new Color(0.23f, 0.77f, 0.95f, 1f), new Color(1f, 1f, 1f, 0.75f), 18, 2);
            GUI.Label(pillRect, text, pillStyle);
            GUILayout.Space(6f);
        }

        public static void CompactLabelPill(string text)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 28f, GUILayout.ExpandWidth(true));
            Rect pillRect = new Rect(rect.x + rect.width * 0.22f, rect.y, rect.width * 0.56f, rect.height);
            DrawRoundedRect(pillRect, new Color(0.23f, 0.77f, 0.95f, 1f), new Color(1f, 1f, 1f, 0.75f), 16, 2);
            GUI.Label(pillRect, text, pillStyle);
            GUILayout.Space(3f);
        }

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

        public static bool SettingsModal(AudioManager audioManager)
        {
            EnsureStyles();
            if (audioManager == null)
            {
                audioManager = AudioManager.Instance;
            }

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), GetSolidTexture(new Color(0.02f, 0.09f, 0.14f, 0.42f)));

            Rect rect = CenteredRect(610f, 460f);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 24, 0);
            DrawRoundedRect(rect, PanelFill, PanelBorder, 24, 4);

            GUI.Label(new Rect(rect.x + 40f, rect.y + 18f, rect.width - 80f, 42f), "Settings", modalTitleStyle);
            GUI.Label(new Rect(rect.x + 60f, rect.y + 58f, rect.width - 120f, 24f), "Tune sound and battle feedback.", modalBodyStyle);

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

            Rect statusRect = new Rect(rect.x + 55f, rect.y + 92f, rect.width - 110f, 28f);
            DrawAudioStatusPill(statusRect, audioManager);

            Rect controlsRect = new Rect(rect.x + 55f, rect.y + 132f, rect.width - 110f, 238f);
            DrawSettingsControls(controlsRect, audioManager);

            Rect previewSlot = new Rect(rect.x + 95f, rect.y + rect.height - 86f, rect.width - 190f, 36f);
            DrawSettingsPreviewButtons(previewSlot, audioManager);

            Rect buttonSlot = new Rect(rect.x + 105f, rect.y + rect.height - 42f, rect.width - 210f, 36f);
            float buttonWidth = Mathf.Min(210f, (buttonSlot.width - 20f) * 0.5f);
            Rect resetRect = new Rect(buttonSlot.center.x - buttonWidth - 10f, buttonSlot.y, buttonWidth, buttonSlot.height);
            Rect closeRect = new Rect(buttonSlot.center.x + 10f, buttonSlot.y, buttonWidth, buttonSlot.height);

            if (CartoonButton(
                resetRect,
                "RESET",
                new Color(1f, 0.58f, 0.28f, 1f),
                new Color(1f, 0.72f, 0.38f, 1f),
                new Color(0.88f, 0.42f, 0.18f, 1f),
                Color.white))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.ResetToDefaults();
                audioManager?.ReloadFromGameSettings();
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

        public static bool GuideModal()
        {
            EnsureStyles();
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

        public static bool ModeCard(string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 210f, GUILayout.ExpandWidth(true));
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

        public static bool ModeCard(Rect rect, string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

        public static bool CompactModeCard(string title, string tag, string description, Color accentColor)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(180f, 176f, GUILayout.ExpandWidth(true));
            return DrawModeCard(rect, title, tag, description, accentColor);
        }

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
            return DrawMapCard(rect, title, tag, description, accentColor, groundColor, blockColor, pathColor, isSelected, previewPattern);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

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
            MapPreviewPattern previewPattern)
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

            Rect previewRect = new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 60f);
            DrawMapPreview(previewRect, groundColor, blockColor, pathColor, accentColor, previewPattern);

            if (isSelected)
            {
                Rect readyRect = new Rect(previewRect.x + previewRect.width - 64f, previewRect.y + 6f, 54f, 20f);
                DrawRoundedRect(readyRect, accentColor, Color.white, 11, 2);
                GUI.Label(readyRect, "READY", pillStyle);
            }

            GUI.Label(new Rect(rect.x + 16f, rect.y + 76f, rect.width - 32f, 28f), title, cardTitleStyle);
            float tagWidth = Mathf.Min(rect.width - 42f, Mathf.Max(92f, 58f + tag.Length * 8f));
            Rect tagRect = new Rect(rect.x + rect.width * 0.5f - tagWidth * 0.5f, rect.y + 106f, tagWidth, 25f);
            DrawRoundedRect(tagRect, accentColor, Color.white, 12, 2);
            GUI.Label(tagRect, tag, cardTagStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 135f, rect.width - 36f, 24f), description, cardBodyStyle);

            GUI.matrix = previousMatrix;
            return GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
        }

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

        private static void ApplyScaleAround(Rect rect, float scale)
        {
            if (Mathf.Approximately(scale, 1f))
            {
                return;
            }

            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
        }

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
            }

            row.y += rowHeight + rowGap;
            float newBgmVolume = DrawSettingsSlider(row, "BGM Volume", bgmVolume, green);
            if (!Mathf.Approximately(newBgmVolume, bgmVolume))
            {
                audioManager?.SetBgmVolume(newBgmVolume);
            }

            row.y += rowHeight + rowGap;
            float newSfxVolume = DrawSettingsSlider(row, "SFX Volume", sfxVolume, orange);
            if (!Mathf.Approximately(newSfxVolume, sfxVolume))
            {
                audioManager?.SetSfxVolume(newSfxVolume);
                audioManager?.PlaySettingsPreviewSFX();
            }

            row.y += rowHeight + rowGap + 4f;
            bool newMuteBGM = DrawSettingsToggle(row, "Mute BGM", muteBGM, green);
            if (newMuteBGM != muteBGM)
            {
                audioManager?.SetBgmMuted(newMuteBGM);
                AudioManager.Instance?.PlayButtonClickSFX();
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
            }

            row.y += rowHeight + rowGap;
            bool shakeEnabled = GameSettings.ScreenShakeEnabled;
            bool newShakeEnabled = DrawSettingsToggle(row, "Screen Shake", shakeEnabled, blue);
            if (newShakeEnabled != shakeEnabled)
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameSettings.SetScreenShakeEnabled(newShakeEnabled);
            }
        }

        private static void DrawAudioStatusPill(Rect rect, AudioManager audioManager)
        {
            float width = Mathf.Min(rect.width, 500f);
            Rect pillRect = new Rect(rect.x + (rect.width - width) * 0.5f, rect.y, width, rect.height);
            bool audioReady = audioManager != null && audioManager.IsAudioReady;
            Color accentColor = audioReady
                ? new Color(0.42f, 0.88f, 0.38f, 1f)
                : new Color(1f, 0.62f, 0.22f, 1f);
            string status = audioReady ? "Audio Ready" : "Audio Clips Missing";
            string volumeText = audioManager != null
                ? $"BGM {Mathf.RoundToInt(audioManager.BgmVolume * 100f)}%  |  SFX {Mathf.RoundToInt(audioManager.SfxVolume * 100f)}%"
                : "Audio manager loading";

            DrawRoundedRect(new Rect(pillRect.x + 3f, pillRect.y + 4f, pillRect.width, pillRect.height), PanelShadow, PanelShadow, 14, 0);
            DrawRoundedRect(pillRect, new Color(1f, 0.96f, 0.76f, 0.96f), Color.Lerp(accentColor, Color.white, 0.2f), 14, 2);
            DrawRoundedRect(new Rect(pillRect.x + 8f, pillRect.y + 7f, 5f, pillRect.height - 14f), accentColor, Color.clear, 4, 0);
            GUI.Label(new Rect(pillRect.x + 20f, pillRect.y + 4f, 170f, pillRect.height - 8f), status, settingsLabelStyle);
            GUI.Label(new Rect(pillRect.x + 190f, pillRect.y + 4f, pillRect.width - 208f, pillRect.height - 8f), volumeText, settingsValueStyle);
        }

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
            }
        }

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

            if (GUI.Button(toggleRect, GUIContent.none, invisibleButtonStyle))
            {
                return !value;
            }

            return value;
        }

        private static void DrawSettingsRowBackground(Rect rect, Color accentColor)
        {
            DrawRoundedRect(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), PanelShadow, PanelShadow, 14, 0);
            DrawRoundedRect(rect, new Color(1f, 0.96f, 0.76f, 0.96f), Color.Lerp(accentColor, Color.white, 0.2f), 14, 2);
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 7f, 5f, rect.height - 14f), accentColor, Color.clear, 4, 0);
        }

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

        private static void DrawTinyCharacter(Rect rect, Color accentColor)
        {
            DrawBubble(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.02f, rect.width * 0.68f, rect.height * 0.68f), new Color(1f, 0.9f, 0.7f, 1f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.28f, rect.y + rect.height * 0.58f, rect.width * 0.44f, rect.height * 0.32f), accentColor, Color.white, 10, 2);
            DrawBubble(new Rect(rect.x + rect.width * 0.3f, rect.y + rect.height * 0.22f, rect.width * 0.11f, rect.height * 0.11f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawBubble(new Rect(rect.x + rect.width * 0.59f, rect.y + rect.height * 0.22f, rect.width * 0.11f, rect.height * 0.11f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.39f, rect.y + rect.height * 0.41f, rect.width * 0.22f, 3f), new Color(0.1f, 0.25f, 0.32f, 0.75f), Color.clear, 2, 0);
        }

        private static void DrawAiModeIcon(Rect rect, Color accentColor)
        {
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.18f, rect.width * 0.68f, rect.height * 0.55f), accentColor, Color.white, 16, 3);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.04f, rect.width * 0.1f, rect.height * 0.2f), new Color(0.15f, 0.32f, 0.42f, 1f), Color.clear, 3, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.42f, rect.y, rect.width * 0.16f, rect.width * 0.16f), new Color(1f, 0.95f, 0.48f, 1f));
            DrawBubble(new Rect(rect.x + rect.width * 0.3f, rect.y + rect.height * 0.34f, rect.width * 0.12f, rect.width * 0.12f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawBubble(new Rect(rect.x + rect.width * 0.58f, rect.y + rect.height * 0.34f, rect.width * 0.12f, rect.width * 0.12f), new Color(0.1f, 0.25f, 0.32f, 0.88f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.36f, rect.y + rect.height * 0.56f, rect.width * 0.28f, 4f), new Color(0.1f, 0.25f, 0.32f, 0.78f), Color.clear, 2, 0);
        }

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

        private static void DrawQuitIcon(Rect rect)
        {
            Color iconColor = new Color(1f, 0.96f, 0.74f, 1f);
            DrawBubble(rect, new Color(1f, 1f, 1f, 0.2f));
            DrawRotatedRoundedRect(new Rect(rect.x + 7f, rect.y + 17f, rect.width - 14f, 8f), iconColor, 45f, 4);
            DrawRotatedRoundedRect(new Rect(rect.x + 7f, rect.y + 17f, rect.width - 14f, 8f), iconColor, -45f, 4);
        }

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

        private static void DrawHouse(Rect rect, Color roof)
        {
            Color wall = new Color(1f, 0.95f, 0.72f, 1f);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.13f, rect.y + rect.height * 0.34f, rect.width * 0.74f, rect.height * 0.55f), wall, Color.white, 9, 2);
            DrawRotatedRoundedRect(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.22f, rect.width * 0.84f, rect.height * 0.18f), roof, -10f, 8);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.44f, rect.y + rect.height * 0.58f, rect.width * 0.16f, rect.height * 0.31f), new Color(0.18f, 0.42f, 0.55f, 0.74f), Color.clear, 6, 0);
        }

        private static void DrawTree(Rect rect, Color leaves)
        {
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.43f, rect.y + rect.height * 0.48f, rect.width * 0.14f, rect.height * 0.42f), new Color(0.56f, 0.34f, 0.18f, 1f), Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.1f, rect.width * 0.58f, rect.width * 0.58f), leaves);
            DrawBubble(new Rect(rect.x + rect.width * 0.34f, rect.y, rect.width * 0.58f, rect.width * 0.58f), leaves);
            DrawBubble(new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.22f, rect.width * 0.62f, rect.width * 0.62f), Color.Lerp(leaves, Color.white, 0.12f));
        }

        private static void DrawCloud(Rect rect, Color color)
        {
            DrawBubble(new Rect(rect.x, rect.y + rect.height * 0.28f, rect.width * 0.36f, rect.height * 0.58f), color);
            DrawBubble(new Rect(rect.x + rect.width * 0.22f, rect.y, rect.width * 0.42f, rect.height * 0.76f), color);
            DrawBubble(new Rect(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.22f, rect.width * 0.42f, rect.height * 0.64f), color);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.12f, rect.y + rect.height * 0.48f, rect.width * 0.76f, rect.height * 0.28f), color, Color.clear, 10, 0);
        }

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

        private static void DrawBombIcon(Rect rect, Color accentColor)
        {
            DrawBubble(rect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.95f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.58f, rect.y + 4f, 16f, 12f), new Color(1f, 0.92f, 0.48f, 1f), Color.white, 5, 1);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.72f, rect.y - 4f, 24f, 7f), new Color(0.12f, 0.28f, 0.36f, 1f), Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.9f, rect.y - 11f, 15f, 15f), new Color(1f, 0.65f, 0.2f, 0.9f));
        }

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

        private static void DrawPowerUpIcon(Rect rect, Color accentColor)
        {
            DrawBubble(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.82f));
            Rect vertical = new Rect(rect.x + rect.width * 0.5f - 7f, rect.y + 12f, 14f, rect.height - 24f);
            Rect horizontal = new Rect(rect.x + 12f, rect.y + rect.height * 0.5f - 7f, rect.width - 24f, 14f);
            DrawRoundedRect(vertical, Color.white, new Color(1f, 0.97f, 0.56f, 1f), 7, 2);
            DrawRoundedRect(horizontal, Color.white, new Color(1f, 0.97f, 0.56f, 1f), 7, 2);
            DrawBubble(new Rect(rect.x + rect.width - 18f, rect.y + 4f, 16f, 16f), new Color(1f, 0.48f, 0.72f, 0.86f));
        }

        private static void DrawSparkle(Rect rect, Color color)
        {
            Rect vertical = new Rect(rect.x + rect.width * 0.5f - rect.width * 0.12f, rect.y, rect.width * 0.24f, rect.height);
            Rect horizontal = new Rect(rect.x, rect.y + rect.height * 0.5f - rect.height * 0.12f, rect.width, rect.height * 0.24f);
            DrawRoundedRect(vertical, color, Color.clear, 4, 0);
            DrawRoundedRect(horizontal, color, Color.clear, 4, 0);
            DrawBubble(new Rect(rect.x + rect.width * 0.25f, rect.y + rect.height * 0.25f, rect.width * 0.5f, rect.height * 0.5f), new Color(1f, 1f, 1f, color.a * 0.4f));
        }

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

        private static void DrawBubble(Rect rect, Color color)
        {
            GUI.DrawTexture(rect, GetCircleTexture(color));
            Rect highlight = new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.18f, rect.width * 0.26f, rect.height * 0.26f);
            GUI.DrawTexture(highlight, GetCircleTexture(new Color(1f, 1f, 1f, color.a * 0.55f)));
        }

        private static void DrawRoundedRect(Rect rect, Color fill, Color border, int radius, int borderSize)
        {
            GUI.DrawTexture(rect, GetRoundedTexture(fill, border, radius, borderSize));
        }

        private static void DrawRotatedRoundedRect(Rect rect, Color fill, float angle, int radius)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, rect.center);
            DrawRoundedRect(rect, fill, Color.clear, radius, 0);
            GUI.matrix = previousMatrix;
        }

        private static void DrawRotatedRoundedRect(Rect rect, Color fill, float angle, int radius, Vector2 pivot)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            DrawRoundedRect(rect, fill, Color.clear, radius, 0);
            GUI.matrix = previousMatrix;
        }

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

            invisibleButtonStyle = new GUIStyle(GUIStyle.none);
        }

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

        private static string ColorKey(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
