using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Main game manager that handles core game systems and state management.
/// Implements singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private GameState currentGameState = GameState.MainMenu;
    
    [Header("Player References")]
    [SerializeField] private SimplePlayerController playerController;
    [SerializeField] private Camera playerCamera;
    
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject gameOverUI;
    
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    
    // Properties
    public float GameTime => gameTime;
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public GameState CurrentGameState => currentGameState;
    public bool IsGamePaused => currentGameState == GameState.Paused;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadGameSettings();
        SetGameState(GameState.MainMenu);
    }
    
    private void Update()
    {
        if (currentGameState == GameState.Playing)
        {
            gameTime += Time.deltaTime;
        }
        
        HandleInput();
    }
    
    /// <summary>
    /// Initialize core game systems
    /// </summary>
    private void InitializeGame()
    {
        // Load saved data
        LoadHighScore();
        
        // Setup audio
        if (musicSource != null)
        {
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
    }
    
    /// <summary>
    /// Handle global input
    /// </summary>
    private void HandleInput()
    {
        // Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        
        // Restart game
        if (Input.GetKeyDown(KeyCode.R) && currentGameState == GameState.GameOver)
        {
            RestartGame();
        }
    }
    
    /// <summary>
    /// Set the current game state
    /// </summary>
    public void SetGameState(GameState newState)
    {
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        OnGameStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameState.Playing:
                StartGame();
                break;
            case GameState.Paused:
                PauseGame();
                break;
            case GameState.GameOver:
                EndGame();
                break;
        }
    }
    
    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartGame()
    {
        currentScore = 0;
        gameTime = 0f;
        
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        ShowGameUI();
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        ShowPauseUI();
        OnGamePaused?.Invoke();
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ShowGameUI();
        OnGameResumed?.Invoke();
    }
    
    /// <summary>
    /// End the game
    /// </summary>
    public void EndGame()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        ShowGameOverUI();
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Add score to the current game
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
    }
    
    /// <summary>
    /// Load game settings from PlayerPrefs
    /// </summary>
    private void LoadGameSettings()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    /// <summary>
    /// Show main menu UI
    /// </summary>
    private void ShowMainMenu()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
    
    /// <summary>
    /// Show game UI
    /// </summary>
    private void ShowGameUI()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
    
    /// <summary>
    /// Show pause UI
    /// </summary>
    private void ShowPauseUI()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
    
    /// <summary>
    /// Show game over UI
    /// </summary>
    private void ShowGameOverUI()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

/// <summary>
/// Enum for different game states
/// </summary>
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
} 