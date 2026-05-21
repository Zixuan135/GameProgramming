using BubbleTown.Managers;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BubbleTown.UI
{
    /// <summary>
    /// Main menu button callbacks and chibi-style layout.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/MainMenu/MainMenu";
        private const string StartButtonResourcePath = "UI/MainMenu/StartGame";
        private const string GuideButtonResourcePath = "UI/MainMenu/Guide";
        private const string SettingsButtonResourcePath = "UI/MainMenu/Settings";
        private const string QuitButtonResourcePath = "UI/MainMenu/Quit";
        private const float ButtonBaseScaleX = 1.02f;
        private const float ButtonBaseScaleY = 1.20f;
        private const float ButtonHoverScale = 1.035f;
        private const float ButtonPressedScale = 0.975f;
        private const float ButtonFloatSpeed = 2.8f;
        private const float ButtonFloatAmount = 0.010f;

        private Texture2D backgroundTexture;
        private Texture2D startButtonTexture;
        private Texture2D guideButtonTexture;
        private Texture2D settingsButtonTexture;
        private Texture2D quitButtonTexture;
        private GUIStyle transparentButtonStyle;
        private bool showGuide;
        private bool showSettings;

        /// <summary>
        /// Purpose: Loads the image assets before the first OnGUI pass draws the main menu.
        /// Inputs: no direct parameters; reads Texture2D assets from the Resources/UI/MainMenu folder.
        /// Output: no return value; caches textures for later drawing.
        /// </summary>
        private void Awake()
        {
            LoadMenuTextures();
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            AudioManager.Instance?.PlayMenuBGM();
        }

        /// <summary>
        /// Purpose: Draws and handles immediate-mode GUI controls for this screen.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnGUI()
        {
            if (HasImageMenuAssets())
            {
                DrawImageMenu();
            }
            else
            {
                DrawFallbackMenu();
            }

            if (showGuide)
            {
                bool shouldClose = SimpleUIFactory.GuideModal();
                if (shouldClose)
                {
                    OnClosePopup();
                }
            }

            if (showSettings)
            {
                bool shouldClose = SimpleUIFactory.SettingsModal(AudioManager.Instance);
                if (shouldClose)
                {
                    OnClosePopup();
                }
            }
        }

        /// <summary>
        /// Purpose: Loads the background and button art used by the image-based main menu.
        /// Inputs: no direct parameters; uses Resources.Load paths without file extensions.
        /// Output: no return value; assigns cached Texture2D references or leaves them null if missing.
        /// </summary>
        private void LoadMenuTextures()
        {
            backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
            startButtonTexture = Resources.Load<Texture2D>(StartButtonResourcePath);
            guideButtonTexture = Resources.Load<Texture2D>(GuideButtonResourcePath);
            settingsButtonTexture = Resources.Load<Texture2D>(SettingsButtonResourcePath);
            quitButtonTexture = Resources.Load<Texture2D>(QuitButtonResourcePath);
        }

        /// <summary>
        /// Purpose: Checks whether the full art-driven main menu can be drawn.
        /// Inputs: no direct parameters; reads cached Texture2D fields.
        /// Output: true when all required menu art is available, otherwise false.
        /// </summary>
        private bool HasImageMenuAssets()
        {
            return backgroundTexture != null
                   && startButtonTexture != null
                   && guideButtonTexture != null
                   && settingsButtonTexture != null
                   && quitButtonTexture != null;
        }

        /// <summary>
        /// Purpose: Draws the new full-screen background and places image buttons into the empty slots.
        /// Inputs: no direct parameters; reads cached textures and popup state.
        /// Output: no return value; calls menu button handlers when a menu slot is clicked.
        /// </summary>
        private void DrawImageMenu()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            Rect backgroundRect = CalculateAspectFitRect(backgroundTexture, screenRect);
            GUI.DrawTexture(backgroundRect, backgroundTexture, ScaleMode.StretchToFill, false);

            bool popupOpen = showGuide || showSettings;
            GUI.enabled = !popupOpen;
            DrawImageMenuButtons(backgroundRect);
            GUI.enabled = true;
        }

        /// <summary>
        /// Purpose: Draws the original generated menu if image assets are missing.
        /// Inputs: no direct parameters; reads popup state.
        /// Output: no return value; calls menu button handlers when generated buttons are clicked.
        /// </summary>
        private void DrawFallbackMenu()
        {
            SimpleUIFactory.DrawCandyBackground();

            Rect panel = SimpleUIFactory.CenteredRect(680f, 540f);
            SimpleUIFactory.BeginPanel(panel);
            SimpleUIFactory.Title("BubbleTown");
            SimpleUIFactory.Body("A candy castle adventure with bubble bombs.");
            SimpleUIFactory.MainMenuDecorations();

            bool popupOpen = showGuide || showSettings;
            GUI.enabled = !popupOpen;
            GUILayout.BeginHorizontal();
            if (SimpleUIFactory.MenuTileButton("START GAME", SimpleUIFactory.MenuButtonIcon.Play, new Color(0.08f, 0.72f, 1f, 1f)))
            {
                OnClickStart();
            }

            if (SimpleUIFactory.MenuTileButton("GUIDE", SimpleUIFactory.MenuButtonIcon.Guide, new Color(0.38f, 0.86f, 0.36f, 1f)))
            {
                OnClickGuide();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (SimpleUIFactory.MenuTileButton("SETTINGS", SimpleUIFactory.MenuButtonIcon.Settings, new Color(1f, 0.66f, 0.22f, 1f)))
            {
                OnClickSettings();
            }

            if (SimpleUIFactory.MenuTileButton("QUIT", SimpleUIFactory.MenuButtonIcon.Quit, new Color(1f, 0.44f, 0.36f, 1f)))
            {
                OnClickQuit();
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            SimpleUIFactory.EndPanel();
        }

        /// <summary>
        /// Purpose: Places all four clickable labels over the matching button wells in the background art.
        /// Inputs: backgroundRect is the on-screen rectangle where the main menu background was drawn.
        /// Output: no return value; invokes the correct click handler for any selected button.
        /// </summary>
        private void DrawImageMenuButtons(Rect backgroundRect)
        {
            // These normalized slots match the four empty wells baked into MainMenu.png.
            Rect startRect = GetNormalizedRect(backgroundRect, 0.192f, 0.649f, 0.295f, 0.112f);
            Rect guideRect = GetNormalizedRect(backgroundRect, 0.512f, 0.649f, 0.295f, 0.112f);
            Rect settingsRect = GetNormalizedRect(backgroundRect, 0.192f, 0.795f, 0.295f, 0.112f);
            Rect quitRect = GetNormalizedRect(backgroundRect, 0.512f, 0.795f, 0.295f, 0.112f);

            if (DrawImageButton(startRect, startButtonTexture, 0f))
            {
                OnClickStart();
            }

            if (DrawImageButton(guideRect, guideButtonTexture, 0.7f))
            {
                OnClickGuide();
            }

            if (DrawImageButton(settingsRect, settingsButtonTexture, 1.4f))
            {
                OnClickSettings();
            }

            if (DrawImageButton(quitRect, quitButtonTexture, 2.1f))
            {
                OnClickQuit();
            }
        }

        /// <summary>
        /// Purpose: Draws one transparent PNG button and uses the same rectangle as the click target.
        /// Inputs: clickRect is the active button area; texture is the transparent button art; animationPhase offsets idle motion.
        /// Output: true when the player clicks the button, otherwise false.
        /// </summary>
        private bool DrawImageButton(Rect clickRect, Texture2D texture, float animationPhase)
        {
            bool isHovered = clickRect.Contains(Event.current.mousePosition);
            bool isPressed = isHovered && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            float idleOffset = Mathf.Sin(Time.realtimeSinceStartup * ButtonFloatSpeed + animationPhase) * clickRect.height * ButtonFloatAmount;

            Rect drawRect = ScaleRectAroundCenter(clickRect, ButtonBaseScaleX, ButtonBaseScaleY);
            drawRect.y += idleOffset;

            if (isHovered)
            {
                drawRect = ScaleRectAroundCenter(drawRect, ButtonHoverScale, ButtonHoverScale);
            }

            if (isPressed)
            {
                drawRect = ScaleRectAroundCenter(drawRect, ButtonPressedScale, ButtonPressedScale);
                drawRect.y += clickRect.height * 0.025f;
            }

            // The button textures are tightly cropped now, so ScaleToFit keeps text sharp and avoids distortion.
            GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit, true);

            // The invisible button handles clicks without adding hover colors over the button image.
            return GUI.Button(clickRect, GUIContent.none, GetTransparentButtonStyle());
        }

        /// <summary>
        /// Purpose: Creates or returns a style used by invisible button hitboxes.
        /// Inputs: no direct parameters.
        /// Output: a GUIStyle with no visible background or hover state.
        /// </summary>
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
        private Rect GetNormalizedRect(Rect parent, float x, float y, float width, float height)
        {
            return new Rect(
                parent.x + parent.width * x,
                parent.y + parent.height * y,
                parent.width * width,
                parent.height * height);
        }

        /// <summary>
        /// Purpose: Scales a rectangle around its center point.
        /// Inputs: rect is the original area; scaleX and scaleY are horizontal and vertical multipliers.
        /// Output: a new Rect with the same center and scaled size.
        /// </summary>
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
        /// Purpose: Fits a texture into a target rectangle without stretching its aspect ratio.
        /// Inputs: texture is the source image; targetRect is the maximum available drawing space.
        /// Output: a centered Rect that preserves the texture aspect ratio.
        /// </summary>
        private Rect CalculateAspectFitRect(Texture2D texture, Rect targetRect)
        {
            if (texture == null || texture.height == 0)
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
        /// Purpose: Handles the start button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickStart()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        /// <summary>
        /// Purpose: Handles the guide button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickGuide()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showGuide = true;
            showSettings = false;
        }

        /// <summary>
        /// Purpose: Handles the settings button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickSettings()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showSettings = true;
            showGuide = false;
        }

        /// <summary>
        /// Purpose: Handles the quit button click.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void OnClickQuit()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            Debug.Log("[MainMenuUI] Quit requested.");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Purpose: Handles the close popup event or callback.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnClosePopup()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showGuide = false;
            showSettings = false;
        }
    }
}
