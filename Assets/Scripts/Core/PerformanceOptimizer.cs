using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Performance optimization and monitoring system.
/// Handles LOD, object pooling, and performance metrics.
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private bool enableLOD = true;
    [SerializeField] private bool enableObjectPooling = true;
    [SerializeField] private bool enableCulling = true;
    [SerializeField] private float targetFPS = 60f;
    
    [Header("LOD Settings")]
    [SerializeField] private float lodDistance1 = 10f;
    [SerializeField] private float lodDistance2 = 25f;
    [SerializeField] private float lodDistance3 = 50f;
    
    [Header("Object Pooling")]
    [SerializeField] private int maxPooledObjects = 100;
    [SerializeField] private float poolCleanupInterval = 30f;
    
    [Header("Culling Settings")]
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private LayerMask cullingLayers = -1;
    
    // Performance metrics
    private float currentFPS;
    private float averageFPS;
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;
    private int frameCount = 0;
    private float fpsTimer = 0f;
    
    // Object pooling
    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
    private List<GameObject> activePooledObjects = new List<GameObject>();
    
    // LOD objects
    private List<LODObject> lodObjects = new List<LODObject>();
    
    // Culling objects
    private List<GameObject> cullableObjects = new List<GameObject>();
    
    // Singleton instance
    public static PerformanceOptimizer Instance { get; private set; }
    
    // Events
    public System.Action<float> OnFPSChanged;
    public System.Action<float> OnAverageFPSChanged;
    
    // Properties
    public float CurrentFPS => currentFPS;
    public float AverageFPS => averageFPS;
    public float MinFPS => minFPS;
    public float MaxFPS => maxFPS;
    public bool IsPerformanceGood => currentFPS >= targetFPS * 0.8f;
    
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
        InitializeOptimizer();
        StartCoroutine(PerformanceMonitoring());
    }
    
    private void Update()
    {
        UpdateFPS();
        UpdateLOD();
        UpdateCulling();
    }
    
    /// <summary>
    /// Initialize performance optimizer
    /// </summary>
    private void InitializeOptimizer()
    {
        // Find LOD objects
        FindLODObjects();
        
        // Find cullable objects
        FindCullableObjects();
        
        // Start cleanup coroutine
        if (enableObjectPooling)
        {
            StartCoroutine(PoolCleanup());
        }
        
        Debug.Log("Performance optimizer initialized");
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
            currentFPS = frameCount / fpsTimer;
            
            // Update min/max FPS
            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;
            
            // Update average FPS
            averageFPS = (averageFPS + currentFPS) / 2f;
            
            frameCount = 0;
            fpsTimer = 0f;
            
            OnFPSChanged?.Invoke(currentFPS);
            OnAverageFPSChanged?.Invoke(averageFPS);
            
            // Log performance info
            if (frameCount % 60 == 0) // Log every 60 frames
            {
                Debug.Log($"Performance: FPS={currentFPS:F1}, Avg={averageFPS:F1}, Min={minFPS:F1}, Max={maxFPS:F1}");
            }
        }
    }
    
    /// <summary>
    /// Update LOD system
    /// </summary>
    private void UpdateLOD()
    {
        if (!enableLOD) return;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        Vector3 cameraPosition = mainCamera.transform.position;
        
        foreach (LODObject lodObject in lodObjects)
        {
            if (lodObject == null) continue;
            
            float distance = Vector3.Distance(cameraPosition, lodObject.transform.position);
            
            // Determine LOD level
            int lodLevel = 0;
            if (distance <= lodDistance1)
            {
                lodLevel = 0; // High detail
            }
            else if (distance <= lodDistance2)
            {
                lodLevel = 1; // Medium detail
            }
            else if (distance <= lodDistance3)
            {
                lodLevel = 2; // Low detail
            }
            else
            {
                lodLevel = 3; // Culled
            }
            
            lodObject.SetLODLevel(lodLevel);
        }
    }
    
    /// <summary>
    /// Update culling system
    /// </summary>
    private void UpdateCulling()
    {
        if (!enableCulling) return;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        Vector3 cameraPosition = mainCamera.transform.position;
        
        foreach (GameObject obj in cullableObjects)
        {
            if (obj == null) continue;
            
            float distance = Vector3.Distance(cameraPosition, obj.transform.position);
            bool shouldBeVisible = distance <= cullingDistance;
            
            // Toggle visibility based on distance
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = shouldBeVisible;
            }
            
            // Toggle colliders for performance
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = shouldBeVisible;
            }
        }
    }
    
    /// <summary>
    /// Find LOD objects in scene
    /// </summary>
    private void FindLODObjects()
    {
        LODObject[] objects = FindObjectsByType<LODObject>(FindObjectsSortMode.None);
        lodObjects.AddRange(objects);
        
        Debug.Log($"Found {lodObjects.Count} LOD objects");
    }
    
    /// <summary>
    /// Find cullable objects in scene
    /// </summary>
    private void FindCullableObjects()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & cullingLayers) != 0)
            {
                cullableObjects.Add(obj);
            }
        }
        
        Debug.Log($"Found {cullableObjects.Count} cullable objects");
    }
    
    /// <summary>
    /// Get object from pool
    /// </summary>
    public GameObject GetPooledObject(string poolName, GameObject prefab)
    {
        if (!enableObjectPooling) return Instantiate(prefab);
        
        if (!objectPools.ContainsKey(poolName))
        {
            objectPools[poolName] = new Queue<GameObject>();
        }
        
        GameObject obj;
        if (objectPools[poolName].Count > 0)
        {
            obj = objectPools[poolName].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
        }
        
        activePooledObjects.Add(obj);
        return obj;
    }
    
    /// <summary>
    /// Return object to pool
    /// </summary>
    public void ReturnToPool(string poolName, GameObject obj)
    {
        if (!enableObjectPooling)
        {
            Destroy(obj);
            return;
        }
        
        if (obj == null) return;
        
        obj.SetActive(false);
        
        if (!objectPools.ContainsKey(poolName))
        {
            objectPools[poolName] = new Queue<GameObject>();
        }
        
        objectPools[poolName].Enqueue(obj);
        activePooledObjects.Remove(obj);
        
        // Limit pool size
        if (objectPools[poolName].Count > maxPooledObjects)
        {
            GameObject excessObj = objectPools[poolName].Dequeue();
            if (excessObj != null)
            {
                Destroy(excessObj);
            }
        }
    }
    
    /// <summary>
    /// Performance monitoring coroutine
    /// </summary>
    private System.Collections.IEnumerator PerformanceMonitoring()
    {
        while (true)
        {
            // Check if performance is poor
            if (!IsPerformanceGood)
            {
                Debug.LogWarning($"Performance is poor! FPS: {currentFPS:F1} (Target: {targetFPS})");
                
                // Apply performance optimizations
                ApplyPerformanceOptimizations();
            }
            
            yield return new WaitForSeconds(5f);
        }
    }
    
    /// <summary>
    /// Apply performance optimizations
    /// </summary>
    private void ApplyPerformanceOptimizations()
    {
        // Reduce particle effects
        if (ParticleEffectManager.Instance != null)
        {
            // Note: Add method to reduce particle count
            Debug.Log("Reducing particle effects for performance");
        }
        
        // Reduce LOD distances
        if (enableLOD)
        {
            lodDistance1 *= 0.8f;
            lodDistance2 *= 0.8f;
            lodDistance3 *= 0.8f;
            Debug.Log("Reducing LOD distances for performance");
        }
        
        // Reduce culling distance
        if (enableCulling)
        {
            cullingDistance *= 0.8f;
            Debug.Log("Reducing culling distance for performance");
        }
    }
    
    /// <summary>
    /// Pool cleanup coroutine
    /// </summary>
    private System.Collections.IEnumerator PoolCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(poolCleanupInterval);
            
            // Remove null objects from active list
            activePooledObjects.RemoveAll(obj => obj == null);
            
            // Clean up empty pools
            List<string> poolsToRemove = new List<string>();
            foreach (var pool in objectPools)
            {
                if (pool.Value.Count == 0)
                {
                    poolsToRemove.Add(pool.Key);
                }
            }
            
            foreach (string poolName in poolsToRemove)
            {
                objectPools.Remove(poolName);
            }
            
            Debug.Log($"Pool cleanup complete. Active objects: {activePooledObjects.Count}");
        }
    }
    
    /// <summary>
    /// Get performance report
    /// </summary>
    public string GetPerformanceReport()
    {
        return $"Performance Report:\n" +
               $"Current FPS: {currentFPS:F1}\n" +
               $"Average FPS: {averageFPS:F1}\n" +
               $"Min FPS: {minFPS:F1}\n" +
               $"Max FPS: {maxFPS:F1}\n" +
               $"Target FPS: {targetFPS}\n" +
               $"Performance Good: {IsPerformanceGood}\n" +
               $"Active Pooled Objects: {activePooledObjects.Count}\n" +
               $"LOD Objects: {lodObjects.Count}\n" +
               $"Cullable Objects: {cullableObjects.Count}";
    }
    
    /// <summary>
    /// Reset performance metrics
    /// </summary>
    public void ResetPerformanceMetrics()
    {
        currentFPS = 0f;
        averageFPS = 0f;
        minFPS = float.MaxValue;
        maxFPS = 0f;
        frameCount = 0;
        fpsTimer = 0f;
        
        Debug.Log("Performance metrics reset");
    }
    
    /// <summary>
    /// Set target FPS
    /// </summary>
    public void SetTargetFPS(float target)
    {
        targetFPS = Mathf.Clamp(target, 30f, 144f);
        Application.targetFrameRate = Mathf.RoundToInt(targetFPS);
        
        Debug.Log($"Target FPS set to {targetFPS}");
    }
    
    /// <summary>
    /// Toggle LOD system
    /// </summary>
    public void ToggleLOD()
    {
        enableLOD = !enableLOD;
        Debug.Log($"LOD system {(enableLOD ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Toggle object pooling
    /// </summary>
    public void ToggleObjectPooling()
    {
        enableObjectPooling = !enableObjectPooling;
        Debug.Log($"Object pooling {(enableObjectPooling ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Toggle culling system
    /// </summary>
    public void ToggleCulling()
    {
        enableCulling = !enableCulling;
        Debug.Log($"Culling system {(enableCulling ? "enabled" : "disabled")}");
    }
}

/// <summary>
/// LOD object component for level of detail system
/// </summary>
public class LODObject : MonoBehaviour
{
    [Header("LOD Settings")]
    [SerializeField] private GameObject[] lodLevels;
    [SerializeField] private int currentLODLevel = 0;
    
    private void Start()
    {
        SetLODLevel(0);
    }
    
    /// <summary>
    /// Set LOD level
    /// </summary>
    public void SetLODLevel(int level)
    {
        if (level == currentLODLevel) return;
        
        currentLODLevel = Mathf.Clamp(level, 0, lodLevels.Length - 1);
        
        // Disable all LOD levels
        for (int i = 0; i < lodLevels.Length; i++)
        {
            if (lodLevels[i] != null)
            {
                lodLevels[i].SetActive(i == currentLODLevel);
            }
        }
    }
    
    /// <summary>
    /// Get current LOD level
    /// </summary>
    public int GetCurrentLODLevel()
    {
        return currentLODLevel;
    }
} 