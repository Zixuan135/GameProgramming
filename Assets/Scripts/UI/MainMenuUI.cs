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
        private bool showGuide;
        private bool showSettings;

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
