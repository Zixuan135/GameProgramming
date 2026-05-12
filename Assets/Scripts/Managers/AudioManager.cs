using BubbleTown.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleTown.Managers
{
    /// <summary>
    /// Central audio entry point for lightweight BGM and SFX playback.
    /// Clips can stay unassigned during placeholder development; calls safely do nothing.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private const string RuntimeObjectName = "AudioManager";
        private const string BgmSourceName = "BGMSource";
        private const string SfxSourceName = "SFXSource";
        private const string MenuBGMResourcePath = "Audio/BGM/MenuLoop";
        private const string BattleBGMResourcePath = "Audio/BGM/BattleLoop";
        private const string ResultBGMResourcePath = "Audio/BGM/ResultLoop";
        private const string MoveSFXResourcePath = "Audio/SFX/MoveStep";
        private const string PlaceBombSFXResourcePath = "Audio/SFX/PlaceBomb";
        private const string ExplosionSFXResourcePath = "Audio/SFX/Explosion";
        private const string ItemPickupSFXResourcePath = "Audio/SFX/ItemPickup";
        private const string ButtonClickSFXResourcePath = "Audio/SFX/ButtonClick";
        private const string CharacterDeathSFXResourcePath = "Audio/SFX/CharacterDeath";
        private const string VictorySFXResourcePath = "Audio/SFX/Victory";
        private const string DefeatSFXResourcePath = "Audio/SFX/Defeat";
        private const float SettingsPreviewCooldown = 0.12f;

        private static AudioManager instance;
        private static bool isQuitting;
        private float nextSettingsPreviewTime;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null && !isQuitting)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject audioManagerObject = new GameObject(RuntimeObjectName);
                        instance = audioManagerObject.AddComponent<AudioManager>();
                    }
                }

                return instance;
            }
            private set => instance = value;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioListener fallbackAudioListener;

        [Header("BGM Clips")]
        [SerializeField] private AudioClip menuBGM;
        [SerializeField] private AudioClip battleBGM;
        [SerializeField] private AudioClip resultBGM;
        [SerializeField] private bool autoPlaySceneBGM = true;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip moveSFX;
        [SerializeField] private AudioClip placeBombSFX;
        [SerializeField] private AudioClip explosionSFX;
        [SerializeField] private AudioClip itemPickupSFX;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip characterDeathSFX;
        [SerializeField] private AudioClip victorySFX;
        [SerializeField] private AudioClip defeatSFX;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField] private bool muteBGM;
        [SerializeField] private bool muteSFX;

        public AudioSource BgmSource => bgmSource;
        public AudioSource SfxSource => sfxSource;
        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;
        public bool MuteBGM => muteBGM;
        public bool MuteSFX => muteSFX;
        public bool HasLoadedBGM => menuBGM != null && battleBGM != null && resultBGM != null;
        public bool HasLoadedSFX => moveSFX != null &&
                                    placeBombSFX != null &&
                                    explosionSFX != null &&
                                    itemPickupSFX != null &&
                                    buttonClickSFX != null &&
                                    characterDeathSFX != null &&
                                    victorySFX != null &&
                                    defeatSFX != null;
        public bool IsAudioReady
        {
            get
            {
                LoadDefaultAudioClips();
                return HasLoadedBGM && HasLoadedSFX;
            }
        }

        /// <summary>
        /// Purpose: Initializes this component before the scene starts running.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioListener();
            EnsureAudioSources();
            LoadDefaultAudioClips();
            LoadSavedSettings();
            ApplyVolumeSettings();
        }

        /// <summary>
        /// Purpose: Subscribes or refreshes runtime state when this component becomes active.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        /// <summary>
        /// Purpose: Initializes this component after Unity enables it in the scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void Start()
        {
            if (autoPlaySceneBGM)
            {
                PlayBGMForScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// Purpose: Cleans up subscriptions or runtime state when this component becomes inactive.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        /// <summary>
        /// Purpose: Handles application shutdown cleanup.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// Purpose: Plays menu bgm.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayMenuBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(menuBGM);
        }

        /// <summary>
        /// Purpose: Plays battle bgm.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayBattleBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(battleBGM);
        }

        /// <summary>
        /// Purpose: Plays result bgm.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayResultBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(resultBGM != null ? resultBGM : menuBGM);
        }

        /// <summary>
        /// Purpose: Plays current scene bgmpreview.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayCurrentSceneBGMPreview()
        {
            LoadDefaultAudioClips();
            PlayBGMForScene(SceneManager.GetActiveScene().name, true);
        }

        /// <summary>
        /// Purpose: Plays bgm.
        /// Inputs: `clip`, `restartIfSameClip`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="clip">Input value used by this method.</param>
        /// <param name="restartIfSameClip">Input value used by this method.</param>
        public void PlayBGM(AudioClip clip, bool restartIfSameClip = false)
        {
            EnsureAudioSources();
            if (bgmSource == null || clip == null)
            {
                return;
            }

            if (muteBGM)
            {
                bgmSource.clip = clip;
                bgmSource.loop = true;
                bgmSource.Stop();
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying && !restartIfSameClip)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = masterVolume * bgmVolume;
            bgmSource.Play();
        }

        /// <summary>
        /// Purpose: Stops bgm.
        /// Inputs: `clearClip`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="clearClip">Input value used by this method.</param>
        public void StopBGM(bool clearClip = false)
        {
            if (bgmSource == null)
            {
                return;
            }

            bgmSource.Stop();
            if (clearClip)
            {
                bgmSource.clip = null;
            }
        }

        /// <summary>
        /// Purpose: Plays move sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayMoveSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(moveSFX, 0.65f);
        }

        /// <summary>
        /// Purpose: Plays place bomb sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayPlaceBombSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(placeBombSFX);
        }

        /// <summary>
        /// Purpose: Plays explosion sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayExplosionSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(explosionSFX, 1f);
        }

        /// <summary>
        /// Purpose: Plays item pickup sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayItemPickupSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(itemPickupSFX, 0.9f);
        }

        /// <summary>
        /// Purpose: Plays button click sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayButtonClickSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(buttonClickSFX, 0.75f);
        }

        /// <summary>
        /// Purpose: Plays settings preview sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlaySettingsPreviewSFX()
        {
            if (Time.unscaledTime < nextSettingsPreviewTime)
            {
                return;
            }

            nextSettingsPreviewTime = Time.unscaledTime + SettingsPreviewCooldown;
            LoadDefaultAudioClips();
            PlaySFX(buttonClickSFX != null ? buttonClickSFX : itemPickupSFX, 0.85f);
        }

        /// <summary>
        /// Purpose: Plays character death sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayCharacterDeathSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(characterDeathSFX, 1f);
        }

        /// <summary>
        /// Purpose: Plays victory sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayVictorySFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(victorySFX, 1f);
        }

        /// <summary>
        /// Purpose: Plays defeat sfx.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void PlayDefeatSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(defeatSFX, 1f);
        }

        /// <summary>
        /// Purpose: Plays sfx.
        /// Inputs: `clip`, `volumeScale`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="clip">Input value used by this method.</param>
        /// <param name="volumeScale">Input value used by this method.</param>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            EnsureAudioSources();
            if (sfxSource == null || clip == null || muteSFX)
            {
                return;
            }

            float resolvedVolume = masterVolume * sfxVolume * Mathf.Clamp01(volumeScale);
            sfxSource.PlayOneShot(clip, resolvedVolume);
        }

        /// <summary>
        /// Purpose: Sets master volume.
        /// Inputs: `volume`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="volume">Input value used by this method.</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            GameSettings.SetMasterVolume(masterVolume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// Purpose: Sets bgm volume.
        /// Inputs: `volume`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="volume">Input value used by this method.</param>
        public void SetBgmVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            GameSettings.SetBgmVolume(bgmVolume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// Purpose: Sets sfx volume.
        /// Inputs: `volume`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="volume">Input value used by this method.</param>
        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            GameSettings.SetSfxVolume(sfxVolume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// Purpose: Sets bgm muted.
        /// Inputs: `isMuted`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="isMuted">Input value used by this method.</param>
        public void SetBgmMuted(bool isMuted)
        {
            muteBGM = isMuted;
            GameSettings.SetMuteBGM(muteBGM);
            ApplyVolumeSettings();
            if (!muteBGM && autoPlaySceneBGM && bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBGMForScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// Purpose: Sets sfx muted.
        /// Inputs: `isMuted`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="isMuted">Input value used by this method.</param>
        public void SetSfxMuted(bool isMuted)
        {
            muteSFX = isMuted;
            GameSettings.SetMuteSFX(muteSFX);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// Purpose: Performs reload from game settings for this component.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        public void ReloadFromGameSettings()
        {
            LoadSavedSettings();
            ApplyVolumeSettings();
            if (!muteBGM && autoPlaySceneBGM && bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBGMForScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// Purpose: Handles scene loaded.
        /// Inputs: `scene`, `mode`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="scene">Input value used by this method.</param>
        /// <param name="mode">Input value used by this method.</param>
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!autoPlaySceneBGM)
            {
                return;
            }

            PlayBGMForScene(scene.name);
        }

        /// <summary>
        /// Purpose: Plays bgmfor scene.
        /// Inputs: `sceneName`, `restartIfSameClip`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="sceneName">Input value used by this method.</param>
        /// <param name="restartIfSameClip">Input value used by this method.</param>
        private void PlayBGMForScene(string sceneName, bool restartIfSameClip = false)
        {
            switch (sceneName)
            {
                case GameConstants.SceneMainMenu:
                case GameConstants.SceneModeSelect:
                case GameConstants.SceneMapSelect:
                case GameConstants.SceneCharacterSelect:
                    LoadDefaultAudioClips();
                    PlayBGM(menuBGM, restartIfSameClip);
                    break;
                case GameConstants.SceneBattle:
                    LoadDefaultAudioClips();
                    PlayBGM(battleBGM, restartIfSameClip);
                    break;
                case GameConstants.SceneResult:
                    LoadDefaultAudioClips();
                    PlayBGM(resultBGM != null ? resultBGM : menuBGM, restartIfSameClip);
                    break;
            }
        }

        /// <summary>
        /// Purpose: Ensures audio sources exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureAudioSources()
        {
            EnsureAudioListener();

            if (bgmSource == null)
            {
                bgmSource = CreateChildAudioSource(BgmSourceName, true);
            }

            if (sfxSource == null)
            {
                sfxSource = CreateChildAudioSource(SfxSourceName, false);
            }

            ConfigureAudioSource(bgmSource, true);
            ConfigureAudioSource(sfxSource, false);
        }

        /// <summary>
        /// Purpose: Ensures audio listener exists or is initialized before use.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void EnsureAudioListener()
        {
            if (fallbackAudioListener != null)
            {
                return;
            }

            AudioListener existingListener = FindObjectOfType<AudioListener>();
            if (existingListener != null)
            {
                fallbackAudioListener = existingListener;
                return;
            }

            fallbackAudioListener = gameObject.GetComponent<AudioListener>();
            if (fallbackAudioListener == null)
            {
                fallbackAudioListener = gameObject.AddComponent<AudioListener>();
            }
        }

        /// <summary>
        /// Purpose: Creates child audio source.
        /// Inputs: `sourceName`, `loops`; may also read serialized fields and current runtime state.
        /// Output: a `AudioSource` value.
        /// </summary>
        /// <param name="sourceName">Input value used by this method.</param>
        /// <param name="loops">Input value used by this method.</param>
        /// <returns>a `AudioSource` value.</returns>
        private AudioSource CreateChildAudioSource(string sourceName, bool loops)
        {
            Transform existingChild = transform.Find(sourceName);
            GameObject sourceObject = existingChild != null ? existingChild.gameObject : new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            sourceObject.transform.localPosition = Vector3.zero;

            AudioSource source = sourceObject.GetComponent<AudioSource>();
            if (source == null)
            {
                source = sourceObject.AddComponent<AudioSource>();
            }

            ConfigureAudioSource(source, loops);
            return source;
        }

        /// <summary>
        /// Purpose: Configures audio source for the current battle or scene.
        /// Inputs: `source`, `loops`; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        /// <param name="source">Input value used by this method.</param>
        /// <param name="loops">Input value used by this method.</param>
        private void ConfigureAudioSource(AudioSource source, bool loops)
        {
            if (source == null)
            {
                return;
            }

            source.playOnAwake = false;
            source.loop = loops;
            source.spatialBlend = 0f;
        }

        /// <summary>
        /// Purpose: Loads saved settings.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void LoadSavedSettings()
        {
            GameSettings.Load();
            masterVolume = GameSettings.MasterVolume;
            bgmVolume = GameSettings.BgmVolume;
            sfxVolume = GameSettings.SfxVolume;
            muteBGM = GameSettings.MuteBGM;
            muteSFX = GameSettings.MuteSFX;
        }

        /// <summary>
        /// Purpose: Loads default audio clips.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void LoadDefaultAudioClips()
        {
            menuBGM = LoadClipIfMissing(menuBGM, MenuBGMResourcePath);
            battleBGM = LoadClipIfMissing(battleBGM, BattleBGMResourcePath);
            resultBGM = LoadClipIfMissing(resultBGM, ResultBGMResourcePath);
            moveSFX = LoadClipIfMissing(moveSFX, MoveSFXResourcePath);
            placeBombSFX = LoadClipIfMissing(placeBombSFX, PlaceBombSFXResourcePath);
            explosionSFX = LoadClipIfMissing(explosionSFX, ExplosionSFXResourcePath);
            itemPickupSFX = LoadClipIfMissing(itemPickupSFX, ItemPickupSFXResourcePath);
            buttonClickSFX = LoadClipIfMissing(buttonClickSFX, ButtonClickSFXResourcePath);
            characterDeathSFX = LoadClipIfMissing(characterDeathSFX, CharacterDeathSFXResourcePath);
            victorySFX = LoadClipIfMissing(victorySFX, VictorySFXResourcePath);
            defeatSFX = LoadClipIfMissing(defeatSFX, DefeatSFXResourcePath);
        }

        /// <summary>
        /// Purpose: Loads clip if missing.
        /// Inputs: `currentClip`, `resourcePath`; may also read serialized fields and current runtime state.
        /// Output: a `AudioClip` value.
        /// </summary>
        /// <param name="currentClip">Input value used by this method.</param>
        /// <param name="resourcePath">Input value used by this method.</param>
        /// <returns>a `AudioClip` value.</returns>
        private AudioClip LoadClipIfMissing(AudioClip currentClip, string resourcePath)
        {
            return currentClip != null ? currentClip : Resources.Load<AudioClip>(resourcePath);
        }

        /// <summary>
        /// Purpose: Applies volume settings to the current object or scene.
        /// Inputs: no direct parameters; may also read serialized fields and current runtime state.
        /// Output: no return value; updates component, scene, or game state as needed.
        /// </summary>
        private void ApplyVolumeSettings()
        {
            if (bgmSource != null)
            {
                bgmSource.mute = muteBGM;
                bgmSource.volume = masterVolume * bgmVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.mute = muteSFX;
                sfxSource.volume = masterVolume * sfxVolume;
            }
        }
    }
}
