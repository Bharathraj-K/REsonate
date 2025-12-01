using UnityEngine;

/// <summary>
/// Debug helper for troubleshooting OverloadMode issues
/// Shows detailed information about overload state and energy
/// </summary>
public class OverloadModeDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugUI = true;
    public bool enableConsoleLogging = true;
    public KeyCode forceEnergyKey = KeyCode.F5;
    public KeyCode forceActivateKey = KeyCode.F6;
    public KeyCode resetCooldownKey = KeyCode.F7;
    
    private OverloadMode overloadMode;
    private ScoreManager scoreManager;
    private ResonanceSystem resonanceSystem;
    
    void Start()
    {
        overloadMode = FindFirstObjectByType<OverloadMode>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        resonanceSystem = FindFirstObjectByType<ResonanceSystem>();
        
        if (enableConsoleLogging)
        {
            Debug.Log("=== OVERLOAD MODE DEBUGGER ===");
            Debug.Log($"OverloadMode found: {overloadMode != null}");
            Debug.Log($"ScoreManager found: {scoreManager != null}");
            Debug.Log($"ResonanceSystem found: {resonanceSystem != null}");
        }
    }
    
    void Update()
    {
        if (overloadMode == null) return;
        
        // Debug key inputs
        if (Input.GetKeyDown(forceEnergyKey))
        {
            overloadMode.AddEnergy(100f);
            Debug.Log("Debug: Forced energy to 100%");
        }
        
        if (Input.GetKeyDown(forceActivateKey))
        {
            bool activated = overloadMode.TryActivateOverload();
            Debug.Log($"Debug: Force activate overload result: {activated}");
        }
        
        if (Input.GetKeyDown(resetCooldownKey))
        {
            // Use reflection to reset cooldown
            var overloadType = typeof(OverloadMode);
            var cooldownField = overloadType.GetField("isOnCooldown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cooldownField != null)
            {
                cooldownField.SetValue(overloadMode, false);
                Debug.Log("Debug: Reset overload cooldown");
            }
        }
    }
    
    void OnGUI()
    {
        if (!showDebugUI || overloadMode == null) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 500));
        GUILayout.Label("=== OVERLOAD MODE DEBUG ===");
        
        // Energy info
        GUILayout.Label($"Energy: {overloadMode.GetEnergyPercentage():F1}%");
        GUILayout.Label($"Is Active: {overloadMode.IsOverloadActive()}");
        GUILayout.Label($"On Cooldown: {overloadMode.IsOnCooldown()}");
        GUILayout.Label($"Time Remaining: {overloadMode.GetOverloadTimeRemaining():F1}s");
        GUILayout.Label($"Cooldown Remaining: {overloadMode.GetCooldownTimeRemaining():F1}s");
        
        GUILayout.Space(10);
        
        // Player info
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GUILayout.Label($"Player Position: {player.transform.position}");
            ResonanceCore core = player.GetComponent<ResonanceCore>();
            if (core != null)
            {
                GUILayout.Label($"Player Color: {core.GetCurrentColor()}");
            }
        }
        
        GUILayout.Space(10);
        
        // Enemy info
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        GUILayout.Label($"Active Enemies: {enemies.Length}");
        
        if (player != null)
        {
            int enemiesInRange = 0;
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && !enemy.IsDestroyed())
                {
                    float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                    if (distance <= 15f) // Default shockwave radius
                    {
                        enemiesInRange++;
                    }
                }
            }
            GUILayout.Label($"Enemies in Shockwave Range: {enemiesInRange}");
        }
        
        GUILayout.Space(10);
        
        // Score info
        if (scoreManager != null)
        {
            GUILayout.Label($"Score: {scoreManager.GetCurrentScore()}");
            GUILayout.Label($"Combo: {scoreManager.GetCurrentCombo()}");
            GUILayout.Label($"Combo Level: {scoreManager.GetComboLevel()}");
        }
        
        // Resonance system info
        if (resonanceSystem != null)
        {
            GUILayout.Label($"Active Links: {resonanceSystem.GetActiveLinksCount()}");
        }
        
        GUILayout.Space(10);
        
        // Controls
        GUILayout.Label("=== DEBUG CONTROLS ===");
        GUILayout.Label("F5: Add 100% Energy");
        GUILayout.Label("F6: Force Activate");
        GUILayout.Label("F7: Reset Cooldown");
        GUILayout.Label("SPACE: Normal Activate");
        
        // Test buttons
        if (GUILayout.Button("Add 25% Energy"))
        {
            overloadMode.AddEnergy(25f);
        }
        
        if (GUILayout.Button("Trigger Test Kill"))
        {
            if (scoreManager != null)
            {
                scoreManager.AddEnemyKill(); // This should also add energy
            }
        }
        
        if (GUILayout.Button("Force Refresh Links"))
        {
            if (resonanceSystem != null)
            {
                resonanceSystem.ForceRefreshLinks();
            }
        }
        
        if (GUILayout.Button("Test Shockwave Effect"))
        {
            if (overloadMode != null)
            {
                // Use reflection to call private CreateShockwaveEffect method
                var methodInfo = typeof(OverloadMode).GetMethod("CreateShockwaveEffect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(overloadMode, null);
                }
            }
        }
        
        GUILayout.EndArea();
    }
}