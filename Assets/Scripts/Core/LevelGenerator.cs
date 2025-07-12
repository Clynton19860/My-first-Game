using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple level generator for creating test arenas with cover and obstacles.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private float arenaSize = 50f;
    [SerializeField] private int coverCount = 10;
    [SerializeField] private int obstacleCount = 5;
    [SerializeField] private bool generateOnStart = true;
    
    [Header("Cover Settings")]
    [SerializeField] private GameObject coverPrefab;
    [SerializeField] private float minCoverHeight = 1f;
    [SerializeField] private float maxCoverHeight = 2f;
    // [SerializeField] private float coverSpacing = 8f; // Unused - removed to avoid warning
    
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float minObstacleScale = 0.5f;
    [SerializeField] private float maxObstacleScale = 2f;
    
    [Header("Environment")]
    [SerializeField] private Material groundMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private bool createWalls = true;
    [SerializeField] private float wallHeight = 5f;
    
    private List<GameObject> generatedObjects = new List<GameObject>();
    
    private void Start()
    {
        if (generateOnStart)
        {
            GenerateLevel();
        }
    }
    
    /// <summary>
    /// Generate a complete level
    /// </summary>
    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        ClearLevel();
        CreateGround();
        CreateWalls();
        GenerateCover();
        GenerateObstacles();
        SetupLighting();
        
        Debug.Log($"Level generated: {arenaSize}x{arenaSize} arena with {coverCount} cover points and {obstacleCount} obstacles");
    }
    
    /// <summary>
    /// Clear all generated objects
    /// </summary>
    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();
    }
    
    /// <summary>
    /// Create the ground plane
    /// </summary>
    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(arenaSize / 10f, 1f, arenaSize / 10f);
        
        if (groundMaterial != null)
        {
            ground.GetComponent<Renderer>().material = groundMaterial;
        }
        else
        {
            Material defaultGround = new Material(Shader.Find("Standard"));
            defaultGround.color = new Color(0.3f, 0.5f, 0.3f);
            ground.GetComponent<Renderer>().material = defaultGround;
        }
        
        generatedObjects.Add(ground);
    }
    
    /// <summary>
    /// Create arena walls
    /// </summary>
    private void CreateWalls()
    {
        if (!createWalls) return;
        
        float wallThickness = 0.5f;
        
        // North wall
        GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        northWall.name = "NorthWall";
        northWall.transform.position = new Vector3(0, wallHeight / 2f, arenaSize / 2f);
        northWall.transform.localScale = new Vector3(arenaSize, wallHeight, wallThickness);
        
        // South wall
        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.name = "SouthWall";
        southWall.transform.position = new Vector3(0, wallHeight / 2f, -arenaSize / 2f);
        southWall.transform.localScale = new Vector3(arenaSize, wallHeight, wallThickness);
        
        // East wall
        GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eastWall.name = "EastWall";
        eastWall.transform.position = new Vector3(arenaSize / 2f, wallHeight / 2f, 0);
        eastWall.transform.localScale = new Vector3(wallThickness, wallHeight, arenaSize);
        
        // West wall
        GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        westWall.name = "WestWall";
        westWall.transform.position = new Vector3(-arenaSize / 2f, wallHeight / 2f, 0);
        westWall.transform.localScale = new Vector3(wallThickness, wallHeight, arenaSize);
        
        // Apply wall material
        Material wallMat = wallMaterial != null ? wallMaterial : new Material(Shader.Find("Standard"));
        if (wallMaterial == null)
        {
            wallMat.color = new Color(0.5f, 0.5f, 0.5f);
        }
        
        Renderer[] wallRenderers = { northWall.GetComponent<Renderer>(), 
                                   southWall.GetComponent<Renderer>(), 
                                   eastWall.GetComponent<Renderer>(), 
                                   westWall.GetComponent<Renderer>() };
        
        foreach (Renderer renderer in wallRenderers)
        {
            renderer.material = wallMat;
        }
        
        generatedObjects.AddRange(new GameObject[] { northWall, southWall, eastWall, westWall });
    }
    
    /// <summary>
    /// Generate cover points around the arena
    /// </summary>
    private void GenerateCover()
    {
        if (coverPrefab == null)
        {
            CreateDefaultCover();
        }
        
        for (int i = 0; i < coverCount; i++)
        {
            Vector3 position = GetRandomPositionInArena();
            float height = Random.Range(minCoverHeight, maxCoverHeight);
            
            GameObject cover;
            if (coverPrefab != null)
            {
                cover = Instantiate(coverPrefab, position, Quaternion.identity);
                cover.transform.localScale = new Vector3(1f, height, 1f);
            }
            else
            {
                cover = CreateDefaultCover(position, height);
            }
            
            cover.name = $"Cover_{i}";
            generatedObjects.Add(cover);
        }
    }
    
    /// <summary>
    /// Create default cover if no prefab is assigned
    /// </summary>
    private void CreateDefaultCover()
    {
        GameObject defaultCover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        defaultCover.name = "DefaultCover";
        
        Material coverMaterial = new Material(Shader.Find("Standard"));
        coverMaterial.color = new Color(0.6f, 0.4f, 0.2f);
        defaultCover.GetComponent<Renderer>().material = coverMaterial;
        
        coverPrefab = defaultCover;
        defaultCover.SetActive(false);
    }
    
    /// <summary>
    /// Create a default cover object at position
    /// </summary>
    private GameObject CreateDefaultCover(Vector3 position, float height)
    {
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cover.transform.position = position;
        cover.transform.localScale = new Vector3(2f, height, 1f);
        
        Material coverMaterial = new Material(Shader.Find("Standard"));
        coverMaterial.color = new Color(0.6f, 0.4f, 0.2f);
        cover.GetComponent<Renderer>().material = coverMaterial;
        
        return cover;
    }
    
    /// <summary>
    /// Generate random obstacles
    /// </summary>
    private void GenerateObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            CreateDefaultObstacles();
        }
        
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 position = GetRandomPositionInArena();
            float scale = Random.Range(minObstacleScale, maxObstacleScale);
            
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            obstacle.transform.localScale = Vector3.one * scale;
            
            obstacle.name = $"Obstacle_{i}";
            generatedObjects.Add(obstacle);
        }
    }
    
    /// <summary>
    /// Create default obstacles if no prefabs are assigned
    /// </summary>
    private void CreateDefaultObstacles()
    {
        obstaclePrefabs = new GameObject[3];
        
        // Create barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        Material barrelMaterial = new Material(Shader.Find("Standard"));
        barrelMaterial.color = new Color(0.8f, 0.6f, 0.2f);
        barrel.GetComponent<Renderer>().material = barrelMaterial;
        barrel.SetActive(false);
        obstaclePrefabs[0] = barrel;
        
        // Create crate
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        Material crateMaterial = new Material(Shader.Find("Standard"));
        crateMaterial.color = new Color(0.4f, 0.3f, 0.2f);
        crate.GetComponent<Renderer>().material = crateMaterial;
        crate.SetActive(false);
        obstaclePrefabs[1] = crate;
        
        // Create rock
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        Material rockMaterial = new Material(Shader.Find("Standard"));
        rockMaterial.color = new Color(0.5f, 0.5f, 0.5f);
        rock.GetComponent<Renderer>().material = rockMaterial;
        rock.SetActive(false);
        obstaclePrefabs[2] = rock;
    }
    
    /// <summary>
    /// Setup lighting for the arena
    /// </summary>
    private void SetupLighting()
    {
        // Create main directional light
        GameObject mainLight = new GameObject("MainLight");
        Light directionalLight = mainLight.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = 1f;
        directionalLight.color = Color.white;
        mainLight.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        
        // Create ambient light
        GameObject ambientLight = new GameObject("AmbientLight");
        Light ambient = ambientLight.AddComponent<Light>();
        ambient.type = LightType.Point;
        ambient.intensity = 0.3f;
        ambient.color = new Color(0.8f, 0.8f, 1f);
        ambientLight.transform.position = new Vector3(0, 10f, 0);
        ambient.range = arenaSize * 2f;
        
        generatedObjects.AddRange(new GameObject[] { mainLight, ambientLight });
    }
    
    /// <summary>
    /// Get a random position within the arena bounds
    /// </summary>
    private Vector3 GetRandomPositionInArena()
    {
        float halfSize = arenaSize / 2f - 2f; // Keep away from walls
        float x = Random.Range(-halfSize, halfSize);
        float z = Random.Range(-halfSize, halfSize);
        return new Vector3(x, 0, z);
    }
    
    /// <summary>
    /// Get spawn positions for enemies
    /// </summary>
    public Vector3[] GetEnemySpawnPositions(int count)
    {
        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            positions[i] = GetRandomPositionInArena();
        }
        return positions;
    }
    
    /// <summary>
    /// Get spawn positions for players
    /// </summary>
    public Vector3[] GetPlayerSpawnPositions(int count)
    {
        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            positions[i] = GetRandomPositionInArena();
        }
        return positions;
    }
} 