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
        private const int TextureSize = 64;

        private static GUIStyle titleStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle smallBodyStyle;
        private static GUIStyle panelStyle;
        private static GUIStyle pillStyle;
        private static GUIStyle cardTitleStyle;
        private static GUIStyle cardTagStyle;
        private static GUIStyle cardBodyStyle;
        private static GUIStyle invisibleButtonStyle;

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
            DrawVerticalGradient(
                new Rect(0f, 0f, Screen.width, Screen.height),
                new Color(0.45f, 0.86f, 1f, 1f),
                new Color(0.98f, 0.73f, 0.52f, 1f));

            DrawBubble(new Rect(Screen.width * 0.08f, Screen.height * 0.12f, 96f, 96f), new Color(1f, 1f, 1f, 0.22f));
            DrawBubble(new Rect(Screen.width * 0.78f, Screen.height * 0.08f, 132f, 132f), new Color(0.45f, 1f, 0.9f, 0.22f));
            DrawBubble(new Rect(Screen.width * 0.12f, Screen.height * 0.72f, 150f, 150f), new Color(1f, 0.95f, 0.45f, 0.2f));
            DrawBubble(new Rect(Screen.width * 0.72f, Screen.height * 0.68f, 110f, 110f), new Color(1f, 0.42f, 0.68f, 0.18f));
            DrawBubble(new Rect(Screen.width * 0.5f - 42f, Screen.height * 0.1f, 84f, 84f), new Color(1f, 1f, 1f, 0.18f));
        }

        public static void BeginPanel(Rect rect)
        {
            EnsureStyles();
            DrawRoundedRect(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), PanelShadow, PanelShadow, 22, 0);
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
            GUILayout.Label(text, titleStyle);
            GUILayout.Space(10f);
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
            bool isSelected)
        {
            EnsureStyles();
            Rect rect = GUILayoutUtility.GetRect(190f, 230f, GUILayout.ExpandWidth(true));
            return DrawMapCard(rect, title, tag, description, accentColor, groundColor, blockColor, pathColor, isSelected);
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        private static bool CartoonButton(Rect rect, string text, Color normalColor, Color hoverColor, Color activeColor, Color textColor)
        {
            EnsureStyles();
            Rect shadowRect = new Rect(rect.x + 4f, rect.y + 6f, rect.width, rect.height);
            DrawRoundedRect(shadowRect, new Color(0.05f, 0.28f, 0.38f, 0.35f), new Color(0.05f, 0.28f, 0.38f, 0.35f), 18, 0);

            GUIStyle style = GetButtonStyle(normalColor, hoverColor, activeColor, textColor);
            return GUI.Button(rect, text, style);
        }

        private static bool DrawModeCard(Rect rect, string title, string tag, string description, Color accentColor)
        {
            bool isHovering = rect.Contains(Event.current.mousePosition);
            Color cardFill = isHovering ? new Color(1f, 0.98f, 0.83f, 1f) : new Color(1f, 0.94f, 0.72f, 1f);
            Color border = Color.Lerp(accentColor, Color.white, 0.25f);

            DrawRoundedRect(new Rect(rect.x + 4f, rect.y + 7f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, 3);
            DrawRoundedRect(new Rect(rect.x + 14f, rect.y + 14f, rect.width - 28f, 12f), accentColor, accentColor, 6, 0);

            Rect iconRect = new Rect(rect.x + rect.width * 0.5f - 34f, rect.y + 38f, 68f, 68f);
            DrawBubble(iconRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.85f));
            GUI.Label(iconRect, tag, cardTagStyle);

            GUI.Label(new Rect(rect.x + 14f, rect.y + 112f, rect.width - 28f, 34f), title, cardTitleStyle);
            GUI.Label(new Rect(rect.x + 20f, rect.y + 148f, rect.width - 40f, 52f), description, cardBodyStyle);

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
            bool isSelected)
        {
            bool isHovering = rect.Contains(Event.current.mousePosition);
            Color cardFill = isSelected
                ? new Color(1f, 0.98f, 0.8f, 1f)
                : isHovering ? new Color(1f, 0.96f, 0.78f, 1f) : new Color(1f, 0.92f, 0.66f, 1f);
            Color border = isSelected ? accentColor : Color.Lerp(accentColor, Color.white, 0.35f);
            int borderSize = isSelected ? 5 : 3;

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), PanelShadow, PanelShadow, 20, 0);
            DrawRoundedRect(rect, cardFill, border, 20, borderSize);

            Rect previewRect = new Rect(rect.x + 16f, rect.y + 18f, rect.width - 32f, 96f);
            DrawMapPreview(previewRect, groundColor, blockColor, pathColor, accentColor);

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

            return GUI.Button(rect, GUIContent.none, invisibleButtonStyle);
        }

        private static void DrawMapPreview(Rect rect, Color groundColor, Color blockColor, Color pathColor, Color accentColor)
        {
            DrawRoundedRect(rect, groundColor, Color.white, 14, 2);

            int columns = 5;
            int rows = 3;
            float cellGap = 5f;
            float cellWidth = (rect.width - 24f - cellGap * (columns - 1)) / columns;
            float cellHeight = (rect.height - 22f - cellGap * (rows - 1)) / rows;
            Vector2 start = new Vector2(rect.x + 12f, rect.y + 11f);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    bool edge = x == 0 || y == 0 || x == columns - 1 || y == rows - 1;
                    bool checker = (x + y) % 2 == 0;
                    Color cellColor = edge ? blockColor : checker ? pathColor : groundColor;
                    Rect cellRect = new Rect(
                        start.x + x * (cellWidth + cellGap),
                        start.y + y * (cellHeight + cellGap),
                        cellWidth,
                        cellHeight);
                    DrawRoundedRect(cellRect, cellColor, new Color(1f, 1f, 1f, 0.5f), 7, 1);
                }
            }

            DrawBubble(new Rect(rect.x + rect.width - 34f, rect.y + rect.height - 34f, 26f, 26f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.55f));
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

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };

            smallBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };

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

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = TextPrimary }
            };

            cardTagStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = TextSecondary }
            };

            invisibleButtonStyle = new GUIStyle(GUIStyle.none);
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
