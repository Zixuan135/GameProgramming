using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Dedicated AI difficulty selection screen shown after map selection for AI Battle.
    /// Keeping this on its own page avoids overcrowding the map select layout.
    /// </summary>
    public class DifficultySelectUI : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/DifficultySelect/DifficultySelect";
        private const string EasyCardResourcePath = "UI/DifficultySelect/Easy";
        private const string NormalCardResourcePath = "UI/DifficultySelect/Normal";
        private const string HardCardResourcePath = "UI/DifficultySelect/Hard";
        private const string SelectedBadgeResourcePath = "UI/DifficultySelect/Selected2";
        private const string StartBattleButtonResourcePath = "UI/DifficultySelect/StartBattle";
        private const string BackButtonResourcePath = "UI/DifficultySelect/Back4";

        private AIDifficulty selectedDifficulty = AIDifficulty.Normal;
        private bool hasInitializedSelection;

        private readonly Color easyColor = new Color(0.45f, 0.9f, 0.36f, 1f);
        private readonly Color normalColor = new Color(0.12f, 0.72f, 1f, 1f);
        private readonly Color hardColor = new Color(1f, 0.48f, 0.24f, 1f);

        private Texture2D backgroundTexture;
        private Texture2D easyCardTexture;
        private Texture2D normalCardTexture;
        private Texture2D hardCardTexture;
        private Texture2D selectedBadgeTexture;
        private Texture2D startBattleButtonTexture;
        private Texture2D backButtonTexture;
        private Texture2D mapLabelBackgroundTexture;
        private GUIStyle transparentButtonStyle;
        private GUIStyle mapLabelStyle;

        /// <summary>
        /// Purpose: Loads optional illustrated textures before the difficulty screen is first displayed.
        /// Inputs: no direct parameters; reads texture assets stored under Resources/UI/DifficultySelect.
        /// Output: no return value; caches the image set used by the upgraded screen.
        /// </summary>
        private void Awake()
        {
            LoadDifficultySelectTextures();
        }

        /// <summary>
        /// Purpose: Reloads illustrated textures whenever this component becomes active in the editor or in play mode.
        /// Inputs: no direct parameters; reads texture assets stored under Resources/UI/DifficultySelect.
        /// Output: no return value; refreshes cached texture references after resource imports.
        /// </summary>
        private void OnEnable()
        {
            LoadDifficultySelectTextures();
        }

        /// <summary>
        /// Purpose: Draws the full difficulty selection screen.
        /// Inputs: no direct parameters; reads GameManager for current map and selected difficulty.
        /// Output: no return value; button clicks update the selected difficulty or change scenes.
        /// </summary>
        private void OnGUI()
        {
            InitializeSelectionIfNeeded();

            if (HasImageDifficultySelectAssets())
            {
                DrawImageDifficultySelect();
                return;
            }

            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(900f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.CompactLabelPill("AI RIVAL SETUP");
            SimpleUIFactory.CompactTitle("Choose Difficulty");
            SimpleUIFactory.Body("Map: " + FormatMapName(GetCurrentMapType()));
            SimpleUIFactory.MapSelectDecorations(panel.width, panel.height);

            DrawDifficultyCards();
            DrawBottomButtons(panel);

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Loads every illustration needed for the image-driven AI difficulty screen.
        /// Inputs: no direct parameters; resolves the Resource paths declared by this class.
        /// Output: no return value; stores loaded textures or null when an image is unavailable.
        /// </summary>
        private void LoadDifficultySelectTextures()
        {
            backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
            easyCardTexture = Resources.Load<Texture2D>(EasyCardResourcePath);
            normalCardTexture = Resources.Load<Texture2D>(NormalCardResourcePath);
            hardCardTexture = Resources.Load<Texture2D>(HardCardResourcePath);
            selectedBadgeTexture = Resources.Load<Texture2D>(SelectedBadgeResourcePath);
            startBattleButtonTexture = Resources.Load<Texture2D>(StartBattleButtonResourcePath);
            backButtonTexture = Resources.Load<Texture2D>(BackButtonResourcePath);

            ApplyDifficultySelectTextureSettings(backgroundTexture);
            ApplyDifficultySelectTextureSettings(easyCardTexture);
            ApplyDifficultySelectTextureSettings(normalCardTexture);
            ApplyDifficultySelectTextureSettings(hardCardTexture);
            ApplyDifficultySelectTextureSettings(selectedBadgeTexture);
            ApplyDifficultySelectTextureSettings(startBattleButtonTexture);
            ApplyDifficultySelectTextureSettings(backButtonTexture);
        }

        /// <summary>
        /// Purpose: Determines whether the upgraded difficulty screen has a complete safe-to-render image set.
        /// Inputs: no direct parameters; reads cached texture references.
        /// Output: returns true only when the background, cards, badge, and navigation controls are loaded.
        /// </summary>
        /// <returns>True when the illustrated screen can be used; otherwise false.</returns>
        private bool HasImageDifficultySelectAssets()
        {
            return backgroundTexture != null
                && easyCardTexture != null
                && normalCardTexture != null
                && hardCardTexture != null
                && selectedBadgeTexture != null
                && startBattleButtonTexture != null
                && backButtonTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the upgraded image-driven difficulty screen without stretching supplied artwork.
        /// Inputs: no direct parameters; reads loaded images, selected difficulty, and selected map.
        /// Output: no return value; delegates card and footer click actions to existing gameplay callbacks.
        /// </summary>
        private void DrawImageDifficultySelect()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect backgroundRect = PixelSnapRect(CalculateAspectFitRect(backgroundTexture, screenRect));
            GUI.DrawTexture(backgroundRect, backgroundTexture, ScaleMode.StretchToFill, false);

            DrawDynamicMapName(backgroundRect);
            DrawImageDifficultyCards(backgroundRect);
            DrawImageNavigationButtons(backgroundRect);
        }

        /// <summary>
        /// Purpose: Draws the selected map name over the background's decorative map label area.
        /// Inputs: `backgroundRect` defines the fitted illustration bounds; current map data comes from GameManager.
        /// Output: no return value; keeps the displayed map accurate when the player chose a non-default map.
        /// </summary>
        /// <param name="backgroundRect">Rendered difficulty-screen background bounds.</param>
        private void DrawDynamicMapName(Rect backgroundRect)
        {
            // The source illustration includes a sample map label, so a matching soft pill keeps runtime map selection truthful.
            Rect mapLabelRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.338f, 0.253f, 0.324f, 0.061f));
            GUI.DrawTexture(mapLabelRect, GetMapLabelBackgroundTexture(), ScaleMode.StretchToFill, true);

            GUIStyle style = GetMapLabelStyle();
            style.fontSize = Mathf.Max(15, Mathf.RoundToInt(mapLabelRect.height * 0.54f));
            GUI.Label(mapLabelRect, "Map: " + FormatMapName(GetCurrentMapType()), style);
        }

        /// <summary>
        /// Purpose: Builds a small reusable rounded background for the dynamic map-name label.
        /// Inputs: no direct parameters.
        /// Output: returns a cached transparent texture with a warm fill and subtle outline.
        /// </summary>
        /// <returns>Rounded texture used behind the selected map name.</returns>
        private Texture2D GetMapLabelBackgroundTexture()
        {
            if (mapLabelBackgroundTexture != null)
            {
                return mapLabelBackgroundTexture;
            }

            const int width = 420;
            const int height = 72;
            const int radius = 30;
            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color fill = new Color(1f, 0.965f, 0.83f, 0.98f);
            Color edge = new Color(1f, 0.91f, 0.63f, 1f);

            mapLabelBackgroundTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            mapLabelBackgroundTexture.name = "DifficultyMapLabelBackground";
            mapLabelBackgroundTexture.wrapMode = TextureWrapMode.Clamp;
            mapLabelBackgroundTexture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool insideOuter = IsInsideRoundedRect(x, y, width, height, radius);
                    bool insideInner = IsInsideRoundedRect(x - 2, y - 2, width - 4, height - 4, radius - 2);
                    Color color = !insideOuter ? transparent : insideInner ? fill : edge;
                    mapLabelBackgroundTexture.SetPixel(x, y, color);
                }
            }

            mapLabelBackgroundTexture.Apply(false, true);
            return mapLabelBackgroundTexture;
        }

        /// <summary>
        /// Purpose: Tests whether a pixel sits inside a rounded rectangle during the label texture build.
        /// Inputs: x and y identify the pixel, width and height define the rectangle, and radius defines its corners.
        /// Output: returns true when the pixel should be colored as part of the rounded shape.
        /// </summary>
        /// <param name="x">Pixel x position relative to the tested rectangle.</param>
        /// <param name="y">Pixel y position relative to the tested rectangle.</param>
        /// <param name="width">Tested rectangle width in pixels.</param>
        /// <param name="height">Tested rectangle height in pixels.</param>
        /// <param name="radius">Rounded-corner radius in pixels.</param>
        /// <returns>True when the pixel is within the rounded rectangle; otherwise false.</returns>
        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return false;
            }

            int nearestX = Mathf.Clamp(x, radius, width - radius - 1);
            int nearestY = Mathf.Clamp(y, radius, height - radius - 1);
            int deltaX = x - nearestX;
            int deltaY = y - nearestY;
            return deltaX * deltaX + deltaY * deltaY <= radius * radius;
        }

        /// <summary>
        /// Purpose: Provides a readable stable text style for the dynamic selected-map label.
        /// Inputs: no direct parameters.
        /// Output: returns a cached GUIStyle whose color never changes on pointer interaction.
        /// </summary>
        /// <returns>Map label text style.</returns>
        private GUIStyle GetMapLabelStyle()
        {
            if (mapLabelStyle == null)
            {
                Color textColor = new Color(0.12f, 0.33f, 0.49f, 1f);
                mapLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = textColor },
                    hover = { textColor = textColor },
                    active = { textColor = textColor },
                    focused = { textColor = textColor }
                };
            }

            return mapLabelStyle;
        }

        /// <summary>
        /// Purpose: Positions the three difficulty illustrations in isolated card slots.
        /// Inputs: `backgroundRect` provides the fitted page bounds used for normalized placement.
        /// Output: no return value; draws all selectable difficulty options.
        /// </summary>
        /// <param name="backgroundRect">Rendered difficulty-screen background bounds.</param>
        private void DrawImageDifficultyCards(Rect backgroundRect)
        {
            Rect easyRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.064f, 0.402f, 0.276f, 0.328f));
            Rect normalRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.362f, 0.402f, 0.276f, 0.328f));
            Rect hardRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.660f, 0.402f, 0.276f, 0.328f));

            DrawImageDifficultyCard(easyRect, easyCardTexture, AIDifficulty.Easy);
            DrawImageDifficultyCard(normalRect, normalCardTexture, AIDifficulty.Normal);
            DrawImageDifficultyCard(hardRect, hardCardTexture, AIDifficulty.Hard);
        }

        /// <summary>
        /// Purpose: Draws one illustrated difficulty card with light interaction feedback and a selected marker.
        /// Inputs: `clickRect` defines its slot, `texture` supplies its art, and `difficulty` identifies its value.
        /// Output: no return value; selecting the card stores its difficulty through the existing callback.
        /// </summary>
        /// <param name="clickRect">Screen region that accepts input for this card.</param>
        /// <param name="texture">Difficulty card artwork.</param>
        /// <param name="difficulty">Difficulty preset represented by the card.</param>
        private void DrawImageDifficultyCard(Rect clickRect, Texture2D texture, AIDifficulty difficulty)
        {
            Event currentEvent = Event.current;
            bool isHovered = clickRect.Contains(currentEvent.mousePosition);
            bool isPressed = isHovered && currentEvent.type == EventType.MouseDown && currentEvent.button == 0;

            Rect drawRect = CalculateAspectFitRect(texture, clickRect);
            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.985f);
            }
            else if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.01f);
            }

            drawRect = PixelSnapRect(drawRect);
            GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);

            if (selectedDifficulty == difficulty)
            {
                // Reserve the artwork's lower empty section for the badge so the descriptive text is never covered.
                Rect badgeSlot = new Rect(
                    drawRect.x + drawRect.width * 0.18f,
                    drawRect.y + drawRect.height * 0.80f,
                    drawRect.width * 0.64f,
                    drawRect.height * 0.12f);
                Rect badgeRect = PixelSnapRect(CalculateAspectFitRect(selectedBadgeTexture, badgeSlot));
                GUI.DrawTexture(badgeRect, selectedBadgeTexture, ScaleMode.ScaleToFit, true);
            }

            if (GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle()))
            {
                SelectDifficulty(difficulty);
            }
        }

        /// <summary>
        /// Purpose: Positions the illustrated Start Battle and Back controls in the footer slots.
        /// Inputs: `backgroundRect` provides fitted page bounds used for normalized placement.
        /// Output: no return value; starts battle or returns to map selection when clicked.
        /// </summary>
        /// <param name="backgroundRect">Rendered difficulty-screen background bounds.</param>
        private void DrawImageNavigationButtons(Rect backgroundRect)
        {
            Rect startRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.115f, 0.815f, 0.371f, 0.091f));
            Rect backRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.514f, 0.815f, 0.371f, 0.091f));

            if (DrawImageButton(startRect, startBattleButtonTexture))
            {
                OnClickStartBattle();
            }

            if (DrawImageButton(backRect, backButtonTexture))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Draws one supplied image as a clickable button with subtle hover and press scale feedback.
        /// Inputs: `clickRect` specifies the hit target and `texture` specifies the visible button art.
        /// Output: returns true only for the GUI event in which the button is activated.
        /// </summary>
        /// <param name="clickRect">Screen area that receives pointer input.</param>
        /// <param name="texture">Artwork displayed within the button area.</param>
        /// <returns>True when clicked during this GUI pass; otherwise false.</returns>
        private bool DrawImageButton(Rect clickRect, Texture2D texture)
        {
            Event currentEvent = Event.current;
            bool isHovered = clickRect.Contains(currentEvent.mousePosition);
            bool isPressed = isHovered && currentEvent.type == EventType.MouseDown && currentEvent.button == 0;

            Rect drawRect = CalculateAspectFitRect(texture, clickRect);
            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.975f);
            }
            else if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.012f);
            }

            GUI.DrawTexture(PixelSnapRect(drawRect), texture, ScaleMode.ScaleToFit, true);
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Supplies an invisible button skin so imported artwork remains the only visible button surface.
        /// Inputs: no direct parameters.
        /// Output: returns a cached transparent GUIStyle used for image-backed hit targets.
        /// </summary>
        /// <returns>Invisible GUI button style.</returns>
        private GUIStyle GetTransparentButtonStyle()
        {
            if (transparentButtonStyle == null)
            {
                transparentButtonStyle = new GUIStyle(GUIStyle.none);
            }

            return transparentButtonStyle;
        }

        /// <summary>
        /// Purpose: Sets display options that keep high-resolution interface artwork clear and prevent repeated edges.
        /// Inputs: `texture`, a Resources-loaded UI texture; null is allowed while assets are still importing.
        /// Output: no return value; updates sampling and wrapping settings for a loaded texture.
        /// </summary>
        /// <param name="texture">Texture prepared for direct GUI rendering.</param>
        private void ApplyDifficultySelectTextureSettings(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            // Smooth source artwork remains clean at multiple window sizes, while rounded placement removes sub-pixel blur.
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Converts normalized design coordinates into a screen rectangle relative to the fitted background.
        /// Inputs: `parent` defines page bounds; x, y, width, and height are fractions of those bounds.
        /// Output: returns a screen-space rectangle for artwork and its click target.
        /// </summary>
        /// <param name="parent">Rendered background bounds.</param>
        /// <param name="x">Horizontal placement as a fraction of parent width.</param>
        /// <param name="y">Vertical placement as a fraction of parent height.</param>
        /// <param name="width">Rectangle width as a fraction of parent width.</param>
        /// <param name="height">Rectangle height as a fraction of parent height.</param>
        /// <returns>Screen-space rectangle matching the design coordinate.</returns>
        private Rect GetNormalizedRect(Rect parent, float x, float y, float width, float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        /// <summary>
        /// Purpose: Fits an image inside a layout slot without distorting its artwork or baked text.
        /// Inputs: `texture` supplies the original aspect ratio and `targetRect` supplies available space.
        /// Output: returns a centered screen rectangle that fits completely within the slot.
        /// </summary>
        /// <param name="texture">Image whose native aspect ratio is retained.</param>
        /// <param name="targetRect">Largest allowed drawing area.</param>
        /// <returns>Aspect-preserving centered drawing rectangle.</returns>
        private Rect CalculateAspectFitRect(Texture2D texture, Rect targetRect)
        {
            if (texture == null || texture.height == 0 || targetRect.height <= 0f)
            {
                return targetRect;
            }

            float textureAspect = (float)texture.width / texture.height;
            float targetAspect = targetRect.width / targetRect.height;
            if (targetAspect > textureAspect)
            {
                float fittedWidth = targetRect.height * textureAspect;
                return new Rect(targetRect.center.x - fittedWidth * 0.5f, targetRect.y, fittedWidth, targetRect.height);
            }

            float fittedHeight = targetRect.width / textureAspect;
            return new Rect(targetRect.x, targetRect.center.y - fittedHeight * 0.5f, targetRect.width, fittedHeight);
        }

        /// <summary>
        /// Purpose: Scales a drawn control around its center for lightweight interaction animation.
        /// Inputs: `rect` is its original placement and `scale` is the requested size multiplier.
        /// Output: returns the resized rectangle centered at the same point.
        /// </summary>
        /// <param name="rect">Base drawing rectangle.</param>
        /// <param name="scale">Uniform size multiplier.</param>
        /// <returns>Centered scaled rectangle.</returns>
        private Rect ScaleRectAroundCenter(Rect rect, float scale)
        {
            float width = rect.width * scale;
            float height = rect.height * scale;
            return new Rect(
                rect.center.x - width * 0.5f,
                rect.center.y - height * 0.5f,
                width,
                height);
        }

        /// <summary>
        /// Purpose: Aligns artwork placement to whole screen pixels to avoid fractional-position softness.
        /// Inputs: `rect`, a calculated GUI rectangle.
        /// Output: returns the same rectangle with rounded position and size components.
        /// </summary>
        /// <param name="rect">Calculated placement rectangle.</param>
        /// <returns>Pixel-aligned placement rectangle.</returns>
        private Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Round(rect.width),
                Mathf.Round(rect.height));
        }

        /// <summary>
        /// Purpose: Initializes the visible selection from the current GameManager session once.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: no return value; caches the selected AI difficulty for this screen.
        /// </summary>
        private void InitializeSelectionIfNeeded()
        {
            if (hasInitializedSelection)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                selectedDifficulty = GameManager.Instance.CurrentAIDifficulty;
            }

            hasInitializedSelection = true;
        }

        /// <summary>
        /// Purpose: Draws all difficulty cards in a responsive row or column.
        /// Inputs: no direct parameters; reads Screen.width to choose layout.
        /// Output: no return value; updates selection if the player clicks a card.
        /// </summary>
        private void DrawDifficultyCards()
        {
            if (Screen.width >= 720f)
            {
                GUILayout.BeginHorizontal();
                DrawDifficultyCard(
                    AIDifficulty.Easy,
                    "Easy",
                    "Slower rival with fewer bomb attempts.",
                    easyColor);
                GUILayout.Space(14f);
                DrawDifficultyCard(
                    AIDifficulty.Normal,
                    "Normal",
                    "Balanced rival for the regular game feel.",
                    normalColor);
                GUILayout.Space(14f);
                DrawDifficultyCard(
                    AIDifficulty.Hard,
                    "Hard",
                    "Faster rival with stronger pressure.",
                    hardColor);
                GUILayout.EndHorizontal();
                GUILayout.Space(14f);
                return;
            }

            DrawDifficultyCard(AIDifficulty.Easy, "Easy", "Slower rival with fewer bomb attempts.", easyColor);
            DrawDifficultyCard(AIDifficulty.Normal, "Normal", "Balanced rival for the regular game feel.", normalColor);
            DrawDifficultyCard(AIDifficulty.Hard, "Hard", "Faster rival with stronger pressure.", hardColor);
        }

        /// <summary>
        /// Purpose: Draws one selectable AI difficulty card.
        /// Inputs: difficulty is the represented preset, title and description are visible text, accentColor styles the card.
        /// Output: no return value; clicking the card stores the selected difficulty in GameManager.
        /// </summary>
        /// <param name="difficulty">AI difficulty represented by this card.</param>
        /// <param name="title">Player-facing card title.</param>
        /// <param name="description">Short explanation of how the AI behaves.</param>
        /// <param name="accentColor">Color used to highlight this difficulty.</param>
        private void DrawDifficultyCard(AIDifficulty difficulty, string title, string description, Color accentColor)
        {
            Rect rect = GUILayoutUtility.GetRect(190f, 150f, GUILayout.ExpandWidth(true));
            bool isSelected = selectedDifficulty == difficulty;

            if (SimpleUIFactory.ChoicePill(rect, string.Empty, isSelected, accentColor))
            {
                SelectDifficulty(difficulty);
            }

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                // Keep text color stable; selection is shown through the card fill and SELECTED badge instead.
                normal = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                hover = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                active = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                focused = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) }
            };

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                hover = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                active = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) },
                focused = { textColor = new Color(0.23f, 0.45f, 0.55f, 1f) }
            };

            GUIStyle badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                hover = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                active = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) },
                focused = { textColor = new Color(0.11f, 0.28f, 0.42f, 1f) }
            };

            Rect titleRect = new Rect(rect.x + 16f, rect.y + 24f, rect.width - 32f, 34f);
            Rect bodyRect = new Rect(rect.x + 28f, rect.y + 66f, rect.width - 56f, 42f);
            Rect badgeRect = new Rect(rect.x + rect.width * 0.5f - 48f, rect.y + 113f, 96f, 22f);

            GUI.Label(titleRect, title.ToUpperInvariant(), titleStyle);
            GUI.Label(bodyRect, description, bodyStyle);
            GUI.Label(badgeRect, isSelected ? "SELECTED" : "PICK", badgeStyle);
        }

        /// <summary>
        /// Purpose: Draws the Start Battle and Back buttons at the bottom of the panel.
        /// Inputs: panel supplies local panel size for fixed button placement.
        /// Output: no return value; clicks start the battle or return to map selection.
        /// </summary>
        /// <param name="panel">Current menu panel rectangle used for bottom button placement.</param>
        private void DrawBottomButtons(Rect panel)
        {
            const float buttonHeight = 50f;
            const float horizontalPadding = 50f;
            const float buttonGap = 12f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.height - 78f;
            Rect startRect = new Rect(horizontalPadding, buttonY, buttonWidth, buttonHeight);
            Rect backRect = new Rect(horizontalPadding + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight);

            if (SimpleUIFactory.FixedPrimaryButton(startRect, "START BATTLE"))
            {
                OnClickStartBattle();
            }

            if (SimpleUIFactory.FixedSecondaryButton(backRect, "BACK"))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Stores the selected difficulty locally and in GameManager.
        /// Inputs: difficulty is the newly selected AI preset.
        /// Output: no return value; the choice survives the next scene load.
        /// </summary>
        /// <param name="difficulty">AI difficulty selected by the player.</param>
        private void SelectDifficulty(AIDifficulty difficulty)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            selectedDifficulty = difficulty;
            GameManager.Instance?.SetAIDifficulty(selectedDifficulty);
        }

        /// <summary>
        /// Purpose: Starts AI Battle using the currently selected difficulty.
        /// Inputs: no direct parameters; writes the selected difficulty to GameManager before loading battle.
        /// Output: no return value; triggers the battle scene flow.
        /// </summary>
        public void OnClickStartBattle()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetAIDifficulty(selectedDifficulty);
            SceneFlowManager.Instance?.LoadBattle();
        }

        /// <summary>
        /// Purpose: Returns to map selection so the player can pick a different map.
        /// Inputs: no direct parameters.
        /// Output: no return value; loads the map selection scene.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadMapSelect();
        }

        /// <summary>
        /// Purpose: Reads the selected battle map from GameManager with a safe fallback.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: returns the current battle map type.
        /// </summary>
        /// <returns>Selected map type, or Default when GameManager is unavailable.</returns>
        private BattleMapType GetCurrentMapType()
        {
            return GameManager.Instance != null
                ? GameManager.Instance.CurrentMapType
                : BattleMapType.Default;
        }

        /// <summary>
        /// Purpose: Converts a map enum into the player-facing map name used on this screen.
        /// Inputs: mapType is the selected map enum.
        /// Output: returns a readable map name.
        /// </summary>
        /// <param name="mapType">Map type to format.</param>
        /// <returns>Readable map name.</returns>
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
    }
}
