using UnityEngine;

/// <summary>
/// Quick start script to automatically set up a basic scene for testing.
/// Attach this to any GameObject and press the button in the inspector.
/// </summary>
public class QuickStart : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private KeyCode setupKey = KeyCode.F1;
    
    [Header("Player Settings")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float groundSize = 20f;
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupBasicScene();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(setupKey))
        {
            SetupBasicScene();
        }
    }
    
    /// <summary>
    /// Automatically set up a basic scene for testing
    /// </summary>
    [ContextMenu("Setup Basic Scene")]
    public void SetupBasicScene()
    {
        Debug.Log("Setting up basic scene...");
        
        // Create Player
        GameObject player = CreatePlayer();
        
        // Create Game Manager
        CreateGameManager();
        
        // Create Level Generator
        CreateLevelGenerator();
        
        // Create Basic UI
        CreateBasicUI();
        
        // Create Weapon
        CreateWeapon(player);
        
        Debug.Log("Basic scene setup complete! Press Play to test.");
    }
    
    /// <summary>
    /// Create player with all necessary components
    /// </summary>
    private GameObject CreatePlayer()
    {
        // Create Player GameObject
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, playerHeight, 0);
        
        // Add Character Controller
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        characterController.center = Vector3.zero;
        
        // Add Simple Player Controller script (works without Input System)
        SimplePlayerController playerController = player.AddComponent<SimplePlayerController>();
        
        // Create Camera
        GameObject camera = new GameObject("Main Camera");
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.transform.localRotation = Quaternion.identity;
        
        Camera cam = camera.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        
        // Set as main camera
        camera.tag = "MainCamera";
        
        // Add Audio Listener
        camera.AddComponent<AudioListener>();
        
        return player;
    }
    
    /// <summary>
    /// Create game manager
    /// </summary>
    private void CreateGameManager()
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
    }
    
    /// <summary>
    /// Create level generator
    /// </summary>
    private void CreateLevelGenerator()
    {
        GameObject levelGenerator = new GameObject("LevelGenerator");
        levelGenerator.AddComponent<LevelGenerator>();
    }
    
    // Note: Walls are now created by the LevelGenerator component
    
    /// <summary>
    /// Create basic UI elements
    /// </summary>
    private void CreateBasicUI()
    {
        // Create Canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        // Note: CanvasScaler and GraphicRaycaster removed to avoid UI package dependencies
        
        // Create UI Manager
        GameObject uiManager = new GameObject("UIManager");
        uiManager.transform.SetParent(canvas.transform);
        SimpleGameUI gameUI = uiManager.AddComponent<SimpleGameUI>();
        
        Debug.Log("Basic UI created - UI elements will be managed by SimpleGameUI component");
    }
    
    /// <summary>
    /// Create basic weapon
    /// </summary>
    private void CreateWeapon(GameObject player)
    {
        // Find camera
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null) return;
        
        // Create weapon holder
        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(playerCamera.transform);
        weaponHolder.transform.localPosition = new Vector3(0.5f, -0.3f, 0.5f);
        
        // Add simple weapon controller (works without Input System)
        SimpleWeaponController weaponController = weaponHolder.AddComponent<SimpleWeaponController>();
        
        // Create simple weapon model (cube)
        GameObject weaponModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weaponModel.name = "WeaponModel";
        weaponModel.transform.SetParent(weaponHolder.transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = new Vector3(0.1f, 0.1f, 0.3f);
        
        // Create weapon material
        Material weaponMaterial = new Material(Shader.Find("Standard"));
        weaponMaterial.color = Color.black;
        weaponModel.GetComponent<Renderer>().material = weaponMaterial;
    }
    
    /// <summary>
    /// Create a simple enemy for testing
    /// </summary>
    [ContextMenu("Create Test Enemy")]
    public void CreateTestEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "TestEnemy";
        enemy.transform.position = new Vector3(5f, 1f, 5f);
        
        // Add enemy components
        enemy.AddComponent<Health>();
        enemy.AddComponent<Enemy>();
        
        // Create enemy material
        Material enemyMaterial = new Material(Shader.Find("Standard"));
        enemyMaterial.color = Color.red;
        enemy.GetComponent<Renderer>().material = enemyMaterial;
        
        Debug.Log("Test enemy created at position (5, 1, 5)");
    }
} 