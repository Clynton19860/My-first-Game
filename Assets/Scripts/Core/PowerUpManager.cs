using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages power-ups and collectibles that enhance player abilities.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpData
    {
        public string powerUpName;
        public GameObject powerUpPrefab;
        public PowerUpType powerUpType;
        public float duration = 10f;
        public float effectValue = 1f;
        public Color powerUpColor = Color.yellow;
        public int scoreValue = 50;
        public float spawnChance = 0.3f;
        public string description;
    }
    
    public enum PowerUpType
    {
        Health,
        Speed,
        Damage,
        FireRate,
        Ammo,
        Shield,
        MultiShot,
        ExplosiveRounds
    }
    
    [Header("Power-up Settings")]
    [SerializeField] private PowerUpData[] powerUpTypes;
    [SerializeField] private float powerUpSpawnInterval = 15f;
    [SerializeField] private int maxPowerUpsOnField = 3;
    [SerializeField] private float powerUpLifetime = 20f;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private LayerMask spawnLayerMask = 1;
    
    // Active power-ups
    private List<GameObject> activePowerUps = new List<GameObject>();
    private Dictionary<PowerUpType, float> activeEffects = new Dictionary<PowerUpType, float>();
    
    // Components
    private SimplePlayerController playerController;
    private SimpleWeaponController weaponController;
    private Health playerHealth;
    
    // Singleton instance
    public static PowerUpManager Instance { get; private set; }
    
    // Events
    public System.Action<PowerUpType> OnPowerUpCollected;
    public System.Action<PowerUpType> OnPowerUpExpired;
    public System.Action<PowerUpData> OnPowerUpSpawned;
    
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
        StartCoroutine(PowerUpSpawning());
        StartCoroutine(PowerUpCleanup());
    }
    
    private void Update()
    {
        UpdateActiveEffects();
    }
    
    /// <summary>
    /// Find required components
    /// </summary>
    private void FindComponents()
    {
        playerController = FindFirstObjectByType<SimplePlayerController>();
        weaponController = FindFirstObjectByType<SimpleWeaponController>();
        
        if (playerController != null)
        {
            playerHealth = playerController.GetComponent<Health>();
        }
    }
    
    /// <summary>
    /// Power-up spawning coroutine
    /// </summary>
    private System.Collections.IEnumerator PowerUpSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(powerUpSpawnInterval);
            
            if (activePowerUps.Count < maxPowerUpsOnField)
            {
                SpawnRandomPowerUp();
            }
        }
    }
    
    /// <summary>
    /// Power-up cleanup coroutine
    /// </summary>
    private System.Collections.IEnumerator PowerUpCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            // Remove expired power-ups
            activePowerUps.RemoveAll(powerUp => powerUp == null);
        }
    }
    
    /// <summary>
    /// Spawn random power-up
    /// </summary>
    private void SpawnRandomPowerUp()
    {
        if (powerUpTypes.Length == 0) return;
        
        // Select random power-up based on spawn chance
        List<PowerUpData> availablePowerUps = new List<PowerUpData>();
        float totalChance = 0f;
        
        foreach (PowerUpData powerUp in powerUpTypes)
        {
            if (powerUp != null)
            {
                availablePowerUps.Add(powerUp);
                totalChance += powerUp.spawnChance;
            }
        }
        
        if (availablePowerUps.Count == 0) return;
        
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;
        PowerUpData selectedPowerUp = null;
        
        foreach (PowerUpData powerUp in availablePowerUps)
        {
            currentChance += powerUp.spawnChance;
            if (randomValue <= currentChance)
            {
                selectedPowerUp = powerUp;
                break;
            }
        }
        
        if (selectedPowerUp == null)
        {
            selectedPowerUp = availablePowerUps[0];
        }
        
        SpawnPowerUp(selectedPowerUp);
    }
    
    /// <summary>
    /// Spawn specific power-up
    /// </summary>
    public void SpawnPowerUp(PowerUpData powerUpData)
    {
        if (powerUpData == null || powerUpData.powerUpPrefab == null) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject powerUp = Instantiate(powerUpData.powerUpPrefab, spawnPosition, Quaternion.identity);
        
        // Setup power-up
        PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
        if (powerUpComponent == null)
        {
            powerUpComponent = powerUp.AddComponent<PowerUp>();
        }
        
        powerUpComponent.Initialize(powerUpData);
        activePowerUps.Add(powerUp);
        
        // Auto-destroy after lifetime
        Destroy(powerUp, powerUpLifetime);
        
        OnPowerUpSpawned?.Invoke(powerUpData);
        Debug.Log($"Spawned {powerUpData.powerUpName} power-up at {spawnPosition}");
    }
    
    /// <summary>
    /// Get random spawn position
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = spawnPoint.position;
        }
        else
        {
            // Use random position in arena
            spawnPosition = new Vector3(
                Random.Range(-20f, 20f),
                1f,
                Random.Range(-20f, 20f)
            );
        }
        
        // Add random offset
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        spawnPosition += new Vector3(randomOffset.x, 0f, randomOffset.y);
        
        // Raycast to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, spawnLayerMask))
        {
            spawnPosition = hit.point + Vector3.up * 0.5f;
        }
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Apply power-up effect
    /// </summary>
    public void ApplyPowerUp(PowerUpType powerUpType, float duration, float effectValue)
    {
        // Store active effect
        activeEffects[powerUpType] = Time.time + duration;
        
        // Apply immediate effect
        switch (powerUpType)
        {
            case PowerUpType.Health:
                if (playerHealth != null)
                {
                    playerHealth.Heal(Mathf.RoundToInt(effectValue));
                }
                break;
                
            case PowerUpType.Speed:
                if (playerController != null)
                {
                    // Note: Add speed multiplier to player controller
                    Debug.Log($"Speed boost applied: {effectValue}x for {duration}s");
                }
                break;
                
            case PowerUpType.Damage:
                if (weaponController != null)
                {
                    // Note: Add damage multiplier to weapon controller
                    Debug.Log($"Damage boost applied: {effectValue}x for {duration}s");
                }
                break;
                
            case PowerUpType.FireRate:
                if (weaponController != null)
                {
                    // Note: Add fire rate multiplier to weapon controller
                    Debug.Log($"Fire rate boost applied: {effectValue}x for {duration}s");
                }
                break;
                
            case PowerUpType.Ammo:
                if (weaponController != null)
                {
                    weaponController.AddAmmo(Mathf.RoundToInt(effectValue));
                }
                break;
                
            case PowerUpType.Shield:
                if (playerHealth != null)
                {
                    // Note: Add shield system to health component
                    Debug.Log($"Shield applied: {effectValue} protection for {duration}s");
                }
                break;
                
            case PowerUpType.MultiShot:
                if (weaponController != null)
                {
                    // Note: Add multi-shot capability to weapon controller
                    Debug.Log($"Multi-shot applied: {effectValue} bullets for {duration}s");
                }
                break;
                
            case PowerUpType.ExplosiveRounds:
                if (weaponController != null)
                {
                    // Note: Add explosive rounds to weapon controller
                    Debug.Log($"Explosive rounds applied for {duration}s");
                }
                break;
        }
        
        OnPowerUpCollected?.Invoke(powerUpType);
        Debug.Log($"Applied {powerUpType} power-up for {duration} seconds");
    }
    
    /// <summary>
    /// Update active effects
    /// </summary>
    private void UpdateActiveEffects()
    {
        List<PowerUpType> expiredEffects = new List<PowerUpType>();
        
        foreach (var effect in activeEffects)
        {
            if (Time.time >= effect.Value)
            {
                expiredEffects.Add(effect.Key);
            }
        }
        
        foreach (PowerUpType expiredEffect in expiredEffects)
        {
            RemovePowerUpEffect(expiredEffect);
        }
    }
    
    /// <summary>
    /// Remove power-up effect
    /// </summary>
    private void RemovePowerUpEffect(PowerUpType powerUpType)
    {
        activeEffects.Remove(powerUpType);
        
        // Reset effect to default
        switch (powerUpType)
        {
            case PowerUpType.Speed:
                Debug.Log("Speed boost expired");
                break;
            case PowerUpType.Damage:
                Debug.Log("Damage boost expired");
                break;
            case PowerUpType.FireRate:
                Debug.Log("Fire rate boost expired");
                break;
            case PowerUpType.Shield:
                Debug.Log("Shield expired");
                break;
            case PowerUpType.MultiShot:
                Debug.Log("Multi-shot expired");
                break;
            case PowerUpType.ExplosiveRounds:
                Debug.Log("Explosive rounds expired");
                break;
        }
        
        OnPowerUpExpired?.Invoke(powerUpType);
    }
    
    /// <summary>
    /// Get active effects info
    /// </summary>
    public string GetActiveEffectsInfo()
    {
        if (activeEffects.Count == 0)
        {
            return "No active power-ups";
        }
        
        string info = "Active Power-ups:\n";
        foreach (var effect in activeEffects)
        {
            float remainingTime = effect.Value - Time.time;
            info += $"- {effect.Key}: {remainingTime:F1}s remaining\n";
        }
        return info;
    }
    
    /// <summary>
    /// Check if power-up is active
    /// </summary>
    public bool IsPowerUpActive(PowerUpType powerUpType)
    {
        return activeEffects.ContainsKey(powerUpType) && Time.time < activeEffects[powerUpType];
    }
    
    /// <summary>
    /// Get remaining time for power-up
    /// </summary>
    public float GetPowerUpRemainingTime(PowerUpType powerUpType)
    {
        if (activeEffects.ContainsKey(powerUpType))
        {
            return Mathf.Max(0f, activeEffects[powerUpType] - Time.time);
        }
        return 0f;
    }
    
    /// <summary>
    /// Clear all power-up effects
    /// </summary>
    public void ClearAllPowerUps()
    {
        activeEffects.Clear();
        Debug.Log("All power-up effects cleared");
    }
    
    /// <summary>
    /// Spawn power-up at specific position
    /// </summary>
    public void SpawnPowerUpAtPosition(PowerUpType powerUpType, Vector3 position)
    {
        PowerUpData powerUpData = GetPowerUpData(powerUpType);
        if (powerUpData != null)
        {
            GameObject powerUp = Instantiate(powerUpData.powerUpPrefab, position, Quaternion.identity);
            PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
            if (powerUpComponent != null)
            {
                powerUpComponent.Initialize(powerUpData);
            }
            activePowerUps.Add(powerUp);
        }
    }
    
    /// <summary>
    /// Get power-up data by type
    /// </summary>
    private PowerUpData GetPowerUpData(PowerUpType powerUpType)
    {
        foreach (PowerUpData powerUp in powerUpTypes)
        {
            if (powerUp.powerUpType == powerUpType)
            {
                return powerUp;
            }
        }
        return null;
    }
}

