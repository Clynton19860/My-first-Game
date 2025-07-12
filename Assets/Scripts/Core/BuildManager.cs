using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Manages game builds, deployment, and build settings.
/// </summary>
public class BuildManager : MonoBehaviour
{
    [System.Serializable]
    public class BuildSettings
    {
        public string buildName = "FPSGame";
        public string version = "1.0.0";
        public string companyName = "YourCompany";
        public string productName = "FPS Game";
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        public bool developmentBuild = false;
        public bool debugBuild = false;
        public string[] scenesToInclude;
        public string outputPath = "Builds/";
    }
    
    public enum BuildTarget
    {
        StandaloneWindows64,
        StandaloneOSX,
        StandaloneLinux64,
        Android,
        iOS,
        WebGL
    }
    
    [Header("Build Settings")]
    [SerializeField] private BuildSettings buildSettings;
    [SerializeField] private bool autoIncrementVersion = true;
    [SerializeField] private bool createBuildFolders = true;
    
    [Header("Build Info")]
    [SerializeField] private string buildDate;
    [SerializeField] private string buildNumber;
    [SerializeField] private string buildHash;
    
    // Build state
    private bool isBuilding = false;
    private float buildProgress = 0f;
    private string buildStatus = "Ready";
    
    // Singleton instance
    public static BuildManager Instance { get; private set; }
    
    // Events
    public System.Action OnBuildStarted;
    public System.Action OnBuildCompleted;
    public System.Action OnBuildFailed;
    public System.Action<float> OnBuildProgress;
    
    // Properties
    public bool IsBuilding => isBuilding;
    public float BuildProgress => buildProgress;
    public string BuildStatus => buildStatus;
    public BuildSettings CurrentBuildSettings => buildSettings;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBuildManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize build manager
    /// </summary>
    private void InitializeBuildManager()
    {
        // Load build settings
        LoadBuildSettings();
        
        // Generate build info
        GenerateBuildInfo();
        
        Debug.Log("Build manager initialized");
    }
    
    /// <summary>
    /// Load build settings
    /// </summary>
    private void LoadBuildSettings()
    {
        if (buildSettings == null)
        {
            buildSettings = new BuildSettings();
        }
        
        // Load from PlayerPrefs if available
        buildSettings.buildName = PlayerPrefs.GetString("BuildName", buildSettings.buildName);
        buildSettings.version = PlayerPrefs.GetString("BuildVersion", buildSettings.version);
        buildSettings.companyName = PlayerPrefs.GetString("CompanyName", buildSettings.companyName);
        buildSettings.productName = PlayerPrefs.GetString("ProductName", buildSettings.productName);
    }
    
    /// <summary>
    /// Generate build info
    /// </summary>
    private void GenerateBuildInfo()
    {
        buildDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        buildNumber = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
        buildHash = System.Guid.NewGuid().ToString().Substring(0, 8);
    }
    
    /// <summary>
    /// Start build process
    /// </summary>
    public void StartBuild()
    {
        if (isBuilding)
        {
            Debug.LogWarning("Build already in progress!");
            return;
        }
        
        isBuilding = true;
        buildProgress = 0f;
        buildStatus = "Starting build...";
        
        OnBuildStarted?.Invoke();
        
        Debug.Log("Starting build process...");
        
        // Start build coroutine
        StartCoroutine(BuildProcess());
    }
    
    /// <summary>
    /// Build process coroutine
    /// </summary>
    private System.Collections.IEnumerator BuildProcess()
    {
        // Step 1: Validate build settings
        buildStatus = "Validating build settings...";
        buildProgress = 0.1f;
        OnBuildProgress?.Invoke(buildProgress);
        yield return new WaitForSeconds(0.5f);
        
        if (!ValidateBuildSettings())
        {
            BuildFailed("Invalid build settings");
            yield break;
        }
        
        // Step 2: Prepare build environment
        buildStatus = "Preparing build environment...";
        buildProgress = 0.2f;
        OnBuildProgress?.Invoke(buildProgress);
        yield return new WaitForSeconds(0.5f);
        
        if (!PrepareBuildEnvironment())
        {
            BuildFailed("Failed to prepare build environment");
            yield break;
        }
        
        // Step 3: Build the game
        buildStatus = "Building game...";
        buildProgress = 0.3f;
        OnBuildProgress?.Invoke(buildProgress);
        yield return new WaitForSeconds(0.5f);
        
        if (!BuildGame())
        {
            BuildFailed("Build failed");
            yield break;
        }
        
        // Step 4: Post-build processing
        buildStatus = "Post-processing build...";
        buildProgress = 0.8f;
        OnBuildProgress?.Invoke(buildProgress);
        yield return new WaitForSeconds(0.5f);
        
        if (!PostProcessBuild())
        {
            BuildFailed("Post-processing failed");
            yield break;
        }
        
        // Step 5: Complete build
        buildStatus = "Build completed successfully!";
        buildProgress = 1f;
        OnBuildProgress?.Invoke(buildProgress);
        
        BuildCompleted();
    }
    
