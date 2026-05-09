using UnityEngine;

public class AssignmentAudioManager : MonoBehaviour
{
    private const string AudioBasePath = "Assignment/Audio/";

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip pickupClip;
    public AudioClip powerUpClip;
    public AudioClip hitClip;
    public AudioClip winClip;
    public AudioClip gameOverClip;

    [Header("Music Clips")]
    public AudioClip menuMusicClip;
    public AudioClip gameMusicClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float musicVolume = 0.45f;

    private void Awake()
    {
        EnsureSources();
        LoadFallbackClips();
        ApplyVolumes();
    }

    public void PlayButtonClick()
    {
        PlayOneShot(buttonClickClip, 0.8f);
    }

    public void PlayPickup()
    {
        PlayOneShot(pickupClip, 0.85f);
    }

    public void PlayPowerUp()
    {
        PlayOneShot(powerUpClip, 0.9f);
    }

    public void PlayHit()
    {
        PlayOneShot(hitClip, 0.95f);
    }

    public void PlayWin()
    {
        PlayOneShot(winClip, 1f);
    }

    public void PlayGameOver()
    {
        PlayOneShot(gameOverClip, 1f);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusicClip);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusicClip);
    }

    private void EnsureSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    private void LoadFallbackClips()
    {
        buttonClickClip = buttonClickClip != null ? buttonClickClip : Resources.Load<AudioClip>(AudioBasePath + "ButtonClick");
        pickupClip = pickupClip != null ? pickupClip : Resources.Load<AudioClip>(AudioBasePath + "Pickup");
        powerUpClip = powerUpClip != null ? powerUpClip : Resources.Load<AudioClip>(AudioBasePath + "PowerUp");
        hitClip = hitClip != null ? hitClip : Resources.Load<AudioClip>(AudioBasePath + "Hit");
        winClip = winClip != null ? winClip : Resources.Load<AudioClip>(AudioBasePath + "Win");
        gameOverClip = gameOverClip != null ? gameOverClip : Resources.Load<AudioClip>(AudioBasePath + "GameOver");
        menuMusicClip = menuMusicClip != null ? menuMusicClip : Resources.Load<AudioClip>(AudioBasePath + "MenuMusic");
        gameMusicClip = gameMusicClip != null ? gameMusicClip : Resources.Load<AudioClip>(AudioBasePath + "GameMusic");
    }

    private void ApplyVolumes()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    private void PlayOneShot(AudioClip clip, float volumeScale)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }
}
