using UnityEngine;
using System.Collections;

/// <summary>
/// CameraShake - Provides various camera shake effects for game juice and polish
/// Integrates with game events to provide satisfying screen feedback
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Camera to apply shake effects to")]
    public Camera targetCamera;
    
    [Tooltip("Base intensity multiplier for all shake effects")]
    public float globalIntensity = 1f;
    
    [Header("Shake Presets")]
    [Tooltip("Light shake for small actions (enemy destruction, hits)")]
    public ShakePreset lightShake = new ShakePreset(0.1f, 0.2f, 10f);
    
    [Tooltip("Medium shake for moderate actions (combos, special kills)")]
    public ShakePreset mediumShake = new ShakePreset(0.3f, 0.5f, 15f);
    
    [Tooltip("Heavy shake for major actions (overload activation, boss events)")]
    public ShakePreset heavyShake = new ShakePreset(0.8f, 1.0f, 20f);
    
    [Tooltip("Explosion shake for dramatic moments (large enemy groups destroyed)")]
    public ShakePreset explosionShake = new ShakePreset(1.2f, 0.8f, 25f);
    
    // Internal state
    private Vector3 originalPosition;
    private Coroutine currentShakeCoroutine;
    private bool isShaking = false;
    
    // Singleton instance
    public static CameraShake Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Auto-find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (targetCamera != null)
        {
            originalPosition = targetCamera.transform.localPosition;
        }
        else
        {
            Debug.LogError("CameraShake: No camera found to apply shake effects to!");
        }
    }
    
    /// <summary>
    /// Trigger a light shake effect
    /// </summary>
    public void ShakeLight()
    {
        TriggerShake(lightShake);
    }
    
    /// <summary>
    /// Trigger a medium shake effect
    /// </summary>
    public void ShakeMedium()
    {
        TriggerShake(mediumShake);
    }
    
    /// <summary>
    /// Trigger a heavy shake effect
    /// </summary>
    public void ShakeHeavy()
    {
        TriggerShake(heavyShake);
    }
    
    /// <summary>
    /// Trigger an explosion shake effect
    /// </summary>
    public void ShakeExplosion()
    {
        TriggerShake(explosionShake);
    }
    
    /// <summary>
    /// Trigger a custom shake with specific parameters
    /// </summary>
    public void ShakeCustom(float intensity, float duration, float frequency = 20f)
    {
        ShakePreset customShake = new ShakePreset(intensity, duration, frequency);
        TriggerShake(customShake);
    }
    
    /// <summary>
    /// Stop any active shake effects immediately
    /// </summary>
    public void StopShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }
        
        isShaking = false;
        
        if (targetCamera != null)
        {
            targetCamera.transform.localPosition = originalPosition;
        }
    }
    
    private void TriggerShake(ShakePreset preset)
    {
        if (targetCamera == null || globalIntensity <= 0f) return;
        
        // Stop any existing shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        
        // Start new shake
        currentShakeCoroutine = StartCoroutine(ShakeCoroutine(preset));
    }
    
    private IEnumerator ShakeCoroutine(ShakePreset preset)
    {
        isShaking = true;
        float elapsed = 0f;
        float adjustedIntensity = preset.intensity * globalIntensity;
        
        while (elapsed < preset.duration)
        {
            elapsed += Time.deltaTime;
            
            // Calculate shake intensity (fade out over time)
            float currentIntensity = adjustedIntensity * (1f - (elapsed / preset.duration));
            
            // Generate shake offset
            Vector3 shakeOffset = Random.insideUnitSphere * currentIntensity;
            shakeOffset.z = 0f; // Keep camera at same depth
            
            // Apply shake to camera
            targetCamera.transform.localPosition = originalPosition + shakeOffset;
            
            // Wait for next frame based on frequency
            yield return new WaitForSeconds(1f / preset.frequency);
        }
        
        // Ensure camera returns to original position
        targetCamera.transform.localPosition = originalPosition;
        isShaking = false;
        currentShakeCoroutine = null;
    }
    
    // Event subscription helpers for automatic shake triggering
    void OnEnable()
    {
        // Subscribe to game events for automatic shake effects
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnComboLevelChanged += OnComboLevelChanged;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnComboLevelChanged -= OnComboLevelChanged;
        }
    }
    
    private void OnComboLevelChanged(int newLevel)
    {
        // Increase shake intensity based on combo level
        if (newLevel >= 8)
        {
            ShakeHeavy();
        }
        else if (newLevel >= 5)
        {
            ShakeMedium();
        }
        else if (newLevel >= 3)
        {
            ShakeLight();
        }
    }
    
    // Public getters
    public bool IsShaking() => isShaking;
    public float GetGlobalIntensity() => globalIntensity;
    
    // Allow runtime intensity adjustment
    public void SetGlobalIntensity(float intensity)
    {
        globalIntensity = Mathf.Clamp01(intensity);
    }
}

/// <summary>
/// Data structure for shake effect presets
/// </summary>
[System.Serializable]
public class ShakePreset
{
    [Tooltip("Intensity of the shake effect")]
    public float intensity;
    
    [Tooltip("Duration of the shake effect in seconds")]
    public float duration;
    
    [Tooltip("Frequency of shake updates per second")]
    public float frequency;
    
    public ShakePreset(float intensity, float duration, float frequency)
    {
        this.intensity = intensity;
        this.duration = duration;
        this.frequency = frequency;
    }
}