using UnityEngine;

/// <summary>
/// Centralized configuration for RESONATE! game balance and settings
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "RESONATE!/Game Configuration")]
public class GameConfiguration : ScriptableObject
{
    [Header("Player Settings")]
    [Tooltip("Force applied for player movement")]
    public float playerForce = 50f;
    
    [Tooltip("Maximum speed the player can reach")]
    public float playerMaxSpeed = 5f;
    
    [Tooltip("Arena boundary radius")]
    public float arenaRadius = 50f;
    
    [Tooltip("Force pushing player back when near boundary")]
    public float boundaryForce = 100f;
    
    [Header("Resonance System")]
    [Tooltip("Range at which player can link to enemies")]
    public float linkRange = 5f;
    
    [Tooltip("Time required to destroy an enemy via resonance link")]
    public float linkDuration = 2f;
    
    [Tooltip("How often to scan for new enemies (in seconds)")]
    public float enemyScanInterval = 0.1f;
    
    [Header("Enemy Settings")]
    [Tooltip("Base movement speed for basic enemies")]
    public float basicEnemySpeed = 2f;
    
    [Tooltip("Health points for basic enemies")]
    public float basicEnemyHealth = 1f;
    
    [Header("Visual Feedback")]
    [Tooltip("Speed of color pulse effect on resonance core")]
    public float corePulseSpeed = 2f;
    
    [Tooltip("Intensity of the pulse effect (0-1)")]
    [Range(0f, 1f)]
    public float corePulseIntensity = 0.3f;
    
    [Header("Spawning")]
    [Tooltip("Base time between enemy spawns")]
    public float baseSpawnInterval = 3f;
    
    [Tooltip("Maximum number of enemies allowed at once")]
    public int maxEnemies = 10;
    
    [Tooltip("Distance from arena center where enemies spawn")]
    public float spawnRadius = 60f;
    
    // Singleton instance for easy access
    private static GameConfiguration _instance;
    public static GameConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfiguration>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogWarning("GameConfiguration not found in Resources folder! Using default values.");
                    _instance = CreateInstance<GameConfiguration>();
                }
            }
            return _instance;
        }
    }
}