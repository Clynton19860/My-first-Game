using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enemy spawner that handles enemy spawning, wave management, and difficulty scaling.
/// Manages spawn points and enemy types.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private EnemyData[] enemyTypes;
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
    
    [Header("Spawn Effects")]
    [SerializeField] private GameObject spawnEffect;
    [SerializeField] private AudioSource spawnAudio;
    [SerializeField] private AudioClip spawnSound;
    
    // Private variables
    private int currentWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesKilledThisWave = 0;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private bool isSpawning = false;
    private float lastWaveTime;
    
    // Events
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action<Enemy> OnEnemySpawned;
    public System.Action<Enemy> OnEnemyKilled;
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
        
        // Get random enemy type
        EnemyData enemyType = GetRandomEnemyType();
        if (enemyType == null) return;
        
        // Find valid spawn position
        Vector3 spawnPosition = GetValidSpawnPosition(spawnPoint.position);
        
        // Create enemy
        GameObject enemyObject = Instantiate(enemyType.enemyModel, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            // Setup enemy
            enemy.OnEnemyDeath += OnEnemyDeath;
            activeEnemies.Add(enemy);
            
            // Play spawn effects
            PlaySpawnEffects(spawnPosition);
            
            // Trigger event
            OnEnemySpawned?.Invoke(enemy);
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
    /// Get random enemy type
    /// </summary>
    private EnemyData GetRandomEnemyType()
    {
        if (enemyTypes.Length == 0) return null;
        return enemyTypes[Random.Range(0, enemyTypes.Length)];
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
    private void OnEnemyDeath(Enemy enemy)
    {
        // Remove from active enemies
        activeEnemies.Remove(enemy);
        
        // Update kill count
        enemiesKilledThisWave++;
        
        // Trigger event
        OnEnemyKilled?.Invoke(enemy);
        
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(enemy.ScoreValue);
        }
    }
    
    /// <summary>
    /// Complete the current wave
    /// </summary>
    private void CompleteWave()
    {
        OnWaveCompleted?.Invoke(currentWave);
        
        // Check if all waves are complete
        if (AllWavesCompleted)
        {
            OnAllWavesCompleted?.Invoke();
        }
    }
    
    /// <summary>
    /// Monitor enemy deaths
    /// </summary>
    private IEnumerator MonitorEnemyDeaths()
    {
        while (true)
        {
            // Clean up destroyed enemies
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// Spawn a specific enemy type at a specific position
    /// </summary>
    public void SpawnSpecificEnemy(EnemyData enemyType, Vector3 position)
    {
        if (enemyType == null) return;
        
        GameObject enemyObject = Instantiate(enemyType.enemyModel, position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.OnEnemyDeath += OnEnemyDeath;
            activeEnemies.Add(enemy);
            
            PlaySpawnEffects(position);
            OnEnemySpawned?.Invoke(enemy);
        }
    }
    
    /// <summary>
    /// Spawn enemies at all spawn points
    /// </summary>
    public void SpawnAtAllPoints(EnemyData enemyType)
    {
        if (enemyType == null) return;
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(spawnPoint.position);
            SpawnSpecificEnemy(enemyType, spawnPosition);
        }
    }
    
    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
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
    /// Set maximum enemies alive
    /// </summary>
    public void SetMaxEnemiesAlive(int maxEnemies)
    {
        maxEnemiesAlive = maxEnemies;
    }
    
    /// <summary>
    /// Get wave progress (0-1)
    /// </summary>
    public float GetWaveProgress()
    {
        if (enemiesPerWave == 0) return 0f;
        return (float)enemiesKilledThisWave / enemiesPerWave;
    }
    
    /// <summary>
    /// Get remaining enemies in current wave
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
    
    private void OnDrawGizmosSelected()
    {
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                }
            }
        }
    }
} 