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

        private void Start()
        {
            AudioManager.Instance?.PlayMenuBGM();
        }

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

        public void OnClickStart()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadModeSelect();
        }

        public void OnClickGuide()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showGuide = true;
            showSettings = false;
        }

        public void OnClickSettings()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showSettings = true;
            showGuide = false;
        }

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

        private void OnClosePopup()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            showGuide = false;
            showSettings = false;
        }
    }
}
