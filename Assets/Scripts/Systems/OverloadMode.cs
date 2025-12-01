using UnityEngine;
using System.Collections;

/// <summary>
/// OverloadMode - The ultimate power fantasy mode for RESONATE!
/// When activated, player can link to any color and gains special abilities
/// </summary>
public class OverloadMode : MonoBehaviour
{
    [Header("Overload Settings")]
    [Tooltip("Duration of overload mode in seconds")]
    public float overloadDuration = 10f;
    
    [Tooltip("Cooldown time after overload ends")]
    public float overloadCooldown = 30f;
    
    [Tooltip("Energy required to activate overload (0-100)")]
    public float energyRequired = 100f;
    
    [Header("Energy Settings")]
    [Tooltip("Energy gained per enemy killed via resonance")]
    public float energyPerKill = 15f;
    
    [Tooltip("Energy gained per combo level")]
    public float energyPerComboLevel = 5f;
    
    [Tooltip("Energy decay rate per second when not gaining energy")]
    public float energyDecayRate = 2f;
    
    [Tooltip("Time before energy starts decaying")]
    public float energyDecayDelay = 5f;
    
    [Header("Overload Effects")]
    [Tooltip("Damage dealt by shockwave")]
    public float shockwaveDamage = 999f;
    
    [Tooltip("Radius of shockwave effect")]
    public float shockwaveRadius = 15f;
    
    [Tooltip("Force applied to enemies by shockwave")]
    public float shockwaveForce = 500f;
    
    [Header("Visual Effects")]
    [Tooltip("Shockwave effect prefab (from Cartoon FX Remaster Free or similar)")]
    public GameObject shockwaveEffectPrefab;
    
    [Tooltip("Duration to keep the shockwave effect active (if not auto-destroyed)")]
    public float shockwaveEffectDuration = 3f;
    
    [Tooltip("Scale multiplier for the shockwave effect")]
    public float shockwaveEffectScale = 1f;
    
    [Header("Audio Effects")]
    [Tooltip("Audio source for playing overload sounds")]
    public AudioSource audioSource;
    
    [Tooltip("Sound to play when overload activates")]
    public AudioClip overloadActivationSound;
    
    [Tooltip("Sound to play for shockwave effect")]
    public AudioClip shockwaveSound;
    
    // Current state
    private float currentEnergy = 0f;
    private bool isOverloadActive = false;
    private bool isOnCooldown = false;
    private float overloadEndTime = 0f;
    private float cooldownEndTime = 0f;
    private float lastEnergyGainTime = 0f;
    private ResonanceColor colorBeforeOverload = ResonanceColor.None;
    
    // References
    private ResonanceCore playerResonanceCore;
    private ResonanceSystem resonanceSystem;
    private Camera playerCamera;
    private Transform playerTransform;
    
    // Events
    public System.Action<float> OnEnergyChanged; // 0-100 percentage
    public System.Action OnOverloadActivated;
    public System.Action OnOverloadDeactivated;
    public System.Action<float> OnCooldownChanged; // 0-1 percentage
    
