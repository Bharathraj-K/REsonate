using UnityEngine;
using System.Collections;

/// <summary>
/// EffectsManager - Manages particle effects, visual polish, and Cartoon FX integration
/// Provides centralized effect spawning and management for RESONATE!
/// </summary>
public class EffectsManager : MonoBehaviour
{
    [Header("Cartoon FX Prefabs")]
    [Tooltip("Enemy destruction effect prefab")]
    public GameObject enemyDestructionPrefab;
    
    [Tooltip("Resonance link effect prefab")]
    public GameObject resonanceLinkPrefab;
    
    [Tooltip("Resonance complete effect prefab")]
    public GameObject resonanceCompletePrefab;
    

    
    [Tooltip("Combo effect prefab")]
    public GameObject comboPrefab;
    
    [Tooltip("High score effect prefab")]
    public GameObject highScorePrefab;
    
    [Header("Fallback Effects")]
    [Tooltip("Basic particle system for fallback effects")]
    public ParticleSystem fallbackParticleSystem;
    
    [Header("Effect Settings")]
    [Tooltip("Automatically destroy effect objects after duration")]
    public bool autoDestroyEffects = true;
    
    [Tooltip("Default effect lifetime in seconds")]
    public float defaultEffectLifetime = 3f;
    
    [Tooltip("Effect intensity multiplier")]
    [Range(0f, 2f)]
    public float effectIntensity = 1f;
    
    [Tooltip("Enable screen-space effects")]
    public bool enableScreenEffects = true;
    
    // Effect pools for performance
    private Transform effectsParent;
    private System.Collections.Generic.Queue<GameObject> pooledEffects;
    
    // Singleton instance
    public static EffectsManager Instance { get; private set; }
    
    // Events
    public System.Action<Vector3, EffectType> OnEffectTriggered;
    
    public enum EffectType
    {
        EnemyDestruction,
        ResonanceLink,
        ResonanceComplete,
        Combo,
        HighScore,
        Custom
    }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            InitializeEffectsManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeEffectsManager()
    {
        // Create effects parent object for organization
        GameObject effectsContainer = new GameObject("Effects");
        effectsContainer.transform.SetParent(transform);
        effectsParent = effectsContainer.transform;
        
        // Initialize effect pool
        pooledEffects = new System.Collections.Generic.Queue<GameObject>();
        
        // Create fallback particle system if not assigned
        if (fallbackParticleSystem == null)
        {
            CreateFallbackParticleSystem();
        }
        
        Debug.Log("EffectsManager initialized with Cartoon FX integration");
    }
    
    private void CreateFallbackParticleSystem()
    {
        GameObject fallbackObject = new GameObject("FallbackParticleSystem");
        fallbackObject.transform.SetParent(effectsParent);
        
        fallbackParticleSystem = fallbackObject.AddComponent<ParticleSystem>();
        
        // Configure basic fallback particle system
        var main = fallbackParticleSystem.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 5f;
        main.maxParticles = 50;
        main.startSize = 0.3f;
        main.startColor = Color.white;
        
        var emission = fallbackParticleSystem.emission;
        emission.rateOverTime = 0f; // Only burst emission
        
        var shape = fallbackParticleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        var velocityOverLifetime = fallbackParticleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);
        
