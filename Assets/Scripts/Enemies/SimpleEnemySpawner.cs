using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple enemy spawner that works with SimpleEnemyAI.
/// Doesn't require EnemyData ScriptableObjects.
/// </summary>
public class SimpleEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private LayerMask spawnLayerMask = 1;
    
    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private int maxEnemiesAlive = 20;
    
    [Header("Difficulty")]
    [SerializeField] private float difficultyScaling = 1.2f;
    [SerializeField] private int maxWaves = 10;
    [SerializeField] private bool infiniteWaves = false;
    
    [Header("Enemy Types")]
    // [SerializeField] private bool spawnMeleeEnemies = true; // Unused - removed to avoid warning
    [SerializeField] private bool spawnRangedEnemies = false;
    [SerializeField] private float rangedEnemyChance = 0.3f;
    
    [Header("Spawn Effects")]
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private AudioSource spawnAudio;
    [SerializeField] private AudioClip spawnSound;
    
    // Private variables
    private int currentWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesKilledThisWave = 0;
    private List<SimpleEnemyAI> activeEnemies = new List<SimpleEnemyAI>();
    private bool isSpawning = false;
    private float lastWaveTime;
    
    // Events
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action<SimpleEnemyAI> OnEnemySpawned;
    public System.Action<SimpleEnemyAI> OnEnemyKilled;
    public System.Action OnAllWavesCompleted;
    
    // Properties
    public int CurrentWave => currentWave;
    public int EnemiesSpawnedThisWave => enemiesSpawnedThisWave;
    public int EnemiesKilledThisWave => enemiesKilledThisWave;
    public int ActiveEnemiesCount => activeEnemies.Count;
    public bool IsSpawning => isSpawning;
    public bool AllWavesCompleted => !infiniteWaves && currentWave >= maxWaves;
    
    private void Start()
    {
        // Subscribe to enemy death events
        StartCoroutine(MonitorEnemyDeaths());
        
        // Start first wave after delay
        Invoke(nameof(StartNextWave), 5f);
    }
    
    private void Update()
    {
        // Check if wave is complete
        if (enemiesKilledThisWave >= enemiesPerWave && !isSpawning)
        {
            CompleteWave();
        }
        
        // Start next wave if enough time has passed
        if (!isSpawning && Time.time - lastWaveTime > timeBetweenWaves)
        {
            StartNextWave();
        }
    }
    
    /// <summary>
    /// Start the next wave
    /// </summary>
    private void StartNextWave()
    {
        if (AllWavesCompleted) return;
        
        currentWave++;
        enemiesSpawnedThisWave = 0;
        enemiesKilledThisWave = 0;
        lastWaveTime = Time.time;
        
        // Calculate enemies for this wave
        int enemiesThisWave = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(difficultyScaling, currentWave - 1));
        
        OnWaveStarted?.Invoke(currentWave);
        Debug.Log($"Starting Wave {currentWave} with {enemiesThisWave} enemies");
        
        // Start spawning
        StartCoroutine(SpawnWave(enemiesThisWave));
    }
    
    /// <summary>
    /// Spawn enemies for the current wave
    /// </summary>
    private IEnumerator SpawnWave(int enemyCount)
    {
        isSpawning = true;
        
        for (int i = 0; i < enemyCount; i++)
        {
            // Wait if too many enemies are alive
            while (activeEnemies.Count >= maxEnemiesAlive)
            {
                yield return new WaitForSeconds(1f);
            }
            
            // Spawn enemy
            SpawnEnemy();
            enemiesSpawnedThisWave++;
            
            // Wait between spawns
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        isSpawning = false;
    }
    
    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    private void SpawnEnemy()
    {
        // Get random spawn point
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;
        
        // Find valid spawn position
        Vector3 spawnPosition = GetValidSpawnPosition(spawnPoint.position);
        
        // Create enemy
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        SimpleEnemyAI enemy = enemyObject.GetComponent<SimpleEnemyAI>();
        
        if (enemy != null)
        {
            // Setup enemy type
            SetupEnemyType(enemy);
            
            // Subscribe to death event
            enemy.OnEnemyDeath += OnEnemyDeath;
            activeEnemies.Add(enemy);
            
            // Play spawn effects
            PlaySpawnEffects(spawnPosition);
            
            // Trigger event
            OnEnemySpawned?.Invoke(enemy);
            
            Debug.Log($"Spawned enemy at {spawnPosition}");
        }
    }
    
    /// <summary>
    /// Setup enemy type (melee or ranged)
    /// </summary>
    private void SetupEnemyType(SimpleEnemyAI enemy)
    {
        // Determine if this should be a ranged enemy
        bool shouldBeRanged = spawnRangedEnemies && Random.value < rangedEnemyChance;
        
        // Set enemy properties based on type
        if (shouldBeRanged)
        {
            // Ranged enemy settings
            enemy.isRangedEnemy = true;
        }
        else
        {
            // Melee enemy settings
            enemy.isRangedEnemy = false;
        }
    }
    
    /// <summary>
    /// Get random spawn point
    /// </summary>
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    /// <summary>
    /// Get valid spawn position
    /// </summary>
    private Vector3 GetValidSpawnPosition(Vector3 basePosition)
    {
        Vector3 spawnPosition = basePosition;
        
        // Add random offset within spawn radius
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        spawnPosition += new Vector3(randomOffset.x, 0f, randomOffset.y);
        
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, spawnLayerMask))
        {
            spawnPosition = hit.point;
        }
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Play spawn effects
    /// </summary>
    private void PlaySpawnEffects(Vector3 position)
    {
        // Spawn effect
        if (spawnEffect != null)
        {
            GameObject effect = Instantiate(spawnEffect, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Spawn sound
        if (spawnAudio != null && spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, position);
        }
    }
    
    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void OnEnemyDeath(SimpleEnemyAI enemy)
    {
        // Remove from active enemies
        activeEnemies.Remove(enemy);
        
        // Update kill count
        enemiesKilledThisWave++;
        
        // Trigger event
        OnEnemyKilled?.Invoke(enemy);
        
        Debug.Log($"Enemy killed! Wave progress: {enemiesKilledThisWave}/{enemiesPerWave}");
    }
    
    /// <summary>
    /// Complete the current wave
    /// </summary>
    private void CompleteWave()
    {
        OnWaveCompleted?.Invoke(currentWave);
        Debug.Log($"Wave {currentWave} completed!");
        
        // Check if all waves are complete
        if (AllWavesCompleted)
        {
            OnAllWavesCompleted?.Invoke();
            Debug.Log("All waves completed!");
        }
    }
    
    /// <summary>
    /// Monitor enemy deaths
    /// </summary>
    private IEnumerator MonitorEnemyDeaths()
    {
        while (true)
        {
            // Remove null enemies from list
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// Spawn a specific enemy at position
    /// </summary>
    public void SpawnSpecificEnemy(Vector3 position)
    {
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
        SimpleEnemyAI enemy = enemyObject.GetComponent<SimpleEnemyAI>();
        
        if (enemy != null)
        {
            enemy.OnEnemyDeath += OnEnemyDeath;
            activeEnemies.Add(enemy);
            OnEnemySpawned?.Invoke(enemy);
        }
    }
    
    /// <summary>
    /// Spawn enemies at all spawn points
    /// </summary>
    public void SpawnAtAllPoints()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Vector3 spawnPosition = GetValidSpawnPosition(spawnPoint.position);
                SpawnSpecificEnemy(spawnPosition);
            }
        }
    }
    
    /// <summary>
    /// Clear all enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (SimpleEnemyAI enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        
        activeEnemies.Clear();
    }
    
    /// <summary>
    /// Set wave settings
    /// </summary>
    public void SetWaveSettings(int enemiesPerWave, float timeBetweenWaves, float timeBetweenSpawns)
    {
        this.enemiesPerWave = enemiesPerWave;
        this.timeBetweenWaves = timeBetweenWaves;
        this.timeBetweenSpawns = timeBetweenSpawns;
    }
    
    /// <summary>
    /// Set difficulty scaling
    /// </summary>
    public void SetDifficultyScaling(float scaling)
    {
        difficultyScaling = scaling;
    }
    
    /// <summary>
    /// Set max enemies alive
    /// </summary>
    public void SetMaxEnemiesAlive(int maxEnemies)
    {
        maxEnemiesAlive = maxEnemies;
    }
    
    /// <summary>
    /// Get wave progress
    /// </summary>
    public float GetWaveProgress()
    {
        if (enemiesPerWave == 0) return 0f;
        return (float)enemiesKilledThisWave / enemiesPerWave;
    }
    
    /// <summary>
    /// Get remaining enemies in wave
    /// </summary>
    public int GetRemainingEnemiesInWave()
    {
        return enemiesPerWave - enemiesKilledThisWave;
    }
    
    /// <summary>
    /// Get time until next wave
    /// </summary>
    public float GetTimeUntilNextWave()
    {
        return Mathf.Max(0f, timeBetweenWaves - (Time.time - lastWaveTime));
    }
    
    /// <summary>
    /// Draw gizmos for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Spawn points
        Gizmos.color = Color.green;
        if (spawnPoints != null)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                }
            }
        }
        
        // Active enemies
        Gizmos.color = Color.red;
        if (activeEnemies != null)
        {
            foreach (SimpleEnemyAI enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawWireSphere(enemy.transform.position, 1f);
                }
            }
        }
    }
} 