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

        private static AudioManager instance;
        private static bool isQuitting;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            if (autoPlaySceneBGM)
            {
                PlayBGMForScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public void PlayMenuBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(menuBGM);
        }

        public void PlayBattleBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(battleBGM);
        }

        public void PlayResultBGM()
        {
            LoadDefaultAudioClips();
            PlayBGM(resultBGM != null ? resultBGM : menuBGM);
        }

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

        public void PlayMoveSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(moveSFX, 0.65f);
        }

        public void PlayPlaceBombSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(placeBombSFX);
        }

        public void PlayExplosionSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(explosionSFX, 1f);
        }

        public void PlayItemPickupSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(itemPickupSFX, 0.9f);
        }

        public void PlayButtonClickSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(buttonClickSFX, 0.75f);
        }

        public void PlayCharacterDeathSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(characterDeathSFX, 1f);
        }

        public void PlayVictorySFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(victorySFX, 1f);
        }

        public void PlayDefeatSFX()
        {
            LoadDefaultAudioClips();
            PlaySFX(defeatSFX, 1f);
        }

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

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            GameSettings.SetMasterVolume(masterVolume);
            ApplyVolumeSettings();
        }

        public void SetBgmVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            GameSettings.SetBgmVolume(bgmVolume);
            ApplyVolumeSettings();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            GameSettings.SetSfxVolume(sfxVolume);
            ApplyVolumeSettings();
        }

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

        public void SetSfxMuted(bool isMuted)
        {
            muteSFX = isMuted;
            GameSettings.SetMuteSFX(muteSFX);
            ApplyVolumeSettings();
        }

        public void ReloadFromGameSettings()
        {
            LoadSavedSettings();
            ApplyVolumeSettings();
            if (!muteBGM && autoPlaySceneBGM && bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBGMForScene(SceneManager.GetActiveScene().name);
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!autoPlaySceneBGM)
            {
                return;
            }

            PlayBGMForScene(scene.name);
        }

        private void PlayBGMForScene(string sceneName)
        {
            switch (sceneName)
            {
                case GameConstants.SceneMainMenu:
                case GameConstants.SceneModeSelect:
                case GameConstants.SceneMapSelect:
                    PlayMenuBGM();
                    break;
                case GameConstants.SceneBattle:
                    PlayBattleBGM();
                    break;
                case GameConstants.SceneResult:
                    PlayResultBGM();
                    break;
            }
        }

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

        private void LoadSavedSettings()
        {
            GameSettings.Load();
            masterVolume = GameSettings.MasterVolume;
            bgmVolume = GameSettings.BgmVolume;
            sfxVolume = GameSettings.SfxVolume;
            muteBGM = GameSettings.MuteBGM;
            muteSFX = GameSettings.MuteSFX;
        }

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

        private AudioClip LoadClipIfMissing(AudioClip currentClip, string resourcePath)
        {
            return currentClip != null ? currentClip : Resources.Load<AudioClip>(resourcePath);
        }

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
