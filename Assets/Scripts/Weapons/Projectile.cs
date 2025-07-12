using UnityEngine;

/// <summary>
/// Projectile component for ranged weapons and enemy attacks.
/// Handles movement, collision detection, and damage application.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 25;
    [SerializeField] private LayerMask targetLayers = -1;
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravity = 9.81f;
    
    [Header("Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private AudioClip flybySound;
    
    [Header("Penetration")]
    [SerializeField] private bool canPenetrate = false;
    [SerializeField] private int maxPenetrations = 1;
    
    // Private variables
    private Vector3 velocity;
    private int currentPenetrations = 0;
    private bool hasHit = false;
    private float spawnTime;
    
    // Events
    public System.Action<Projectile> OnProjectileHit;
    public System.Action<Projectile> OnProjectileDestroyed;
    
    private void Start()
    {
        spawnTime = Time.time;
        
        // Play trail effect
        if (trailEffect != null)
        {
            trailEffect.Play();
        }
    }
    
    private void Update()
    {
        if (hasHit) return;
        
        // Check lifetime
        if (Time.time - spawnTime > lifetime)
        {
            DestroyProjectile();
            return;
        }
        
        // Update position
        UpdateMovement();
    }
    
    /// <summary>
    /// Initialize projectile with custom settings
    /// </summary>
    public void Initialize(int projectileDamage, LayerMask targetLayerMask)
    {
        damage = projectileDamage;
        targetLayers = targetLayerMask;
        
        // Set initial velocity
        velocity = transform.forward * speed;
    }
    
    /// <summary>
    /// Update projectile movement
    /// </summary>
    private void UpdateMovement()
    {
        // Apply gravity if enabled
        if (useGravity)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        
        // Move projectile
        transform.position += velocity * Time.deltaTime;
        
        // Rotate projectile to face movement direction
        if (velocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Check if we hit a valid target
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            HandleHit(other);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        // Check if we hit a valid target
        if (((1 << collision.gameObject.layer) & targetLayers) != 0)
        {
            HandleHit(collision.collider);
        }
        else
        {
            // Hit environment
            HandleEnvironmentHit(collision);
        }
    }
    
    /// <summary>
    /// Handle hitting a target
    /// </summary>
    private void HandleHit(Collider target)
    {
        // Apply damage
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        
        // Play impact effects
        PlayImpactEffects(target.transform.position, target.transform.rotation);
        
        // Trigger events
        OnProjectileHit?.Invoke(this);
        
        // Handle penetration
        if (canPenetrate && currentPenetrations < maxPenetrations)
        {
            currentPenetrations++;
            // Continue flying (don't destroy)
        }
        else
        {
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Handle hitting environment
    /// </summary>
    private void HandleEnvironmentHit(Collision collision)
    {
        // Play impact effects
        PlayImpactEffects(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        
        // Destroy projectile
        DestroyProjectile();
    }
    
    /// <summary>
    /// Play impact effects
    /// </summary>
    private void PlayImpactEffects(Vector3 position, Quaternion rotation)
    {
        // Spawn impact effect
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, position, rotation);
            Destroy(effect, 2f);
        }
        
        // Play impact sound
        if (audioSource != null && impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, position);
        }
    }
    
    /// <summary>
    /// Destroy the projectile
    /// </summary>
    private void DestroyProjectile()
    {
        hasHit = true;
        
        // Stop trail effect
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
        
        // Trigger events
        OnProjectileDestroyed?.Invoke(this);
        
        // Destroy game object
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Set projectile speed
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        velocity = transform.forward * speed;
    }
    
    /// <summary>
    /// Set projectile damage
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// Set projectile lifetime
    /// </summary>
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    /// <summary>
    /// Enable or disable gravity
    /// </summary>
    public void SetUseGravity(bool useGrav)
    {
        useGravity = useGrav;
    }
    
    /// <summary>
    /// Set penetration settings
    /// </summary>
    public void SetPenetration(bool canPen, int maxPen)
    {
        canPenetrate = canPen;
        maxPenetrations = maxPen;
    }
    
    /// <summary>
    /// Get current velocity
    /// </summary>
    public Vector3 GetVelocity()
    {
        return velocity;
    }
    
    /// <summary>
    /// Get remaining lifetime
    /// </summary>
    public float GetRemainingLifetime()
    {
        return lifetime - (Time.time - spawnTime);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw projectile trajectory
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
        
        // Draw lifetime indicator
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
} 