using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages level progression, variety, and different arena layouts.
/// </summary>
public class LevelProgressionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public int levelNumber;
        public float arenaSize = 50f;
        public int coverCount = 15;
        public int obstacleCount = 8;
        public int enemiesPerWave = 5;
        public float waveInterval = 10f;
        public Color ambientColor = Color.white;
        public Material groundMaterial;
        public Material wallMaterial;
        public string[] availableEnemyTypes;
        public string[] availablePowerUps;
        public bool hasBoss = false;
        public string bossType;
        public int bossWave = 5;
        public string description;
    }
    
    [Header("Level Settings")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool enableProgressiveDifficulty = true;
    [SerializeField] private float difficultyScaling = 1.2f;
    
    [Header("Progression")]
    [SerializeField] private int wavesPerLevel = 10;
    [SerializeField] private bool infiniteMode = false;
    [SerializeField] private int maxLevel = 10;
    
    // Level state
    private int currentWave = 0;
    private bool isLevelComplete = false;
    private LevelData currentLevelData;
    
    // Components
    private LevelGenerator levelGenerator;
    private SimpleEnemySpawner enemySpawner;
    private PowerUpManager powerUpManager;
    private EnemyTypeManager enemyTypeManager;
    
    // Singleton instance
    public static LevelProgressionManager Instance { get; private set; }
    
    // Events
    public System.Action<int> OnLevelStarted;
    public System.Action<int> OnLevelCompleted;
    public System.Action<int> OnWaveCompleted;
    public System.Action<LevelData> OnLevelDataChanged;
    
    // Properties
    public int CurrentLevel => currentLevel;
    public int CurrentWave => currentWave;
    public bool IsLevelComplete => isLevelComplete;
    public LevelData CurrentLevelData => currentLevelData;
    public bool IsInfiniteMode => infiniteMode;
    
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
        InitializeLevel();
    }
    
    /// <summary>
    /// Find required components
    /// </summary>
    private void FindComponents()
    {
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        enemySpawner = FindFirstObjectByType<SimpleEnemySpawner>();
        powerUpManager = FindFirstObjectByType<PowerUpManager>();
        enemyTypeManager = FindFirstObjectByType<EnemyTypeManager>();
    }
    
    /// <summary>
    /// Initialize current level
    /// </summary>
    private void InitializeLevel()
    {
        if (levels.Length == 0)
        {
            CreateDefaultLevels();
        }
        
        currentLevelData = GetLevelData(currentLevel);
        if (currentLevelData == null)
        {
            currentLevelData = CreateDefaultLevel(currentLevel);
        }
        
        ApplyLevelSettings();
        OnLevelStarted?.Invoke(currentLevel);
        
        Debug.Log($"Level {currentLevel} initialized: {currentLevelData.levelName}");
    }
    
    /// <summary>
    /// Apply level settings to components
    /// </summary>
    private void ApplyLevelSettings()
    {
        if (levelGenerator != null)
        {
            // Apply level settings to level generator
            Debug.Log($"Applying level settings: Arena={currentLevelData.arenaSize}, Cover={currentLevelData.coverCount}");
        }
        
        if (enemySpawner != null)
        {
            // Apply enemy spawner settings
            enemySpawner.SetWaveSettings(
                currentLevelData.enemiesPerWave,
                currentLevelData.waveInterval,
                2f // Time between spawns
            );
        }
        
        // Apply lighting
        RenderSettings.ambientLight = currentLevelData.ambientColor;
        
        // Apply materials
        if (currentLevelData.groundMaterial != null)
        {
            // Apply ground material
            Debug.Log("Applied custom ground material");
        }
        
        if (currentLevelData.wallMaterial != null)
        {
            // Apply wall material
            Debug.Log("Applied custom wall material");
        }
    }
    
    /// <summary>
    /// Get level data by level number
    /// </summary>
    public LevelData GetLevelData(int levelNumber)
    {
        foreach (LevelData level in levels)
        {
            if (level.levelNumber == levelNumber)
            {
                return level;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Start next level
    /// </summary>
    public void StartNextLevel()
    {
        if (infiniteMode || currentLevel < maxLevel)
        {
            currentLevel++;
            currentWave = 0;
            isLevelComplete = false;
            
            InitializeLevel();
        }
        else
        {
            Debug.Log("Game completed! All levels finished.");
        }
    }
    
    /// <summary>
    /// Complete current wave
    /// </summary>
    public void CompleteWave()
    {
        currentWave++;
        OnWaveCompleted?.Invoke(currentWave);
        
        // Check for boss wave
        if (currentLevelData.hasBoss && currentWave == currentLevelData.bossWave)
        {
            SpawnBoss();
        }
        
        // Check if level is complete
        if (currentWave >= wavesPerLevel)
        {
            CompleteLevel();
        }
        
        Debug.Log($"Wave {currentWave} completed for level {currentLevel}");
    }
    
    /// <summary>
    /// Complete current level
    /// </summary>
    public void CompleteLevel()
    {
        isLevelComplete = true;
        OnLevelCompleted?.Invoke(currentLevel);
        
        Debug.Log($"Level {currentLevel} completed!");
        
        // Show level completion UI or auto-advance
        StartCoroutine(LevelCompletionSequence());
    }
    
    /// <summary>
    /// Level completion sequence
    /// </summary>
    private System.Collections.IEnumerator LevelCompletionSequence()
    {
        // Wait a moment for effects
        yield return new WaitForSeconds(2f);
        
        // Show completion message
        Debug.Log($"Level {currentLevel} Complete! Starting next level...");
        
        // Auto-advance to next level
        yield return new WaitForSeconds(3f);
        StartNextLevel();
    }
    
    /// <summary>
    /// Spawn boss enemy
    /// </summary>
    private void SpawnBoss()
    {
        if (string.IsNullOrEmpty(currentLevelData.bossType))
        {
            currentLevelData.bossType = "Boss";
        }
        
        Debug.Log($"Boss wave! Spawning {currentLevelData.bossType}");
        
        // Spawn boss at center of arena
        Vector3 bossPosition = new Vector3(0, 1, 0);
        
        if (enemyTypeManager != null)
        {
            EnemyTypeManager.EnemyTypeData bossData = enemyTypeManager.GetEnemyType(currentLevelData.bossType);
            if (bossData != null)
            {
                GameObject boss = enemyTypeManager.CreateEnemy(bossData, bossPosition);
                if (boss != null)
                {
                    // Make boss larger and more intimidating
                    boss.transform.localScale = Vector3.one * 2f;
                    
                    // Add boss effects
                    if (ParticleEffectManager.Instance != null)
                    {
                        ParticleEffectManager.Instance.CreateExplosion(bossPosition, 5f);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get level progression info
    /// </summary>
    public string GetLevelProgressionInfo()
    {
        return $"Level {currentLevel}: {currentLevelData.levelName}\n" +
               $"Wave {currentWave}/{wavesPerLevel}\n" +
               $"Enemies per wave: {currentLevelData.enemiesPerWave}\n" +
               $"Arena size: {currentLevelData.arenaSize}\n" +
               $"Cover objects: {currentLevelData.coverCount}\n" +
               $"Obstacles: {currentLevelData.obstacleCount}";
    }
    
    /// <summary>
    /// Get current level difficulty
    /// </summary>
    public float GetCurrentDifficulty()
    {
        if (!enableProgressiveDifficulty) return 1f;
        
        return Mathf.Pow(difficultyScaling, currentLevel - 1);
    }
    
    /// <summary>
    /// Restart current level
    /// </summary>
    public void RestartLevel()
    {
        currentWave = 0;
        isLevelComplete = false;
        
        // Reset enemy spawner
        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
        }
        
        // Reset power-ups
        if (powerUpManager != null)
        {
            powerUpManager.ClearAllPowerUps();
        }
        
        // Regenerate level
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel();
        }
        
        Debug.Log($"Level {currentLevel} restarted");
    }
    
    /// <summary>
    /// Set infinite mode
    /// </summary>
    public void SetInfiniteMode(bool enabled)
    {
        infiniteMode = enabled;
        Debug.Log($"Infinite mode {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Create default levels
    /// </summary>
    private void CreateDefaultLevels()
    {
        levels = new LevelData[]
        {
            CreateDefaultLevel(1),
            CreateDefaultLevel(2),
            CreateDefaultLevel(3),
            CreateDefaultLevel(4),
            CreateDefaultLevel(5)
        };
    }
    
    /// <summary>
    /// Create default level
    /// </summary>
    private LevelData CreateDefaultLevel(int levelNumber)
    {
        LevelData level = new LevelData();
        level.levelNumber = levelNumber;
        level.levelName = $"Level {levelNumber}";
        level.arenaSize = 30f + (levelNumber * 5f);
        level.coverCount = 8 + levelNumber;
        level.obstacleCount = 3 + levelNumber;
        level.enemiesPerWave = 3 + levelNumber;
        level.waveInterval = Mathf.Max(5f, 15f - levelNumber);
        level.ambientColor = Color.white;
        level.availableEnemyTypes = new string[] { "Basic" };
        level.availablePowerUps = new string[] { "Health", "Ammo" };
        level.hasBoss = levelNumber % 5 == 0; // Boss every 5 levels
        level.bossType = "Boss";
        level.bossWave = 5;
        level.description = $"Default level {levelNumber}";
        
        // Add enemy types based on level
        if (levelNumber >= 2)
        {
            level.availableEnemyTypes = new string[] { "Basic", "Fast" };
        }
        if (levelNumber >= 3)
        {
            level.availableEnemyTypes = new string[] { "Basic", "Fast", "Tank" };
        }
        if (levelNumber >= 4)
        {
            level.availableEnemyTypes = new string[] { "Basic", "Fast", "Tank", "Ranged" };
        }
        
        // Add power-ups based on level
        if (levelNumber >= 2)
        {
            level.availablePowerUps = new string[] { "Health", "Ammo", "Speed" };
        }
        if (levelNumber >= 3)
        {
            level.availablePowerUps = new string[] { "Health", "Ammo", "Speed", "Damage" };
        }
        
        return level;
    }
    
    /// <summary>
    /// Get all levels
    /// </summary>
    public LevelData[] GetAllLevels()
    {
        return levels;
    }
    
    /// <summary>
    /// Get level completion percentage
    /// </summary>
    public float GetLevelCompletionPercentage()
    {
        return (float)currentWave / wavesPerLevel;
    }
    
    /// <summary>
    /// Get total game completion percentage
    /// </summary>
    public float GetGameCompletionPercentage()
    {
        if (infiniteMode) return 0f;
        
        int totalLevels = maxLevel;
        int completedLevels = currentLevel - 1;
        float currentLevelProgress = GetLevelCompletionPercentage();
        
        return (completedLevels + currentLevelProgress) / totalLevels;
    }
    
    /// <summary>
    /// Get level rewards
    /// </summary>
    public string GetLevelRewards()
    {
        string rewards = $"Level {currentLevel} Rewards:\n";
        rewards += $"- Score: +{currentLevel * 1000}\n";
        rewards += $"- Experience: +{currentLevel * 100}\n";
        
        if (currentLevelData.hasBoss && currentWave >= currentLevelData.bossWave)
        {
            rewards += $"- Boss defeated: +{currentLevel * 5000}\n";
        }
        
        return rewards;
    }
} 