using UnityEngine;

/// <summary>
/// Initializes all game systems for complete play testing.
/// Add this to a GameObject in your scene to set up everything automatically.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createTestEnemies = true;
    [SerializeField] private bool createTestPowerUps = true;
    
    [Header("Test Settings")]
    [SerializeField] private int testEnemyCount = 5;
    [SerializeField] private int testPowerUpCount = 3;
    [SerializeField] private bool enableAllSystems = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            InitializeAllSystems();
        }
    }
    
    /// <summary>
    /// Initialize all game systems
    /// </summary>
    public void InitializeAllSystems()
    {
        Debug.Log("=== Initializing Complete FPS Game Systems ===");
        
        // 1. Create Game Manager
        CreateGameManager();
        
        // 2. Create Level Generator
        CreateLevelGenerator();
        
        // 3. Create Player
        CreatePlayer();
        
        // 4. Create Enemy Systems
        CreateEnemySystems();
        
        // 5. Create Power-up System
        CreatePowerUpSystem();
        
        // 6. Create UI Systems
        CreateUISystems();
        
        // 7. Create Audio System
        CreateAudioSystem();
        
        // 8. Create Particle System
        CreateParticleSystem();
        
        // 9. Create Performance Optimizer
        CreatePerformanceOptimizer();
        
        // 10. Create Level Progression
        CreateLevelProgression();
        
        // 11. Create Menu System
        CreateMenuSystem();
        
        // 12. Create Build Manager
        CreateBuildManager();
        
        // 13. Create Test Objects
        if (createTestEnemies)
        {
            CreateTestEnemies();
        }
        
        if (createTestPowerUps)
        {
            CreateTestPowerUps();
        }
        
        Debug.Log("=== All Systems Initialized Successfully! ===");
        Debug.Log("Controls: WASD to move, Mouse to look, Left Click to shoot, R to reload, Escape to pause");
    }
    
    /// <summary>
    /// Create game manager
    /// </summary>
    private void CreateGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        Debug.Log("✓ Game Manager created");
    }
    
    /// <summary>
    /// Create level generator
    /// </summary>
    private void CreateLevelGenerator()
    {
        GameObject levelGen = new GameObject("LevelGenerator");
        levelGen.AddComponent<LevelGenerator>();
        Debug.Log("✓ Level Generator created");
    }
    
    /// <summary>
    /// Create player
    /// </summary>
    private void CreatePlayer()
    {
        // Create player GameObject
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        
        // Add player controller
        player.AddComponent<SimplePlayerController>();
        
        // Add health component
        Health playerHealth = player.AddComponent<Health>();
        playerHealth.SetMaxHealth(100);
        
        // Add weapon controller
        SimpleWeaponController weapon = player.AddComponent<SimpleWeaponController>();
        
        // Add score manager
        player.AddComponent<ScoreManager>();
        
        // Position player
        player.transform.position = new Vector3(0, 1, 0);
        
        Debug.Log("✓ Player created with all components");
    }
    
    /// <summary>
    /// Create enemy systems
    /// </summary>
    private void CreateEnemySystems()
    {
        // Create enemy spawner
        GameObject enemySpawner = new GameObject("EnemySpawner");
        enemySpawner.AddComponent<SimpleEnemySpawner>();
        
        // Create enemy type manager
        GameObject enemyTypeManager = new GameObject("EnemyTypeManager");
        enemyTypeManager.AddComponent<EnemyTypeManager>();
        
        Debug.Log("✓ Enemy systems created");
    }
    
    /// <summary>
    /// Create power-up system
    /// </summary>
    private void CreatePowerUpSystem()
    {
        GameObject powerUpManager = new GameObject("PowerUpManager");
        powerUpManager.AddComponent<PowerUpManager>();
        Debug.Log("✓ Power-up system created");
    }
    
    /// <summary>
    /// Create UI systems
    /// </summary>
    private void CreateUISystems()
    {
        // Create UI canvas
        GameObject canvas = new GameObject("GameCanvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create UI manager
        GameObject uiManager = new GameObject("UIManager");
        uiManager.AddComponent<EnhancedGameUI>();
        
        Debug.Log("✓ UI systems created");
    }
    
    /// <summary>
    /// Create audio system
    /// </summary>
    private void CreateAudioSystem()
    {
        GameObject audioManager = new GameObject("AudioManager");
        audioManager.AddComponent<AudioManager>();
        Debug.Log("✓ Audio system created");
    }
    
    /// <summary>
    /// Create particle system
    /// </summary>
    private void CreateParticleSystem()
    {
        GameObject particleManager = new GameObject("ParticleEffectManager");
        particleManager.AddComponent<ParticleEffectManager>();
        Debug.Log("✓ Particle system created");
    }
    
    /// <summary>
    /// Create performance optimizer
    /// </summary>
    private void CreatePerformanceOptimizer()
    {
        GameObject performanceOptimizer = new GameObject("PerformanceOptimizer");
        performanceOptimizer.AddComponent<PerformanceOptimizer>();
        Debug.Log("✓ Performance optimizer created");
    }
    
    /// <summary>
    /// Create level progression
    /// </summary>
    private void CreateLevelProgression()
    {
        GameObject levelProgression = new GameObject("LevelProgressionManager");
        levelProgression.AddComponent<LevelProgressionManager>();
        Debug.Log("✓ Level progression created");
    }
    
    /// <summary>
    /// Create menu system
    /// </summary>
    private void CreateMenuSystem()
    {
        GameObject menuManager = new GameObject("MenuManager");
        menuManager.AddComponent<MenuManager>();
        Debug.Log("✓ Menu system created");
    }
    
    /// <summary>
    /// Create build manager
    /// </summary>
    private void CreateBuildManager()
    {
        GameObject buildManager = new GameObject("BuildManager");
        buildManager.AddComponent<BuildManager>();
        Debug.Log("✓ Build manager created");
    }
    
    /// <summary>
    /// Create test enemies
    /// </summary>
    private void CreateTestEnemies()
    {
        for (int i = 0; i < testEnemyCount; i++)
        {
            GameObject enemy = CreateBasicEnemy();
            Vector3 randomPos = new Vector3(
                Random.Range(-10f, 10f),
                1f,
                Random.Range(-10f, 10f)
            );
            enemy.transform.position = randomPos;
        }
        Debug.Log($"✓ Created {testEnemyCount} test enemies");
    }
    
    /// <summary>
    /// Create basic enemy
    /// </summary>
    private GameObject CreateBasicEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "TestEnemy";
        enemy.tag = "Enemy";
        
        // Add enemy components
        enemy.AddComponent<Health>();
        enemy.AddComponent<SimpleEnemyAI>();
        
        // Set material color
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.red;
            renderer.material = material;
        }
        
        return enemy;
    }
    
    /// <summary>
    /// Create test power-ups
    /// </summary>
    private void CreateTestPowerUps()
    {
        for (int i = 0; i < testPowerUpCount; i++)
        {
            GameObject powerUp = CreateBasicPowerUp();
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),
                1f,
                Random.Range(-8f, 8f)
            );
            powerUp.transform.position = randomPos;
        }
        Debug.Log($"✓ Created {testPowerUpCount} test power-ups");
    }
    
    /// <summary>
    /// Create basic power-up
    /// </summary>
    private GameObject CreateBasicPowerUp()
    {
        GameObject powerUp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerUp.name = "TestPowerUp";
        powerUp.tag = "PowerUp";
        
        // Add power-up component
        powerUp.AddComponent<PowerUp>();
        
        // Set material color
        Renderer renderer = powerUp.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.yellow;
            renderer.material = material;
        }
        
        // Add trigger collider
        Collider collider = powerUp.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        return powerUp;
    }
    
    /// <summary>
    /// Get system status
    /// </summary>
    public string GetSystemStatus()
    {
        string status = "=== Game System Status ===\n";
        
        // Check if all managers exist
        status += $"GameManager: {(FindFirstObjectByType<GameManager>() != null ? "✓" : "✗")}\n";
        status += $"LevelGenerator: {(FindFirstObjectByType<LevelGenerator>() != null ? "✓" : "✗")}\n";
        status += $"PlayerController: {(FindFirstObjectByType<SimplePlayerController>() != null ? "✓" : "✗")}\n";
        status += $"EnemySpawner: {(FindFirstObjectByType<SimpleEnemySpawner>() != null ? "✓" : "✗")}\n";
        status += $"EnemyTypeManager: {(FindFirstObjectByType<EnemyTypeManager>() != null ? "✓" : "✗")}\n";
        status += $"PowerUpManager: {(FindFirstObjectByType<PowerUpManager>() != null ? "✓" : "✗")}\n";
        status += $"AudioManager: {(FindFirstObjectByType<AudioManager>() != null ? "✓" : "✗")}\n";
        status += $"ParticleEffectManager: {(FindFirstObjectByType<ParticleEffectManager>() != null ? "✓" : "✗")}\n";
        status += $"PerformanceOptimizer: {(FindFirstObjectByType<PerformanceOptimizer>() != null ? "✓" : "✗")}\n";
        status += $"LevelProgressionManager: {(FindFirstObjectByType<LevelProgressionManager>() != null ? "✓" : "✗")}\n";
        status += $"MenuManager: {(FindFirstObjectByType<MenuManager>() != null ? "✓" : "✗")}\n";
        status += $"BuildManager: {(FindFirstObjectByType<BuildManager>() != null ? "✓" : "✗")}\n";
        
        return status;
    }
} 