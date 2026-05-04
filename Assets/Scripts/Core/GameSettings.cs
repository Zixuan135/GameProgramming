using UnityEngine;

namespace BubbleTown.Core
{
    /// <summary>
    /// Lightweight persistent player settings shared by UI, audio, and camera feedback.
    /// </summary>
    public static class GameSettings
    {
        public const float DefaultMasterVolume = 1f;
        public const float DefaultBgmVolume = 0.45f;
        public const float DefaultSfxVolume = 0.85f;
        public const bool DefaultMuteBGM = false;
        public const bool DefaultMuteSFX = false;
        public const bool DefaultScreenShakeEnabled = true;

        private const string MasterVolumeKey = "BubbleTown.Settings.MasterVolume";
        private const string BgmVolumeKey = "BubbleTown.Settings.BgmVolume";
        private const string SfxVolumeKey = "BubbleTown.Settings.SfxVolume";
        private const string MuteBgmKey = "BubbleTown.Settings.MuteBGM";
        private const string MuteSfxKey = "BubbleTown.Settings.MuteSFX";
        private const string ScreenShakeKey = "BubbleTown.Settings.ScreenShake";

        private static bool hasLoaded;
        private static float masterVolume = DefaultMasterVolume;
        private static float bgmVolume = DefaultBgmVolume;
        private static float sfxVolume = DefaultSfxVolume;
        private static bool muteBGM = DefaultMuteBGM;
        private static bool muteSFX = DefaultMuteSFX;
        private static bool screenShakeEnabled = DefaultScreenShakeEnabled;

        public static float MasterVolume
        {
            get
            {
                EnsureLoaded();
                return masterVolume;
            }
        }

        public static float BgmVolume
        {
            get
            {
                EnsureLoaded();
                return bgmVolume;
            }
        }

        public static float SfxVolume
        {
            get
            {
                EnsureLoaded();
                return sfxVolume;
            }
        }

        public static bool MuteBGM
        {
            get
            {
                EnsureLoaded();
                return muteBGM;
            }
        }

        public static bool MuteSFX
        {
            get
            {
                EnsureLoaded();
                return muteSFX;
            }
        }

        public static bool ScreenShakeEnabled
        {
            get
            {
                EnsureLoaded();
                return screenShakeEnabled;
            }
        }

        public static void Load()
        {
            masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
            bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, DefaultBgmVolume));
            sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));
            muteBGM = PlayerPrefs.GetInt(MuteBgmKey, BoolToInt(DefaultMuteBGM)) == 1;
            muteSFX = PlayerPrefs.GetInt(MuteSfxKey, BoolToInt(DefaultMuteSFX)) == 1;
            screenShakeEnabled = PlayerPrefs.GetInt(ScreenShakeKey, BoolToInt(DefaultScreenShakeEnabled)) == 1;
            hasLoaded = true;
        }

        public static void SetMasterVolume(float volume)
        {
            EnsureLoaded();
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.Save();
        }

        public static void SetBgmVolume(float volume)
        {
            EnsureLoaded();
            bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
            PlayerPrefs.Save();
        }

        public static void SetSfxVolume(float volume)
        {
            EnsureLoaded();
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public static void SetMuteBGM(bool isMuted)
        {
            EnsureLoaded();
            muteBGM = isMuted;
            PlayerPrefs.SetInt(MuteBgmKey, BoolToInt(muteBGM));
            PlayerPrefs.Save();
        }

        public static void SetMuteSFX(bool isMuted)
        {
            EnsureLoaded();
            muteSFX = isMuted;
            PlayerPrefs.SetInt(MuteSfxKey, BoolToInt(muteSFX));
            PlayerPrefs.Save();
        }

        public static void SetScreenShakeEnabled(bool isEnabled)
        {
            EnsureLoaded();
            screenShakeEnabled = isEnabled;
            PlayerPrefs.SetInt(ScreenShakeKey, BoolToInt(screenShakeEnabled));
            PlayerPrefs.Save();
        }

        public static void ResetToDefaults()
        {
            masterVolume = DefaultMasterVolume;
            bgmVolume = DefaultBgmVolume;
            sfxVolume = DefaultSfxVolume;
            muteBGM = DefaultMuteBGM;
            muteSFX = DefaultMuteSFX;
            screenShakeEnabled = DefaultScreenShakeEnabled;
            hasLoaded = true;
            SaveAll();
        }

        private static void EnsureLoaded()
        {
            if (!hasLoaded)
            {
                Load();
            }
        }

        private static void SaveAll()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.SetInt(MuteBgmKey, BoolToInt(muteBGM));
            PlayerPrefs.SetInt(MuteSfxKey, BoolToInt(muteSFX));
            PlayerPrefs.SetInt(ScreenShakeKey, BoolToInt(screenShakeEnabled));
            PlayerPrefs.Save();
        }

        private static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }
    }
}
