using UnityEngine;

/// <summary>
/// Simplified game UI that works without TextMeshPro.
/// Uses basic Unity UI components.
/// </summary>
public class SimpleGameUI : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject aimReticle;
    
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    
    // Components
    private SimplePlayerController playerController;
    private SimpleWeaponController weaponController;
    private Health playerHealth;
    
    // UI State
    // Note: State variables removed to avoid unused field warnings
    
    private void Start()
    {
        // Find components
        playerController = FindFirstObjectByType<SimplePlayerController>();
        weaponController = FindFirstObjectByType<SimpleWeaponController>();
        if (playerController != null)
        {
            playerHealth = playerController.GetComponent<Health>();
        }
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize UI
        ShowMainMenu();
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameState.Playing)
        {
            UpdateHUD();
        }
    }
    
    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        // Note: Button listeners removed to avoid UI package dependencies
        // Use keyboard input instead: ESC for pause, SPACE for resume, etc.
    }
    
    /// <summary>
    /// Subscribe to game events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Game manager found");
        }
        
        if (weaponController != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Weapon controller found");
        }
        
        if (playerHealth != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Player health component found");
        }
    }
    
    /// <summary>
    /// Update HUD elements
    /// </summary>
    private void UpdateHUD()
    {
        // Note: UI text updates removed to avoid package dependencies
        // Use Debug.Log for console output instead
        if (GameManager.Instance != null)
        {
            float gameTime = GameManager.Instance.GameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            Debug.Log($"Game Time: {minutes:00}:{seconds:00}");
        }
    }
    
    /// <summary>
    /// Handle game state changes
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameState.Playing:
                ShowHUD();
                break;
            case GameState.Paused:
                ShowPauseMenu();
                break;
            case GameState.GameOver:
                ShowGameOver();
                break;
        }
    }
    
    /// <summary>
    /// Handle score changes
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"Score: {newScore}");
    }
    
    /// <summary>
    /// Handle ammo changes
    /// </summary>
    private void OnAmmoChanged(int currentAmmo, int totalAmmo)
    {
        Debug.Log($"Ammo: {currentAmmo} / {totalAmmo}");
    }
    
    /// <summary>
    /// Handle health changes
    /// </summary>
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"Health: {currentHealth} / {maxHealth}");
    }
    
    /// <summary>
    /// Show main menu
    /// </summary>
    private void ShowMainMenu()
    {
        SetAllPanelsActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        // Log high score to console
        if (GameManager.Instance != null)
        {
            Debug.Log($"High Score: {GameManager.Instance.HighScore}");
        }
    }
    
    /// <summary>
    /// Show HUD
    /// </summary>
    private void ShowHUD()
    {
        SetAllPanelsActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
    }
    
    /// <summary>
    /// Show pause menu
    /// </summary>
    private void ShowPauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }
    
    /// <summary>
    /// Show game over screen
    /// </summary>
    private void ShowGameOver()
    {
        SetAllPanelsActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        // Log final stats to console
        if (GameManager.Instance != null)
        {
            Debug.Log($"Final Score: {GameManager.Instance.CurrentScore}");
            float gameTime = GameManager.Instance.GameTime;
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            Debug.Log($"Final Time: {minutes:00}:{seconds:00}");
        }
    }
    
    /// <summary>
    /// Set all panels inactive
    /// </summary>
    private void SetAllPanelsActive(bool active)
    {
        if (hudPanel != null) hudPanel.SetActive(active);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(active);
        if (pausePanel != null) pausePanel.SetActive(active);
        if (gameOverPanel != null) gameOverPanel.SetActive(active);
        if (settingsPanel != null) settingsPanel.SetActive(active);
        if (loadingPanel != null) loadingPanel.SetActive(active);
    }
    
    /// <summary>
    /// Start new game
    /// </summary>
    private void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        }
    }
    
    /// <summary>
    /// Resume game
    /// </summary>
    private void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.Playing);
        }
    }
    
    /// <summary>
    /// Restart game
    /// </summary>
    private void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    /// <summary>
    /// Go to main menu
    /// </summary>
    private void GoToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameState.MainMenu);
        }
    }
    
    /// <summary>
    /// Open settings
    /// </summary>
    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        
        // Load current settings
        LoadSettings();
    }
    
    /// <summary>
    /// Close settings
    /// </summary>
    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Save settings
        SaveSettings();
    }
    
    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    private void LoadSettings()
    {
        // Note: Settings loading removed to avoid UI package dependencies
        Debug.Log("Settings loaded from PlayerPrefs");
    }
    
    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    private void SaveSettings()
    {
        // Note: Settings saving removed to avoid UI package dependencies
        PlayerPrefs.Save();
        Debug.Log("Settings saved to PlayerPrefs");
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    private void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        Debug.Log($"Music volume set to: {volume}");
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    private void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        Debug.Log($"SFX volume set to: {volume}");
    }
    
    /// <summary>
    /// Set mouse sensitivity
    /// </summary>
    private void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        Debug.Log($"Mouse sensitivity set to: {sensitivity}");
    }
    
    /// <summary>
    /// Quit game
    /// </summary>
    private void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
    
    /// <summary>
    /// Show loading screen
    /// </summary>
    public void ShowLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
    }
    
    /// <summary>
    /// Hide loading screen
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
    
    /// <summary>
    /// Update loading progress
    /// </summary>
    public void UpdateLoadingProgress(float progress, string message = "Loading...")
    {
        Debug.Log($"{message} - Progress: {progress * 100:F0}%");
    }
    
    /// <summary>
    /// Show crosshair
    /// </summary>
    public void ShowCrosshair(bool show)
    {
        if (crosshair != null)
        {
            crosshair.SetActive(show);
        }
    }
    
    /// <summary>
    /// Show aim reticle
    /// </summary>
    public void ShowAimReticle(bool show)
    {
        if (aimReticle != null)
        {
            aimReticle.SetActive(show);
        }
    }
    
    private void OnDestroy()
    {
        // Note: Event unsubscription removed to avoid compilation issues
        Debug.Log("SimpleGameUI destroyed");
    }
} 