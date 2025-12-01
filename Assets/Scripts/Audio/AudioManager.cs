using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// AudioManager - Central audio system for RESONATE! with advanced features
/// Handles music, SFX, spatial audio, and dynamic audio mixing
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Primary music audio source")]
    public AudioSource musicSource;
    
    [Tooltip("UI and menu sounds source")]
    public AudioSource uiSource;
    
    [Tooltip("Pool of audio sources for sound effects")]
    public AudioSource[] sfxSources;
    
    [Header("Music")]
    [Tooltip("Main menu background music")]
    public AudioClip menuMusic;
    
    [Tooltip("Main gameplay music")]
    public AudioClip gameplayMusic;
    
    [Tooltip("Game over music")]
    public AudioClip gameOverMusic;
    
    [Header("Resonance Sounds")]
    [Tooltip("Sound when nodes link together")]
    public AudioClip resonanceLinkSound;
    
    [Tooltip("Sound when resonance chain completes")]
    public AudioClip resonanceCompleteSound;
    
    [Tooltip("Sound when overload mode activates")]
    public AudioClip overloadActivateSound;
    
    [Tooltip("Sound when overload mode deactivates")]
    public AudioClip overloadDeactivateSound;
    
    [Tooltip("Looping sound during overload mode")]
    public AudioClip overloadLoopSound;
    
    [Header("Enemy Sounds")]
    [Tooltip("Basic enemy destruction sound")]
    public AudioClip enemyDestroySound;
    
    [Tooltip("Charger enemy rush sound")]
    public AudioClip chargerRushSound;
    
    [Tooltip("Flicker enemy color change sound")]
    public AudioClip flickerChangeSound;
    
    [Tooltip("Cluster drone explosion sound")]
    public AudioClip clusterExplodeSound;
    
    [Tooltip("Resonant hunter avoid sound")]
    public AudioClip hunterAvoidSound;
    
    [Header("UI Sounds")]
    [Tooltip("Button click sound")]
    public AudioClip buttonClickSound;
    
    [Tooltip("Menu navigation sound")]
    public AudioClip menuNavigateSound;
    
    [Tooltip("Game start sound")]
    public AudioClip gameStartSound;
    
    [Tooltip("Game over sound")]
    public AudioClip gameOverSound;
    
    [Tooltip("High score sound")]
    public AudioClip highScoreSound;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Master volume control")]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("Music volume control")]
    public float musicVolume = 0.7f;
    
    [Range(0f, 1f)]
    [Tooltip("Sound effects volume control")]
    public float sfxVolume = 0.8f;
    
    [Range(0f, 1f)]
    [Tooltip("UI sounds volume control")]
    public float uiVolume = 0.9f;
    
    [Header("Advanced Audio")]
    [Tooltip("Enable spatial audio for positioned sounds")]
    public bool enableSpatialAudio = true;
    
    [Tooltip("Maximum distance for spatial audio")]
    public float maxAudioDistance = 50f;
    
    [Tooltip("Audio rolloff curve")]
    public AnimationCurve audioRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    // Internal state
    private Dictionary<string, AudioClip> audioLibrary;
    private Queue<AudioSource> availableSfxSources;
    private List<AudioSource> activeSfxSources;
    private AudioSource overloadLoopSource;
    private Coroutine musicFadeCoroutine;
    
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    // Audio events
    public System.Action<string, float> OnSoundPlayed;
    public System.Action<string> OnMusicChanged;
    
    void Awake()
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
    
    void Start()
    {
        // Start with gameplay music (since no menu implemented yet)
        PlayMusic("gameplay", 0.5f);
    }
    
    void OnDestroy()
    {
        // Clean up singleton reference when destroyed
        if (Instance == this)
        {
            Instance = null;
        }
        
        // Stop all audio sources
        if (musicSource != null) musicSource.Stop();
        if (uiSource != null) uiSource.Stop();
        if (overloadLoopSource != null) overloadLoopSource.Stop();
        
        if (sfxSources != null)
        {
            foreach (var source in sfxSources)
            {
                if (source != null) source.Stop();
            }
        }
        
        // Clear collections
        if (availableSfxSources != null) availableSfxSources.Clear();
        if (activeSfxSources != null) activeSfxSources.Clear();
        if (audioLibrary != null) audioLibrary.Clear();
    }
    
    private void InitializeAudioManager()
    {
        // Initialize audio library
        audioLibrary = new Dictionary<string, AudioClip>();
        BuildAudioLibrary();
        
        // Initialize SFX source pools
        availableSfxSources = new Queue<AudioSource>();
        activeSfxSources = new List<AudioSource>();
        
        // Setup SFX sources if not assigned
        if (sfxSources == null || sfxSources.Length == 0)
        {
            CreateSfxSources(8); // Create 8 SFX sources by default
        }
        else
        {
            foreach (var source in sfxSources)
            {
                if (source != null)
                {
                    availableSfxSources.Enqueue(source);
                    SetupSfxSource(source);
                }
            }
        }
        
        // Setup music source
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
        }
        
        // Setup UI source
        if (uiSource != null)
        {
            uiSource.volume = uiVolume * masterVolume;
        }
        
        // Create overload loop source
        CreateOverloadLoopSource();
        
        Debug.Log($"AudioManager initialized with {availableSfxSources.Count} SFX sources");
    }
    
    private void BuildAudioLibrary()
    {
        // Add all audio clips to library for easy access
        if (menuMusic) audioLibrary["menu_music"] = menuMusic;
        if (gameplayMusic) audioLibrary["gameplay_music"] = gameplayMusic;
        if (gameOverMusic) audioLibrary["gameover_music"] = gameOverMusic;
        
        if (resonanceLinkSound) audioLibrary["resonance_link"] = resonanceLinkSound;
        if (resonanceCompleteSound) audioLibrary["resonance_complete"] = resonanceCompleteSound;
        if (overloadActivateSound) audioLibrary["overload_activate"] = overloadActivateSound;
        if (overloadDeactivateSound) audioLibrary["overload_deactivate"] = overloadDeactivateSound;
        if (overloadLoopSound) audioLibrary["overload_loop"] = overloadLoopSound;
        
        if (enemyDestroySound) audioLibrary["enemy_destroy"] = enemyDestroySound;
        if (chargerRushSound) audioLibrary["charger_rush"] = chargerRushSound;
        if (flickerChangeSound) audioLibrary["flicker_change"] = flickerChangeSound;
        if (clusterExplodeSound) audioLibrary["cluster_explode"] = clusterExplodeSound;
        if (hunterAvoidSound) audioLibrary["hunter_avoid"] = hunterAvoidSound;
        
        if (buttonClickSound) audioLibrary["button_click"] = buttonClickSound;
        if (menuNavigateSound) audioLibrary["menu_navigate"] = menuNavigateSound;
        if (gameStartSound) audioLibrary["game_start"] = gameStartSound;
        if (gameOverSound) audioLibrary["game_over"] = gameOverSound;
        if (highScoreSound) audioLibrary["high_score"] = highScoreSound;
    }
    
    private void CreateSfxSources(int count)
    {
        sfxSources = new AudioSource[count];
        
        for (int i = 0; i < count; i++)
        {
            GameObject sfxObject = new GameObject($"SFX_Source_{i}");
            sfxObject.transform.SetParent(transform);
            
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            SetupSfxSource(source);
            
            sfxSources[i] = source;
            availableSfxSources.Enqueue(source);
        }
    }
    
    private void SetupSfxSource(AudioSource source)
    {
        source.loop = false;
        source.volume = sfxVolume * masterVolume;
        source.spatialBlend = enableSpatialAudio ? 1f : 0f;
        source.maxDistance = maxAudioDistance;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, audioRolloffCurve);
    }
    
    private void CreateOverloadLoopSource()
    {
        GameObject overloadObject = new GameObject("OverloadLoopSource");
        overloadObject.transform.SetParent(transform);
        
        overloadLoopSource = overloadObject.AddComponent<AudioSource>();
        overloadLoopSource.loop = true;
        overloadLoopSource.volume = sfxVolume * masterVolume * 0.6f; // Slightly quieter
        overloadLoopSource.spatialBlend = 0f; // Always 2D for overload
    }
    
    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySFX(string soundName, float volumeMultiplier = 1f)
    {
        if (audioLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySFX(clip, volumeMultiplier);
        }
        else
        {
            Debug.LogWarning($"AudioManager: Sound '{soundName}' not found in library");
        }
    }
    
    /// <summary>
    /// Play a sound effect with AudioClip
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSfxSource();
        if (source != null)
        {
            // Reset to 2D audio and AudioManager position for non-spatial sounds
            source.transform.position = transform.position;
            source.spatialBlend = 0f;
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeMultiplier;
            source.Play();
            
            StartCoroutine(ReturnSfxSourceWhenFinished(source));
            OnSoundPlayed?.Invoke(clip.name, volumeMultiplier);
        }
    }
    
    /// <summary>
    /// Play a positioned sound effect in 3D space
    /// </summary>
    public void PlaySFXAtPosition(string soundName, Vector3 position, float volumeMultiplier = 1f)
    {
        if (audioLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySFXAtPosition(clip, position, volumeMultiplier);
        }
    }
    
    /// <summary>
    /// Play a positioned sound effect in 3D space with AudioClip
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null || !enableSpatialAudio) 
        {
            PlaySFX(clip, volumeMultiplier);
            return;
        }
        
        AudioSource source = GetAvailableSfxSource();
        if (source != null)
        {
            // Configure for 3D spatial audio
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeMultiplier;
            source.Play();
            
            StartCoroutine(ReturnSfxSourceWhenFinished(source));
            OnSoundPlayed?.Invoke(clip.name, volumeMultiplier);
        }
    }
    
    /// <summary>
    /// Play music by name with optional fade
    /// </summary>
    public void PlayMusic(string musicName, float fadeTime = 1f)
    {
        string musicKey = musicName.ToLower();
        if (!musicKey.EndsWith("_music")) musicKey += "_music";
        
        if (audioLibrary.TryGetValue(musicKey, out AudioClip clip))
        {
            PlayMusic(clip, fadeTime);
        }
        else
        {
            Debug.LogWarning($"AudioManager: Music '{musicName}' not found in library");
        }
    }
    
    /// <summary>
    /// Play music with AudioClip and optional fade
    /// </summary>
    public void PlayMusic(AudioClip clip, float fadeTime = 1f)
    {
        if (clip == null || musicSource == null) return;
        
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(clip, fadeTime));
        OnMusicChanged?.Invoke(clip.name);
    }
    
    /// <summary>
    /// Stop music with optional fade
    /// </summary>
    public void StopMusic(float fadeTime = 1f)
    {
        if (musicSource == null) return;
        
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(fadeTime));
    }
    
    /// <summary>
    /// Play UI sound effect
    /// </summary>
    public void PlayUI(string soundName, float volumeMultiplier = 1f)
    {
        if (uiSource == null) return;
        
        if (audioLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            uiSource.volume = uiVolume * masterVolume * volumeMultiplier;
            uiSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// Start overload mode audio (looping sound)
    /// </summary>
    public void StartOverloadAudio()
    {
        PlaySFX("overload_activate");
        
        if (overloadLoopSource != null && overloadLoopSound != null)
        {
            overloadLoopSource.clip = overloadLoopSound;
            overloadLoopSource.volume = sfxVolume * masterVolume * 0.6f;
            overloadLoopSource.Play();
        }
    }
    
    /// <summary>
    /// Stop overload mode audio
    /// </summary>
    public void StopOverloadAudio()
    {
        PlaySFX("overload_deactivate");
        
        if (overloadLoopSource != null)
        {
            overloadLoopSource.Stop();
        }
    }
    
    /// <summary>
    /// Stop all instances of a specific sound by name
    /// </summary>
    public void StopSFX(string soundName)
    {
        if (!audioLibrary.TryGetValue(soundName, out AudioClip clip))
            return;
            
        // Stop all active sources playing this clip
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSfxSources[i];
            if (source.clip == clip && source.isPlaying)
            {
                source.Stop();
                // Source will be returned to pool by ReturnSfxSourceWhenFinished coroutine
            }
        }
    }
    
    private AudioSource GetAvailableSfxSource()
    {
        // Clean up finished sources
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                availableSfxSources.Enqueue(activeSfxSources[i]);
                activeSfxSources.RemoveAt(i);
            }
        }
        
        if (availableSfxSources.Count > 0)
        {
            AudioSource source = availableSfxSources.Dequeue();
            activeSfxSources.Add(source);
            return source;
        }
        
        Debug.LogWarning("AudioManager: No available SFX sources, sound may be skipped");
        return null;
    }
    
    private IEnumerator ReturnSfxSourceWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        
        activeSfxSources.Remove(source);
        availableSfxSources.Enqueue(source);
        
        // Reset position for non-spatial sounds
        source.transform.position = transform.position;
        source.spatialBlend = enableSpatialAudio ? 1f : 0f;
    }
    
    private IEnumerator FadeMusicCoroutine(AudioClip newClip, float fadeTime)
    {
        float originalVolume = musicVolume * masterVolume;
        
        // Fade out current music
        if (musicSource.isPlaying)
        {
            float fadeOutTime = fadeTime * 0.5f;
            float elapsed = 0f;
            
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / fadeOutTime);
                yield return null;
            }
        }
        
        // Change clip
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in new music
        float fadeInTime = fadeTime * 0.5f;
        float elapsedIn = 0f;
        
        while (elapsedIn < fadeInTime)
        {
            elapsedIn += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, originalVolume, elapsedIn / fadeInTime);
            yield return null;
        }
        
        musicSource.volume = originalVolume;
        musicFadeCoroutine = null;
    }
    
    private IEnumerator FadeOutMusicCoroutine(float fadeTime)
    {
        float originalVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(originalVolume, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = musicVolume * masterVolume;
        musicFadeCoroutine = null;
    }
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateSfxVolumes();
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (uiSource != null)
        {
            uiSource.volume = uiVolume * masterVolume;
        }
    }
    
    private void UpdateAllVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
        
        UpdateSfxVolumes();
        
        if (overloadLoopSource != null)
            overloadLoopSource.volume = sfxVolume * masterVolume * 0.6f;
    }
    
    private void UpdateSfxVolumes()
    {
        float targetVolume = sfxVolume * masterVolume;
        
        foreach (var source in activeSfxSources)
        {
            if (source != null)
            {
                source.volume = targetVolume;
            }
        }
        
        foreach (var source in availableSfxSources)
        {
            if (source != null)
            {
                source.volume = targetVolume;
            }
        }
    }
    
    // Public getters
    public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;
    public bool IsOverloadAudioPlaying() => overloadLoopSource != null && overloadLoopSource.isPlaying;
    public int GetActiveSfxCount() => activeSfxSources.Count;
    public int GetAvailableSfxCount() => availableSfxSources.Count;
}