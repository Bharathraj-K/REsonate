using UnityEngine;

/// <summary>
/// ScoreManager - handles scoring, combos, and high score tracking for RESONATE!
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Scoring Settings")]
    [Tooltip("Base points awarded for destroying an enemy via resonance")]
    public int baseEnemyPoints = 100;
    
    [Tooltip("Bonus points for each combo level")]
    public int comboMultiplier = 50;
    
    [Tooltip("Maximum combo multiplier")]
    public int maxComboLevel = 10;
    
    [Header("Combo Settings")]
    [Tooltip("Time before combo resets (in seconds)")]
    public float comboTimeout = 3f;
    
    [Tooltip("Points needed to increase combo level")]
    public int pointsPerComboLevel = 200;
    
    // Current game state
    private int currentScore = 0;
    private int currentCombo = 0;
    private int comboLevel = 1;
    private float lastKillTime = 0f;
    private int highScore = 0;
    private bool highScoreEffectPlayed = false;
    private int enemiesDestroyed = 0;
    
    // Events for UI updates
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnComboChanged;
    public System.Action<int> OnComboLevelChanged;
    public System.Action<int> OnHighScoreChanged;
    
    // Singleton instance for easy access
    public static ScoreManager Instance { get; private set; }
    
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
        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Initialize UI
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnComboLevelChanged?.Invoke(comboLevel);
        OnHighScoreChanged?.Invoke(highScore);
    }
    
    void Update()
    {
        // Check for combo timeout
        if (currentCombo > 0 && Time.time - lastKillTime > comboTimeout)
        {
            ResetCombo();
        }
    }
    
    public void AddEnemyKill()
    {
        // Record kill time for combo timeout
        lastKillTime = Time.time;
        
        // Increase combo
        currentCombo++;
        enemiesDestroyed++;
        
        // Calculate points with combo bonus
        int basePoints = baseEnemyPoints;
        int comboBonus = (comboLevel - 1) * comboMultiplier;
        int totalPoints = basePoints + comboBonus;
        
        // Add points to score
        AddScore(totalPoints);
        
        // Check for combo level increase
        CheckComboLevelIncrease();
        
        // Notify OverloadMode of enemy kill for energy gain
        if (OverloadMode.Instance != null)
        {
            OverloadMode.Instance.AddEnergyForKill();
        }
        
        // Notify UI
        OnComboChanged?.Invoke(currentCombo);
        
        Debug.Log($"Enemy destroyed! +{totalPoints} points (Base: {basePoints}, Combo Bonus: {comboBonus})");
    }
    
    void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Check for new high score
        if (currentScore > highScore)
        {
            bool wasFirstHighScore = highScore == 0;
            bool isNewHighScore = !highScoreEffectPlayed;
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            OnHighScoreChanged?.Invoke(highScore);
            
            // Phase 8: High score celebration effects (only once per session)
            if (!wasFirstHighScore && EffectsManager.Instance != null && isNewHighScore)
            {
                // Only celebrate if it's not the first score ever and haven't celebrated yet
                Vector3 celebrationPos = Camera.main != null ? 
                    Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)) : Vector3.zero;
                EffectsManager.Instance.PlayHighScoreEffect(celebrationPos);
                highScoreEffectPlayed = true;
            }
            
            Debug.Log($"New High Score: {highScore}!");
        }
    }
    
    void CheckComboLevelIncrease()
    {
        int newComboLevel = Mathf.Min(1 + (currentScore / pointsPerComboLevel), maxComboLevel);
        
        if (newComboLevel > comboLevel)
        {
            int previousLevel = comboLevel;
            comboLevel = newComboLevel;
            OnComboLevelChanged?.Invoke(comboLevel);
            
            // Phase 8: Enhanced combo level increase effects
            // Combo text could be added here if needed
            
            // Screen shake increases with combo level
            if (CameraShake.Instance != null)
            {
                if (comboLevel >= 5)
                    CameraShake.Instance.ShakeHeavy();
                else if (comboLevel >= 3)
                    CameraShake.Instance.ShakeMedium();
                else
                    CameraShake.Instance.ShakeLight();
            }
            
            Debug.Log($"Combo Level increased to {comboLevel}!");
        }
    }
    
    void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"Combo ended at {currentCombo} kills");
            currentCombo = 0;
            OnComboChanged?.Invoke(currentCombo);
        }
    }
    
    public void ResetGame()
    {
        currentScore = 0;
        currentCombo = 0;
        comboLevel = 1;
        enemiesDestroyed = 0;
        lastKillTime = 0f;
        
        // Update UI
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnComboLevelChanged?.Invoke(comboLevel);
    }
    
    // Public getters for other systems
    public int GetCurrentScore() => currentScore;
    public int GetCurrentCombo() => currentCombo;
    public int GetComboLevel() => comboLevel;
    public int GetHighScore() => highScore;
    public int GetEnemiesDestroyed() => enemiesDestroyed;
    
    // Phase 8: Method to get points for visual effects
    public int GetEnemyPoints()
    {
        int basePoints = baseEnemyPoints;
        int comboBonus = (comboLevel - 1) * comboMultiplier;
        return basePoints + comboBonus;
    }
    
    // Get high scores list for menu system
    public System.Collections.Generic.List<int> GetHighScores()
    {
        // For now, just return the single high score in a list
        // This can be expanded to support multiple high scores later
        var scores = new System.Collections.Generic.List<int>();
        if (highScore > 0)
        {
            scores.Add(highScore);
        }
        return scores;
    }
    
    // Method for bonus scoring (power-ups, special achievements, etc.)
    public void AddBonusPoints(int points, string reason = "Bonus")
    {
        AddScore(points);
        Debug.Log($"{reason}: +{points} points");
    }
}