        var colorOverLifetime = fallbackParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
    }
    
    /// <summary>
    /// Play enemy destruction effect at position
    /// </summary>
    public void PlayEnemyDestruction(Vector3 position, Color enemyColor)
    {
        PlayEffect(EffectType.EnemyDestruction, position, enemyColor);
        
        // Trigger screen shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeLight();
        }
        
        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAtPosition("enemy_destroy", position, 0.8f);
        }
    }
    
    /// <summary>
    /// Play resonance link effect between two positions
    /// </summary>
    public void PlayResonanceLink(Vector3 startPos, Vector3 endPos, Color linkColor)
    {
        Vector3 midpoint = (startPos + endPos) * 0.5f;
        PlayEffect(EffectType.ResonanceLink, midpoint, linkColor);
        
        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("resonance_link", 0.6f);
        }
        
        OnEffectTriggered?.Invoke(midpoint, EffectType.ResonanceLink);
    }
    
    /// <summary>
    /// Play resonance complete effect
    /// </summary>
    public void PlayResonanceComplete(Vector3 position, Color resonanceColor, int chainLength)
    {
        PlayEffect(EffectType.ResonanceComplete, position, resonanceColor);
        
        // Scale screen shake based on chain length
        if (CameraShake.Instance != null)
        {
            if (chainLength >= 6)
                CameraShake.Instance.ShakeHeavy();
            else if (chainLength >= 4)
                CameraShake.Instance.ShakeMedium();
            else
                CameraShake.Instance.ShakeLight();
        }
        
        // Play audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("resonance_complete", Mathf.Clamp(chainLength * 0.2f, 0.5f, 1.5f));
        }
        
        OnEffectTriggered?.Invoke(position, EffectType.ResonanceComplete);
    }
    

    
    /// <summary>
    /// Play combo effect
    /// </summary>
    public void PlayComboEffect(Vector3 position, int comboLevel)
    {
        Color comboColor = GetComboColor(comboLevel);
        PlayEffect(EffectType.Combo, position, comboColor);
        
        OnEffectTriggered?.Invoke(position, EffectType.Combo);
    }
    
    /// <summary>
    /// Play high score effect
    /// </summary>
    public void PlayHighScoreEffect(Vector3 position)
    {
        PlayEffect(EffectType.HighScore, position, Color.gold);
        
        // Special screen shake for high score
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeHeavy();
        }
        
        // Play high score audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("high_score", 1.2f);
        }
        
        OnEffectTriggered?.Invoke(position, EffectType.HighScore);
    }
    
    private void PlayEffect(EffectType effectType, Vector3 position, Color color)
    {
        GameObject effectPrefab = GetEffectPrefab(effectType);
        
        if (effectPrefab != null)
        {
            // Instantiate Cartoon FX prefab
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity, effectsParent);
            
            // Apply color tinting if possible
            ApplyColorToEffect(effect, color);
            
            // Scale effect based on intensity
            if (effectIntensity != 1f)
            {
                effect.transform.localScale = Vector3.one * effectIntensity;
            }
            
            // Auto-destroy effect
            if (autoDestroyEffects)
            {
                StartCoroutine(DestroyEffectAfterDelay(effect, defaultEffectLifetime));
            }
        }
        else
        {
            // Use fallback particle system
            PlayFallbackEffect(position, color, effectType);
        }
    }
    
    private GameObject GetEffectPrefab(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.EnemyDestruction:
                return enemyDestructionPrefab;
            case EffectType.ResonanceLink:
                return resonanceLinkPrefab;
            case EffectType.ResonanceComplete:
                return resonanceCompletePrefab;

            case EffectType.Combo:
                return comboPrefab;
            case EffectType.HighScore:
                return highScorePrefab;
            default:
                return null;
        }
    }
    
    private void ApplyColorToEffect(GameObject effect, Color color)
    {
        // Try to apply color to particle systems in the effect
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
        
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            if (main.startColor.mode == ParticleSystemGradientMode.Color)
            {
                main.startColor = color;
            }
            else if (main.startColor.mode == ParticleSystemGradientMode.TwoColors)
            {
                main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.5f));
            }
        }
        
        // Try to apply color to renderers with materials that support tinting
        Renderer[] renderers = effect.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            if (renderer.material.HasProperty("_TintColor"))
            {
                renderer.material.SetColor("_TintColor", color);
            }
            else if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.SetColor("_Color", color);
            }
        }
    }
    
    private void PlayFallbackEffect(Vector3 position, Color color, EffectType effectType)
    {
        if (fallbackParticleSystem == null) return;
        
        // Move fallback system to position
        fallbackParticleSystem.transform.position = position;
        
        // Configure fallback effect based on type
        var main = fallbackParticleSystem.main;
        var emission = fallbackParticleSystem.emission;
        
        main.startColor = color;
        
        switch (effectType)
        {
            case EffectType.EnemyDestruction:
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 15) });
                main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
                break;
            
            case EffectType.ResonanceLink:
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 8) });
                main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
                break;
            
            case EffectType.ResonanceComplete:
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 25) });
                main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
                break;
            

            
            default:
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 10) });
                main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
                break;
        }
        
        fallbackParticleSystem.Play();
    }
    
    private Color GetComboColor(int comboLevel)
    {
        // Return different colors based on combo level
        if (comboLevel >= 10) return Color.magenta;      // Ultra combo
        if (comboLevel >= 7) return Color.red;           // Super combo  
        if (comboLevel >= 5) return Color.yellow;        // Great combo
        if (comboLevel >= 3) return Color.green;         // Good combo
        return Color.white;                              // Basic combo
    }
    
    private IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effect != null)
        {
            Destroy(effect);
        }
    }
    
    // Public getters and setters
    public void SetEffectIntensity(float intensity)
    {
        effectIntensity = Mathf.Clamp(intensity, 0f, 2f);
    }
    
    public void SetAutoDestroy(bool autoDestroy)
    {
        autoDestroyEffects = autoDestroy;
    }
    
    public void SetDefaultLifetime(float lifetime)
    {
        defaultEffectLifetime = Mathf.Max(0.1f, lifetime);
    }
    
    public bool HasEffect(EffectType effectType)
    {
        return GetEffectPrefab(effectType) != null;
    }
    
    public int GetActiveEffectsCount()
    {
        return effectsParent != null ? effectsParent.childCount : 0;
    }
}