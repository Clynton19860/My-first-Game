using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages menu systems including main menu, pause menu, and game UI navigation.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Menu Settings")]
    [SerializeField] private bool showCursorInMenus = true;
    [SerializeField] private bool pauseGameOnPauseMenu = true;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    
    [Header("Audio")]
    [SerializeField] private AudioClip menuClickSound;
    [SerializeField] private AudioClip menuHoverSound;
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private AudioClip gameOverSound;
    
    // Menu state
    private bool isPaused = false;
    private bool isInMenu = true;
    private GameObject currentPanel;
    
    // Components
    private AudioManager audioManager;
    private SimplePlayerController playerController;
    private LevelProgressionManager levelManager;
    
    // Singleton instance
    public static MenuManager Instance { get; private set; }
    
    // Events
    public System.Action OnGameStarted;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    public System.Action OnGameOver;
    public System.Action OnGameVictory;
    
    // Properties
    public bool IsPaused => isPaused;
    public bool IsInMenu => isInMenu;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        FindComponents();
        InitializeMenus();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// Find required components
    /// </summary>
    private void FindComponents()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        playerController = FindFirstObjectByType<SimplePlayerController>();
        levelManager = FindFirstObjectByType<LevelProgressionManager>();
    }
    
    /// <summary>
    /// Initialize menu system
    /// </summary>
    private void InitializeMenus()
    {
        // Hide all panels initially
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // Show main menu
        ShowMainMenu();
        
        // Setup cursor
        SetupCursor();
    }
    
    /// <summary>
    /// Handle input
    /// </summary>
    private void HandleInput()
    {
        // Pause menu toggle
        if (Input.GetKeyDown(pauseKey) && !isInMenu)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    /// <summary>
    /// Setup cursor visibility
    /// </summary>
    private void SetupCursor()
    {
        if (showCursorInMenus)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    /// <summary>
    /// Show main menu
    /// </summary>
    public void ShowMainMenu()
    {
        isInMenu = true;
        isPaused = false;
        
        ShowPanel(mainMenuPanel);
        SetupCursor();
        
        // Pause game systems
        if (pauseGameOnPauseMenu)
        {
            Time.timeScale = 0f;
        }
        
        Debug.Log("Main menu shown");
    }
    
    /// <summary>
    /// Start new game
    /// </summary>
    public void StartNewGame()
    {
        PlaySound(gameStartSound);
        
        isInMenu = false;
        isPaused = false;
        
        ShowPanel(gameUIPanel);
        SetupCursor();
        
        // Resume game
        Time.timeScale = 1f;
        
        // Reset game state
        ResetGameState();
        
        OnGameStarted?.Invoke();
        
        Debug.Log("New game started");
    }
    
    /// <summary>
    /// Pause game
    /// </summary>
    public void PauseGame()
    {
        if (isInMenu) return;
        
        isPaused = true;
        
        ShowPanel(pauseMenuPanel);
        SetupCursor();
        
        // Pause game
        if (pauseGameOnPauseMenu)
        {
            Time.timeScale = 0f;
        }
        
        OnGamePaused?.Invoke();
        
        Debug.Log("Game paused");
    }
    
    /// <summary>
    /// Resume game
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        
        ShowPanel(gameUIPanel);
        SetupCursor();
        
        // Resume game
        Time.timeScale = 1f;
        
        OnGameResumed?.Invoke();
        
        Debug.Log("Game resumed");
    }
    
    /// <summary>
    /// Show settings menu
    /// </summary>
    public void ShowSettings()
    {
        ShowPanel(settingsPanel);
        Debug.Log("Settings menu shown");
    }
    
    /// <summary>
    /// Show level select
    /// </summary>
    public void ShowLevelSelect()
    {
        ShowPanel(levelSelectPanel);
        Debug.Log("Level select shown");
    }
    
    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver()
    {
        PlaySound(gameOverSound);
        
        ShowPanel(gameOverPanel);
        SetupCursor();
        
        // Pause game
        Time.timeScale = 0f;
        
        OnGameOver?.Invoke();
        
        Debug.Log("Game over screen shown");
    }
    
    /// <summary>
    /// Show victory screen
    /// </summary>
    public void ShowVictory()
    {
        PlaySound(gameStartSound); // Use start sound for victory
        
        ShowPanel(victoryPanel);
        SetupCursor();
        
        // Pause game
        Time.timeScale = 0f;
        
        OnGameVictory?.Invoke();
        
        Debug.Log("Victory screen shown");
    }
    
    /// <summary>
    /// Restart current level
    /// </summary>
    public void RestartLevel()
    {
        if (levelManager != null)
        {
            levelManager.RestartLevel();
        }
        
        ResumeGame();
        
        Debug.Log("Level restarted");
    }
    
    /// <summary>
    /// Load specific level
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        if (levelManager != null)
        {
            // Note: This would need to be implemented in LevelProgressionManager
            Debug.Log($"Loading level {levelNumber}");
        }
        
        StartNewGame();
    }
    
    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Show panel and hide others
    /// </summary>
    private void ShowPanel(GameObject panel)
    {
        // Hide current panel
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        
        // Show new panel
        if (panel != null)
        {
            panel.SetActive(true);
            currentPanel = panel;
        }
    }
    
    /// <summary>
    /// Play menu sound
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioManager != null && clip != null)
        {
            // Use PlaySFX for menu sounds
            audioManager.PlaySFX("ButtonClick", 1f);
        }
    }
    
    /// <summary>
    /// Reset game state
    /// </summary>
    private void ResetGameState()
    {
        // Reset level progression
        if (levelManager != null)
        {
            // Note: Add reset method to LevelProgressionManager
            Debug.Log("Game state reset");
        }
        
        // Reset score
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        // Reset player health
        if (playerController != null)
        {
            Health playerHealth = playerController.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.SetMaxHealth(100);
            }
        }
        
        // Clear enemies
        SimpleEnemySpawner enemySpawner = FindFirstObjectByType<SimpleEnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
        }
        
        // Clear power-ups
        PowerUpManager powerUpManager = FindFirstObjectByType<PowerUpManager>();
        if (powerUpManager != null)
        {
            powerUpManager.ClearAllPowerUps();
        }
        
        // Regenerate level
        LevelGenerator levelGenerator = FindFirstObjectByType<LevelGenerator>();
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel();
        }
    }
    
    /// <summary>
    /// Get current menu info
    /// </summary>
    public string GetMenuInfo()
    {
        string info = "Menu System Status:\n";
        info += $"In Menu: {isInMenu}\n";
        info += $"Paused: {isPaused}\n";
        info += $"Current Panel: {(currentPanel != null ? currentPanel.name : "None")}\n";
        info += $"Time Scale: {Time.timeScale}\n";
        info += $"Cursor Visible: {Cursor.visible}\n";
        info += $"Cursor Lock State: {Cursor.lockState}";
        
        return info;
    }
    
    /// <summary>
    /// Set menu audio settings
    /// </summary>
    public void SetMenuAudioSettings(float masterVolume, float musicVolume, float sfxVolume)
    {
        if (audioManager != null)
        {
            // Note: Add volume settings to AudioManager
            Debug.Log($"Audio settings: Master={masterVolume}, Music={musicVolume}, SFX={sfxVolume}");
        }
    }
    
    /// <summary>
    /// Set graphics quality
    /// </summary>
    public void SetGraphicsQuality(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(qualityLevel);
        Debug.Log($"Graphics quality set to level {qualityLevel}");
    }
    
    /// <summary>
    /// Set fullscreen mode
    /// </summary>
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        Debug.Log($"Fullscreen mode {(fullscreen ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Set resolution
    /// </summary>
    public void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        Debug.Log($"Resolution set to {width}x{height}");
    }
    
    /// <summary>
    /// Get available resolutions
    /// </summary>
    public Resolution[] GetAvailableResolutions()
    {
        return Screen.resolutions;
    }
    
    /// <summary>
    /// Get current resolution
    /// </summary>
    public Resolution GetCurrentResolution()
    {
        return Screen.currentResolution;
    }
    
    /// <summary>
    /// Get graphics quality levels
    /// </summary>
    public string[] GetQualityLevels()
    {
        return QualitySettings.names;
    }
    
    /// <summary>
    /// Get current quality level
    /// </summary>
    public int GetCurrentQualityLevel()
    {
        return QualitySettings.GetQualityLevel();
    }
    
    /// <summary>
    /// Save settings
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
        PlayerPrefs.Save();
        
        Debug.Log("Settings saved");
    }
    
    /// <summary>
    /// Load settings
    /// </summary>
    public void LoadSettings()
    {
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        int width = PlayerPrefs.GetInt("ResolutionWidth", 1920);
        int height = PlayerPrefs.GetInt("ResolutionHeight", 1080);
        
        SetGraphicsQuality(qualityLevel);
        SetFullscreen(fullscreen);
        SetResolution(width, height);
        
        Debug.Log("Settings loaded");
    }
} 