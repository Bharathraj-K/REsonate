using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Core resonance linking system - manages the central gameplay mechanic
/// where players can link to same-colored enemies through harmonic energy beams
/// </summary>
public class ResonanceSystem : MonoBehaviour
{
    [Header("Resonance Settings")]
    public float linkRange = 5f;
    public float linkDuration = 2f;
    public LayerMask enemyLayerMask = -1;
    
    [Header("Visual Settings")]
    public GameObject linkBeamPrefab;
    public Material redBeamMaterial;
    public Material greenBeamMaterial;
    public Material blueBeamMaterial;
    
    // Core components
    private ResonanceCore playerCore;
    private Transform playerTransform;
    
    // Active links tracking
    private Dictionary<Enemy, ResonanceLink> activeLinks = new Dictionary<Enemy, ResonanceLink>();
    
    // Performance optimization
    private float lastEnemyScanTime = 0f;
    private const float ENEMY_SCAN_INTERVAL = 0.1f; // Scan for enemies every 100ms instead of every frame
    
    // Audio control
    private bool suppressLinkAudio = false;
    
    // Events
    public System.Action<Enemy> OnEnemyLinked;
    public System.Action<Enemy> OnEnemyDestroyed;
    public System.Action<int> OnComboChanged;
    
    // Singleton instance for OverloadMode integration
    public static ResonanceSystem Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
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
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCore = player.GetComponent<ResonanceCore>();
            playerTransform = player.transform;
        }
        
        if (playerCore == null)
        {
            Debug.LogError("ResonanceSystem: Could not find ResonanceCore component on player!");
        }
    }
    
    void Update()
    {
        if (playerCore == null || playerTransform == null) return;
        
        // Only scan for new enemies periodically to improve performance
        if (Time.time - lastEnemyScanTime >= ENEMY_SCAN_INTERVAL)
        {
            lastEnemyScanTime = Time.time;
            
            // Only link if player has an active color
            if (playerCore.GetCurrentColor() != ResonanceColor.None)
            {
                UpdateResonanceLinks();
            }
            else
            {
                // Clear all links if player has no color
                ClearAllLinks();
            }
            
            // Clean up any null references from destroyed enemies
            CleanupDestroyedEnemies();
        }
        
        // Update existing link timers every frame (for smooth visual updates)
        UpdateLinkTimers();
    }
    
    void UpdateResonanceLinks()
    {
        // Find all enemies in range
        Collider[] enemiesInRange = Physics.OverlapSphere(
            playerTransform.position, 
            linkRange, 
            enemyLayerMask
        );
        
        ResonanceColor playerColor = playerCore.GetCurrentColor();
        
        foreach (Collider col in enemiesInRange)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning($"Collider {col.name} has no Enemy component!");
                continue;
            }
            
            if (enemy.IsDestroyed())
            {
                continue;
            }
            
            // Check if colors match (or if in overload mode, link to any color)
            bool canLink = false;
            bool isOverloadActive = (OverloadMode.Instance != null && OverloadMode.Instance.IsOverloadActive());
            
            if (isOverloadActive)
            {
                // During overload, can link to ANY enemy color (multi-frequency mode)
                // Player color is irrelevant during overload
                canLink = (enemy.GetColor() != ResonanceColor.None);
                
                // Debug log for overload linking
                if (canLink && !activeLinks.ContainsKey(enemy))
                {
                    Debug.Log($"ResonanceSystem: OVERLOAD MULTI-FREQUENCY LINK - Player: {playerColor}, Enemy: {enemy.GetColor()} -> LINKED!");
                }
            }
            else
            {
                // Normal mode: colors must match exactly AND player must have a color selected
                canLink = (enemy.GetColor() == playerColor && playerColor != ResonanceColor.None);
                
                // Debug log for normal linking
                if (canLink && !activeLinks.ContainsKey(enemy))
                {
                    Debug.Log($"ResonanceSystem: NORMAL COLOR MATCH - Player: {playerColor}, Enemy: {enemy.GetColor()} -> LINKED!");
                }
            }
            
            if (canLink)
            {
                // Create or maintain link
                if (!activeLinks.ContainsKey(enemy))
                {
                    CreateLink(enemy);
                }
            }
            else
            {
                // Remove link if colors no longer match (or overload ended)
                if (activeLinks.ContainsKey(enemy))
                {
                    RemoveLink(enemy);
                }
            }
        }
        
        // Remove links for enemies that are out of range or destroyed
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (var kvp in activeLinks)
        {
            Enemy enemy = kvp.Key;
            
            // Check if enemy still exists and is not destroyed
            if (enemy == null || enemy.IsDestroyed())
            {
                enemiesToRemove.Add(enemy);
                continue;
            }
            
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            
            if (distance > linkRange)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        foreach (Enemy enemy in enemiesToRemove)
        {
            RemoveLink(enemy);
        }
    }
    
    void CreateLink(Enemy enemy)
    {
        // Create new resonance link
        ResonanceLink newLink = new ResonanceLink
        {
            enemy = enemy,
            startTime = Time.time,
            linkBeam = CreateLinkBeam(enemy)
        };
        
        activeLinks[enemy] = newLink;
        
        // Phase 8: Enhanced link creation effects
        Vector3 linkPosition = enemy.transform.position;
        Color linkColor = GetResonanceColor(enemy.GetColor());
        
        // Play link effect (suppress during bulk refresh to prevent audio spam)
        if (EffectsManager.Instance != null && !suppressLinkAudio)
        {
            EffectsManager.Instance.PlayResonanceLink(transform.position, linkPosition, linkColor);
        }
        

        
        OnEnemyLinked?.Invoke(enemy);
    }
    
    void RemoveLink(Enemy enemy)
    {
        if (activeLinks.ContainsKey(enemy))
        {
            ResonanceLink link = activeLinks[enemy];
            
            // Stop link audio immediately when link is removed
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopSFX("resonance_link");
            }
            
            // Destroy visual beam
            if (link.linkBeam != null)
            {
                Destroy(link.linkBeam);
            }
            
            activeLinks.Remove(enemy);
        }
    }
    
    void UpdateLinkTimers()
    {
        List<Enemy> completedLinks = new List<Enemy>();
        
        foreach (var kvp in activeLinks)
        {
            Enemy enemy = kvp.Key;
            ResonanceLink link = kvp.Value;
            
            // Skip if enemy has been destroyed externally
            if (enemy == null)
            {
                continue;
            }
            
            float elapsed = Time.time - link.startTime;
            float progress = elapsed / linkDuration;
            
            // Update beam visual (pulsing, intensity, etc.)
            UpdateLinkBeam(link, progress);
            
            // Check if link duration completed
            if (elapsed >= linkDuration)
            {
                completedLinks.Add(enemy);
            }
        }
        
        // Destroy enemies with completed links
        foreach (Enemy enemy in completedLinks)
        {
            DestroyLinkedEnemy(enemy);
        }
    }
    
    void DestroyLinkedEnemy(Enemy enemy)
    {
        if (activeLinks.ContainsKey(enemy))
        {
            Vector3 enemyPosition = enemy.transform.position;
            Color resonanceColor = GetResonanceColor(enemy.GetColor());
            int chainLength = activeLinks.Count;
            
            RemoveLink(enemy);
            
            // Phase 8: Enhanced resonance completion effects
            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.PlayResonanceComplete(enemyPosition, resonanceColor, chainLength);
            }
            

            
            // Destroy the enemy
            enemy.DestroyEnemy();
            
            // Award points for resonance kill
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddEnemyKill();
            }
            
            OnEnemyDestroyed?.Invoke(enemy);
            OnComboChanged?.Invoke(activeLinks.Count);
        }
    }
    
    GameObject CreateLinkBeam(Enemy enemy)
    {
        if (linkBeamPrefab == null) return null;
        
        // Create beam object
        GameObject beam = Instantiate(linkBeamPrefab);
        LinkBeam linkBeamComponent = beam.GetComponent<LinkBeam>();
        
        if (linkBeamComponent != null)
        {
            // Get appropriate material for color
            Material beamMaterial = GetBeamMaterial(enemy.GetColor());
            linkBeamComponent.Initialize(playerTransform, enemy.transform, beamMaterial);
        }
        
        return beam;
    }
    
    void UpdateLinkBeam(ResonanceLink link, float progress)
    {
        if (link.linkBeam != null)
        {
            LinkBeam linkBeamComponent = link.linkBeam.GetComponent<LinkBeam>();
            if (linkBeamComponent != null)
            {
                linkBeamComponent.UpdateProgress(progress);
            }
        }
    }
    
    Material GetBeamMaterial(ResonanceColor color)
    {
        switch (color)
        {
            case ResonanceColor.Red: return redBeamMaterial;
            case ResonanceColor.Green: return greenBeamMaterial;
            case ResonanceColor.Blue: return blueBeamMaterial;
            default: return redBeamMaterial;
        }
    }
    
    void ClearAllLinks()
    {
        List<Enemy> allLinkedEnemies = new List<Enemy>(activeLinks.Keys);
        foreach (Enemy enemy in allLinkedEnemies)
        {
            RemoveLink(enemy);
        }
    }
    
    void CleanupDestroyedEnemies()
    {
        // Remove any null enemy references from activeLinks
        List<Enemy> destroyedEnemies = new List<Enemy>();
        foreach (var kvp in activeLinks)
        {
            if (kvp.Key == null)
            {
                destroyedEnemies.Add(kvp.Key);
            }
        }
        
        foreach (Enemy destroyedEnemy in destroyedEnemies)
        {
            RemoveLink(destroyedEnemy);
        }
    }
    
    // Public methods for external systems
    public int GetActiveLinksCount()
    {
        return activeLinks.Count;
    }
    
    public bool IsEnemyLinked(Enemy enemy)
    {
        return activeLinks.ContainsKey(enemy);
    }
    
    /// <summary>
    /// Forces an immediate refresh of all resonance links - used when overload mode activates
    /// </summary>
    public void ForceRefreshLinks()
    {
        bool isOverloadActive = (OverloadMode.Instance != null && OverloadMode.Instance.IsOverloadActive());
        Debug.Log($"ResonanceSystem: Force refreshing all links. Overload Active: {isOverloadActive}");
        
        // Clear existing links first
        var linksToRemove = new System.Collections.Generic.List<Enemy>(activeLinks.Keys);
        foreach (Enemy enemy in linksToRemove)
        {
            RemoveLink(enemy);
        }
        
        // Suppress audio during bulk refresh to prevent audio spam
        bool previousSuppressAudio = suppressLinkAudio;
        suppressLinkAudio = true;
        
        // Force immediate link update (ignore timing interval)
        UpdateResonanceLinks();
        lastEnemyScanTime = Time.time;
        
        // Restore audio setting
        suppressLinkAudio = previousSuppressAudio;
        
        Debug.Log($"ResonanceSystem: Link refresh complete. New active links: {activeLinks.Count}");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw link range
        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, linkRange);
        }
        
        // Draw active links
        Gizmos.color = Color.magenta;
        foreach (var kvp in activeLinks)
        {
            if (kvp.Key != null && playerTransform != null)
            {
                Gizmos.DrawLine(playerTransform.position, kvp.Key.transform.position);
            }
        }
    }
    
    // Helper method to convert ResonanceColor to Unity Color
    Color GetResonanceColor(ResonanceColor resonanceColor)
    {
        switch (resonanceColor)
        {
            case ResonanceColor.Red: return new Color(1f, 0.27f, 0f, 1f); // Hot Orange
            case ResonanceColor.Green: return Color.magenta;
            case ResonanceColor.Blue: return Color.cyan;
            default: return Color.white;
        }
    }
}

// Data structure for tracking individual links
[System.Serializable]
public class ResonanceLink
{
    public Enemy enemy;
    public float startTime;
    public GameObject linkBeam;
}