using System.Collections.Generic;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Lightweight IMGUI helper for MVP scene flow screens.
    /// This keeps placeholder UI package-free while giving the menus a more colorful chibi style.
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
        private static GUIStyle invisibleButtonStyle;

        private enum MenuDecorationIcon
        {
            Bomb,
            Blocks,
            PowerUp
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
            Rect rect = GUILayoutUtility.GetRect(190f, 230f, GUILayout.ExpandWidth(true));
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
            bool isHovering = rect.Contains(Event.current.mousePosition);
            Color cardFill = isHovering ? new Color(1f, 0.98f, 0.83f, 1f) : new Color(1f, 0.94f, 0.72f, 1f);
            Color border = Color.Lerp(accentColor, Color.white, 0.25f);
            Matrix4x4 previousMatrix = GUI.matrix;
            ApplyScaleAround(rect, ResolveInteractiveScale(rect, 1.018f, 0.972f));

            DrawRoundedRect(new Rect(rect.x + 4f, rect.y + 7f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, 3);
            DrawRoundedRect(new Rect(rect.x + 14f, rect.y + 14f, rect.width - 28f, 12f), accentColor, accentColor, 6, 0);

            Rect iconRect = new Rect(rect.x + rect.width * 0.5f - 34f, rect.y + 38f, 68f, 68f);
            DrawBubble(iconRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.85f));
            GUI.Label(iconRect, tag, cardTagStyle);

            GUI.Label(new Rect(rect.x + 14f, rect.y + 112f, rect.width - 28f, 34f), title, cardTitleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 148f, rect.width - 40f, 52f), description, cardBodyStyle);

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
            Color border = isSelected ? accentColor : Color.Lerp(accentColor, Color.white, 0.35f);
            int borderSize = isSelected ? 5 : 3;
            Matrix4x4 previousMatrix = GUI.matrix;
            ApplyScaleAround(rect, ResolveInteractiveScale(rect, 1.015f, 0.974f));

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, borderSize);

            Rect previewRect = new Rect(rect.x + 16f, rect.y + 18f, rect.width - 32f, 96f);
            DrawMapPreview(previewRect, groundColor, blockColor, pathColor, accentColor, previewPattern);

            Rect selectedPill = new Rect(rect.x + rect.width - 86f, rect.y + 14f, 68f, 26f);
            if (isSelected)
            {
                DrawRoundedRect(selectedPill, accentColor, Color.white, 13, 2);
                GUI.Label(selectedPill, "READY", pillStyle);
            }

            GUI.Label(new Rect(rect.x + 16f, rect.y + 124f, rect.width - 32f, 30f), title, cardTitleStyle);
            Rect tagRect = new Rect(rect.x + rect.width * 0.5f - 42f, rect.y + 153f, 84f, 24f);
            DrawRoundedRect(tagRect, accentColor, Color.white, 12, 2);
            GUI.Label(tagRect, tag, cardTagStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 180f, rect.width - 36f, 42f), description, cardBodyStyle);

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
                fontSize = 20,
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
