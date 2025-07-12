using UnityEngine;

/// <summary>
/// Enhanced weapon controller with realistic shooting mechanics.
/// Features: bullet drop, penetration, weapon types, realistic recoil.
/// </summary>
public class SimpleWeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private string weaponName = "Assault Rifle";
    [SerializeField] private WeaponType weaponType = WeaponType.AssaultRifle;
    [SerializeField] private int damage = 25;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float range = 100f;
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int maxAmmo = 300;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float hipSpread = 0.05f;
    [SerializeField] private float aimedSpread = 0.01f;
    
    [Header("Realistic Physics")]
    [SerializeField] private float bulletVelocity = 800f;
    [SerializeField] private float bulletDrop = 9.8f;
    [SerializeField] private int penetrationPower = 1;
    [SerializeField] private float penetrationDamageReduction = 0.3f;
    
    [Header("Recoil Settings")]
    [SerializeField] private Vector3 recoilPattern = new Vector3(1f, 2f, 0f);
    [SerializeField] private float recoilRandomness = 0.5f;
    [SerializeField] private float recoilRecoveryTime = 0.5f;
    [SerializeField] private float recoilRecoverySpeed = 5f;
    [SerializeField] private float recoilSmoothness = 10f;
    [SerializeField] private bool hasRecoil = true;
    
    [Header("Weapon Effects")]
    [SerializeField] private ParticleSystem muzzleFlashEffect;
    [SerializeField] private AudioSource weaponAudio;
    [SerializeField] private AudioClip[] shootSounds;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject bulletHolePrefab;
    
    [Header("UI")]
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject aimReticle;
    
    [Header("Weapon Model")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform muzzleFlash;
    [SerializeField] private LayerMask shootableLayers = -1;
    
    // Weapon state
    private int currentAmmo;
    private int totalAmmo;
    private bool isReloading;
    private bool isAiming;
    private float lastShootTime;
    private float recoilRecoveryTimer;
    private bool isAutomatic;
    
    // Recoil variables
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private Vector3 recoilVelocity;
    
    // Components
    private SimplePlayerController playerController;
    private Camera playerCamera;
    
    // Events
    public System.Action<int, int> OnAmmoChanged;
    public System.Action OnWeaponFired;
    public System.Action OnWeaponReloaded;
    
    // Properties
    public string WeaponName => weaponName;
    public WeaponType WeaponType => weaponType;
    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;
    public bool IsReloading => isReloading;
    public bool IsAiming => isAiming;
    public bool CanShoot => !isReloading && currentAmmo > 0 && Time.time >= lastShootTime + fireRate;
    public int MagazineSize => magazineSize;
    
    private void Awake()
    {
        playerController = GetComponent<SimplePlayerController>();
        if (playerController != null)
        {
            playerCamera = playerController.GetCameraTransform().GetComponent<Camera>();
        }
        
        // Set weapon type properties
        SetWeaponTypeProperties();
        
        // Initialize weapon
        InitializeWeapon();
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused)
            return;
            
        HandleRecoil();
        HandleAiming();
        HandleInput();
    }
    
    /// <summary>
    /// Set properties based on weapon type
    /// </summary>
    private void SetWeaponTypeProperties()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                isAutomatic = false;
                fireRate = 0.3f;
                damage = 35;
                range = 50f;
                hipSpread = 0.08f;
                aimedSpread = 0.02f;
                recoilPattern = new Vector3(0.5f, 1f, 0f);
                break;
                
            case WeaponType.AssaultRifle:
                isAutomatic = true;
                fireRate = 0.1f;
                damage = 25;
                range = 100f;
                hipSpread = 0.05f;
                aimedSpread = 0.01f;
                recoilPattern = new Vector3(1f, 2f, 0f);
                break;
                
            case WeaponType.SniperRifle:
                isAutomatic = false;
                fireRate = 1.5f;
                damage = 100;
                range = 200f;
                hipSpread = 0.15f;
                aimedSpread = 0.001f;
                recoilPattern = new Vector3(0.2f, 3f, 0f);
                break;
                
            case WeaponType.Shotgun:
                isAutomatic = false;
                fireRate = 0.8f;
                damage = 15;
                range = 30f;
                hipSpread = 0.2f;
                aimedSpread = 0.1f;
                recoilPattern = new Vector3(2f, 4f, 0f);
                break;
        }
    }
    
    /// <summary>
    /// Handle input using legacy Input Manager
    /// </summary>
    private void HandleInput()
    {
        // Shoot
        if (isAutomatic)
        {
            if (Input.GetMouseButton(0) && CanShoot)
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && CanShoot)
            {
                Shoot();
            }
        }
        
        // Aim
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
        }
        
        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartReload();
        }
    }
    
    /// <summary>
    /// Initialize weapon with default values
    /// </summary>
    private void InitializeWeapon()
    {
        currentAmmo = magazineSize;
        totalAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        Debug.Log($"Weapon initialized: {weaponName} ({weaponType}) - Ammo: {currentAmmo}/{totalAmmo}");
    }
    
    /// <summary>
    /// Handle weapon recoil
    /// </summary>
    private void HandleRecoil()
    {
        if (!hasRecoil) return;
        
        // Apply recoil to camera
        if (playerCamera != null)
        {
            Vector3 recoilRotation = new Vector3(-currentRecoil.y, currentRecoil.x, 0f);
            playerCamera.transform.localRotation = Quaternion.Lerp(
                playerCamera.transform.localRotation,
                Quaternion.Euler(recoilRotation),
                recoilSmoothness * Time.deltaTime
            );
        }
        
        // Recover from recoil
        if (Time.time > recoilRecoveryTimer)
        {
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Handle aiming mechanics
    /// </summary>
    private void HandleAiming()
    {
        if (crosshair != null)
        {
            crosshair.SetActive(!isAiming);
        }
        
        if (aimReticle != null)
        {
            aimReticle.SetActive(isAiming);
        }
    }
    
    /// <summary>
    /// Perform weapon shot with realistic physics
    /// </summary>
    private void Shoot()
    {
        // Reduce ammo
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        
        // Update last shoot time
        lastShootTime = Time.time;
        
        // Apply recoil
        if (hasRecoil)
        {
            ApplyRecoil();
        }
        
        // Play effects
        PlayShootEffects();
        
        // Perform realistic shot
        if (weaponType == WeaponType.Shotgun)
        {
            PerformShotgunShot();
        }
        else
        {
            PerformSingleShot();
        }
        
        // Trigger events
        OnWeaponFired?.Invoke();
        
        // Auto reload if empty
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
    
    /// <summary>
    /// Perform single shot with bullet drop and penetration
    /// </summary>
    private void PerformSingleShot()
    {
        if (playerCamera == null) return;
        
        Vector3 shootOrigin = playerCamera.transform.position;
        Vector3 shootDirection = playerCamera.transform.forward;
        
        // Add spread based on weapon accuracy
        float spread = isAiming ? aimedSpread : hipSpread;
        shootDirection += Random.insideUnitSphere * spread;
        shootDirection.Normalize();
        
        // Simulate bullet trajectory with drop
        SimulateBulletTrajectory(shootOrigin, shootDirection);
    }
    
    /// <summary>
    /// Perform shotgun shot with multiple pellets
    /// </summary>
    private void PerformShotgunShot()
    {
        if (playerCamera == null) return;
        
        Vector3 shootOrigin = playerCamera.transform.position;
        Vector3 shootDirection = playerCamera.transform.forward;
        
        // Fire multiple pellets
        int pelletCount = 8;
        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 pelletDirection = shootDirection + Random.insideUnitSphere * 0.3f;
            pelletDirection.Normalize();
            
            SimulateBulletTrajectory(shootOrigin, pelletDirection);
        }
    }
    
    /// <summary>
    /// Simulate realistic bullet trajectory with drop and penetration
    /// </summary>
    private void SimulateBulletTrajectory(Vector3 origin, Vector3 direction)
    {
        Vector3 currentPosition = origin;
        Vector3 currentVelocity = direction * bulletVelocity;
        float timeStep = 0.01f;
        int maxSteps = 100;
        
        for (int step = 0; step < maxSteps; step++)
        {
            // Apply gravity
            currentVelocity.y -= bulletDrop * timeStep;
            
            // Update position
            Vector3 newPosition = currentPosition + currentVelocity * timeStep;
            
            // Check for hits
            RaycastHit hit;
            if (Physics.Linecast(currentPosition, newPosition, out hit, shootableLayers))
            {
                HandleHit(hit, currentVelocity, step * timeStep);
                break;
            }
            
            currentPosition = newPosition;
            
            // Stop if bullet has traveled too far
            if (Vector3.Distance(origin, currentPosition) > range)
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// Handle hit detection with penetration
    /// </summary>
    private void HandleHit(RaycastHit hit, Vector3 bulletVelocity, float travelTime)
    {
        // Calculate damage based on distance and penetration
        float distance = Vector3.Distance(transform.position, hit.point);
        float damageMultiplier = Mathf.Clamp01(1f - (distance / range));
        int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);
        
        // Check if hit object has health component
        Health health = hit.collider.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(finalDamage);
            
            // Add score if enemy killed
            if (health.CurrentHealth <= 0)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    GameManager.Instance?.AddScore(enemy.ScoreValue);
                }
            }
        }
        
        // Spawn impact effects
        SpawnImpactEffect(hit, bulletVelocity);
        
        // Handle penetration
        if (penetrationPower > 0)
        {
            HandlePenetration(hit, bulletVelocity, finalDamage);
        }
    }
    
    /// <summary>
    /// Handle bullet penetration through materials
    /// </summary>
    private void HandlePenetration(RaycastHit hit, Vector3 bulletVelocity, int originalDamage)
    {
        Vector3 penetrationDirection = Vector3.Reflect(bulletVelocity.normalized, hit.normal);
        
        // Reduce damage for penetrated bullets
        int penetrationDamage = Mathf.RoundToInt(originalDamage * (1f - penetrationDamageReduction));
        
        RaycastHit penetrationHit;
        if (Physics.Raycast(hit.point + hit.normal * 0.1f, penetrationDirection, out penetrationHit, 5f, shootableLayers))
        {
            Health health = penetrationHit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(penetrationDamage);
            }
            
            SpawnImpactEffect(penetrationHit, penetrationDirection);
        }
    }
    
    /// <summary>
    /// Apply recoil to the weapon
    /// </summary>
    private void ApplyRecoil()
    {
        Vector3 recoilAmount = recoilPattern;
        
        // Add randomness to recoil
        recoilAmount += new Vector3(
            Random.Range(-recoilRandomness, recoilRandomness),
            Random.Range(-recoilRandomness, recoilRandomness),
            0f
        );
        
        currentRecoil += recoilAmount;
        recoilRecoveryTimer = Time.time + recoilRecoveryTime;
    }
    
    /// <summary>
    /// Play shoot effects (muzzle flash, sound, etc.)
    /// </summary>
    private void PlayShootEffects()
    {
        // Muzzle flash
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.Play();
        }
        
        // Sound
        if (weaponAudio != null && shootSounds.Length > 0)
        {
            AudioClip randomSound = shootSounds[Random.Range(0, shootSounds.Length)];
            weaponAudio.PlayOneShot(randomSound);
        }
    }
    
    /// <summary>
    /// Spawn impact effect at hit point
    /// </summary>
    private void SpawnImpactEffect(RaycastHit hit, Vector3 bulletVelocity)
    {
        // Create impact effect based on surface type
        if (impactEffect != null)
        {
            GameObject spawnedEffect = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(spawnedEffect, 2f);
        }
        
        // Create bullet hole
        if (bulletHolePrefab != null)
        {
            GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            Destroy(bulletHole, 10f);
        }
    }
    
    /// <summary>
    /// Start reload process
    /// </summary>
    private void StartReload()
    {
        if (isReloading || totalAmmo <= 0) return;
        
        isReloading = true;
        
        // Play reload sound
        if (weaponAudio != null && reloadSound != null)
        {
            weaponAudio.PlayOneShot(reloadSound);
        }
        
        // Invoke reload completion after delay
        Invoke(nameof(CompleteReload), reloadTime);
    }
    
    /// <summary>
    /// Complete reload process
    /// </summary>
    private void CompleteReload()
    {
        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToAdd = Mathf.Min(ammoNeeded, totalAmmo);
        
        currentAmmo += ammoToAdd;
        totalAmmo -= ammoToAdd;
        
        isReloading = false;
        
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        OnWeaponReloaded?.Invoke();
    }
    
    /// <summary>
    /// Switch to a different weapon (simplified version)
    /// </summary>
    public void SwitchWeapon(string newWeaponName, WeaponType newWeaponType)
    {
        weaponName = newWeaponName;
        weaponType = newWeaponType;
        SetWeaponTypeProperties();
        InitializeWeapon();
        Debug.Log($"Switched to weapon: {weaponName} ({weaponType})");
    }
    
    /// <summary>
    /// Add ammo to total ammo
    /// </summary>
    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
    }
    
    /// <summary>
    /// Get weapon accuracy based on current state
    /// </summary>
    public float GetCurrentAccuracy()
    {
        return isAiming ? aimedSpread : hipSpread;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            // Draw weapon range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerCamera.transform.position, range);
        }
    }
}

/// <summary>
/// Weapon types with different characteristics
/// </summary>
public enum WeaponType
{
    Pistol,
    AssaultRifle,
    SniperRifle,
    Shotgun
} 