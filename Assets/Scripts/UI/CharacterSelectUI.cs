using System;
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
        private const string BackgroundResourcePath = "UI/CharacterSelect/CharacterSelect";
        private const string BackButtonResourcePath = "UI/CharacterSelect/Back2";
        private const string ContinueButtonResourcePath = "UI/CharacterSelect/Continue";
        private const string ReadyBadgeResourcePath = "UI/CharacterSelect/Ready";
        private const string RoleResourcePrefix = "UI/CharacterSelect/Role";
        private const int Player1Slot = 1;
        private const int Player2Slot = 2;
        private const int RoleTextureCount = 6;
        private const int TextureSize = 64;
        private const float CardFloatSpeed = 2.1f;
        private const float CardFloatAmount = 0.005f;
        private const float ButtonFloatSpeed = 2.8f;
        private const float ButtonFloatAmount = 0.004f;

        private static readonly string[] ImageRoleCharacterIds =
        {
            "bubble_ranger",
            "star_mage",
            "frog_hopper",
            "bear_blaster",
            "gear_kid",
            "bunny_pop"
        };

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
        private GUIStyle imageBannerStyle;
        private GUIStyle imageSubtleBannerStyle;
        private GUIStyle transparentButtonStyle;
        private Texture2D backgroundTexture;
        private Texture2D backButtonTexture;
        private Texture2D continueButtonTexture;
        private Texture2D readyBadgeTexture;
        private Texture2D whiteTexture;
        private readonly Texture2D[] roleTextures = new Texture2D[RoleTextureCount];
        private readonly Dictionary<string, Texture2D> roleTexturesByCharacterId = new Dictionary<string, Texture2D>();

        private readonly Dictionary<string, Texture2D> roundedTextureCache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Purpose: Loads imported character-select textures before the first IMGUI draw pass.
        /// Inputs: no direct parameters; reads Texture2D assets from Resources/UI/CharacterSelect.
        /// Output: no return value; caches textures for the image-based character select screen.
        /// </summary>
        private void Awake()
        {
            LoadCharacterSelectTextures();
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            LoadCharacterSelectTextures();
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

            if (HasImageCharacterSelectAssets())
            {
                DrawImageCharacterSelect(mode);
                return;
            }

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
        /// Purpose: Loads all imported textures used by the new art-driven character select screen.
        /// Inputs: no direct parameters; uses Resources.Load paths without file extensions.
        /// Output: no return value; assigns cached Texture2D references or keeps them null if missing.
        /// </summary>
        private void LoadCharacterSelectTextures()
        {
            backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
            backButtonTexture = Resources.Load<Texture2D>(BackButtonResourcePath);
            continueButtonTexture = Resources.Load<Texture2D>(ContinueButtonResourcePath);
            readyBadgeTexture = Resources.Load<Texture2D>(ReadyBadgeResourcePath);

            roleTexturesByCharacterId.Clear();
            for (int i = 0; i < roleTextures.Length; i++)
            {
                Texture2D roleTexture = Resources.Load<Texture2D>($"{RoleResourcePrefix}{i + 1}");
                roleTextures[i] = roleTexture;
                ApplyCharacterSelectTextureSettings(roleTexture);

                // The imported role images follow the visual card order, not CharacterRoster order.
                // Binding each texture to a stable characterId prevents clicked art and selected name from drifting apart.
                if (i < ImageRoleCharacterIds.Length && roleTexture != null)
                {
                    roleTexturesByCharacterId[ImageRoleCharacterIds[i]] = roleTexture;
                }
            }

            ApplyCharacterSelectTextureSettings(backgroundTexture);
            ApplyCharacterSelectTextureSettings(backButtonTexture);
            ApplyCharacterSelectTextureSettings(continueButtonTexture);
            ApplyCharacterSelectTextureSettings(readyBadgeTexture);
        }

        /// <summary>
        /// Purpose: Checks whether every required imported character-select image is available.
        /// Inputs: no direct parameters; reads cached Texture2D fields.
        /// Output: true when the image-based layout can be drawn; otherwise false so the old fallback remains usable.
        /// </summary>
        /// <returns>True if all required character select textures are loaded; otherwise false.</returns>
        private bool HasImageCharacterSelectAssets()
        {
            if (backgroundTexture == null || backButtonTexture == null || continueButtonTexture == null || readyBadgeTexture == null)
            {
                return false;
            }

            for (int i = 0; i < roleTextures.Length; i++)
            {
                if (roleTextures[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Purpose: Draws the imported character select background, character cards, ready badge, and navigation buttons.
        /// Inputs: mode is the current game mode, which decides whether P2 can choose a separate character.
        /// Output: no return value; updates selected character data or scene flow when the player clicks controls.
        /// </summary>
        /// <param name="mode">Current game mode used to choose Single/AIBattle/LocalVS selection behavior.</param>
        private void DrawImageCharacterSelect(GameMode mode)
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect backgroundRect = PixelSnapRect(CalculateAspectFitRect(backgroundTexture, screenRect));
            GUI.DrawTexture(backgroundRect, backgroundTexture, ScaleMode.StretchToFill, false);

            DrawImageSlotBanner(backgroundRect, mode);
            DrawImageRoster(backgroundRect, mode);
            DrawImageBottomButtons(backgroundRect);
        }

        /// <summary>
        /// Purpose: Draws the active player selection banner over the blue slot baked into CharacterSelect.png.
        /// Inputs: backgroundRect is the screen-space area where the imported background is drawn; mode controls one-slot vs two-slot behavior.
        /// Output: no return value; may switch the active LocalVS player slot when clicked.
        /// </summary>
        /// <param name="backgroundRect">Screen-space rectangle containing the imported character-select background.</param>
        /// <param name="mode">Current game mode.</param>
        private void DrawImageSlotBanner(Rect backgroundRect, GameMode mode)
        {
            // The imported background already contains the blue selection bar.
            // This rectangle is tuned to sit inside that art instead of floating above it.
            Rect bannerRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.255f, 0.194f, 0.490f, 0.060f));

            if (mode != GameMode.LocalVS)
            {
                string label = $"Player 1: {(selectedPlayer1 != null ? selectedPlayer1.DisplayName : "Choose Hero")}";
                GUI.Label(bannerRect, label, imageBannerStyle);
                return;
            }

            float gap = bannerRect.width * 0.018f;
            float slotWidth = (bannerRect.width - gap) * 0.5f;
            Rect player1Rect = new Rect(bannerRect.x, bannerRect.y, slotWidth, bannerRect.height);
            Rect player2Rect = new Rect(bannerRect.x + slotWidth + gap, bannerRect.y, slotWidth, bannerRect.height);

            DrawImageSlotToggle(player1Rect, Player1Slot, selectedPlayer1);
            DrawImageSlotToggle(player2Rect, Player2Slot, selectedPlayer2);
        }

        /// <summary>
        /// Purpose: Draws one selectable P1/P2 slot inside the LocalVS banner.
        /// Inputs: rect is the clickable slot area, slot is Player1Slot or Player2Slot, and character is the current selection.
        /// Output: no return value; updates activeSlot when the player clicks this slot.
        /// </summary>
        /// <param name="rect">Screen-space slot rectangle.</param>
        /// <param name="slot">Player slot identifier.</param>
        /// <param name="character">Currently selected character for this slot.</param>
        private void DrawImageSlotToggle(Rect rect, int slot, CharacterData character)
        {
            bool isActive = activeSlot == slot;
            if (isActive)
            {
                DrawRoundedRect(new Rect(rect.x + 5f, rect.y + 6f, rect.width - 10f, rect.height - 12f), new Color(1f, 1f, 1f, 0.18f), Color.white, 18, 2);
            }

            string playerLabel = slot == Player1Slot ? "P1" : "P2";
            string characterName = character != null ? character.DisplayName : "Choose Hero";
            GUI.Label(rect, $"{playerLabel}: {characterName}", imageSubtleBannerStyle);

            if (GUI.Button(rect, GUIContent.none, GetTransparentButtonStyle()))
            {
                AudioManager.Instance?.PlayButtonClickSFX();
                activeSlot = slot;
            }
        }

        /// <summary>
        /// Purpose: Draws the six imported role cards and wires each card to the existing character selection logic.
        /// Inputs: backgroundRect is the screen-space area where the imported background is drawn; mode controls LocalVS duplicate prevention.
        /// Output: no return value; calls HandleCharacterClicked when a role card is selected.
        /// </summary>
        /// <param name="backgroundRect">Screen-space rectangle containing the imported character-select background.</param>
        /// <param name="mode">Current game mode.</param>
        private void DrawImageRoster(Rect backgroundRect, GameMode mode)
        {
            if (roster == null || roster.Length == 0)
            {
                Rect messageRect = GetNormalizedRect(backgroundRect, 0.25f, 0.42f, 0.5f, 0.12f);
                GUI.Label(messageRect, "No CharacterData assets found in Resources/Characters.", lockedLabelStyle);
                return;
            }

            // Card centers are tuned to the six empty card wells baked into CharacterSelect.png.
            float[] centerXs = { 0.272f, 0.500f, 0.728f };
            float[] centerYs = { 0.426f, 0.654f };
            float cardSize = Mathf.Min(backgroundRect.width * 0.165f, backgroundRect.height * 0.245f);
            int visibleCount = Mathf.Min(ImageRoleCharacterIds.Length, RoleTextureCount);

            for (int i = 0; i < visibleCount; i++)
            {
                CharacterData character = FindRosterCharacterById(ImageRoleCharacterIds[i]);
                Texture2D roleTexture = GetRoleTextureForCharacterId(ImageRoleCharacterIds[i]);
                if (character == null || roleTexture == null)
                {
                    continue;
                }

                int column = i % 3;
                int row = i / 3;
                Vector2 center = new Vector2(
                    backgroundRect.x + backgroundRect.width * centerXs[column],
                    backgroundRect.y + backgroundRect.height * centerYs[row]);
                Rect cardRect = new Rect(center.x - cardSize * 0.5f, center.y - cardSize * 0.5f, cardSize, cardSize);

                bool isSelectedByActiveSlot = IsSelectedByActiveSlot(character);
                bool isTakenByOtherLocalPlayer = mode == GameMode.LocalVS && IsTakenByOtherLocalPlayer(character);
                DrawImageCharacterCard(PixelSnapRect(cardRect), character, roleTexture, isSelectedByActiveSlot, isTakenByOtherLocalPlayer, i * 0.55f);
            }
        }

        /// <summary>
        /// Purpose: Finds the CharacterData that belongs to a specific imported role image slot.
        /// Inputs: characterId is the stable id expected by the role texture mapping.
        /// Output: the matching CharacterData from the loaded roster, or null when the data asset is missing.
        /// </summary>
        /// <param name="characterId">Stable character id, such as "bubble_ranger" or "star_mage".</param>
        /// <returns>The matching CharacterData asset, or null if none exists.</returns>
        private CharacterData FindRosterCharacterById(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            // Search the currently loaded roster first so this screen uses the same data source as its selection state.
            for (int i = 0; i < roster.Length; i++)
            {
                CharacterData character = roster[i];
                if (character != null &&
                    string.Equals(character.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return character;
                }
            }

            return CharacterRoster.FindById(characterId);
        }

        /// <summary>
        /// Purpose: Gets the imported role card texture that visually represents a character id.
        /// Inputs: characterId is the stable id used by CharacterData.
        /// Output: the matching Texture2D, or null when the image is missing.
        /// </summary>
        /// <param name="characterId">Stable character id used to look up the role art.</param>
        /// <returns>The imported role texture for this character id, or null if not loaded.</returns>
        private Texture2D GetRoleTextureForCharacterId(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            return roleTexturesByCharacterId.TryGetValue(characterId, out Texture2D texture)
                ? texture
                : null;
        }

        /// <summary>
        /// Purpose: Draws one imported character card with selection feedback and an invisible click target.
        /// Inputs: rect is the card slot, character is the backing data, texture is the imported role art, selection flags describe current state, and animationPhase offsets idle motion.
        /// Output: no return value; updates selected character data when clicked.
        /// </summary>
        /// <param name="rect">Screen-space card rectangle.</param>
        /// <param name="character">CharacterData represented by this card.</param>
        /// <param name="texture">Imported role card image.</param>
        /// <param name="isSelected">True when this card is selected by the active player slot.</param>
        /// <param name="isTaken">True when the other LocalVS player already picked this card.</param>
        /// <param name="animationPhase">Idle animation offset used so cards do not move in perfect sync.</param>
        private void DrawImageCharacterCard(Rect rect, CharacterData character, Texture2D texture, bool isSelected, bool isTaken, float animationPhase)
        {
            if (character == null || texture == null)
            {
                return;
            }

            bool isHovered = Event.current != null && rect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered &&
                (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * CardFloatSpeed + animationPhase) * rect.height * CardFloatAmount;

            Rect drawRect = rect;
            drawRect.y += idleOffset;

            if (isHovered && !isTaken)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.025f, 1.025f);
            }

            if (isPressed && !isTaken)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.975f, 0.975f);
                drawRect.y += rect.height * 0.018f;
            }

            if (isSelected)
            {
                DrawRoundedRect(new Rect(drawRect.x - 6f, drawRect.y - 6f, drawRect.width + 12f, drawRect.height + 12f), new Color(1f, 1f, 1f, 0.18f), character.ThemeColor, 18, 3);
            }

            Color previousColor = GUI.color;
            GUI.color = isTaken ? new Color(1f, 1f, 1f, 0.48f) : Color.white;
            GUI.DrawTexture(PixelSnapRect(drawRect), texture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            if (isSelected)
            {
                Rect readyRect = new Rect(drawRect.xMax - drawRect.width * 0.39f, drawRect.y + drawRect.height * 0.045f, drawRect.width * 0.34f, drawRect.height * 0.18f);
                GUI.DrawTexture(PixelSnapRect(readyRect), readyBadgeTexture, ScaleMode.ScaleToFit, true);
            }
            else if (isTaken)
            {
                Rect pickedRect = new Rect(drawRect.x + drawRect.width * 0.18f, drawRect.y + drawRect.height * 0.42f, drawRect.width * 0.64f, drawRect.height * 0.15f);
                DrawRoundedRect(pickedRect, new Color(0.42f, 0.47f, 0.5f, 0.82f), Color.white, 14, 2);
                GUI.Label(pickedRect, "PICKED", smallPillStyle);
            }

            if (GUI.Button(rect, GUIContent.none, GetTransparentButtonStyle()))
            {
                HandleCharacterClicked(character, isTaken);
            }
        }

        /// <summary>
        /// Purpose: Draws Back and Continue with imported button art while preserving the original navigation behavior.
        /// Inputs: backgroundRect is the screen-space area where CharacterSelect.png was drawn.
        /// Output: no return value; loads ModeSelect or MapSelect when clicked.
        /// </summary>
        /// <param name="backgroundRect">Screen-space rectangle containing the imported character-select background.</param>
        private void DrawImageBottomButtons(Rect backgroundRect)
        {
            Rect backRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.135f, 0.788f, 0.348f, 0.105f));
            Rect continueRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.518f, 0.788f, 0.348f, 0.105f));

            if (DrawImagePillButton(backRect, backButtonTexture, 0f))
            {
                OnClickBack();
            }

            if (DrawImagePillButton(continueRect, continueButtonTexture, 0.7f))
            {
                OnClickContinue();
            }
        }

        /// <summary>
        /// Purpose: Draws an imported pill button with subtle idle, hover, and pressed feedback.
        /// Inputs: clickRect is the clickable area, texture is the button art, and animationPhase offsets idle motion.
        /// Output: true when clicked; otherwise false.
        /// </summary>
        /// <param name="clickRect">Screen-space clickable button rectangle.</param>
        /// <param name="texture">Imported button texture.</param>
        /// <param name="animationPhase">Time offset for idle floating motion.</param>
        /// <returns>True when the player clicks this button; otherwise false.</returns>
        private bool DrawImagePillButton(Rect clickRect, Texture2D texture, float animationPhase)
        {
            if (texture == null)
            {
                return false;
            }

            bool isHovered = Event.current != null && clickRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered &&
                (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * ButtonFloatSpeed + animationPhase) * clickRect.height * ButtonFloatAmount;

            Rect drawRect = CalculateAspectFitRect(texture, clickRect);
            drawRect.y += idleOffset;

            if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.025f, 1.025f);
            }

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.975f, 0.975f);
                drawRect.y += clickRect.height * 0.02f;
            }

            GUI.DrawTexture(PixelSnapRect(drawRect), texture, ScaleMode.ScaleToFit, true);
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Handles the Back action shared by the fallback and imported-image layouts.
        /// Inputs: no direct parameters; uses SceneFlowManager to leave character select.
        /// Output: no return value; loads the mode select scene.
        /// </summary>
        private void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        /// <summary>
        /// Purpose: Handles the Continue action shared by the fallback and imported-image layouts.
        /// Inputs: no direct parameters; reads current selections and current game mode from GameManager.
        /// Output: no return value; stores selected characters and loads the map select scene.
        /// </summary>
        private void OnClickContinue()
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

        /// <summary>
        /// Purpose: Applies stable runtime sampling for imported character-select art.
        /// Inputs: texture is a loaded UI texture that may be null while assets are missing.
        /// Output: no return value; updates runtime texture sampling settings only.
        /// </summary>
        /// <param name="texture">Texture to prepare for IMGUI drawing.</param>
        private void ApplyCharacterSelectTextureSettings(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            // Bilinear sampling keeps the high-resolution PNGs smooth while pixel snapping avoids sub-pixel blur.
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Creates or returns the invisible style used for image button hitboxes.
        /// Inputs: no direct parameters.
        /// Output: a GUIStyle with no visible normal, hover, active, or focused state.
        /// </summary>
        /// <returns>A transparent GUIStyle used by image-backed buttons and cards.</returns>
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
        /// Purpose: Converts normalized coordinates into a screen rectangle inside a parent rectangle.
        /// Inputs: parent is the containing rectangle; x, y, width, and height are normalized from 0 to 1.
        /// Output: a Rect in screen pixels.
        /// </summary>
        /// <param name="parent">Screen-space parent rectangle.</param>
        /// <param name="x">Normalized horizontal offset from the parent left edge.</param>
        /// <param name="y">Normalized vertical offset from the parent top edge.</param>
        /// <param name="width">Normalized width relative to the parent width.</param>
        /// <param name="height">Normalized height relative to the parent height.</param>
        /// <returns>A screen-space rectangle based on normalized input values.</returns>
        private Rect GetNormalizedRect(Rect parent, float x, float y, float width, float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        /// <summary>
        /// Purpose: Fits a texture into a target rectangle without stretching its aspect ratio.
        /// Inputs: texture is the source image; targetRect is the maximum available drawing area.
        /// Output: a centered Rect that preserves the texture aspect ratio.
        /// </summary>
        /// <param name="texture">Texture whose dimensions define the desired aspect ratio.</param>
        /// <param name="targetRect">Maximum screen-space rectangle available for drawing.</param>
        /// <returns>A centered rectangle that fits inside targetRect.</returns>
        private Rect CalculateAspectFitRect(Texture2D texture, Rect targetRect)
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
        /// Purpose: Scales a rectangle around its center point.
        /// Inputs: rect is the source rectangle; scaleX and scaleY are horizontal and vertical multipliers.
        /// Output: a new Rect with the same center and scaled size.
        /// </summary>
        /// <param name="rect">Source rectangle to scale.</param>
        /// <param name="scaleX">Horizontal scale multiplier.</param>
        /// <param name="scaleY">Vertical scale multiplier.</param>
        /// <returns>A scaled rectangle that keeps the same center as rect.</returns>
        private Rect ScaleRectAroundCenter(Rect rect, float scaleX, float scaleY)
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
        /// Purpose: Aligns a rectangle to whole screen pixels before drawing imported UI art.
        /// Inputs: rect is the floating-point IMGUI rectangle produced by layout or animation.
        /// Output: a Rect with rounded position and size to reduce sub-pixel texture blur.
        /// </summary>
        /// <param name="rect">Source rectangle in screen-space pixels.</param>
        /// <returns>A pixel-aligned rectangle with the same approximate bounds.</returns>
        private Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Round(rect.width),
                Mathf.Round(rect.height));
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
                OnClickBack();
            }

            if (SimpleUIFactory.FixedPrimaryButton(continueRect, "CONTINUE"))
            {
                OnClickContinue();
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

            imageBannerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(imageBannerStyle, new Color(0.08f, 0.28f, 0.45f, 1f));

            imageSubtleBannerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };
            LockTextColor(imageSubtleBannerStyle, new Color(0.08f, 0.28f, 0.45f, 1f));
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
