using UnityEngine;
using TMPro;

/// <summary>
/// GameUI - handles UI display for score, combo, lives, and other game information
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("UI Text References")]
    [Tooltip("TextMeshPro component to display current score")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("TextMeshPro component to display current combo")]
    public TextMeshProUGUI comboText;
    
    [Tooltip("TextMeshPro component to display combo level")]
    public TextMeshProUGUI comboLevelText;
    
    [Tooltip("TextMeshPro component to display current lives")]
    public TextMeshProUGUI livesText;
    
    [Tooltip("TextMeshPro component to display high score")]
    public TextMeshProUGUI highScoreText;
    
    [Tooltip("TextMeshPro component to display energy/overload status")]
    public TextMeshProUGUI energyText;
    
    [Tooltip("TextMeshPro component to display overload status")]
    public TextMeshProUGUI overloadStatusText;
    
    [Header("UI Display Settings")]
    [Tooltip("Prefix text for score display")]
    public string scorePrefix = "Score: ";
    
    [Tooltip("Prefix text for combo display")]
    public string comboPrefix = "Combo: ";
    
    [Tooltip("Prefix text for combo level display")]
    public string comboLevelPrefix = "Level: ";
    
    [Tooltip("Prefix text for lives display")]
    public string livesPrefix = "Lives: ";
    
    [Tooltip("Prefix text for high score display")]
    public string highScorePrefix = "High Score: ";
    
    [Tooltip("Prefix text for energy display")]
    public string energyPrefix = "Energy: ";
    
    [Header("Overload UI Settings")]
    [Tooltip("Color for energy text when overload is ready")]
    public Color energyReadyColor = Color.yellow;
    
    [Tooltip("Color for energy text when charging")]
    public Color energyChargingColor = Color.white;
    
    [Tooltip("Color for overload status text when active")]
    public Color overloadActiveColor = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
    
    [Tooltip("Color for overload status text when on cooldown")]
    public Color overloadCooldownColor = Color.gray;
    
    // Component references
    private ScoreManager scoreManager;
    private PlayerHealth playerHealth;
    private OverloadMode overloadMode;
    
    void Start()
    {
        // Find required components
        scoreManager = FindFirstObjectByType<ScoreManager>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        overloadMode = FindFirstObjectByType<OverloadMode>();
        
        // Subscribe to events
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += UpdateScore;
            scoreManager.OnComboChanged += UpdateCombo;
            scoreManager.OnComboLevelChanged += UpdateComboLevel;
            scoreManager.OnHighScoreChanged += UpdateHighScore;
        }
        
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged += UpdateLives;
        }
        
        if (overloadMode != null)
        {
            overloadMode.OnEnergyChanged += UpdateEnergy;
            overloadMode.OnOverloadActivated += OnOverloadActivated;
            overloadMode.OnOverloadDeactivated += OnOverloadDeactivated;
            overloadMode.OnCooldownChanged += UpdateOverloadCooldown;
        }
        
        // Initialize display with current values
        InitializeUI();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= UpdateScore;
            scoreManager.OnComboChanged -= UpdateCombo;
            scoreManager.OnComboLevelChanged -= UpdateComboLevel;
            scoreManager.OnHighScoreChanged -= UpdateHighScore;
        }
        
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateLives;
        }
        
        if (overloadMode != null)
        {
            overloadMode.OnEnergyChanged -= UpdateEnergy;
            overloadMode.OnOverloadActivated -= OnOverloadActivated;
            overloadMode.OnOverloadDeactivated -= OnOverloadDeactivated;
            overloadMode.OnCooldownChanged -= UpdateOverloadCooldown;
        }
    }
    
    void InitializeUI()
    {
        // Set initial values for all UI elements
        if (scoreManager != null)
        {
            UpdateScore(scoreManager.GetCurrentScore());
            UpdateCombo(scoreManager.GetCurrentCombo());
            UpdateComboLevel(scoreManager.GetComboLevel());
            UpdateHighScore(scoreManager.GetHighScore());
        }
        
        if (playerHealth != null)
        {
            UpdateLives(playerHealth.GetCurrentLives());
        }
        
        // Initialize overload UI
        InitializeOverloadUI();
        
        // Initialize overload UI
        InitializeOverloadUI();
    }
    
    void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + newScore.ToString("N0");
        }
    }
    
    void UpdateCombo(int newCombo)
    {
        if (comboText != null)
        {
            Debug.Log($"UpdateCombo called with: {newCombo}");
            
            if (newCombo > 0)
            {
                comboText.text = comboPrefix + newCombo.ToString() + "x";
                
                // Optional: Change color based on combo level
                if (newCombo >= 10)
                    comboText.color = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
                else if (newCombo >= 5)
                    comboText.color = Color.yellow;
                else
                    comboText.color = Color.white;
            }
            else
            {
                // Show "Combo: 0x" instead of empty string for clarity
                comboText.text = comboPrefix + "0x";
                comboText.color = Color.white;
            }
            
            Debug.Log($"Combo text set to: '{comboText.text}'");
        }
        else
        {
            Debug.LogWarning("ComboText is null! Make sure to assign it in the inspector.");
        }
    }
    
    void UpdateComboLevel(int newLevel)
    {
        if (comboLevelText != null)
        {
            comboLevelText.text = comboLevelPrefix + newLevel.ToString();
            
            // Optional: Change color based on level
            if (newLevel >= 8)
                comboLevelText.color = Color.magenta;
            else if (newLevel >= 5)
                comboLevelText.color = Color.cyan;
            else if (newLevel >= 3)
                comboLevelText.color = Color.magenta;
            else
                comboLevelText.color = Color.white;
        }
    }
    
    void UpdateLives(int newLives)
    {
        if (livesText != null)
        {
            livesText.text = livesPrefix + newLives.ToString();
            
            // Optional: Change color based on remaining lives
            if (newLives <= 1)
                livesText.color = new Color(1f, 0.27f, 0f, 1f); // Hot Orange
            else if (newLives <= 2)
                livesText.color = Color.yellow;
            else
                livesText.color = Color.white;
        }
    }
    
    void UpdateHighScore(int newHighScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = highScorePrefix + newHighScore.ToString("N0");
        }
    }
    
    void UpdateEnergy(float energyPercentage)
    {
        if (energyText != null)
        {
            energyText.text = energyPrefix + energyPercentage.ToString("F0") + "%";
            
            // Change color based on energy level
            if (energyPercentage >= 100f)
            {
                energyText.color = energyReadyColor;
                energyText.text = energyPrefix + "READY!";
            }
            else
            {
                energyText.color = energyChargingColor;
            }
        }
    }
    
    void OnOverloadActivated()
    {
        if (overloadStatusText != null)
        {
            overloadStatusText.color = overloadActiveColor;
            overloadStatusText.text = "OVERLOAD ACTIVE!";
        }
        
        // Make energy text show active state
        if (energyText != null)
        {
            energyText.color = overloadActiveColor;
            energyText.text = "OVERLOAD ACTIVE!";
        }
    }
    
    void OnOverloadDeactivated()
    {
        if (overloadStatusText != null)
        {
            overloadStatusText.color = overloadCooldownColor;
            overloadStatusText.text = "Cooling Down...";
        }
        
        // Reset energy text
        if (energyText != null)
        {
            energyText.color = energyChargingColor;
            energyText.text = energyPrefix + "0%";
        }
    }
    
    void UpdateOverloadCooldown(float cooldownProgress)
    {
        if (overloadStatusText != null && overloadMode != null && overloadMode.IsOnCooldown())
        {
            float timeRemaining = overloadMode.GetCooldownTimeRemaining();
            overloadStatusText.text = $"Cooldown: {timeRemaining:F1}s";
            
            // Fade color as cooldown progresses
            Color lerpedColor = Color.Lerp(overloadCooldownColor, energyChargingColor, cooldownProgress);
            overloadStatusText.color = lerpedColor;
            
            // Clear text when cooldown is complete
            if (cooldownProgress >= 1f)
            {
                overloadStatusText.text = "";
            }
        }
    }
    
    // Initialize overload UI values
    void InitializeOverloadUI()
    {
        if (overloadMode != null)
        {
            UpdateEnergy(overloadMode.GetEnergyPercentage());
            
            if (overloadMode.IsOverloadActive())
            {
                OnOverloadActivated();
            }
            else if (overloadMode.IsOnCooldown())
            {
                UpdateOverloadCooldown(0f);
            }
            else if (overloadStatusText != null)
            {
                overloadStatusText.text = "Press SPACE for Overload";
                overloadStatusText.color = energyChargingColor;
            }
        }
        else
        {
            Debug.LogWarning("GameUI: OverloadMode not found during initialization!");
        }
    }
    
    // Public method to show/hide UI (for game states)
    public void SetUIVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}