using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages different enemy types with unique behaviors and stats.
/// </summary>
public class EnemyTypeManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyTypeData
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public int health = 100;
        public float moveSpeed = 3f;
        public float attackDamage = 20f;
        public float attackRange = 2f;
        public float detectionRange = 10f;
        public bool isRanged = false;
        public GameObject projectilePrefab;
        public Color enemyColor = Color.red;
        public int scoreValue = 100;
        public float spawnChance = 1f;
        public string[] specialAbilities;
    }
    
    [Header("Enemy Types")]
    [SerializeField] private EnemyTypeData[] enemyTypes;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool enableProgressiveSpawning = true;
    [SerializeField] private int waveUnlockThreshold = 3;
    // [SerializeField] private float difficultyScaling = 1.2f; // Unused - commented out to avoid warning
    
    // Enemy type tracking
    private Dictionary<string, EnemyTypeData> enemyTypeDictionary = new Dictionary<string, EnemyTypeData>();
    private List<string> availableEnemyTypes = new List<string>();
    
    // Singleton instance
    public static EnemyTypeManager Instance { get; private set; }
    
    // Events
    public System.Action<string> OnEnemyTypeUnlocked;
    public System.Action<EnemyTypeData> OnEnemySpawned;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemyTypes();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize enemy types
    /// </summary>
    private void InitializeEnemyTypes()
    {
        // Create dictionary for quick lookup
        foreach (EnemyTypeData enemyType in enemyTypes)
        {
            if (enemyType != null && !string.IsNullOrEmpty(enemyType.enemyName))
            {
                enemyTypeDictionary[enemyType.enemyName] = enemyType;
            }
        }
        
        // Start with basic enemy types
        availableEnemyTypes.Add("Basic");
        
        Debug.Log($"Enemy type manager initialized with {enemyTypes.Length} enemy types");
    }
    
    /// <summary>
    /// Get enemy type data by name
    /// </summary>
    public EnemyTypeData GetEnemyType(string enemyName)
    {
        if (enemyTypeDictionary.ContainsKey(enemyName))
        {
            return enemyTypeDictionary[enemyName];
        }
        
        Debug.LogWarning($"Enemy type '{enemyName}' not found!");
        return enemyTypes.Length > 0 ? enemyTypes[0] : null;
    }
    
    /// <summary>
    /// Get random enemy type based on current wave
    /// </summary>
    public EnemyTypeData GetRandomEnemyType(int currentWave)
    {
        List<EnemyTypeData> availableTypes = new List<EnemyTypeData>();
        
        foreach (string typeName in availableEnemyTypes)
        {
            EnemyTypeData enemyType = GetEnemyType(typeName);
            if (enemyType != null)
            {
                availableTypes.Add(enemyType);
            }
        }
        
        if (availableTypes.Count == 0)
        {
            Debug.LogWarning("No available enemy types!");
            return null;
        }
        
        // Weighted random selection based on spawn chance
        float totalWeight = 0f;
        foreach (EnemyTypeData enemyType in availableTypes)
        {
            totalWeight += enemyType.spawnChance;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (EnemyTypeData enemyType in availableTypes)
        {
            currentWeight += enemyType.spawnChance;
            if (randomValue <= currentWeight)
            {
                return enemyType;
            }
        }
        
        return availableTypes[0]; // Fallback
    }
    
    /// <summary>
    /// Unlock new enemy types based on wave progression
    /// </summary>
    public void UnlockEnemyTypes(int currentWave)
    {
        if (!enableProgressiveSpawning) return;
        
        // Unlock enemy types based on wave progression
        if (currentWave >= waveUnlockThreshold && !availableEnemyTypes.Contains("Fast"))
        {
            availableEnemyTypes.Add("Fast");
            OnEnemyTypeUnlocked?.Invoke("Fast");
            Debug.Log("Fast enemy type unlocked!");
        }
        
        if (currentWave >= waveUnlockThreshold * 2 && !availableEnemyTypes.Contains("Tank"))
        {
            availableEnemyTypes.Add("Tank");
            OnEnemyTypeUnlocked?.Invoke("Tank");
            Debug.Log("Tank enemy type unlocked!");
        }
        
        if (currentWave >= waveUnlockThreshold * 3 && !availableEnemyTypes.Contains("Ranged"))
        {
            availableEnemyTypes.Add("Ranged");
            OnEnemyTypeUnlocked?.Invoke("Ranged");
            Debug.Log("Ranged enemy type unlocked!");
        }
        
        if (currentWave >= waveUnlockThreshold * 4 && !availableEnemyTypes.Contains("Boss"))
        {
            availableEnemyTypes.Add("Boss");
            OnEnemyTypeUnlocked?.Invoke("Boss");
            Debug.Log("Boss enemy type unlocked!");
        }
    }
    
    /// <summary>
    /// Create enemy with type data
    /// </summary>
    public GameObject CreateEnemy(EnemyTypeData enemyType, Vector3 position)
    {
        if (enemyType == null || enemyType.enemyPrefab == null)
        {
            Debug.LogError("Invalid enemy type data!");
            return null;
        }
        
        // Create enemy
        GameObject enemy = Instantiate(enemyType.enemyPrefab, position, Quaternion.identity);
        
        // Setup enemy components
        SetupEnemyComponents(enemy, enemyType);
        
        // Trigger event
        OnEnemySpawned?.Invoke(enemyType);
        
        Debug.Log($"Created {enemyType.enemyName} enemy at {position}");
        
        return enemy;
    }
    
    /// <summary>
    /// Setup enemy components based on type data
    /// </summary>
    private void SetupEnemyComponents(GameObject enemy, EnemyTypeData enemyType)
    {
        // Setup health
        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.SetMaxHealth(enemyType.health);
        }
        
        // Setup AI
        SimpleEnemyAI ai = enemy.GetComponent<SimpleEnemyAI>();
        if (ai != null)
        {
            // Note: These properties need to be made public in SimpleEnemyAI
            // For now, we'll use reflection or add public setters
            Debug.Log($"Setting up {enemyType.enemyName} AI with speed: {enemyType.moveSpeed}");
        }
        
        // Setup visual appearance
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = enemyType.enemyColor;
            renderer.material = material;
        }
        
        // Setup scale based on enemy type
        switch (enemyType.enemyName.ToLower())
        {
            case "tank":
            case "boss":
                enemy.transform.localScale = Vector3.one * 1.5f;
                break;
            case "fast":
                enemy.transform.localScale = Vector3.one * 0.8f;
                break;
            default:
                enemy.transform.localScale = Vector3.one;
                break;
        }
    }
    
    /// <summary>
    /// Get available enemy types
    /// </summary>
    public List<string> GetAvailableEnemyTypes()
    {
        return new List<string>(availableEnemyTypes);
    }
    
    /// <summary>
    /// Get all enemy types
    /// </summary>
    public EnemyTypeData[] GetAllEnemyTypes()
    {
        return enemyTypes;
    }
    
    /// <summary>
    /// Get enemy count by type
    /// </summary>
    public Dictionary<string, int> GetEnemyTypeCounts()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        
        SimpleEnemyAI[] enemies = FindObjectsByType<SimpleEnemyAI>(FindObjectsSortMode.None);
        
        foreach (SimpleEnemyAI enemy in enemies)
        {
            if (enemy != null)
            {
                string enemyType = GetEnemyTypeFromGameObject(enemy.gameObject);
                if (counts.ContainsKey(enemyType))
                {
                    counts[enemyType]++;
                }
                else
                {
                    counts[enemyType] = 1;
                }
            }
        }
        
        return counts;
    }
    
    /// <summary>
    /// Get enemy type from game object
    /// </summary>
    private string GetEnemyTypeFromGameObject(GameObject enemy)
    {
        // This is a simplified approach - in a real implementation,
        // you might store the enemy type as a component or tag
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            
            // Determine type based on color (simplified)
            if (color.r > 0.8f && color.g < 0.3f && color.b < 0.3f)
                return "Basic";
            else if (color.r > 0.8f && color.g > 0.8f && color.b < 0.3f)
                return "Fast";
            else if (color.r < 0.3f && color.g < 0.3f && color.b > 0.8f)
                return "Tank";
            else if (color.r > 0.8f && color.g < 0.3f && color.b > 0.8f)
                return "Ranged";
            else if (color.r > 0.8f && color.g > 0.8f && color.b > 0.8f)
                return "Boss";
        }
        
        return "Unknown";
    }
    
    /// <summary>
    /// Reset available enemy types
    /// </summary>
    public void ResetEnemyTypes()
    {
        availableEnemyTypes.Clear();
        availableEnemyTypes.Add("Basic");
        Debug.Log("Enemy types reset to basic");
    }
    
    /// <summary>
    /// Get enemy type info for UI
    /// </summary>
    public string GetEnemyTypeInfo()
    {
        string info = "Available Enemy Types:\n";
        foreach (string typeName in availableEnemyTypes)
        {
            EnemyTypeData enemyType = GetEnemyType(typeName);
            if (enemyType != null)
            {
                info += $"- {enemyType.enemyName}: HP={enemyType.health}, Speed={enemyType.moveSpeed}, Score={enemyType.scoreValue}\n";
            }
        }
        return info;
    }
} 