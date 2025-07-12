using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all audio in the game including sound effects, music, and spatial audio.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource weaponAudioSource;
    
    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;
    
    [Header("Weapon Sounds")]
    [SerializeField] private AudioClip[] shootSounds;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private AudioClip[] impactSounds;
    
    [Header("Player Sounds")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip[] damageSounds;
    [SerializeField] private AudioClip deathSound;
    
    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip[] enemyAttackSounds;
    [SerializeField] private AudioClip[] enemyDeathSounds;
    [SerializeField] private AudioClip[] enemyAlertSounds;
    [SerializeField] private AudioClip[] enemyFootstepSounds;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuOpenSound;
    [SerializeField] private AudioClip menuCloseSound;
    [SerializeField] private AudioClip scoreSound;
    
    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 0.8f;
    // [SerializeField] private float voiceVolume = 0.9f; // Unused - removed to avoid warning
    
    // Audio settings
    private bool isMuted = false;
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    // Events
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;
    
    // Properties
    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool IsMuted => isMuted;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadAudioSettings();
        CreateAudioSources();
    }
    
    /// <summary>
    /// Initialize audio manager
    /// </summary>
    private void InitializeAudioManager()
    {
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Load audio clips into dictionary
        LoadAudioClips();
    }
    
    /// <summary>
    /// Create audio sources
    /// </summary>
    private void CreateAudioSources()
    {
        if (playerAudioSource == null)
        {
            GameObject playerAudioObj = new GameObject("PlayerAudioSource");
            playerAudioObj.transform.SetParent(transform);
            playerAudioSource = playerAudioObj.AddComponent<AudioSource>();
            playerAudioSource.playOnAwake = false;
        }
        
        if (weaponAudioSource == null)
        {
            GameObject weaponAudioObj = new GameObject("WeaponAudioSource");
            weaponAudioObj.transform.SetParent(transform);
            weaponAudioSource = weaponAudioObj.AddComponent<AudioSource>();
            weaponAudioSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// Load audio clips into dictionary
    /// </summary>
    private void LoadAudioClips()
    {
        // Music clips
        if (mainMenuMusic != null) audioClips["MainMenuMusic"] = mainMenuMusic;
        if (gameplayMusic != null) audioClips["GameplayMusic"] = gameplayMusic;
        if (victoryMusic != null) audioClips["VictoryMusic"] = victoryMusic;
        if (defeatMusic != null) audioClips["DefeatMusic"] = defeatMusic;
        
        // Weapon clips
        if (shootSounds != null && shootSounds.Length > 0)
        {
            for (int i = 0; i < shootSounds.Length; i++)
            {
                if (shootSounds[i] != null)
                    audioClips[$"ShootSound_{i}"] = shootSounds[i];
            }
        }
        
        if (reloadSound != null) audioClips["ReloadSound"] = reloadSound;
        if (emptySound != null) audioClips["EmptySound"] = emptySound;
        
        // Player clips
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            for (int i = 0; i < footstepSounds.Length; i++)
            {
                if (footstepSounds[i] != null)
                    audioClips[$"FootstepSound_{i}"] = footstepSounds[i];
            }
        }
        
        if (jumpSound != null) audioClips["JumpSound"] = jumpSound;
        if (landSound != null) audioClips["LandSound"] = landSound;
        if (deathSound != null) audioClips["DeathSound"] = deathSound;
        
        // UI clips
        if (buttonClickSound != null) audioClips["ButtonClick"] = buttonClickSound;
        if (menuOpenSound != null) audioClips["MenuOpen"] = menuOpenSound;
        if (menuCloseSound != null) audioClips["MenuClose"] = menuCloseSound;
        if (scoreSound != null) audioClips["ScoreSound"] = scoreSound;
    }
    
    /// <summary>
    /// Play music
    /// </summary>
    public void PlayMusic(string musicName, bool loop = true)
    {
        if (isMuted || musicSource == null) return;
        
        AudioClip clip = GetAudioClip(musicName);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    /// <summary>
    /// Play sound effect
    /// </summary>
    public void PlaySFX(string soundName, float volume = 1f, Vector3 position = default)
    {
        if (isMuted) return;
        
        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            float finalVolume = volume * sfxVolume * masterVolume;
            
            if (position != default)
            {
                // Play at specific position (3D sound)
                AudioSource.PlayClipAtPoint(clip, position, finalVolume);
            }
            else
            {
                // Play through SFX source
                sfxSource.PlayOneShot(clip, finalVolume);
            }
        }
    }
    
    /// <summary>
    /// Play weapon sound
    /// </summary>
    public void PlayWeaponSound(string soundName, float volume = 1f)
    {
        if (isMuted || weaponAudioSource == null) return;
        
        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            weaponAudioSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }
    }
    
    /// <summary>
    /// Play player sound
    /// </summary>
    public void PlayPlayerSound(string soundName, float volume = 1f)
    {
        if (isMuted || playerAudioSource == null) return;
        
        AudioClip clip = GetAudioClip(soundName);
        if (clip != null)
        {
            playerAudioSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }
    }
    
    /// <summary>
    /// Play random sound from array
    /// </summary>
    public void PlayRandomSound(string[] soundNames, float volume = 1f, Vector3 position = default)
    {
        if (soundNames.Length == 0) return;
        
        string randomSound = soundNames[Random.Range(0, soundNames.Length)];
        PlaySFX(randomSound, volume, position);
    }
    
    /// <summary>
    /// Play weapon shoot sound
    /// </summary>
    public void PlayShootSound()
    {
        if (shootSounds != null && shootSounds.Length > 0)
        {
            AudioClip randomShoot = shootSounds[Random.Range(0, shootSounds.Length)];
            PlayWeaponSound("ShootSound", 0.8f);
        }
    }
    
    /// <summary>
    /// Play reload sound
    /// </summary>
    public void PlayReloadSound()
    {
        PlayWeaponSound("ReloadSound", 0.7f);
    }
    
    /// <summary>
    /// Play empty weapon sound
    /// </summary>
    public void PlayEmptySound()
    {
        PlayWeaponSound("EmptySound", 0.6f);
    }
    
    /// <summary>
    /// Play footstep sound
    /// </summary>
    public void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            PlayPlayerSound("FootstepSound", 0.5f);
        }
    }
    
    /// <summary>
    /// Play jump sound
    /// </summary>
    public void PlayJumpSound()
    {
        PlayPlayerSound("JumpSound", 0.6f);
    }
    
    /// <summary>
    /// Play land sound
    /// </summary>
    public void PlayLandSound()
    {
        PlayPlayerSound("LandSound", 0.7f);
    }
    
    /// <summary>
    /// Play damage sound
    /// </summary>
    public void PlayDamageSound()
    {
        if (damageSounds != null && damageSounds.Length > 0)
        {
            AudioClip randomDamage = damageSounds[Random.Range(0, damageSounds.Length)];
            PlayPlayerSound("DamageSound", 0.8f);
        }
    }
    
    /// <summary>
    /// Play death sound
    /// </summary>
    public void PlayDeathSound()
    {
        PlayPlayerSound("DeathSound", 1f);
    }
    
    /// <summary>
    /// Play UI sound
    /// </summary>
    public void PlayUISound(string soundName)
    {
        PlaySFX(soundName, 0.5f);
    }
    
    /// <summary>
    /// Play button click sound
    /// </summary>
    public void PlayButtonClick()
    {
        PlayUISound("ButtonClick");
    }
    
    /// <summary>
    /// Play menu open sound
    /// </summary>
    public void PlayMenuOpen()
    {
        PlayUISound("MenuOpen");
    }
    
    /// <summary>
    /// Play menu close sound
    /// </summary>
    public void PlayMenuClose()
    {
        PlayUISound("MenuClose");
    }
    
    /// <summary>
    /// Play score sound
    /// </summary>
    public void PlayScoreSound()
    {
        PlayUISound("ScoreSound");
    }
    
    /// <summary>
    /// Get audio clip by name
    /// </summary>
    private AudioClip GetAudioClip(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            return audioClips[clipName];
        }
        
        Debug.LogWarning($"Audio clip '{clipName}' not found!");
        return null;
    }
    
    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnMasterVolumeChanged?.Invoke(masterVolume);
        SaveAudioSettings();
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        OnMusicVolumeChanged?.Invoke(musicVolume);
        SaveAudioSettings();
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        OnSFXVolumeChanged?.Invoke(sfxVolume);
        SaveAudioSettings();
    }
    
    /// <summary>
    /// Toggle mute
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        if (musicSource != null)
        {
            musicSource.volume = isMuted ? 0f : musicVolume * masterVolume;
        }
        
        SaveAudioSettings();
    }
    
    /// <summary>
    /// Update all audio source volumes
    /// </summary>
    private void UpdateAllVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = isMuted ? 0f : musicVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Save audio settings
    /// </summary>
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load audio settings
    /// </summary>
    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        
        UpdateAllVolumes();
    }
    
    /// <summary>
    /// Fade music in
    /// </summary>
    public void FadeMusicIn(float duration = 2f)
    {
        StartCoroutine(FadeMusic(0f, musicVolume * masterVolume, duration));
    }
    
    /// <summary>
    /// Fade music out
    /// </summary>
    public void FadeMusicOut(float duration = 2f)
    {
        StartCoroutine(FadeMusic(musicSource.volume, 0f, duration));
    }
    
    /// <summary>
    /// Fade music coroutine
    /// </summary>
    private System.Collections.IEnumerator FadeMusic(float startVolume, float targetVolume, float duration)
    {
        if (musicSource == null) yield break;
        
        float elapsed = 0f;
        musicSource.volume = startVolume;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
} 