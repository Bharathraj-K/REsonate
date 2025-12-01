using UnityEngine;
using System.Collections.Generic;

// Enemy spawning and wave management with enhanced enemy variety
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject basicNodePrefab;
    public GameObject chargerPrefab;
    public GameObject flickerPrefab;
    public GameObject clusterDronePrefab;
    public GameObject resonantHunterPrefab;
    
    [Header("Spawn Settings")]
    public float spawnRadius = 10f;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;
    
    [Header("Difficulty Scaling")]
    public float difficultyIncreaseRate = 0.1f;
    public float minSpawnInterval = 0.5f;
    
    [Header("Enemy Introduction Timing")]
    [Tooltip("Time (in seconds) when each enemy type becomes available")]
    public float chargerIntroTime = 30f;
    public float flickerIntroTime = 60f;
    public float clusterDroneIntroTime = 90f;
    public float resonantHunterIntroTime = 120f;
    
    [Header("Spawn Weights")]
    [Tooltip("Relative probability of spawning each enemy type (when available)")]
    public float basicNodeWeight = 10f;
    public float chargerWeight = 3f;
    public float flickerWeight = 2f;
    public float clusterDroneWeight = 4f;
    public float resonantHunterWeight = 2f;
    
    [Header("Cluster Drone Special Settings")]
    [Tooltip("How many cluster drones to spawn in a group")]
    public int clusterDroneGroupSize = 3;
    public float clusterDroneSpacing = 2f;
    
    private float lastSpawnTime;
    private float gameStartTime;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private float currentDifficulty = 1f;
    
    void Start()
    {
        lastSpawnTime = Time.time;
        gameStartTime = Time.time;
    }
    
    void Update()
    {
        // Clean up destroyed enemies from list
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        // Spawn new enemies if needed
        if (Time.time - lastSpawnTime >= GetCurrentSpawnInterval() && 
            activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
        
        // Increase difficulty over time
        currentDifficulty += difficultyIncreaseRate * Time.deltaTime;
    }
    
    void SpawnEnemy()
    {
        // Determine which enemy type to spawn based on time and weights
        EnemyType enemyTypeToSpawn = SelectEnemyType();
        
        if (enemyTypeToSpawn == EnemyType.ClusterDrone)
        {
            SpawnClusterDroneGroup();
        }
        else
        {
            SpawnSingleEnemy(enemyTypeToSpawn);
        }
    }
    
    private EnemyType SelectEnemyType()
    {
        float currentTime = Time.time - gameStartTime;
        List<EnemyTypeWeight> availableTypes = new List<EnemyTypeWeight>();
        
        // Add basic nodes (always available)
        if (basicNodePrefab != null)
            availableTypes.Add(new EnemyTypeWeight { type = EnemyType.BasicNode, weight = basicNodeWeight });
        
        // Add other enemy types based on introduction time
        if (currentTime >= chargerIntroTime && chargerPrefab != null)
            availableTypes.Add(new EnemyTypeWeight { type = EnemyType.Charger, weight = chargerWeight });
            
        if (currentTime >= flickerIntroTime && flickerPrefab != null)
            availableTypes.Add(new EnemyTypeWeight { type = EnemyType.Flicker, weight = flickerWeight });
            
        if (currentTime >= clusterDroneIntroTime && clusterDronePrefab != null)
            availableTypes.Add(new EnemyTypeWeight { type = EnemyType.ClusterDrone, weight = clusterDroneWeight });
            
        if (currentTime >= resonantHunterIntroTime && resonantHunterPrefab != null)
            availableTypes.Add(new EnemyTypeWeight { type = EnemyType.ResonantHunter, weight = resonantHunterWeight });
        
        // Select enemy type based on weighted random selection
        return SelectWeightedRandom(availableTypes);
    }
    
    private EnemyType SelectWeightedRandom(List<EnemyTypeWeight> weightedTypes)
    {
        if (weightedTypes.Count == 0) return EnemyType.BasicNode;
        
        float totalWeight = 0f;
        foreach (var type in weightedTypes)
        {
            totalWeight += type.weight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var type in weightedTypes)
        {
            currentWeight += type.weight;
            if (randomValue <= currentWeight)
            {
                return type.type;
            }
        }
        
        // Fallback
        return weightedTypes[0].type;
    }
    
    private void SpawnSingleEnemy(EnemyType enemyType)
    {
        GameObject prefabToSpawn = GetPrefabForEnemyType(enemyType);
        if (prefabToSpawn == null) return;
        
        // Get random spawn position at arena edge
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Instantiate enemy
        GameObject enemyObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            RegisterEnemy(enemy);
        }
    }
    
    private void SpawnClusterDroneGroup()
    {
        if (clusterDronePrefab == null) return;
        
        // Get base spawn position
        Vector3 basePosition = GetRandomSpawnPosition();
        
        // Spawn multiple cluster drones in a small formation
        for (int i = 0; i < clusterDroneGroupSize; i++)
        {
            // Calculate offset for group formation
            float angle = (i * 360f / clusterDroneGroupSize) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * clusterDroneSpacing,
                0f,
                Mathf.Sin(angle) * clusterDroneSpacing
            );
            
            Vector3 spawnPosition = basePosition + offset;
            
            // Instantiate cluster drone
            GameObject enemyObj = Instantiate(clusterDronePrefab, spawnPosition, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                RegisterEnemy(enemy);
            }
        }
    }
    
    private GameObject GetPrefabForEnemyType(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.BasicNode: return basicNodePrefab;
            case EnemyType.Charger: return chargerPrefab;
            case EnemyType.Flicker: return flickerPrefab;
            case EnemyType.ClusterDrone: return clusterDronePrefab;
            case EnemyType.ResonantHunter: return resonantHunterPrefab;
            default: return basicNodePrefab;
        }
    }
    
    private void RegisterEnemy(Enemy enemy)
    {
        // Subscribe to enemy destruction
        enemy.OnEnemyDestroyed += OnEnemyDestroyed;
        
        // Add to active enemies list
        activeEnemies.Add(enemy);
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Get random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Calculate offset from spawner's current position
        float x = Mathf.Cos(angle) * spawnRadius;
        float z = Mathf.Sin(angle) * spawnRadius;
        
        // Use spawner's current position as the center (follows player if spawner is child of player)
        Vector3 offset = new Vector3(x, 0f, z);
        Vector3 spawnPosition = transform.position + offset;
        
        // Set Y to proper ground level
        spawnPosition.y = 0.5f; // Adjust this to match your ground level + enemy height
        
        return spawnPosition;
    }
    
    float GetCurrentSpawnInterval()
    {
        // Spawn faster as difficulty increases
        float interval = spawnInterval / currentDifficulty;
        return Mathf.Max(interval, minSpawnInterval);
    }
    
    void OnEnemyDestroyed(Enemy enemy)
    {
        // Remove from active enemies list
        activeEnemies.Remove(enemy);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn radius (approximated with line segments) relative to spawner position
        Gizmos.color = Color.yellow;
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 center = transform.position;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * spawnRadius, 0, Mathf.Sin(angle1) * spawnRadius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * spawnRadius, 0, Mathf.Sin(angle2) * spawnRadius);
            Gizmos.DrawLine(point1, point2);
        }
        
        // Draw active enemy positions
        Gizmos.color = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
            }
        }
    }
    
    // Public methods for game management
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
    
    public List<Enemy> GetActiveEnemies()
    {
        return new List<Enemy>(activeEnemies);
    }
    
    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
    }
    
    // Get info about enemy type availability (for UI/debug purposes)
    public bool IsEnemyTypeAvailable(EnemyType enemyType)
    {
        float currentTime = Time.time - gameStartTime;
        
        switch (enemyType)
        {
            case EnemyType.BasicNode: return true;
            case EnemyType.Charger: return currentTime >= chargerIntroTime;
            case EnemyType.Flicker: return currentTime >= flickerIntroTime;
            case EnemyType.ClusterDrone: return currentTime >= clusterDroneIntroTime;
            case EnemyType.ResonantHunter: return currentTime >= resonantHunterIntroTime;
            default: return false;
        }
    }
    
    public float GetTimeUntilEnemyType(EnemyType enemyType)
    {
        float currentTime = Time.time - gameStartTime;
        
        switch (enemyType)
        {
            case EnemyType.BasicNode: return 0f;
            case EnemyType.Charger: return Mathf.Max(0f, chargerIntroTime - currentTime);
            case EnemyType.Flicker: return Mathf.Max(0f, flickerIntroTime - currentTime);
            case EnemyType.ClusterDrone: return Mathf.Max(0f, clusterDroneIntroTime - currentTime);
            case EnemyType.ResonantHunter: return Mathf.Max(0f, resonantHunterIntroTime - currentTime);
            default: return float.MaxValue;
        }
    }
}

// Supporting classes for enhanced spawning system
public enum EnemyType
{
    BasicNode,
    Charger,
    Flicker,
    ClusterDrone,
    ResonantHunter
}

[System.Serializable]
public class EnemyTypeWeight
{
    public EnemyType type;
    public float weight;
}