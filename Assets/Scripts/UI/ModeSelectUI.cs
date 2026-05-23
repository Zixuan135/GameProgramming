using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Mode selection callbacks that store selected game mode.
    /// </summary>
    public class ModeSelectUI : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/ModeSelect/SelectMode";
        private const string SinglePlayerButtonResourcePath = "UI/ModeSelect/SinglePlayer";
        private const string AiBattleButtonResourcePath = "UI/ModeSelect/AIBattle";
        private const string LocalVsButtonResourcePath = "UI/ModeSelect/LocalVS";
        private const string BackButtonResourcePath = "UI/ModeSelect/Back";
        private const float CardFloatSpeed = 2.2f;
        private const float CardFloatAmount = 0.005f;
        private const float ButtonFloatSpeed = 2.8f;
        private const float ButtonFloatAmount = 0.004f;

        private readonly Color singlePlayerColor = new Color(0.1f, 0.72f, 1f, 1f);
        private readonly Color aiBattleColor = new Color(1f, 0.55f, 0.18f, 1f);
        private readonly Color localVsColor = new Color(0.52f, 0.9f, 0.35f, 1f);
        private Texture2D backgroundTexture;
        private Texture2D singlePlayerTexture;
        private Texture2D aiBattleTexture;
        private Texture2D localVsTexture;
        private Texture2D backTexture;
        private GUIStyle transparentButtonStyle;

        /// <summary>
        /// Purpose: Loads the image assets before the first OnGUI pass draws the mode select screen.
        /// Inputs: no direct parameters; reads Texture2D assets from the Resources/UI/ModeSelect folder.
        /// Output: no return value; caches textures for image-based drawing.
        /// </summary>
        private void Awake()
        {
            LoadModeSelectTextures();
        }

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            if (HasImageModeSelectAssets())
            {
                DrawImageModeSelect();
                return;
            }

            SimpleUIFactory.DrawCandyBackground();
            Rect panel = SimpleUIFactory.CenteredRect(900f, 560f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.LabelPill("CHOOSE YOUR ROUND");
            SimpleUIFactory.Title("Select Mode");
            SimpleUIFactory.Body("Choose how you want to play today.");

            if (Screen.width >= 820f)
            {
                DrawModeCardsHorizontal();
            }
            else
            {
                DrawModeCardsVertical();
            }

            GUILayout.Space(6f);
            if (SimpleUIFactory.SecondaryButton("BACK"))
            {
                OnClickBack();
            }

            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Loads the full-screen mode select art and the four clickable image buttons.
        /// Inputs: no direct parameters; uses Resources.Load paths without file extensions.
        /// Output: no return value; assigns cached Texture2D references or leaves them null if missing.
        /// </summary>
        private void LoadModeSelectTextures()
        {
            backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
            singlePlayerTexture = Resources.Load<Texture2D>(SinglePlayerButtonResourcePath);
            aiBattleTexture = Resources.Load<Texture2D>(AiBattleButtonResourcePath);
            localVsTexture = Resources.Load<Texture2D>(LocalVsButtonResourcePath);
            backTexture = Resources.Load<Texture2D>(BackButtonResourcePath);

            ApplyModeSelectTextureSettings(backgroundTexture, FilterMode.Bilinear);
            ApplyModeSelectTextureSettings(singlePlayerTexture, FilterMode.Point);
            ApplyModeSelectTextureSettings(aiBattleTexture, FilterMode.Point);
            ApplyModeSelectTextureSettings(localVsTexture, FilterMode.Point);
            ApplyModeSelectTextureSettings(backTexture, FilterMode.Point);
        }

        /// <summary>
        /// Purpose: Checks whether the complete image-based mode select screen can be drawn.
        /// Inputs: no direct parameters; reads cached Texture2D fields.
        /// Output: true when all required mode select art is loaded, otherwise false.
        /// </summary>
        /// <returns>True if all mode select images are available; otherwise false.</returns>
        private bool HasImageModeSelectAssets()
        {
            return backgroundTexture != null
                   && singlePlayerTexture != null
                   && aiBattleTexture != null
                   && localVsTexture != null
                   && backTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the imported mode select background and places clickable image cards into its empty slots.
        /// Inputs: no direct parameters; reads cached textures and current mouse state.
        /// Output: no return value; invokes mode selection or back callbacks when the player clicks a button.
        /// </summary>
        private void DrawImageModeSelect()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect backgroundRect = PixelSnapRect(CalculateAspectFitRect(backgroundTexture, screenRect));
            GUI.DrawTexture(backgroundRect, backgroundTexture, ScaleMode.StretchToFill, false);

            DrawImageModeButtons(backgroundRect);
        }

        /// <summary>
        /// Purpose: Lays out the three mode cards and the back button over the imported background art.
        /// Inputs: backgroundRect is the screen rectangle where SelectMode.png was drawn.
        /// Output: no return value; calls the same callbacks used by the fallback generated UI.
        /// </summary>
        /// <param name="backgroundRect">Screen-space rectangle containing the mode select background.</param>
        private void DrawImageModeButtons(Rect backgroundRect)
        {
            // These normalized slots are tuned to the baked frames in SelectMode.png.
            // The click rectangles are a touch larger than the visible art so the buttons feel forgiving,
            // while the card artwork itself is aspect-fitted and pixel-snapped by DrawImageCardButton.
            Rect singleRect = GetNormalizedRect(backgroundRect, 0.071f, 0.428f, 0.279f, 0.366f);
            Rect aiRect = GetNormalizedRect(backgroundRect, 0.361f, 0.428f, 0.279f, 0.366f);
            Rect localRect = GetNormalizedRect(backgroundRect, 0.650f, 0.428f, 0.279f, 0.366f);
            Rect backRect = GetNormalizedRect(backgroundRect, 0.272f, 0.845f, 0.456f, 0.100f);

            if (DrawImageCardButton(singleRect, singlePlayerTexture, 0f))
            {
                OnSelectSinglePlayer();
            }

            if (DrawImageCardButton(aiRect, aiBattleTexture, 0.75f))
            {
                OnSelectAIBattle();
            }

            if (DrawImageCardButton(localRect, localVsTexture, 1.5f))
            {
                OnSelectLocalVS();
            }

            if (DrawImagePillButton(backRect, backTexture, 2.25f))
            {
                OnClickBack();
            }
        }

        /// <summary>
        /// Purpose: Draws a mode card image with small idle, hover, and pressed feedback.
        /// Inputs: clickRect is the clickable card slot, texture is the transparent card art, and animationPhase offsets idle motion.
        /// Output: true when the card is clicked; otherwise false.
        /// </summary>
        /// <param name="clickRect">Screen-space rectangle used for both drawing and clicks.</param>
        /// <param name="texture">Transparent mode card texture.</param>
        /// <param name="animationPhase">Offset that keeps the cards from floating in perfect sync.</param>
        /// <returns>True when the player clicked this card; otherwise false.</returns>
        private bool DrawImageCardButton(Rect clickRect, Texture2D texture, float animationPhase)
        {
            bool isHovered = Event.current != null && clickRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered &&
                (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * CardFloatSpeed + animationPhase) *
                clickRect.height *
                CardFloatAmount;

            Rect drawRect = CalculateAspectFitRect(texture, clickRect);
            drawRect.y += idleOffset;

            if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.018f, 1.018f);
            }

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.972f, 0.972f);
                drawRect.y += clickRect.height * 0.018f;
            }

            GUI.DrawTexture(PixelSnapRect(drawRect), texture, ScaleMode.ScaleToFit, true);
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Draws the imported Back button with the same lively feedback as other image buttons.
        /// Inputs: clickRect is the button slot, texture is the transparent button art, and animationPhase offsets idle motion.
        /// Output: true when the back button is clicked; otherwise false.
        /// </summary>
        /// <param name="clickRect">Screen-space rectangle used for both drawing and clicks.</param>
        /// <param name="texture">Transparent back button texture.</param>
        /// <param name="animationPhase">Offset that controls idle floating motion.</param>
        /// <returns>True when the player clicked Back; otherwise false.</returns>
        private bool DrawImagePillButton(Rect clickRect, Texture2D texture, float animationPhase)
        {
            bool isHovered = Event.current != null && clickRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered &&
                (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * ButtonFloatSpeed + animationPhase) *
                clickRect.height *
                ButtonFloatAmount;

            Rect drawRect = CalculateAspectFitRect(texture, clickRect);
            drawRect.y += idleOffset;

            if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 1.035f, 1.035f);
            }

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, 0.972f, 0.972f);
                drawRect.y += clickRect.height * 0.02f;
            }

            GUI.DrawTexture(PixelSnapRect(drawRect), texture, ScaleMode.ScaleToFit, true);
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Applies stable sampling settings for imported mode select UI textures inside IMGUI.
        /// Inputs: texture is the loaded UI image and filterMode controls whether it is smoothed or pixel-sharp.
        /// Output: no return value; updates runtime texture sampling settings only.
        /// </summary>
        /// <param name="texture">Loaded UI texture that should stay clean while being scaled on screen.</param>
        /// <param name="filterMode">Texture sampling mode used by Unity while drawing this image.</param>
        private void ApplyModeSelectTextureSettings(Texture2D texture, FilterMode filterMode)
        {
            if (texture == null)
            {
                return;
            }

            // The background benefits from smoothing, but button art reads better with point sampling.
            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }

        /// <summary>
        /// Purpose: Creates or returns the style used by invisible click hitboxes.
        /// Inputs: no direct parameters.
        /// Output: a GUIStyle with no visible hover, active, or focused state.
        /// </summary>
        /// <returns>A transparent GUIStyle for image button hitboxes.</returns>
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
        /// <returns>A screen-space rectangle based on the normalized input values.</returns>
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
        /// Inputs: texture is the source image; targetRect is the maximum available drawing space.
        /// Output: a centered Rect that preserves the texture aspect ratio.
        /// </summary>
        /// <param name="texture">Texture whose width and height define the desired aspect ratio.</param>
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
        /// Inputs: rect is the original area; scaleX and scaleY are horizontal and vertical multipliers.
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
        /// Inputs: rect is the floating-point IMGUI rectangle produced by aspect fitting or animation.
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
        /// Purpose: Draws mode cards horizontal in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawModeCardsHorizontal()
        {
            GUILayout.BeginHorizontal();

            if (SimpleUIFactory.CompactModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.CompactModeCard(
                "AI Battle",
                "AI",
                "Fight a toy opponent and pick its difficulty before battle.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            GUILayout.Space(14f);
            if (SimpleUIFactory.CompactModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle.",
                localVsColor))
            {
                OnSelectLocalVS();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        /// <summary>
        /// Purpose: Draws mode cards vertical in the current GUI or scene context.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void DrawModeCardsVertical()
        {
            if (SimpleUIFactory.CompactModeCard(
                "Single Player",
                "P1",
                "Practice movement, bombs, items, and map rules alone.",
                singlePlayerColor))
            {
                OnSelectSinglePlayer();
            }

            if (SimpleUIFactory.CompactModeCard(
                "AI Battle",
                "AI",
                "Fight a toy opponent and pick its difficulty before battle.",
                aiBattleColor))
            {
                OnSelectAIBattle();
            }

            if (SimpleUIFactory.CompactModeCard(
                "Local VS",
                "2P",
                "Two players share one keyboard for a couch battle.",
                localVsColor))
            {
                OnSelectLocalVS();
            }
        }

        /// <summary>
        /// Purpose: Handles the select single player event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectSinglePlayer() => SelectMode(GameMode.SinglePlayer);
        /// <summary>
        /// Purpose: Handles the select aibattle event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectAIBattle() => SelectMode(GameMode.AIBattle);
        /// <summary>
        /// Purpose: Handles the select local vs event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnSelectLocalVS() => SelectMode(GameMode.LocalVS);

        /// <summary>
        /// Purpose: Performs select mode for this component.
        /// Inputs: `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="mode">Input value used by this method.</param>
        private void SelectMode(GameMode mode)
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.SetGameMode(mode);
            SceneFlowManager.Instance?.LoadCharacterSelect();
        }

        /// <summary>
        /// Purpose: Handles the back button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickBack()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneFlowManager.Instance?.LoadMainMenu();
        }
    }
}
