using System.Collections.Generic;
using BubbleTown.Characters;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Lightweight character selection screen between mode select and map select.
    /// It supports one selected character for Solo/AIBattle and separate P1/P2
    /// selections for LocalVS.
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour
    {
        private const int Player1Slot = 1;
        private const int Player2Slot = 2;
        private const int TextureSize = 64;

        private CharacterData[] roster = new CharacterData[0];
        private CharacterData selectedPlayer1;
        private CharacterData selectedPlayer2;
        private int activeSlot = Player1Slot;

        private GUIStyle titleStyle;
        private GUIStyle titleShadowStyle;
        private GUIStyle hintStyle;
        private GUIStyle pillStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardBodyStyle;
        private GUIStyle slotStyle;
        private GUIStyle smallPillStyle;
        private GUIStyle lockedLabelStyle;
        private Texture2D whiteTexture;

        private readonly Dictionary<string, Texture2D> roundedTextureCache = new Dictionary<string, Texture2D>();

        private void OnEnable()
        {
            LoadRosterAndSelections();
        }

        private void OnGUI()
        {
            EnsureStyles();
            LoadRosterAndSelections();

            GameMode mode = GameManager.Instance != null
                ? GameManager.Instance.CurrentGameMode
                : GameMode.SinglePlayer;

            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(980f, 600f);
            DrawPanelFrame(panel);
            DrawHeader(panel, mode);

            Rect slotRect = new Rect(panel.x + 42f, panel.y + 164f, panel.width - 84f, 54f);
            DrawSlotSelector(slotRect, mode);

            Rect rosterRect = new Rect(panel.x + 42f, panel.y + 236f, panel.width - 84f, panel.height - 338f);
            DrawRoster(rosterRect, mode);

            DrawBottomButtons(panel);
        }

        private void LoadRosterAndSelections()
        {
            roster = CharacterRoster.Characters;

            GameManager gameManager = GameManager.Instance;
            selectedPlayer1 = gameManager != null
                ? gameManager.SelectedPlayer1Character
                : CharacterRoster.GetDefaultCharacter(0);
            selectedPlayer2 = gameManager != null
                ? gameManager.SelectedPlayer2Character
                : CharacterRoster.GetDefaultCharacter(1);

            if (selectedPlayer1 == null)
            {
                selectedPlayer1 = CharacterRoster.GetDefaultCharacter(0);
            }

            if (selectedPlayer2 == null || selectedPlayer2 == selectedPlayer1)
            {
                selectedPlayer2 = CharacterRoster.GetNextDifferent(selectedPlayer1);
            }
        }

        private void DrawPanelFrame(Rect panel)
        {
            DrawRoundedRect(new Rect(panel.x - 8f, panel.y - 8f, panel.width + 16f, panel.height + 16f), new Color(0.27f, 0.8f, 1f, 0.16f), Color.clear, 30, 0);
            DrawRoundedRect(new Rect(panel.x + 8f, panel.y + 10f, panel.width, panel.height), new Color(0.05f, 0.22f, 0.32f, 0.32f), Color.clear, 26, 0);
            DrawRoundedRect(panel, new Color(1f, 0.96f, 0.72f, 0.97f), new Color(0.18f, 0.62f, 0.88f, 1f), 28, 8);

            float t = Time.unscaledTime;
            DrawBubble(new Rect(panel.x + 54f + Mathf.Sin(t * 1.2f) * 5f, panel.y + panel.height - 92f, 58f, 58f), new Color(0.42f, 0.92f, 1f, 0.24f));
            DrawBubble(new Rect(panel.x + panel.width - 118f + Mathf.Cos(t * 1.1f) * 5f, panel.y + 78f, 48f, 48f), new Color(1f, 0.54f, 0.72f, 0.2f));
            DrawBubble(new Rect(panel.x + panel.width - 180f + Mathf.Sin(t * 0.9f) * 4f, panel.y + panel.height - 138f, 42f, 42f), new Color(0.7f, 1f, 0.5f, 0.2f));
        }

        private void DrawHeader(Rect panel, GameMode mode)
        {
            Rect pillRect = new Rect(panel.x + panel.width * 0.5f - 250f, panel.y + 28f, 500f, 30f);
            DrawRoundedRect(pillRect, new Color(0.12f, 0.74f, 1f, 1f), Color.white, 15, 2);
            GUI.Label(pillRect, "ORIGINAL CHIBI HEROES", pillStyle);

            Rect titleRect = new Rect(panel.x + 70f, panel.y + 64f, panel.width - 140f, 54f);
            GUI.Label(new Rect(titleRect.x + 4f, titleRect.y + 4f, titleRect.width, titleRect.height), "Choose Character", titleShadowStyle);
            GUI.Label(titleRect, "Choose Character", titleStyle);

            Rect hintRect = new Rect(panel.x + 110f, panel.y + 124f, panel.width - 220f, 24f);
            GUI.Label(hintRect, BuildModeHint(mode), hintStyle);
        }

        private string BuildModeHint(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.LocalVS:
                    return "Pick different heroes for Player 1 and Player 2.";
                case GameMode.AIBattle:
                    return "Pick your hero. The AI will bring a random rival.";
                default:
                    return "Pick the hero look you like before choosing a map.";
            }
        }

        private void DrawSlotSelector(Rect rect, GameMode mode)
        {
            if (mode == GameMode.SinglePlayer)
            {
                DrawSlotButton(rect, Player1Slot, "Player 1", selectedPlayer1, true);
                return;
            }

            float gap = 14f;
            float slotWidth = (rect.width - gap) * 0.5f;
            DrawSlotButton(new Rect(rect.x, rect.y, slotWidth, rect.height), Player1Slot, "Player 1", selectedPlayer1, true);

            if (mode == GameMode.LocalVS)
            {
                DrawSlotButton(new Rect(rect.x + slotWidth + gap, rect.y, slotWidth, rect.height), Player2Slot, "Player 2", selectedPlayer2, true);
                return;
            }

            DrawSlotButton(
                new Rect(rect.x + slotWidth + gap, rect.y, slotWidth, rect.height),
                Player2Slot,
                "AI Rival",
                GameManager.Instance != null ? GameManager.Instance.SelectedAICharacter : null,
                false);
        }

        private void DrawSlotButton(Rect rect, int slot, string label, CharacterData character, bool canSelect)
        {
            bool isActive = activeSlot == slot;
            Color accent = character != null ? character.ThemeColor : new Color(0.5f, 0.75f, 1f, 1f);
            Color fill = isActive && canSelect
                ? Color.Lerp(accent, Color.white, 0.2f)
                : new Color(1f, 0.94f, 0.72f, 0.96f);

            Color border = isActive && canSelect
                ? Color.Lerp(accent, Color.white, 0.24f)
                : Color.Lerp(accent, Color.white, 0.42f);
            DrawRoundedRect(new Rect(rect.x + 4f, rect.y + 6f, rect.width, rect.height), new Color(0.05f, 0.22f, 0.32f, 0.26f), Color.clear, 18, 0);
            DrawRoundedRect(rect, fill, border, 18, isActive && canSelect ? 4 : 2);
            DrawRoundedRect(new Rect(rect.x + 12f, rect.y + 9f, 9f, rect.height - 18f), accent, Color.clear, 5, 0);
            GUI.Label(new Rect(rect.x + 34f, rect.y + 7f, rect.width - 52f, 20f), label, slotStyle);
            GUI.Label(
                new Rect(rect.x + 34f, rect.y + 29f, rect.width - 52f, 18f),
                character != null ? character.DisplayName : "Random",
                cardBodyStyle);

            if (canSelect && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                activeSlot = slot;
            }
        }

        private void DrawRoster(Rect rect, GameMode mode)
        {
            if (roster == null || roster.Length == 0)
            {
                GUI.Label(rect, "No CharacterData assets found in Resources/Characters.", lockedLabelStyle);
                return;
            }

            int columns = Screen.width >= 840f ? 3 : 1;
            int rows = Mathf.Min(Mathf.CeilToInt((float)roster.Length / columns), columns == 1 ? 3 : 2);
            float gap = 16f;
            float cardWidth = (rect.width - gap * (columns - 1)) / columns;
            float cardHeight = (rect.height - gap * (rows - 1)) / rows;
            int visibleCount = Mathf.Min(roster.Length, columns * rows);

            for (int i = 0; i < visibleCount; i++)
            {
                int column = i % columns;
                int row = i / columns;
                Rect cardRect = new Rect(
                    rect.x + column * (cardWidth + gap),
                    rect.y + row * (cardHeight + gap),
                    cardWidth,
                    cardHeight);

                CharacterData character = roster[i];
                bool isSelectedByActiveSlot = IsSelectedByActiveSlot(character);
                bool isTakenByOtherLocalPlayer = mode == GameMode.LocalVS && IsTakenByOtherLocalPlayer(character);
                DrawCharacterCard(cardRect, character, isSelectedByActiveSlot, isTakenByOtherLocalPlayer);
            }
        }

        private bool IsSelectedByActiveSlot(CharacterData character)
        {
            return activeSlot == Player2Slot ? character == selectedPlayer2 : character == selectedPlayer1;
        }

        private bool IsTakenByOtherLocalPlayer(CharacterData character)
        {
            return activeSlot == Player1Slot
                ? character == selectedPlayer2
                : character == selectedPlayer1;
        }

        private void DrawCharacterCard(Rect rect, CharacterData character, bool isSelected, bool isTaken)
        {
            if (character == null)
            {
                return;
            }

            Event currentEvent = Event.current;
            bool isHovering = currentEvent != null && rect.Contains(currentEvent.mousePosition);
            Color accent = character.ThemeColor;
            Color fill = isSelected
                ? new Color(1f, 0.98f, 0.76f, 1f)
                : isHovering ? new Color(1f, 0.96f, 0.78f, 1f) : new Color(1f, 0.92f, 0.66f, 1f);

            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = isHovering ? 1.012f + Mathf.Sin(Time.unscaledTime * 8f) * 0.003f : 1f;
            if (!Mathf.Approximately(scale, 1f))
            {
                GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), rect.center);
            }

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), new Color(0.05f, 0.22f, 0.32f, 0.3f), Color.clear, 22, 0);
            DrawRoundedRect(rect, fill, Color.Lerp(accent, Color.white, isSelected ? 0.08f : 0.28f), 22, isSelected ? 5 : 3);
            DrawRoundedRect(new Rect(rect.x + 18f, rect.y + 13f, rect.width - 36f, 8f), new Color(1f, 1f, 1f, 0.34f), Color.clear, 6, 0);

            float nameHeight = 30f;
            float nameY = rect.y + rect.height - 42f;
            float iconTop = rect.y + 24f;
            float iconBottomLimit = nameY - 18f;
            float iconSize = Mathf.Clamp(iconBottomLimit - iconTop, 64f, 94f);
            Rect iconBackRect = new Rect(rect.x + rect.width * 0.5f - iconSize * 0.62f, iconTop - 2f, iconSize * 1.24f, iconSize * 1.02f);
            DrawBubble(iconBackRect, new Color(accent.r, accent.g, accent.b, 0.16f));
            Rect iconRect = new Rect(rect.x + rect.width * 0.5f - iconSize * 0.5f, iconTop, iconSize, iconSize * 0.92f);
            DrawCharacterIcon(iconRect, character);

            Rect namePillRect = new Rect(rect.x + 24f, nameY, rect.width - 48f, nameHeight);
            DrawRoundedRect(namePillRect, new Color(1f, 0.98f, 0.84f, 0.92f), Color.Lerp(accent, Color.white, 0.28f), 18, 2);
            GUI.Label(namePillRect, character.DisplayName, cardTitleStyle);

            if (isSelected)
            {
                DrawRoundedRect(new Rect(rect.x + rect.width - 76f, rect.y + 16f, 58f, 23f), accent, Color.white, 12, 2);
                GUI.Label(new Rect(rect.x + rect.width - 76f, rect.y + 16f, 58f, 23f), "READY", smallPillStyle);
            }
            else if (isTaken)
            {
                DrawRoundedRect(new Rect(rect.x + rect.width - 76f, rect.y + 16f, 58f, 23f), new Color(0.72f, 0.72f, 0.68f, 1f), Color.white, 12, 2);
                GUI.Label(new Rect(rect.x + rect.width - 76f, rect.y + 16f, 58f, 23f), "PICKED", smallPillStyle);
            }

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            GUI.matrix = previousMatrix;

            if (clicked)
            {
                HandleCharacterClicked(character, isTaken);
            }
        }

        private void HandleCharacterClicked(CharacterData character, bool isTakenByOtherPlayer)
        {
            if (character == null)
            {
                return;
            }

            if (isTakenByOtherPlayer)
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                return;
            }

            AudioManager.Instance?.PlayButtonClickSFX();
            if (activeSlot == Player2Slot)
            {
                selectedPlayer2 = character;
                GameManager.Instance?.SetPlayer2Character(character);
                return;
            }

            selectedPlayer1 = character;
            GameManager.Instance?.SetPlayer1Character(character);
        }

        private void DrawBottomButtons(Rect panel)
        {
            const float buttonHeight = 52f;
            const float horizontalPadding = 56f;
            const float buttonGap = 16f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.y + panel.height - 78f;
            Rect backRect = new Rect(panel.x + horizontalPadding, buttonY, buttonWidth, buttonHeight);
            Rect continueRect = new Rect(panel.x + horizontalPadding + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight);

            if (SimpleUIFactory.FixedSecondaryButton(backRect, "BACK"))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                SceneFlowManager.Instance?.LoadModeSelect();
            }

            if (SimpleUIFactory.FixedPrimaryButton(continueRect, "CONTINUE"))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                GameManager.Instance?.SetPlayer1Character(selectedPlayer1);
                GameManager.Instance?.SetPlayer2Character(selectedPlayer2);
                if (GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameMode.AIBattle)
                {
                    GameManager.Instance.RandomizeAICharacter();
                }

                SceneFlowManager.Instance?.LoadMapSelect();
            }
        }

        private void DrawCharacterIcon(Rect rect, CharacterData character)
        {
            if (character.Icon != null)
            {
                GUI.DrawTexture(rect, character.Icon.texture, ScaleMode.ScaleToFit, true);
                return;
            }

            float bob = Mathf.Sin(Time.unscaledTime * 3.2f + rect.x * 0.04f) * rect.height * 0.035f;
            Color accent = character.ThemeColor;
            Color skin = new Color(1f, 0.86f, 0.62f, 1f);
            Color outline = Color.Lerp(accent, new Color(0.07f, 0.18f, 0.24f, 1f), 0.36f);

            DrawRoundedRect(new Rect(rect.x + rect.width * 0.34f, rect.y + rect.height * 0.66f + bob, rect.width * 0.34f, rect.height * 0.29f), outline, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.37f, rect.y + rect.height * 0.62f + bob, rect.width * 0.28f, rect.height * 0.33f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.2f, rect.y + rect.height * 0.17f + bob, rect.width * 0.6f, rect.height * 0.59f), outline, Color.clear, Mathf.RoundToInt(rect.height * 0.18f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.24f, rect.y + rect.height * 0.12f + bob, rect.width * 0.52f, rect.height * 0.58f), skin, Color.clear, Mathf.RoundToInt(rect.height * 0.18f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.06f + bob, rect.width * 0.66f, rect.height * 0.18f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.33f, rect.y + rect.height * 0.38f + bob, rect.width * 0.08f, rect.height * 0.1f), Color.white, Color.clear, 3, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.59f, rect.y + rect.height * 0.38f + bob, rect.width * 0.08f, rect.height * 0.1f), Color.white, Color.clear, 3, 0);
            DrawRect(new Rect(rect.x + rect.width * 0.36f, rect.y + rect.height * 0.42f + bob, rect.width * 0.035f, rect.height * 0.035f), new Color(0.06f, 0.18f, 0.24f, 1f));
            DrawRect(new Rect(rect.x + rect.width * 0.62f, rect.y + rect.height * 0.42f + bob, rect.width * 0.035f, rect.height * 0.035f), new Color(0.06f, 0.18f, 0.24f, 1f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.58f + bob, rect.width * 0.12f, rect.height * 0.035f), new Color(0.22f, 0.38f, 0.44f, 1f), Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.72f + bob, rect.width * 0.22f, rect.height * 0.14f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.07f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.72f, rect.y + rect.height * 0.72f + bob, rect.width * 0.22f, rect.height * 0.14f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.07f), 0);
        }

        private void DrawBubble(Rect rect, Color color)
        {
            DrawRoundedRect(rect, color, new Color(1f, 1f, 1f, color.a * 0.65f), Mathf.RoundToInt(rect.height * 0.5f), 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.18f, rect.width * 0.22f, rect.height * 0.22f), new Color(1f, 1f, 1f, color.a * 0.7f), Color.clear, 10, 0);
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawRoundedRect(Rect rect, Color fillColor, Color borderColor, int radius, int borderSize)
        {
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, GetRoundedTexture(fillColor, borderColor, Mathf.Max(0, radius), Mathf.Max(0, borderSize)));
            GUI.color = previousColor;
        }

        private Texture2D GetRoundedTexture(Color fillColor, Color borderColor, int radius, int borderSize)
        {
            string key = $"{ColorUtility.ToHtmlStringRGBA(fillColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}_{radius}_{borderSize}";
            if (roundedTextureCache.TryGetValue(key, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float scaledRadius = Mathf.Clamp(radius, 0, TextureSize / 2f);
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, TextureSize, TextureSize, scaledRadius);
                    if (!inside)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool isBorder = borderSize > 0 && IsBorderPixel(x, y, TextureSize, TextureSize, scaledRadius, borderSize);
                    texture.SetPixel(x, y, isBorder ? borderColor : fillColor);
                }
            }

            texture.Apply();
            roundedTextureCache[key] = texture;
            return texture;
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, float radius)
        {
            if (radius <= 0f)
            {
                return true;
            }

            float px = x + 0.5f;
            float py = y + 0.5f;
            float left = radius;
            float right = width - radius;
            float bottom = radius;
            float top = height - radius;

            float cx = Mathf.Clamp(px, left, right);
            float cy = Mathf.Clamp(py, bottom, top);
            float dx = px - cx;
            float dy = py - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private bool IsBorderPixel(int x, int y, int width, int height, float radius, int borderSize)
        {
            if (x < borderSize || y < borderSize || x >= width - borderSize || y >= height - borderSize)
            {
                return true;
            }

            if (radius <= borderSize)
            {
                return false;
            }

            return !IsInsideRoundedRect(
                x - borderSize,
                y - borderSize,
                width - borderSize * 2,
                height - borderSize * 2,
                radius - borderSize);
        }

        private void EnsureStyles()
        {
            if (whiteTexture == null)
            {
                whiteTexture = Texture2D.whiteTexture;
            }

            if (cardTitleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(titleStyle, new Color(0.11f, 0.28f, 0.42f, 1f));

            titleShadowStyle = new GUIStyle(titleStyle);
            LockTextColor(titleShadowStyle, new Color(0.05f, 0.18f, 0.24f, 0.28f));

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                clipping = TextClipping.Clip
            };
            LockTextColor(hintStyle, new Color(0.23f, 0.45f, 0.55f, 1f));

            pillStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(pillStyle, Color.white);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(cardTitleStyle, new Color(0.11f, 0.28f, 0.42f, 1f));

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                clipping = TextClipping.Clip
            };
            LockTextColor(cardBodyStyle, new Color(0.23f, 0.45f, 0.55f, 1f));

            slotStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };
            LockTextColor(slotStyle, new Color(0.11f, 0.28f, 0.42f, 1f));

            smallPillStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(smallPillStyle, Color.white);

            lockedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            LockTextColor(lockedLabelStyle, new Color(0.11f, 0.28f, 0.42f, 1f));
        }

        private void LockTextColor(GUIStyle style, Color color)
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
    }
}
