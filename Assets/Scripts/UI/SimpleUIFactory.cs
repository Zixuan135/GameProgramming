using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Lightweight IMGUI helper for MVP scene flow screens.
    /// This avoids package dependencies while the project is still using placeholder UI.
    /// </summary>
    public static class SimpleUIFactory
    {
        private static GUIStyle titleStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle panelStyle;

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

        public static void BeginPanel(Rect rect)
        {
            EnsureStyles();
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
            GUILayout.Space(16f);
        }

        public static void Body(string text)
        {
            EnsureStyles();
            GUILayout.Label(text, bodyStyle);
            GUILayout.Space(14f);
        }

        public static bool Button(string text)
        {
            EnsureStyles();
            GUILayout.Space(8f);
            return GUILayout.Button(text, buttonStyle, GUILayout.Height(54f));
        }

        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
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
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.98f, 0.92f, 0.68f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.96f, 1f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(28, 28, 24, 24)
            };
        }
    }
}
