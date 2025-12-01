using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player Health system - tracks lives/health and handles damage from enemy collisions
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Number of lives the player has")]
    public int maxLives = 3;
    
    [Tooltip("Invincibility time after taking damage (in seconds)")]
    public float invincibilityDuration = 2f;
    
    [Header("Visual Feedback")]
    [Tooltip("Material to flash when taking damage")]
    public Material damageMaterial;
    
    [Tooltip("How fast to blink during invincibility")]
    public float blinkSpeed = 5f;
    
    // Current state
    private int currentLives;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    // Components
    private Renderer playerRenderer;
    private Material originalMaterial;
    
    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action OnPlayerDeath;
    public System.Action OnDamageTaken;
    
    void Start()
    {
        // Initialize health
        currentLives = maxLives;
        
        // Get renderer for visual feedback
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }
        
        // Notify UI of initial lives
        OnLivesChanged?.Invoke(currentLives);
    }
    
    void Update()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Handle blinking effect during invincibility
            if (playerRenderer != null)
            {
                float alpha = Mathf.Sin(Time.time * blinkSpeed * 2f * Mathf.PI) * 0.5f + 0.5f;
                Color color = playerRenderer.material.color;
                color.a = Mathf.Lerp(0.3f, 1f, alpha);
                playerRenderer.material.color = color;
            }
            
            // End invincibility
            if (invincibilityTimer <= 0f)
            {
                EndInvincibility();
            }
        }
    }
    
    public void TakeDamage(int damageAmount = 1)
    {
        // Don't take damage if invincible
        if (isInvincible) return;
        
        // Reduce lives
        currentLives -= damageAmount;
        currentLives = Mathf.Max(0, currentLives);
        
        // Trigger events
        OnDamageTaken?.Invoke();
        OnLivesChanged?.Invoke(currentLives);
        
        Debug.Log($"Player took damage! Lives remaining: {currentLives}");
        
        // Check if player died
        if (currentLives <= 0)
        {
            HandlePlayerDeath();
        }
        else
        {
            // Start invincibility frames
            StartInvincibility();
        }
    }
    
    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        
        // Change material to damage material if available
        if (playerRenderer != null && damageMaterial != null)
        {
            playerRenderer.material = damageMaterial;
        }
    }
    
    void EndInvincibility()
    {
        isInvincible = false;
        
        // Restore original material
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
            
            // Ensure full opacity
            Color color = playerRenderer.material.color;
            color.a = 1f;
            playerRenderer.material.color = color;
        }
    }
    
    void HandlePlayerDeath()
    {
        Debug.Log("Player died! Restarting game...");
        
        // Play game over sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("game_over");
        }
        
        // Reset score manager before death
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetGame();
        }
        
        // Trigger death event
        OnPlayerDeath?.Invoke();
        
        // Simple restart for now - reload the current scene
        StartCoroutine(RestartGameAfterDelay());
    }
    
    System.Collections.IEnumerator RestartGameAfterDelay()
    {
        // Brief pause before restart so player can see what happened
        yield return new WaitForSeconds(1f);
        
        // Clean up singletons and persistent objects before scene reload
        CleanupBeforeReload();
        
        // Small delay to ensure cleanup completes
        yield return new WaitForEndOfFrame();
        
        // Reload current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    private void CleanupBeforeReload()
    {
        // Reset singleton instances to prevent conflicts
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllCoroutines();
        }
        
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.StopAllCoroutines();
        }
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StopAllCoroutines();
        }
        
        // Force garbage collection to clean up references
        System.GC.Collect();
        
        Debug.Log("PlayerHealth: Cleaned up before scene reload");
    }
    
    // Public getters for other systems
    public int GetCurrentLives() => currentLives;
    public int GetMaxLives() => maxLives;
    public bool IsInvincible() => isInvincible;
    
    // Public method to restore health (for power-ups later)
    public void AddLife(int amount = 1)
    {
        currentLives += amount;
        currentLives = Mathf.Min(currentLives, maxLives);
        OnLivesChanged?.Invoke(currentLives);
    }
}