    // Singleton instance
    public static OverloadMode Instance { get; private set; }
    
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
        // Find required components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerResonanceCore = player.GetComponent<ResonanceCore>();
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("OverloadMode: Could not find player with 'Player' tag!");
        }
        
        resonanceSystem = FindFirstObjectByType<ResonanceSystem>();
        playerCamera = Camera.main;
        
        // Subscribe to score manager events for energy gain
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnComboChanged += OnComboGained;
        }
        
        // Initial energy update
        OnEnergyChanged?.Invoke(GetEnergyPercentage());
        
        Debug.Log($"OverloadMode: Initialized. Player: {(playerTransform != null ? "Found" : "NOT FOUND")}, Energy: {currentEnergy}/{energyRequired}");
    }
    
    void Update()
    {
        HandleInput();
        UpdateOverloadState();
        UpdateEnergyDecay();
        UpdateCooldown();
    }
    
    private void HandleInput()
    {
        // Activate overload with Space key (or designated overload key)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log($"OverloadMode: Input detected. Current energy: {currentEnergy:F1}/{energyRequired}, Active: {isOverloadActive}, Cooldown: {isOnCooldown}");
            bool success = TryActivateOverload();
            Debug.Log($"OverloadMode: Activation attempt result: {success}");
        }
    }
    
    private void UpdateOverloadState()
    {
        if (isOverloadActive)
        {
            // Check if overload should end
            if (Time.time >= overloadEndTime)
            {
                EndOverload();
            }
        }
    }
    
    private void UpdateEnergyDecay()
    {
        // Only decay energy if not in overload and delay has passed
        if (!isOverloadActive && !isOnCooldown && 
            Time.time - lastEnergyGainTime >= energyDecayDelay && 
            currentEnergy > 0f)
        {
            currentEnergy -= energyDecayRate * Time.deltaTime;
            currentEnergy = Mathf.Max(0f, currentEnergy);
            OnEnergyChanged?.Invoke(GetEnergyPercentage());
        }
    }
    
    private void UpdateCooldown()
    {
        if (isOnCooldown)
        {
            float cooldownProgress = 1f - ((cooldownEndTime - Time.time) / overloadCooldown);
            cooldownProgress = Mathf.Clamp01(cooldownProgress);
            OnCooldownChanged?.Invoke(cooldownProgress);
            
            if (Time.time >= cooldownEndTime)
            {
                isOnCooldown = false;
                OnCooldownChanged?.Invoke(1f);
            }
        }
    }
    
    public void AddEnergy(float amount)
    {
        if (isOverloadActive || isOnCooldown) return;
        
        currentEnergy += amount;
        currentEnergy = Mathf.Min(energyRequired, currentEnergy);
        lastEnergyGainTime = Time.time;
        
        OnEnergyChanged?.Invoke(GetEnergyPercentage());
        
        Debug.Log($"OverloadMode: Energy added {amount}, Total: {currentEnergy:F1}/{energyRequired}");
    }
    
    public void AddEnergyForKill()
    {
        float energy = energyPerKill;
        
        // Bonus energy based on current combo level
        if (ScoreManager.Instance != null)
        {
            int comboLevel = GetCurrentComboLevel();
            energy += comboLevel * energyPerComboLevel;
        }
        
        AddEnergy(energy);
    }
    
    private void OnComboGained(int comboCount)
    {
        // Small energy bonus for maintaining combos
        if (comboCount > 0)
        {
            AddEnergy(energyPerComboLevel * 0.5f);
        }
    }
    
    private int GetCurrentComboLevel()
    {
        // Access combo level from ScoreManager if available
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.GetComboLevel();
        }
        return 1;
    }
    
    public bool TryActivateOverload()
    {
        if (isOverloadActive || isOnCooldown || currentEnergy < energyRequired)
        {
            Debug.Log($"OverloadMode: Cannot activate - Active: {isOverloadActive}, Cooldown: {isOnCooldown}, Energy: {currentEnergy}/{energyRequired}");
            return false;
        }
        
        StartOverload();
        return true;
    }
    
    private void StartOverload()
    {
        isOverloadActive = true;
        overloadEndTime = Time.time + overloadDuration;
        currentEnergy = 0f; // Consume all energy
        
        OnOverloadActivated?.Invoke();
        OnEnergyChanged?.Invoke(0f);
        
        // Phase 8: Enhanced overload activation effects
        Vector3 playerPosition = transform.position;
        
        // Enhanced audio system
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartOverloadAudio();
        }
        else if (audioSource != null && overloadActivationSound != null)
        {
            audioSource.PlayOneShot(overloadActivationSound);
        }
        
        Debug.Log("OverloadMode: ACTIVATED! Duration: " + overloadDuration + " seconds");
        
        // Store current color BEFORE any changes for restoration later
        if (playerResonanceCore != null)
        {
            colorBeforeOverload = playerResonanceCore.GetCurrentColor();
            Debug.Log($"OverloadMode: Storing color before overload: {colorBeforeOverload}");
        }
        
        // Force refresh resonance links immediately for multi-frequency state
        if (ResonanceSystem.Instance != null)
        {
            ResonanceSystem.Instance.ForceRefreshLinks();
        }
        
        // Enable multi-frequency state
        if (playerResonanceCore != null)
        {
            StartCoroutine(MultiFrequencyEffect());
        }
        
        // Phase 8: Enhanced shockwave with effects
        TriggerEnhancedShockwave();
        
        // Start screen effects
        StartScreenEffects();
    }
    
    private void EndOverload()
    {
        isOverloadActive = false;
        isOnCooldown = true;
        cooldownEndTime = Time.time + overloadCooldown;
        
        OnOverloadDeactivated?.Invoke();
        OnCooldownChanged?.Invoke(0f);
        
        // Phase 8: Enhanced overload deactivation effects
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopOverloadAudio();
        }
        
        Debug.Log("OverloadMode: DEACTIVATED! Cooldown: " + overloadCooldown + " seconds");
        
        // Force refresh resonance links to return to normal color-matching
        if (ResonanceSystem.Instance != null)
        {
            ResonanceSystem.Instance.ForceRefreshLinks();
        }
        
        // End screen effects
        EndScreenEffects();
    }
    
    private IEnumerator MultiFrequencyEffect()
    {
        // Visual-only effect - the actual multi-frequency logic is handled in ResonanceSystem
        // by checking IsOverloadActive() and ignoring color matching entirely
        
        while (isOverloadActive)
        {
            // Cycle through colors rapidly for visual feedback to show multi-frequency state
            if (playerResonanceCore != null)
            {
                ResonanceColor[] colors = { ResonanceColor.Red, ResonanceColor.Green, ResonanceColor.Blue };
                int colorIndex = Mathf.FloorToInt(Time.time * 3f) % colors.Length; // Slower cycling to reduce interference
                
                // Only change color if it's different to avoid constant SetColor calls
                ResonanceColor newColor = colors[colorIndex];
                if (playerResonanceCore.GetCurrentColor() != newColor)
                {
                    playerResonanceCore.SetColor(newColor);
                }
            }
            
            yield return new WaitForSeconds(0.3f); // Less frequent updates to reduce interference
        }
        
        // Restore original color when overload ends using the stored color from activation
        if (playerResonanceCore != null)
        {
            playerResonanceCore.SetColor(colorBeforeOverload);
            Debug.Log($"OverloadMode: Multi-frequency effect ended, restored color to: {colorBeforeOverload}");
        }
        
        Debug.Log("OverloadMode: Multi-frequency effect ended, player can manually select colors again");
    }
    
    private void TriggerShockwave()
    {
        if (playerTransform == null)
        {
            Debug.LogError("OverloadMode: Cannot trigger shockwave - player transform not found!");
            return;
        }
        
        Vector3 shockwaveCenter = playerTransform.position;
        
        // Find all enemies in range and destroy them
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int enemiesDestroyed = 0;
        
        Debug.Log($"OverloadMode: Triggering shockwave at {shockwaveCenter} with radius {shockwaveRadius}. Found {allEnemies.Length} enemies to check.");
        
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && !enemy.IsDestroyed())
            {
                float distance = Vector3.Distance(shockwaveCenter, enemy.transform.position);
                Debug.Log($"OverloadMode: Enemy at {enemy.transform.position}, distance: {distance:F2}");
                
                if (distance <= shockwaveRadius)
                {
                    Debug.Log($"OverloadMode: Destroying enemy at distance {distance:F2}");
                    
                    // Apply damage/destruction
                    enemy.TakeDamage(shockwaveDamage);
                    
                    // Award score for shockwave kills
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddEnemyKill();
                    }
                    
                    // Apply force for visual effect
                    Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        Vector3 forceDirection = (enemy.transform.position - shockwaveCenter).normalized;
                        enemyRb.AddForce(forceDirection * shockwaveForce, ForceMode.Impulse);
                    }
                    
                    enemiesDestroyed++;
                }
            }
        }
        
        Debug.Log($"OverloadMode: Shockwave destroyed {enemiesDestroyed} enemies");
        
        // Play shockwave sound
        if (audioSource != null && shockwaveSound != null)
        {
            audioSource.PlayOneShot(shockwaveSound);
        }
        
        // Create visual shockwave effect
        CreateShockwaveEffect();
    }
    
    private void CreateShockwaveEffect()
    {
        if (playerTransform == null) return;
        
        if (shockwaveEffectPrefab != null)
        {
            // Use the assigned prefab effect (Cartoon FX Remaster Free)
            Instantiate(shockwaveEffectPrefab, playerTransform.position, shockwaveEffectPrefab.transform.rotation);
        }
        else
        {
            // Fallback to primitive effect if no prefab assigned
            CreateFallbackShockwaveEffect();
        }
    }
    
    
    
    private void CreateFallbackShockwaveEffect()
    {
        // Fallback primitive effect (original implementation)
        GameObject shockwaveObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shockwaveObj.name = "OverloadShockwave_Fallback";
        shockwaveObj.transform.position = playerTransform.position;
        shockwaveObj.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
        
        // Remove collider
        Collider shockwaveCollider = shockwaveObj.GetComponent<Collider>();
        if (shockwaveCollider != null) Destroy(shockwaveCollider);
        
        // Set material
        Renderer shockwaveRenderer = shockwaveObj.GetComponent<Renderer>();
        if (shockwaveRenderer != null)
        {
            Material shockwaveMaterial = new Material(Shader.Find("Standard"));
            shockwaveMaterial.color = new Color(1f, 1f, 0f, 0.5f); // Yellow, semi-transparent
            shockwaveMaterial.SetFloat("_Mode", 3f); // Transparent mode
            shockwaveMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            shockwaveMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            shockwaveMaterial.SetInt("_ZWrite", 0);
            shockwaveMaterial.DisableKeyword("_ALPHATEST_ON");
            shockwaveMaterial.EnableKeyword("_ALPHABLEND_ON");
            shockwaveMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            shockwaveMaterial.renderQueue = 3000;
            
            shockwaveRenderer.material = shockwaveMaterial;
        }
        
        // Animate expansion
        StartCoroutine(AnimateShockwave(shockwaveObj));
        
        Debug.Log("OverloadMode: Using fallback primitive shockwave effect (no prefab assigned)");
    }
    
    private IEnumerator AnimateShockwave(GameObject shockwaveObj)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.1f, 0.01f, 0.1f);
        Vector3 endScale = new Vector3(shockwaveRadius * 2f, 0.01f, shockwaveRadius * 2f);
        
        while (elapsed < duration && shockwaveObj != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Scale up
            shockwaveObj.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            // Fade out
            Renderer renderer = shockwaveObj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(0.5f, 0f, progress);
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        // Clean up
        if (shockwaveObj != null)
        {
            Destroy(shockwaveObj);
        }
    }
    
    private void StartScreenEffects()
    {
        // Simple screen shake effect
        if (playerCamera != null)
        {
            StartCoroutine(ScreenShakeEffect());
        }
    }
    
    private void EndScreenEffects()
    {
        // Stop screen effects
    }
    
    private IEnumerator ScreenShakeEffect()
    {
        Vector3 originalPosition = playerCamera.transform.localPosition;
        float shakeIntensity = 0.5f;
        
        float elapsed = 0f;
        while (isOverloadActive && elapsed < 1f) // Shake for first second of overload
        {
            elapsed += Time.deltaTime;
            
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            randomOffset.z = 0f; // Keep camera at same depth
            
            playerCamera.transform.localPosition = originalPosition + randomOffset;
            
            yield return null;
        }
        
        // Return to original position
        playerCamera.transform.localPosition = originalPosition;
    }
    
    // Phase 8: Enhanced shockwave with effects
    private void TriggerEnhancedShockwave()
    {
        // Add dramatic screen shake for overload activation
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeExplosion();
        }
        
        // Call original shockwave logic (handles visual effects internally)
        TriggerShockwave();
    }
    
    // Public getters
    public bool IsOverloadActive() => isOverloadActive;
    public bool IsOnCooldown() => isOnCooldown;
    public float GetEnergyPercentage() => (currentEnergy / energyRequired) * 100f;
    public float GetOverloadTimeRemaining() => isOverloadActive ? (overloadEndTime - Time.time) : 0f;
    public float GetCooldownTimeRemaining() => isOnCooldown ? (cooldownEndTime - Time.time) : 0f;
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnComboChanged -= OnComboGained;
        }
    }
}