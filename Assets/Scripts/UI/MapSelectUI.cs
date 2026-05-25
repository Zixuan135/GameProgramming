using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Map selection callbacks that store selected map type before starting battle.
    /// </summary>
    public class MapSelectUI : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/MapSelect/MapSelect";
        private const string CandyParkCardResourcePath = "UI/MapSelect/Map1";
        private const string SnowfieldCardResourcePath = "UI/MapSelect/Map2";
        private const string JellyMazeCardResourcePath = "UI/MapSelect/Map3";
        private const string SelectedBadgeResourcePath = "UI/MapSelect/Selected";
        private const string StartMapButtonResourcePath = "UI/MapSelect/StartMap";
        private const string BackButtonResourcePath = "UI/MapSelect/Back3";

        private BattleMapType selectedMapType = BattleMapType.Default;
        private bool hasInitializedSelection;
        private Vector2 mapScrollPosition;

        private readonly Color defaultAccent = new Color(0.1f, 0.72f, 1f, 1f);
        private readonly Color snowfieldAccent = new Color(0.32f, 0.78f, 1f, 1f);
        private readonly Color mazeAccent = new Color(0.66f, 0.48f, 1f, 1f);

        private Texture2D backgroundTexture;
        private Texture2D candyParkCardTexture;
        private Texture2D snowfieldCardTexture;
        private Texture2D jellyMazeCardTexture;
        private Texture2D selectedBadgeTexture;
        private Texture2D startMapButtonTexture;
        private Texture2D backButtonTexture;
        private GUIStyle transparentButtonStyle;

        /// <summary>
        /// Purpose: Loads the illustrated map-selection assets before the screen is first drawn.
        /// Inputs: no direct parameters; reads textures stored under Resources/UI/MapSelect.
        /// Output: no return value; caches the textures used by the image-driven interface.
        /// </summary>
        private void Awake()
        {
            LoadMapSelectTextures();
        }

        /// <summary>
        /// Purpose: Reloads textures whenever this screen becomes active, which also supports newly imported assets in the editor.
        /// Inputs: no direct parameters; reads textures stored under Resources/UI/MapSelect.
        /// Output: no return value; refreshes the cached texture references.
        /// </summary>
        private void OnEnable()
        {
            LoadMapSelectTextures();
        }

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            InitializeSelectionIfNeeded();

            if (HasImageMapSelectAssets())
            {
                DrawImageMapSelect();
                return;
            }

            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(930f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.CompactLabelPill("PICK YOUR PLAYGROUND");
            SimpleUIFactory.CompactTitle("Select Map");
            SimpleUIFactory.MapSelectDecorations(panel.width, panel.height);

            if (Screen.width >= 720f)
            {
                DrawMapCardsHorizontal();
            }
            else
            {
                float scrollHeight = Mathf.Clamp(Screen.height - 330f, 180f, 260f);
                mapScrollPosition = GUILayout.BeginScrollView(mapScrollPosition, GUILayout.Height(scrollHeight));
                DrawMapCardsVertical();
                GUILayout.EndScrollView();
            }

            DrawBottomButtons(panel);

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Loads every optional illustration used by the upgraded map-selection screen.
        /// Inputs: no direct parameters; resolves Resource paths defined by this class.
        /// Output: no return value; stores loaded textures or null when an asset is unavailable.
        /// </summary>
        private void LoadMapSelectTextures()
        {
            backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
            candyParkCardTexture = Resources.Load<Texture2D>(CandyParkCardResourcePath);
            snowfieldCardTexture = Resources.Load<Texture2D>(SnowfieldCardResourcePath);
            jellyMazeCardTexture = Resources.Load<Texture2D>(JellyMazeCardResourcePath);
            selectedBadgeTexture = Resources.Load<Texture2D>(SelectedBadgeResourcePath);
            startMapButtonTexture = Resources.Load<Texture2D>(StartMapButtonResourcePath);
            backButtonTexture = Resources.Load<Texture2D>(BackButtonResourcePath);

            ApplyMapSelectTextureSettings(backgroundTexture);
            ApplyMapSelectTextureSettings(candyParkCardTexture);
            ApplyMapSelectTextureSettings(snowfieldCardTexture);
            ApplyMapSelectTextureSettings(jellyMazeCardTexture);
            ApplyMapSelectTextureSettings(selectedBadgeTexture);
            ApplyMapSelectTextureSettings(startMapButtonTexture);
            ApplyMapSelectTextureSettings(backButtonTexture);
        }

        /// <summary>
        /// Purpose: Checks whether the complete image-driven map-selection set can be drawn safely.
        /// Inputs: no direct parameters; reads the cached texture references.
        /// Output: returns true only when the background, cards, selected badge, and both navigation buttons exist.
        /// </summary>
        /// <returns>True when all required illustrated textures are available; otherwise false.</returns>
        private bool HasImageMapSelectAssets()
        {
            return backgroundTexture != null
                && candyParkCardTexture != null
                && snowfieldCardTexture != null
                && jellyMazeCardTexture != null
                && selectedBadgeTexture != null
                && startMapButtonTexture != null
                && backButtonTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the upgraded map-selection interface using supplied high-resolution artwork.
        /// Inputs: no direct parameters; reads cached textures and the selected map state.
        /// Output: no return value; invokes map or navigation callbacks when invisible button regions are clicked.
        /// </summary>
        private void DrawImageMapSelect()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect backgroundRect = PixelSnapRect(CalculateAspectFitRect(backgroundTexture, screenRect));
            GUI.DrawTexture(backgroundRect, backgroundTexture, ScaleMode.StretchToFill, false);

            DrawImageMapCards(backgroundRect);
            DrawImageNavigationButtons(backgroundRect);
        }

        /// <summary>
        /// Purpose: Positions three illustrated map cards inside independent slots in the supplied background.
        /// Inputs: `backgroundRect`, the rendered bounds of the map-selection background.
        /// Output: no return value; draws clickable map options without overlapping the title or bottom controls.
        /// </summary>
        /// <param name="backgroundRect">Rendered bounds that define normalized placement for each map card.</param>
        private void DrawImageMapCards(Rect backgroundRect)
        {
            // Each card gets a separate slot so its shadows and selection badge never collide with neighboring artwork.
            Rect candyParkRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.061f, 0.413f, 0.279f, 0.327f));
            Rect snowfieldRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.361f, 0.413f, 0.279f, 0.327f));
            Rect jellyMazeRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.661f, 0.413f, 0.279f, 0.327f));

            DrawImageMapCard(candyParkRect, candyParkCardTexture, BattleMapType.Default);
            DrawImageMapCard(snowfieldRect, snowfieldCardTexture, BattleMapType.OpenField);
            DrawImageMapCard(jellyMazeRect, jellyMazeCardTexture, BattleMapType.Maze);
        }

        /// <summary>
        /// Purpose: Draws one clickable illustrated map card and its selected marker when active.
        /// Inputs: `clickRect` defines the card slot, `texture` is its art, and `mapType` identifies the stored selection value.
        /// Output: no return value; updates the selected map when this card is clicked.
        /// </summary>
        /// <param name="clickRect">Screen rectangle that accepts clicks for the card.</param>
        /// <param name="texture">Illustrated map card texture to draw in the slot.</param>
        /// <param name="mapType">Map value to store if the card is chosen.</param>
        private void DrawImageMapCard(Rect clickRect, Texture2D texture, BattleMapType mapType)
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
                drawRect = ScaleRectAroundCenter(drawRect, 1.012f);
            }

            drawRect = PixelSnapRect(drawRect);
            GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);

            if (selectedMapType == mapType)
            {
                // The marker occupies the map preview corner rather than the title/description text area.
                Rect selectedRect = new Rect(
                    drawRect.xMax - drawRect.width * 0.405f,
                    drawRect.y + drawRect.height * 0.028f,
                    drawRect.width * 0.375f,
                    drawRect.height * 0.118f);
                GUI.DrawTexture(PixelSnapRect(selectedRect), selectedBadgeTexture, ScaleMode.ScaleToFit, true);
            }

            if (GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle()))
            {
                SelectMapCard(mapType);
            }
        }

        /// <summary>
        /// Purpose: Positions the illustrated start and back buttons inside the reserved footer slots.
        /// Inputs: `backgroundRect`, the rendered bounds of the map-selection background.
        /// Output: no return value; starts the selected map or returns to character selection on click.
        /// </summary>
        /// <param name="backgroundRect">Rendered bounds that define normalized footer placement.</param>
        private void DrawImageNavigationButtons(Rect backgroundRect)
        {
            Rect startRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.115f, 0.814f, 0.371f, 0.094f));
            Rect backRect = PixelSnapRect(GetNormalizedRect(backgroundRect, 0.514f, 0.814f, 0.371f, 0.094f));

            if (DrawImageButton(startRect, startMapButtonTexture))
            {
                OnClickStartSelectedMap();
            }

            if (DrawImageButton(backRect, backButtonTexture))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Draws a polished image button with subtle hover and press feedback.
        /// Inputs: `clickRect` defines its interactive region and `texture` provides the visible button art.
        /// Output: returns true during the GUI event in which the button is activated.
        /// </summary>
        /// <param name="clickRect">Screen rectangle that receives mouse input.</param>
        /// <param name="texture">Button image drawn inside the input rectangle.</param>
        /// <returns>True when the button was clicked in this GUI pass; otherwise false.</returns>
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
        /// Purpose: Provides an invisible button style so imported artwork remains the only visible control surface.
        /// Inputs: no direct parameters.
        /// Output: returns a cached GUIStyle that draws no default Unity button textures.
        /// </summary>
        /// <returns>Transparent GUI style for image-backed click targets.</returns>
        private GUIStyle GetTransparentButtonStyle()
        {
            if (transparentButtonStyle == null)
            {
                transparentButtonStyle = new GUIStyle(GUIStyle.none);
            }

            return transparentButtonStyle;
        }

        /// <summary>
        /// Purpose: Applies crisp UI sampling settings to an imported image used by this screen.
        /// Inputs: `texture`, a Resources-loaded UI texture; null is accepted when an asset has not imported yet.
        /// Output: no return value; sets runtime filtering and edge handling for the texture when present.
        /// </summary>
        /// <param name="texture">Texture whose display settings should be prepared for UI rendering.</param>
        private void ApplyMapSelectTextureSettings(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            // Bilinear filtering preserves smooth illustrated curves while pixel-snapped placement avoids sub-pixel softness.
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Converts normalized design coordinates into a rectangle relative to the fitted background image.
        /// Inputs: `parent` supplies rendered bounds; x, y, width, and height are normalized portions of those bounds.
        /// Output: returns a screen-space rectangle used for artwork placement and click handling.
        /// </summary>
        /// <param name="parent">Rendered background rectangle.</param>
        /// <param name="x">Horizontal position as a fraction of parent width.</param>
        /// <param name="y">Vertical position as a fraction of parent height.</param>
        /// <param name="width">Rectangle width as a fraction of parent width.</param>
        /// <param name="height">Rectangle height as a fraction of parent height.</param>
        /// <returns>Screen-space rectangle corresponding to the normalized design coordinates.</returns>
        private Rect GetNormalizedRect(Rect parent, float x, float y, float width, float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        /// <summary>
        /// Purpose: Aspect-fits artwork into a target slot without stretching its illustration or baked lettering.
        /// Inputs: `texture` supplies native aspect ratio and `targetRect` supplies available screen space.
        /// Output: returns a centered rectangle that fits entirely inside the target slot.
        /// </summary>
        /// <param name="texture">Image whose aspect ratio must be preserved.</param>
        /// <param name="targetRect">Maximum layout slot available for the image.</param>
        /// <returns>Centered screen rectangle with the texture's original aspect ratio.</returns>
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
        /// Purpose: Scales a UI rectangle around its center for lightweight interaction feedback.
        /// Inputs: `rect` is the base placement and `scale` is the requested proportional size.
        /// Output: returns the resized rectangle centered at the original position.
        /// </summary>
        /// <param name="rect">Base rectangle before feedback scaling.</param>
        /// <param name="scale">Uniform scale multiplier.</param>
        /// <returns>Centered scaled rectangle.</returns>
        private Rect ScaleRectAroundCenter(Rect rect, float scale)
        {
            float width = rect.width * scale;
            float height = rect.height * scale;
            return new Rect(rect.center.x - width * 0.5f, rect.center.y - height * 0.5f, width, height);
        }

        /// <summary>
        /// Purpose: Aligns image edges to complete pixels to prevent avoidable texture blur in IMGUI rendering.
        /// Inputs: `rect`, a screen-space placement rectangle.
        /// Output: returns the same rectangle rounded to whole-pixel coordinates and size.
        /// </summary>
        /// <param name="rect">Screen-space rectangle to align.</param>
        /// <returns>Pixel-aligned rectangle.</returns>
        private Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Round(rect.width),
                Mathf.Round(rect.height));
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
            const float horizontalPadding = 50f;
            const float buttonGap = 12f;

            float buttonWidth = (panel.width - horizontalPadding * 2f - buttonGap) * 0.5f;
            float buttonY = panel.height - 78f;
            Rect startRect = new Rect(horizontalPadding, buttonY, buttonWidth, buttonHeight);
            Rect backRect = new Rect(horizontalPadding + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight);

            if (SimpleUIFactory.FixedPrimaryButton(startRect, "START MAP"))
            {
                OnClickStartSelectedMap();
            }

            if (SimpleUIFactory.FixedSecondaryButton(backRect, "BACK"))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Performs initialize selection if needed for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void InitializeSelectionIfNeeded()
        {
            if (hasInitializedSelection)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                selectedMapType = GameManager.Instance.CurrentMapType;
            }

            hasInitializedSelection = true;
        }

        /// <summary>
        /// Purpose: Checks whether the current session is selecting a map for AI Battle.
        /// Inputs: no direct parameters; reads GameManager when available.
        /// Output: returns true when the AI difficulty selector should be visible.
        /// </summary>
        /// <returns>True when current mode is AI Battle; otherwise false.</returns>
        private bool IsAIBattleMode()
        {
            return GameManager.Instance != null && GameManager.Instance.CurrentGameMode == GameMode.AIBattle;
        }

        /// <summary>
        /// Purpose: Draws map cards horizontal in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMapCardsHorizontal()
        {
            GUILayout.BeginHorizontal();
            DrawDefaultMapCard();
            GUILayout.Space(14f);
            DrawOpenFieldMapCard();
            GUILayout.Space(14f);
            DrawMazeMapCard();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        /// <summary>
        /// Purpose: Draws map cards vertical in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMapCardsVertical()
        {
            DrawDefaultMapCard();
            DrawOpenFieldMapCard();
            DrawMazeMapCard();
        }

        /// <summary>
        /// Purpose: Draws default map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawDefaultMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Candy Park",
                "BALANCED",
                "Balanced candy paths.",
                defaultAccent,
                new Color(0.58f, 0.92f, 0.72f, 1f),
                new Color(1f, 0.86f, 0.48f, 1f),
                new Color(0.48f, 0.82f, 1f, 1f),
                selectedMapType == BattleMapType.Default,
                SimpleUIFactory.MapPreviewPattern.Balanced))
            {
                OnSelectDefault();
            }
        }

        /// <summary>
        /// Purpose: Draws open field map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawOpenFieldMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Snowfield",
                "SNOW",
                "Open icy lanes.",
                snowfieldAccent,
                new Color(0.88f, 0.98f, 1f, 1f),
                new Color(0.42f, 0.88f, 1f, 1f),
                new Color(1f, 0.56f, 0.78f, 1f),
                selectedMapType == BattleMapType.OpenField,
                SimpleUIFactory.MapPreviewPattern.Open))
            {
                OnSelectOpenField();
            }
        }

        /// <summary>
        /// Purpose: Draws maze map card in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawMazeMapCard()
        {
            if (SimpleUIFactory.MapCard(
                "Jelly Maze",
                "TWISTY",
                "Tight jelly corners.",
                mazeAccent,
                new Color(0.24f, 0.18f, 0.42f, 1f),
                new Color(0.48f, 0.36f, 1f, 1f),
                new Color(0.18f, 0.9f, 1f, 1f),
                selectedMapType == BattleMapType.Maze,
                SimpleUIFactory.MapPreviewPattern.Maze))
            {
                OnSelectMaze();
            }
        }

        /// <summary>
        /// Purpose: Handles the select default event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectDefault() => SelectMapCard(BattleMapType.Default);
        /// <summary>
        /// Purpose: Handles the select open field event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectOpenField() => SelectMapCard(BattleMapType.OpenField);
        /// <summary>
        /// Purpose: Handles the select maze event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectMaze() => SelectMapCard(BattleMapType.Maze);

        /// <summary>
        /// Purpose: Performs select map card for this component.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        private void SelectMapCard(BattleMapType mapType)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            selectedMapType = mapType;
            GameManager.Instance?.SetMapType(selectedMapType);
        }

        /// <summary>
        /// Purpose: Handles the start selected map button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickStartSelectedMap()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetMapType(selectedMapType);

            if (IsAIBattleMode())
            {
                SceneFlowManager.Instance?.LoadDifficultySelect();
                return;
            }

            SceneFlowManager.Instance?.LoadBattle();
        }

        /// <summary>
        /// Purpose: Handles the back button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadCharacterSelect();
        }

        /// <summary>
        /// Purpose: Formats map name for display or logging.
        /// Inputs: `mapType`; may also read serialized fields and current runtime state.
        /// Output: a `string` value.
        /// </summary>
        /// <param name="mapType">Input value used by this method.</param>
        /// <returns>a `string` value.</returns>
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
