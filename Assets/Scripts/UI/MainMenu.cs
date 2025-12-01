using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenu - Simple main menu for RESONATE! with start and exit functionality
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Main Menu UI")]
    [Tooltip("Main menu panel")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Start game button")]
    public Button playButton;
    
    [Tooltip("Quit game button")]
    public Button quitButton;
    
    [Header("Game Settings")]
    [Tooltip("Gameplay scene name")]
    public string gameplaySceneName = "SampleScene";

    void Start()
    {
        InitializeMainMenu();
        SetupButtonListeners();
        
        // Show main menu panel
        if (mainMenuPanel)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("menu", 1f);
        }
    }

    private void InitializeMainMenu()
    {
        Debug.Log("MainMenu initialized successfully");
    }

    private void SetupButtonListeners()
    {
        // Main menu buttons
        if (playButton) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    // Button click handlers
    private void OnPlayButtonClicked()
    {
        PlayButtonSound();
        StartGame();
    }

    private void OnQuitButtonClicked()
    {
        PlayButtonSound();
        QuitGame();
    }

    private void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("button_click");
        }
    }

    // Game flow methods
    public void StartGame()
    {
        Debug.Log("Starting game...");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("game_start");
            AudioManager.Instance.PlayMusic("gameplay", 1.5f);
        }
        
        // Load gameplay scene
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning("MainMenu: Gameplay scene name not set!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}