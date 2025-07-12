using UnityEngine;
using UnityEditor;

/// <summary>
/// Helper script to install required packages for Unity 6.
/// Run this once to set up all necessary packages.
/// </summary>
public class PackageInstaller : MonoBehaviour
{
    [Header("Required Packages")]
    [SerializeField] private bool installInputSystem = true;
    [SerializeField] private bool installTextMeshPro = true;
    [SerializeField] private bool installUIToolkit = true;
    
    [Header("Installation")]
    [SerializeField] private KeyCode installKey = KeyCode.F2;
    
    private void Update()
    {
        if (Input.GetKeyDown(installKey))
        {
            InstallPackages();
        }
    }
    
    /// <summary>
    /// Install all required packages
    /// </summary>
    [ContextMenu("Install Required Packages")]
    public void InstallPackages()
    {
        Debug.Log("Installing required packages...");
        
        #if UNITY_EDITOR
        // Install Input System
        if (installInputSystem)
        {
            InstallPackage("com.unity.inputsystem");
        }
        
        // Install TextMeshPro
        if (installTextMeshPro)
        {
            InstallPackage("com.unity.textmeshpro");
        }
        
        // Install UI Toolkit
        if (installUIToolkit)
        {
            InstallPackage("com.unity.ui");
        }
        
        Debug.Log("Package installation complete! Please restart Unity if prompted.");
        #else
        Debug.Log("Package installation can only be done in the Unity Editor.");
        #endif
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Install a specific package
    /// </summary>
    private void InstallPackage(string packageName)
    {
        try
        {
            UnityEditor.PackageManager.Client.Add(packageName);
            Debug.Log($"Installing {packageName}...");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not install {packageName}: {e.Message}");
        }
    }
    #endif
} 