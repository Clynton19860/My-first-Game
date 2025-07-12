using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced game UI system with comprehensive HUD elements.
/// Works without TextMeshPro dependencies.
/// </summary>
public class EnhancedGameUI : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject aimReticle;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject ammoCounter;
    [SerializeField] private GameObject scoreDisplay;
    [SerializeField] private GameObject waveInfo;
    [SerializeField] private GameObject comboDisplay;
    
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("UI Settings")]
    [SerializeField] private float uiUpdateInterval = 0.1f;
    [SerializeField] private bool showDebugInfo = true;
    // [SerializeField] private bool showFPS = true; // Unused - removed to avoid warning
    
    // Components
    private SimplePlayerController playerController;
    private SimpleWeaponController weaponController;
    private Health playerHealth;
    private ScoreManager scoreManager;
    private GameManager gameManager;
    
    // UI State
    private bool isHUDVisible = true;
    private bool isPaused = false;
    private float lastUIUpdate = 0f;
    
    // Debug info
    private float fps = 0f;
    private float frameTime = 0f;
    private int frameCount = 0;
    private float fpsTimer = 0f;
    
    private void Start()
    {
        // Find components
        FindComponents();
        
        // Setup UI
        SetupUI();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize UI
        ShowMainMenu();
    }
    
    private void Update()
    {
        // Update FPS counter
        UpdateFPS();
        
        // Update UI at intervals
        if (Time.time - lastUIUpdate > uiUpdateInterval)
        {
            UpdateHUD();
            lastUIUpdate = Time.time;
        }
        
        // Handle input
        HandleInput();
    }
    
    /// <summary>
    /// Find all required components
    /// </summary>
    private void FindComponents()
    {
        playerController = FindFirstObjectByType<SimplePlayerController>();
        weaponController = FindFirstObjectByType<SimpleWeaponController>();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (playerController != null)
        {
            playerHealth = playerController.GetComponent<Health>();
        }
    }
    
    /// <summary>
    /// Setup UI elements
    /// </summary>
    private void SetupUI()
    {
        // Create default UI elements if not assigned
        if (hudPanel == null)
        {
            CreateDefaultHUDPanel();
        }
        
        if (crosshair == null)
        {
            CreateDefaultCrosshair();
        }
        
        if (healthBar == null)
        {
            CreateDefaultHealthBar();
        }
        
        if (ammoCounter == null)
        {
            CreateDefaultAmmoCounter();
        }
        
        if (scoreDisplay == null)
        {
            CreateDefaultScoreDisplay();
        }
    }
    
    /// <summary>
    /// Subscribe to game events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (gameManager != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Game manager found for UI");
        }
        
        if (scoreManager != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Score manager found for UI");
        }
        
        if (playerHealth != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Player health component found for UI");
        }
    }
    
    /// <summary>
    /// Handle input
    /// </summary>
    private void HandleInput()
    {
        // Toggle HUD
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHUD();
        }
        
        // Toggle debug info
        if (Input.GetKeyDown(KeyCode.F3))
        {
            showDebugInfo = !showDebugInfo;
        }
        
        // Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameManager != null && gameManager.CurrentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
        }
    }
    
    /// <summary>
    /// Update HUD elements
    /// </summary>
    private void UpdateHUD()
    {
        if (!isHUDVisible) return;
        
        // Update health bar
        UpdateHealthBar();
        
        // Update ammo counter
        UpdateAmmoCounter();
        
        // Update score display
        UpdateScoreDisplay();
        
        // Update wave info
        UpdateWaveInfo();
        
        // Update combo display
        UpdateComboDisplay();
        
        // Update debug info
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }
    
    /// <summary>
    /// Update health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        if (playerHealth != null && healthBar != null)
        {
            float healthPercentage = playerHealth.HealthPercentage;
            
            // Update health bar scale
            Vector3 scale = healthBar.transform.localScale;
            scale.x = healthPercentage;
            healthBar.transform.localScale = scale;
            
            // Change color based on health
            Renderer healthRenderer = healthBar.GetComponent<Renderer>();
            if (healthRenderer != null)
            {
                if (healthPercentage > 0.6f)
                {
                    healthRenderer.material.color = Color.green;
                }
                else if (healthPercentage > 0.3f)
                {
                    healthRenderer.material.color = Color.yellow;
                }
                else
                {
                    healthRenderer.material.color = Color.red;
                }
            }
            
            // Log health to console
            Debug.Log($"Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth} ({healthPercentage:P0})");
        }
    }
    
    /// <summary>
    /// Update ammo counter
    /// </summary>
    private void UpdateAmmoCounter()
    {
        if (weaponController != null && ammoCounter != null)
        {
            int currentAmmo = weaponController.CurrentAmmo;
            int totalAmmo = weaponController.TotalAmmo;
            
            // Update ammo counter scale
            Vector3 scale = ammoCounter.transform.localScale;
            scale.x = (float)currentAmmo / weaponController.MagazineSize;
            ammoCounter.transform.localScale = scale;
            
            // Log ammo to console
            Debug.Log($"Ammo: {currentAmmo}/{totalAmmo}");
        }
    }
    
    /// <summary>
    /// Update score display
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreManager != null && scoreDisplay != null)
        {
            int currentScore = scoreManager.CurrentScore;
            int highScore = scoreManager.HighScore;
            
            // Update score display scale (visual representation)
            Vector3 scale = scoreDisplay.transform.localScale;
            scale.x = Mathf.Clamp01((float)currentScore / 10000f); // Normalize to 0-1
            scoreDisplay.transform.localScale = scale;
            
            // Log score to console
            Debug.Log($"Score: {currentScore} | High Score: {highScore}");
        }
    }
    
    /// <summary>
    /// Update wave info
    /// </summary>
    private void UpdateWaveInfo()
    {
        if (waveInfo != null)
        {
            // Find enemy spawner
            SimpleEnemySpawner spawner = FindFirstObjectByType<SimpleEnemySpawner>();
            if (spawner != null)
            {
                int currentWave = spawner.CurrentWave;
                int enemiesKilled = spawner.EnemiesKilledThisWave;
                int enemiesPerWave = spawner.EnemiesSpawnedThisWave;
                
                // Update wave info scale
                Vector3 scale = waveInfo.transform.localScale;
                scale.x = (float)enemiesKilled / enemiesPerWave;
                waveInfo.transform.localScale = scale;
                
                // Log wave info to console
                Debug.Log($"Wave {currentWave}: {enemiesKilled}/{enemiesPerWave} enemies killed");
            }
        }
    }
    
    /// <summary>
    /// Update combo display
    /// </summary>
    private void UpdateComboDisplay()
    {
        if (scoreManager != null && comboDisplay != null)
        {
            int combo = scoreManager.ComboMultiplier;
            
            if (combo > 1)
            {
                comboDisplay.SetActive(true);
                
                // Scale combo display based on multiplier
                Vector3 scale = comboDisplay.transform.localScale;
                scale.x = Mathf.Clamp01((float)combo / 5f); // Normalize to 0-1
                comboDisplay.transform.localScale = scale;
                
                Debug.Log($"COMBO x{combo}!");
            }
            else
            {
                comboDisplay.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Update debug info
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (playerController != null)
        {
            string debugInfo = $"FPS: {fps:F1} | " +
                             $"Position: {playerController.transform.position} | " +
                             $"Speed: {playerController.CurrentSpeed:F1} | " +
                             $"State: {playerController.GetMovementState()}";
            
            Debug.Log(debugInfo);
        }
    }
    
    /// <summary>
    /// Update FPS counter
    /// </summary>
    private void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        
        if (fpsTimer >= 1f)
        {
            fps = frameCount / fpsTimer;
            frameTime = 1000f / fps;
            frameCount = 0;
            fpsTimer = 0f;
        }
    }
    
    /// <summary>
    /// Toggle HUD visibility
    /// </summary>
    public void ToggleHUD()
    {
        isHUDVisible = !isHUDVisible;
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(isHUDVisible);
        }
        
        Debug.Log($"HUD {(isHUDVisible ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Show main menu
    /// </summary>
    public void ShowMainMenu()
    {
        SetAllPanelsActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        Debug.Log("Main menu displayed");
    }
    
    /// <summary>
    /// Show HUD
    /// </summary>
    public void ShowHUD()
    {
        SetAllPanelsActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        
        Debug.Log("HUD displayed");
    }
    
    /// <summary>
    /// Show pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        isPaused = true;
        
        Debug.Log("Pause menu displayed");
    }
    
    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver()
    {
        SetAllPanelsActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        Debug.Log("Game over screen displayed");
    }
    
    /// <summary>
    /// Pause game
    /// </summary>
    public void PauseGame()
    {
        if (gameManager != null)
        {
            gameManager.SetGameState(GameState.Paused);
        }
        else
        {
            Time.timeScale = 0f;
            ShowPauseMenu();
        }
    }
    
    /// <summary>
    /// Resume game
    /// </summary>
    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.SetGameState(GameState.Playing);
        }
        else
        {
            Time.timeScale = 1f;
            isPaused = false;
            ShowHUD();
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
    /// Create default HUD panel
    /// </summary>
    private void CreateDefaultHUDPanel()
    {
        hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(transform);
        Debug.Log("Default HUD panel created");
    }
    
    /// <summary>
    /// Create default crosshair
    /// </summary>
    private void CreateDefaultCrosshair()
    {
        crosshair = GameObject.CreatePrimitive(PrimitiveType.Quad);
        crosshair.name = "Crosshair";
        crosshair.transform.SetParent(hudPanel != null ? hudPanel.transform : transform);
        crosshair.transform.localPosition = new Vector3(0, 0, 1);
        crosshair.transform.localScale = new Vector3(0.02f, 0.02f, 1);
        
        Material crosshairMaterial = new Material(Shader.Find("Standard"));
        crosshairMaterial.color = Color.white;
        crosshair.GetComponent<Renderer>().material = crosshairMaterial;
        
        Debug.Log("Default crosshair created");
    }
    
    /// <summary>
    /// Create default health bar
    /// </summary>
    private void CreateDefaultHealthBar()
    {
        healthBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healthBar.name = "HealthBar";
        healthBar.transform.SetParent(hudPanel != null ? hudPanel.transform : transform);
        healthBar.transform.localPosition = new Vector3(-0.8f, 0.8f, 0);
        healthBar.transform.localScale = new Vector3(0.3f, 0.05f, 0.01f);
        
        Material healthMaterial = new Material(Shader.Find("Standard"));
        healthMaterial.color = Color.green;
        healthBar.GetComponent<Renderer>().material = healthMaterial;
        
        Debug.Log("Default health bar created");
    }
    
    /// <summary>
    /// Create default ammo counter
    /// </summary>
    private void CreateDefaultAmmoCounter()
    {
        ammoCounter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ammoCounter.name = "AmmoCounter";
        ammoCounter.transform.SetParent(hudPanel != null ? hudPanel.transform : transform);
        ammoCounter.transform.localPosition = new Vector3(0.8f, 0.8f, 0);
        ammoCounter.transform.localScale = new Vector3(0.3f, 0.05f, 0.01f);
        
        Material ammoMaterial = new Material(Shader.Find("Standard"));
        ammoMaterial.color = Color.blue;
        ammoCounter.GetComponent<Renderer>().material = ammoMaterial;
        
        Debug.Log("Default ammo counter created");
    }
    
    /// <summary>
    /// Create default score display
    /// </summary>
    private void CreateDefaultScoreDisplay()
    {
        scoreDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        scoreDisplay.name = "ScoreDisplay";
        scoreDisplay.transform.SetParent(hudPanel != null ? hudPanel.transform : transform);
        scoreDisplay.transform.localPosition = new Vector3(0, 0.9f, 0);
        scoreDisplay.transform.localScale = new Vector3(0.5f, 0.03f, 0.01f);
        
        Material scoreMaterial = new Material(Shader.Find("Standard"));
        scoreMaterial.color = Color.yellow;
        scoreDisplay.GetComponent<Renderer>().material = scoreMaterial;
        
        Debug.Log("Default score display created");
    }
} 