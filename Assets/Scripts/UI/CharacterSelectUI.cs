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
        private GUIStyle cardTitleStyle;
        private GUIStyle slotStyle;
        private GUIStyle smallPillStyle;
        private GUIStyle lockedLabelStyle;
        private Texture2D whiteTexture;

        private readonly Dictionary<string, Texture2D> roundedTextureCache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            LoadRosterAndSelections();
        }

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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
            DrawHeader(panel);

            Rect slotRect = new Rect(panel.x + 150f, panel.y + 104f, panel.width - 300f, 38f);
            DrawSlotSelector(slotRect, mode);

            Rect rosterRect = new Rect(panel.x + 48f, panel.y + 150f, panel.width - 96f, panel.height - 280f);
            DrawRoster(rosterRect, mode);

            DrawBottomButtons(panel);
        }

        /// <summary>
        /// Purpose: Loads roster and selections.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

        /// <summary>
        /// Purpose: Draws panel frame in the current GUI or scene context.
        /// Inputs: `panel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Draws header in the current GUI or scene context.
        /// Inputs: `panel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
        private void DrawHeader(Rect panel)
        {
            Rect titleRect = new Rect(panel.x + 70f, panel.y + 34f, panel.width - 140f, 56f);
            GUI.Label(new Rect(titleRect.x + 4f, titleRect.y + 4f, titleRect.width, titleRect.height), "Choose Character", titleShadowStyle);
            GUI.Label(titleRect, "Choose Character", titleStyle);
        }

        /// <summary>
        /// Purpose: Draws slot selector in the current GUI or scene context.
        /// Inputs: `rect`, `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="mode">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Draws slot button in the current GUI or scene context.
        /// Inputs: `rect`, `slot`, `label`, `character`, `canSelect`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="slot">Input value used by this method.</param>
        /// <param name="label">Input value used by this method.</param>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="canSelect">Input value used by this method.</param>
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
            DrawRoundedRect(new Rect(rect.x + 4f, rect.y + 5f, rect.width, rect.height), new Color(0.05f, 0.22f, 0.32f, 0.26f), Color.clear, 18, 0);
            DrawRoundedRect(rect, fill, border, 18, isActive && canSelect ? 4 : 2);
            DrawRoundedRect(new Rect(rect.x + 12f, rect.y + 8f, 9f, rect.height - 16f), accent, Color.clear, 5, 0);
            GUI.Label(
                new Rect(rect.x + 32f, rect.y + 6f, rect.width - 48f, rect.height - 12f),
                $"{label}: {(character != null ? character.DisplayName : "Random")}",
                slotStyle);

            if (canSelect && GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                activeSlot = slot;
            }
        }

        /// <summary>
        /// Purpose: Draws roster in the current GUI or scene context.
        /// Inputs: `rect`, `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="mode">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Returns whether this object is selected by active slot.
        /// Inputs: `character`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsSelectedByActiveSlot(CharacterData character)
        {
            return activeSlot == Player2Slot ? character == selectedPlayer2 : character == selectedPlayer1;
        }

        /// <summary>
        /// Purpose: Returns whether this object is taken by other local player.
        /// Inputs: `character`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsTakenByOtherLocalPlayer(CharacterData character)
        {
            return activeSlot == Player1Slot
                ? character == selectedPlayer2
                : character == selectedPlayer1;
        }

        /// <summary>
        /// Purpose: Draws character card in the current GUI or scene context.
        /// Inputs: `rect`, `character`, `isSelected`, `isTaken`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="isSelected">Input value used by this method.</param>
        /// <param name="isTaken">Input value used by this method.</param>
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

            DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), new Color(0.05f, 0.22f, 0.32f, 0.3f), Color.clear, 22, 0);
            DrawRoundedRect(rect, fill, Color.Lerp(accent, Color.white, isSelected ? 0.08f : 0.28f), 22, isSelected ? 5 : 3);
            DrawRoundedRect(new Rect(rect.x + 18f, rect.y + 12f, rect.width - 36f, 8f), new Color(1f, 1f, 1f, 0.34f), Color.clear, 6, 0);

            Rect contentRect = new Rect(rect.x + 22f, rect.y + 30f, rect.width - 44f, rect.height - 54f);
            float iconAreaWidth = Mathf.Min(100f, contentRect.width * 0.38f);
            Rect iconArea = new Rect(contentRect.x, contentRect.y, iconAreaWidth, contentRect.height);
            float iconHeight = Mathf.Clamp(iconArea.height - 8f, 76f, 92f);
            float iconWidth = iconHeight * 0.88f;
            Rect iconRect = new Rect(
                iconArea.x + (iconArea.width - iconWidth) * 0.5f,
                iconArea.y + (iconArea.height - iconHeight) * 0.5f,
                iconWidth,
                iconHeight);
            Rect iconBackRect = new Rect(iconRect.x - 9f, iconRect.y - 7f, iconRect.width + 18f, iconRect.height + 14f);
            DrawBubble(iconBackRect, new Color(accent.r, accent.g, accent.b, 0.16f));
            DrawCharacterIcon(iconRect, character);

            Rect nameArea = new Rect(iconArea.xMax + 16f, contentRect.y, contentRect.xMax - iconArea.xMax - 16f, contentRect.height);
            const float nameHeight = 70f;
            Rect namePillRect = new Rect(
                nameArea.x,
                nameArea.y + (nameArea.height - nameHeight) * 0.5f,
                nameArea.width,
                nameHeight);
            DrawRoundedRect(namePillRect, new Color(1f, 0.98f, 0.84f, 0.92f), Color.Lerp(accent, Color.white, 0.28f), 18, 2);
            GUI.Label(namePillRect, BuildTwoLineName(character.DisplayName), cardTitleStyle);

            if (isSelected)
            {
                Rect readyRect = new Rect(rect.x + 24f, rect.y + 9f, 58f, 22f);
                DrawRoundedRect(readyRect, accent, Color.white, 12, 2);
                GUI.Label(readyRect, "READY", smallPillStyle);
            }
            else if (isTaken)
            {
                Rect pickedRect = new Rect(rect.x + 24f, rect.y + 9f, 62f, 22f);
                DrawRoundedRect(pickedRect, new Color(0.72f, 0.72f, 0.68f, 1f), Color.white, 12, 2);
                GUI.Label(pickedRect, "PICKED", smallPillStyle);
            }

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);

            if (clicked)
            {
                HandleCharacterClicked(character, isTaken);
            }
        }

        /// <summary>
        /// Purpose: Builds two line name.
        /// Inputs: `displayName`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="displayName">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
        private string BuildTwoLineName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "Unknown\nHero";
            }

            string[] words = displayName.Trim().Split(' ');
            if (words.Length <= 1)
            {
                return displayName;
            }

            int splitIndex = Mathf.CeilToInt(words.Length * 0.5f);
            string firstLine = string.Join(" ", words, 0, splitIndex);
            string secondLine = string.Join(" ", words, splitIndex, words.Length - splitIndex);
            return $"{firstLine}\n{secondLine}";
        }

        /// <summary>
        /// Purpose: Handles character clicked.
        /// Inputs: `character`, `isTakenByOtherPlayer`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="isTakenByOtherPlayer">Input value used by this method.</param>
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

        /// <summary>
        /// Purpose: Draws bottom buttons in the current GUI or scene context.
        /// Inputs: `panel`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="panel">Input value used by this method.</param>
        private void DrawBottomButtons(Rect panel)
        {
            const float buttonHeight = 50f;
            const float horizontalPadding = 56f;
            const float buttonGap = 16f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.y + panel.height - 70f;
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

        /// <summary>
        /// Purpose: Draws character icon in the current GUI or scene context.
        /// Inputs: `rect`, `character`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="character">Input value used by this method.</param>
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
            Color lightAccent = Color.Lerp(accent, Color.white, 0.32f);
            Color darkAccent = Color.Lerp(accent, new Color(0.07f, 0.18f, 0.24f, 1f), 0.42f);

            DrawCharacterAccessory(rect, character, accent, lightAccent, darkAccent, bob);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.34f, rect.y + rect.height * 0.66f + bob, rect.width * 0.34f, rect.height * 0.29f), outline, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.37f, rect.y + rect.height * 0.62f + bob, rect.width * 0.28f, rect.height * 0.33f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.2f, rect.y + rect.height * 0.17f + bob, rect.width * 0.6f, rect.height * 0.59f), outline, Color.clear, Mathf.RoundToInt(rect.height * 0.18f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.24f, rect.y + rect.height * 0.12f + bob, rect.width * 0.52f, rect.height * 0.58f), skin, Color.clear, Mathf.RoundToInt(rect.height * 0.18f), 0);

            if (!IsCharacter(character, "frog_hopper"))
            {
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.06f + bob, rect.width * 0.66f, rect.height * 0.18f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.09f), 0);
            }

            DrawRoundedRect(new Rect(rect.x + rect.width * 0.33f, rect.y + rect.height * 0.38f + bob, rect.width * 0.08f, rect.height * 0.1f), Color.white, Color.clear, 3, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.59f, rect.y + rect.height * 0.38f + bob, rect.width * 0.08f, rect.height * 0.1f), Color.white, Color.clear, 3, 0);
            DrawRect(new Rect(rect.x + rect.width * 0.36f, rect.y + rect.height * 0.42f + bob, rect.width * 0.035f, rect.height * 0.035f), new Color(0.06f, 0.18f, 0.24f, 1f));
            DrawRect(new Rect(rect.x + rect.width * 0.62f, rect.y + rect.height * 0.42f + bob, rect.width * 0.035f, rect.height * 0.035f), new Color(0.06f, 0.18f, 0.24f, 1f));
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.58f + bob, rect.width * 0.12f, rect.height * 0.035f), new Color(0.22f, 0.38f, 0.44f, 1f), Color.clear, 2, 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.72f + bob, rect.width * 0.22f, rect.height * 0.14f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.07f), 0);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.72f, rect.y + rect.height * 0.72f + bob, rect.width * 0.22f, rect.height * 0.14f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.07f), 0);
        }

        /// <summary>
        /// Purpose: Draws character accessory in the current GUI or scene context.
        /// Inputs: `rect`, `character`, `accent`, `lightAccent`, `darkAccent`, `bob`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="accent">Input value used by this method.</param>
        /// <param name="lightAccent">Input value used by this method.</param>
        /// <param name="darkAccent">Input value used by this method.</param>
        /// <param name="bob">Input value used by this method.</param>
        private void DrawCharacterAccessory(Rect rect, CharacterData character, Color accent, Color lightAccent, Color darkAccent, float bob)
        {
            if (IsCharacter(character, "bear_blaster"))
            {
                DrawBubble(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.02f + bob, rect.width * 0.22f, rect.height * 0.22f), accent);
                DrawBubble(new Rect(rect.x + rect.width * 0.6f, rect.y + rect.height * 0.02f + bob, rect.width * 0.22f, rect.height * 0.22f), accent);
                DrawBubble(new Rect(rect.x + rect.width * 0.24f, rect.y + rect.height * 0.08f + bob, rect.width * 0.1f, rect.height * 0.1f), lightAccent);
                DrawBubble(new Rect(rect.x + rect.width * 0.66f, rect.y + rect.height * 0.08f + bob, rect.width * 0.1f, rect.height * 0.1f), lightAccent);
                return;
            }

            if (IsCharacter(character, "frog_hopper"))
            {
                DrawBubble(new Rect(rect.x + rect.width * 0.22f, rect.y + rect.height * 0.02f + bob, rect.width * 0.2f, rect.height * 0.2f), lightAccent);
                DrawBubble(new Rect(rect.x + rect.width * 0.58f, rect.y + rect.height * 0.02f + bob, rect.width * 0.2f, rect.height * 0.2f), lightAccent);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.29f, rect.y + rect.height * 0.08f + bob, rect.width * 0.06f, rect.height * 0.06f), darkAccent, Color.clear, 2, 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.65f, rect.y + rect.height * 0.08f + bob, rect.width * 0.06f, rect.height * 0.06f), darkAccent, Color.clear, 2, 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.12f + bob, rect.width * 0.66f, rect.height * 0.16f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.08f), 0);
                return;
            }

            if (IsCharacter(character, "gear_kid"))
            {
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.17f, rect.y + rect.height * 0.05f + bob, rect.width * 0.66f, rect.height * 0.18f), accent, Color.clear, Mathf.RoundToInt(rect.height * 0.08f), 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.24f, rect.y + rect.height * 0.01f + bob, rect.width * 0.52f, rect.height * 0.18f), lightAccent, Color.clear, Mathf.RoundToInt(rect.height * 0.08f), 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.32f, rect.y + rect.height * 0.29f + bob, rect.width * 0.36f, rect.height * 0.1f), new Color(0.7f, 0.9f, 1f, 0.9f), darkAccent, 6, 1);
                return;
            }

            if (IsCharacter(character, "bunny_pop"))
            {
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.27f, rect.y - rect.height * 0.02f + bob, rect.width * 0.13f, rect.height * 0.42f), accent, Color.clear, Mathf.RoundToInt(rect.width * 0.06f), 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.6f, rect.y - rect.height * 0.02f + bob, rect.width * 0.13f, rect.height * 0.42f), accent, Color.clear, Mathf.RoundToInt(rect.width * 0.06f), 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.3f, rect.y + rect.height * 0.04f + bob, rect.width * 0.06f, rect.height * 0.28f), lightAccent, Color.clear, Mathf.RoundToInt(rect.width * 0.03f), 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.63f, rect.y + rect.height * 0.04f + bob, rect.width * 0.06f, rect.height * 0.28f), lightAccent, Color.clear, Mathf.RoundToInt(rect.width * 0.03f), 0);
                return;
            }

            if (IsCharacter(character, "star_mage"))
            {
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.26f, rect.y + rect.height * 0.02f + bob, rect.width * 0.48f, rect.height * 0.1f), darkAccent, Color.clear, 5, 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.34f, rect.y - rect.height * 0.02f + bob, rect.width * 0.32f, rect.height * 0.14f), accent, Color.clear, 5, 0);
                DrawRoundedRect(new Rect(rect.x + rect.width * 0.42f, rect.y - rect.height * 0.12f + bob, rect.width * 0.16f, rect.height * 0.16f), lightAccent, Color.clear, 4, 0);
                DrawRect(new Rect(rect.x + rect.width * 0.49f, rect.y + rect.height * 0.01f + bob, rect.width * 0.04f, rect.height * 0.12f), new Color(1f, 0.9f, 0.2f, 1f));
                DrawRect(new Rect(rect.x + rect.width * 0.45f, rect.y + rect.height * 0.05f + bob, rect.width * 0.12f, rect.height * 0.04f), new Color(1f, 0.9f, 0.2f, 1f));
                return;
            }

            DrawBubble(new Rect(rect.x + rect.width * 0.62f, rect.y - rect.height * 0.02f + bob, rect.width * 0.2f, rect.height * 0.2f), lightAccent);
        }

        /// <summary>
        /// Purpose: Returns whether this object is character.
        /// Inputs: `character`, `characterId`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="character">Input value used by this method.</param>
        /// <param name="characterId">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
        private bool IsCharacter(CharacterData character, string characterId)
        {
            return character != null && character.CharacterId == characterId;
        }

        /// <summary>
        /// Purpose: Draws bubble in the current GUI or scene context.
        /// Inputs: `rect`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private void DrawBubble(Rect rect, Color color)
        {
            DrawRoundedRect(rect, color, new Color(1f, 1f, 1f, color.a * 0.65f), Mathf.RoundToInt(rect.height * 0.5f), 2);
            DrawRoundedRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.18f, rect.width * 0.22f, rect.height * 0.22f), new Color(1f, 1f, 1f, color.a * 0.7f), Color.clear, 10, 0);
        }

        /// <summary>
        /// Purpose: Draws rect in the current GUI or scene context.
        /// Inputs: `rect`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        /// <summary>
        /// Purpose: Draws rounded rect in the current GUI or scene context.
        /// Inputs: `rect`, `fillColor`, `borderColor`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="rect">Input value used by this method.</param>
        /// <param name="fillColor">Input value used by this method.</param>
        /// <param name="borderColor">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        private void DrawRoundedRect(Rect rect, Color fillColor, Color borderColor, int radius, int borderSize)
        {
            Color previousColor = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, GetRoundedTexture(fillColor, borderColor, Mathf.Max(0, radius), Mathf.Max(0, borderSize)));
            GUI.color = previousColor;
        }

        /// <summary>
        /// Purpose: Gets rounded texture.
        /// Inputs: `fillColor`, `borderColor`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: a `Texture2D` value.
        /// </summary>
        /// <param name="fillColor">Input value used by this method.</param>
        /// <param name="borderColor">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        /// <returns>a `Texture2D` value.</returns>
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

        /// <summary>
        /// Purpose: Returns whether this object is border pixel.
        /// Inputs: `x`, `y`, `width`, `height`, `radius`, `borderSize`; may also read serialized fields and current runtime state.
        /// Output: a `bool` value.
        /// </summary>
        /// <param name="x">Input value used by this method.</param>
        /// <param name="y">Input value used by this method.</param>
        /// <param name="width">Input value used by this method.</param>
        /// <param name="height">Input value used by this method.</param>
        /// <param name="radius">Input value used by this method.</param>
        /// <param name="borderSize">Input value used by this method.</param>
        /// <returns>a `bool` value.</returns>
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

        /// <summary>
        /// Purpose: Ensures styles exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
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

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(cardTitleStyle, new Color(0.11f, 0.28f, 0.42f, 1f));

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

        /// <summary>
        /// Purpose: Performs lock text color for this component.
        /// Inputs: `style`, `color`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="style">Input value used by this method.</param>
        /// <param name="color">Input value used by this method.</param>
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