    /// <summary>
    /// Validate build settings
    /// </summary>
    private bool ValidateBuildSettings()
    {
        if (string.IsNullOrEmpty(buildSettings.buildName))
        {
            Debug.LogError("Build name is required");
            return false;
        }
        
        if (string.IsNullOrEmpty(buildSettings.version))
        {
            Debug.LogError("Build version is required");
            return false;
        }
        
        if (buildSettings.scenesToInclude == null || buildSettings.scenesToInclude.Length == 0)
        {
            Debug.LogError("No scenes to include in build");
            return false;
        }
        
        Debug.Log("Build settings validated successfully");
        return true;
    }
    
    /// <summary>
    /// Prepare build environment
    /// </summary>
    private bool PrepareBuildEnvironment()
    {
        try
        {
            // Create output directory
            if (createBuildFolders && !Directory.Exists(buildSettings.outputPath))
            {
                Directory.CreateDirectory(buildSettings.outputPath);
            }
            
            // Set player settings
            PlayerSettings.companyName = buildSettings.companyName;
            PlayerSettings.productName = buildSettings.productName;
            PlayerSettings.bundleVersion = buildSettings.version;
            
            // Set build target
            EditorUserBuildSettings.selectedBuildTargetGroup = GetBuildTargetGroup(buildSettings.buildTarget);
            
            Debug.Log("Build environment prepared successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to prepare build environment: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Build the game
    /// </summary>
    private bool BuildGame()
    {
        try
        {
            // Note: This would use Unity's BuildPipeline in a real implementation
            // For now, we'll simulate the build process
            
            Debug.Log($"Building {buildSettings.buildName} v{buildSettings.version}");
            Debug.Log($"Target: {buildSettings.buildTarget}");
            Debug.Log($"Development Build: {buildSettings.developmentBuild}");
            Debug.Log($"Debug Build: {buildSettings.debugBuild}");
            
            // Simulate build time
            System.Threading.Thread.Sleep(2000);
            
            Debug.Log("Game built successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Build failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Post-process build
    /// </summary>
    private bool PostProcessBuild()
    {
        try
        {
            // Create build info file
            CreateBuildInfoFile();
            
            // Copy additional files if needed
            CopyAdditionalFiles();
            
            Debug.Log("Post-processing completed successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Post-processing failed: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Create build info file
    /// </summary>
    private void CreateBuildInfoFile()
    {
        string buildInfo = $"Build Information\n" +
                          $"================\n" +
                          $"Build Name: {buildSettings.buildName}\n" +
                          $"Version: {buildSettings.version}\n" +
                          $"Build Date: {buildDate}\n" +
                          $"Build Number: {buildNumber}\n" +
                          $"Build Hash: {buildHash}\n" +
                          $"Target Platform: {buildSettings.buildTarget}\n" +
                          $"Development Build: {buildSettings.developmentBuild}\n" +
                          $"Debug Build: {buildSettings.debugBuild}\n";
        
        string buildInfoPath = Path.Combine(buildSettings.outputPath, "build_info.txt");
        File.WriteAllText(buildInfoPath, buildInfo);
        
        Debug.Log($"Build info file created: {buildInfoPath}");
    }
    
    /// <summary>
    /// Copy additional files
    /// </summary>
    private void CopyAdditionalFiles()
    {
        // Copy README, license, etc.
        string[] additionalFiles = { "README.md", "LICENSE.txt" };
        
        foreach (string file in additionalFiles)
        {
            if (File.Exists(file))
            {
                string destPath = Path.Combine(buildSettings.outputPath, file);
                File.Copy(file, destPath, true);
                Debug.Log($"Copied {file} to build");
            }
        }
    }
    
    /// <summary>
    /// Get build target group
    /// </summary>
    private BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneLinux64:
                return BuildTargetGroup.Standalone;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;
            default:
                return BuildTargetGroup.Standalone;
        }
    }
    
    /// <summary>
    /// Build completed successfully
    /// </summary>
    private void BuildCompleted()
    {
        isBuilding = false;
        buildStatus = "Build completed successfully!";
        
        // Save build settings
        SaveBuildSettings();
        
        // Increment version if enabled
        if (autoIncrementVersion)
        {
            IncrementVersion();
        }
        
        OnBuildCompleted?.Invoke();
        
        Debug.Log($"Build completed: {buildSettings.buildName} v{buildSettings.version}");
    }
    
    /// <summary>
    /// Build failed
    /// </summary>
    private void BuildFailed(string error)
    {
        isBuilding = false;
        buildStatus = $"Build failed: {error}";
        
        OnBuildFailed?.Invoke();
        
        Debug.LogError($"Build failed: {error}");
    }
    
    /// <summary>
    /// Save build settings
    /// </summary>
    private void SaveBuildSettings()
    {
        PlayerPrefs.SetString("BuildName", buildSettings.buildName);
        PlayerPrefs.SetString("BuildVersion", buildSettings.version);
        PlayerPrefs.SetString("CompanyName", buildSettings.companyName);
        PlayerPrefs.SetString("ProductName", buildSettings.productName);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Increment version number
    /// </summary>
    private void IncrementVersion()
    {
        string[] versionParts = buildSettings.version.Split('.');
        if (versionParts.Length >= 3)
        {
            int patch = int.Parse(versionParts[2]) + 1;
            buildSettings.version = $"{versionParts[0]}.{versionParts[1]}.{patch}";
            Debug.Log($"Version incremented to {buildSettings.version}");
        }
    }
    
    /// <summary>
    /// Get build info
    /// </summary>
    public string GetBuildInfo()
    {
        return $"Build Information:\n" +
               $"Name: {buildSettings.buildName}\n" +
               $"Version: {buildSettings.version}\n" +
               $"Company: {buildSettings.companyName}\n" +
               $"Product: {buildSettings.productName}\n" +
               $"Target: {buildSettings.buildTarget}\n" +
               $"Build Date: {buildDate}\n" +
               $"Build Number: {buildNumber}\n" +
               $"Build Hash: {buildHash}\n" +
               $"Status: {buildStatus}\n" +
               $"Progress: {buildProgress:P0}";
    }
    
    /// <summary>
    /// Set build settings
    /// </summary>
    public void SetBuildSettings(BuildSettings settings)
    {
        buildSettings = settings;
        SaveBuildSettings();
        Debug.Log("Build settings updated");
    }
    
    /// <summary>
    /// Get available build targets
    /// </summary>
    public BuildTarget[] GetAvailableBuildTargets()
    {
        return (BuildTarget[])System.Enum.GetValues(typeof(BuildTarget));
    }
    
    /// <summary>
    /// Get build target name
    /// </summary>
    public string GetBuildTargetName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                return "Windows 64-bit";
            case BuildTarget.StandaloneOSX:
                return "macOS";
            case BuildTarget.StandaloneLinux64:
                return "Linux 64-bit";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            default:
                return target.ToString();
        }
    }
    
    /// <summary>
    /// Cancel build
    /// </summary>
    public void CancelBuild()
    {
        if (isBuilding)
        {
            isBuilding = false;
            buildStatus = "Build cancelled";
            Debug.Log("Build cancelled by user");
        }
    }
    
    /// <summary>
    /// Clean build directory
    /// </summary>
    public void CleanBuildDirectory()
    {
        if (Directory.Exists(buildSettings.outputPath))
        {
            Directory.Delete(buildSettings.outputPath, true);
            Debug.Log("Build directory cleaned");
        }
    }
    
    /// <summary>
    /// Open build directory
    /// </summary>
    public void OpenBuildDirectory()
    {
        if (Directory.Exists(buildSettings.outputPath))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(buildSettings.outputPath);
            #else
                System.Diagnostics.Process.Start(buildSettings.outputPath);
            #endif
        }
    }
}

// Note: These classes would be in UnityEditor namespace in a real implementation
public static class PlayerSettings
{
    public static string companyName { get; set; }
    public static string productName { get; set; }
    public static string bundleVersion { get; set; }
}

public static class EditorUserBuildSettings
{
    public static BuildTargetGroup selectedBuildTargetGroup { get; set; }
}

public enum BuildTargetGroup
{
    Standalone,
    Android,
    iOS,
    WebGL
} 