/// <summary>
/// Individual power-up component
/// </summary>
public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    [SerializeField] private PowerUpManager.PowerUpData powerUpData;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    private float bobTimer = 0f;
    
    private void Start()
    {
        startPosition = transform.position;
        bobTimer = Random.Range(0f, 2f * Mathf.PI); // Random start phase
    }
    
    private void Update()
    {
        // Rotate power-up
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        bobTimer += bobSpeed * Time.deltaTime;
        float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;
    }
    
    /// <summary>
    /// Initialize power-up with data
    /// </summary>
    public void Initialize(PowerUpManager.PowerUpData data)
    {
        powerUpData = data;
        
        // Setup visual appearance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = data.powerUpColor;
            renderer.material = material;
        }
        
        // Setup scale
        transform.localScale = Vector3.one * 0.5f;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectPowerUp();
        }
    }
    
    /// <summary>
    /// Collect power-up
    /// </summary>
    private void CollectPowerUp()
    {
        if (PowerUpManager.Instance != null && powerUpData != null)
        {
            PowerUpManager.Instance.ApplyPowerUp(
                powerUpData.powerUpType,
                powerUpData.duration,
                powerUpData.effectValue
            );
            
            // Add score
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(powerUpData.scoreValue);
            }
            
            // Play collection effect
            if (ParticleEffectManager.Instance != null)
            {
                ParticleEffectManager.Instance.CreateExplosion(transform.position, 2f);
            }
            
            Debug.Log($"Collected {powerUpData.powerUpName} power-up!");
            
            // Destroy power-up
            Destroy(gameObject);
        }
    }
} 