using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cluster Drone enemy - spawns in groups and has chain destruction mechanic
/// When one is destroyed via resonance, nearby cluster drones of the same color are also destroyed
/// </summary>
public class ClusterDrone : Enemy
{
    [Header("Cluster Drone Settings")]
    public float collisionDistance = 1f;
    public float chainDestructionRadius = 4f; // Radius for chain destruction
    public bool isChainDestroying = false; // Prevents infinite loops
    public LayerMask enemyLayerMask = -1; // Layer mask for detecting other enemies
    
    protected override void Start()
    {
        // Set cluster drone specific stats - weaker individually but stronger in groups
        moveSpeed = 1.8f;
        health = 0.8f; // Slightly weaker than basic nodes
        
        // Call base start
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Check for collision with player
        if (playerTransform != null && !isDestroyed)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= collisionDistance)
            {
                OnPlayerCollision();
            }
        }
    }
    
    protected override void OnPlayerCollision()
    {
        // Cluster drones damage the player when they touch
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
        
        // Destroy by collision - no chain destruction
        DestroyEnemyByCollision();
    }
    
    public void DestroyEnemyByCollision()
    {
        Debug.Log("ClusterDrone: Destroyed by collision - no chain destruction");
        // Mark as destroyed by collision to prevent chain destruction
        isChainDestroying = true;
        DestroyEnemy();
    }
    
    public override void DestroyEnemy()
    {
        Debug.Log($"ClusterDrone: DestroyEnemy called, isChainDestroying: {isChainDestroying}");
        
        // Trigger chain destruction BEFORE calling base.DestroyEnemy() 
        // so this object still exists when doing the sphere overlap
        if (!isChainDestroying)
        {
            Debug.Log("ClusterDrone: Destroyed by resonance - triggering chain destruction!");
            TriggerChainDestructionAt(transform.position, chainDestructionRadius, enemyLayerMask);
        }
        else
        {
            Debug.Log("ClusterDrone: Chain destroying or collision - skipping chain destruction");
        }
        
        // Now destroy this drone
        base.DestroyEnemy();
    }
    
    /// <summary>
    /// Static method to trigger chain destruction from a specific position
    /// This ensures the detection works even if the original drone is being destroyed
    /// </summary>
    public static void TriggerChainDestructionAt(Vector3 position, float radius, LayerMask layerMask)
    {
        Debug.Log($"ClusterDrone: Triggering chain destruction at {position} with radius {radius}");
        
        // Use Physics.OverlapSphere to find all colliders within chain destruction radius
        Collider[] collidersInRange = Physics.OverlapSphere(position, radius, layerMask);
        Debug.Log($"ClusterDrone: Found {collidersInRange.Length} colliders in chain destruction sphere");
        
        List<ClusterDrone> dronesInRange = new List<ClusterDrone>();
        
        // Filter for ClusterDrones that aren't already being destroyed
        foreach (Collider col in collidersInRange)
        {
            ClusterDrone drone = col.GetComponent<ClusterDrone>();
            if (drone != null && !drone.isDestroyed && !drone.isChainDestroying)
            {
                float distance = Vector3.Distance(position, drone.transform.position);
                Debug.Log($"ClusterDrone: Found cluster drone at {drone.transform.position}, distance: {distance:F2}, color: {drone.GetColor()}");
                dronesInRange.Add(drone);
            }
        }
        
        Debug.Log($"ClusterDrone: Found {dronesInRange.Count} cluster drones in chain destruction range");
        
        // Destroy all drones in range
        foreach (ClusterDrone drone in dronesInRange)
        {
            Debug.Log($"ClusterDrone: Chain destroying drone at {drone.transform.position}");
            drone.isChainDestroying = true;
            
            // Award points for chain destruction (same as resonance destruction)
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddEnemyKill();
            }
            
            drone.DestroyEnemy();
        }
        
        // Show visual feedback at the trigger position if we destroyed any drones
        if (dronesInRange.Count > 0)
        {
            ShowChainDestructionEffectAt(position);
        }
    }
    
    /// <summary>
    /// Show chain destruction effect at a specific position
    /// </summary>
    private static void ShowChainDestructionEffectAt(Vector3 position)
    {
        // Use EffectsManager instead of creating primitive spheres that can leave pink artifacts
        if (EffectsManager.Instance != null)
        {
            Color explosionColor = Color.yellow; // Use yellow for chain explosion since it affects all colors
            EffectsManager.Instance.PlayEnemyDestruction(position, explosionColor);
        }
        
        // Play cluster explosion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAtPosition("cluster_explode", position, 0.9f);
        }
    }
    

    
    private Color GetEnemyDisplayColor()
    {
        switch (enemyColor)
        {
            case ResonanceColor.Red: return new Color(1f, 0.27f, 0f, 1f); // Hot Orange
            case ResonanceColor.Green: return Color.magenta;
            case ResonanceColor.Blue: return Color.cyan;
            default: return Color.white;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw collision range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);
        
        // Draw chain destruction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chainDestructionRadius);
    }
    

}