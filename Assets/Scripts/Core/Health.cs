using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health system component for handling damage, healing, and death mechanics.
/// Can be used for both player and enemies.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    [Header("Regeneration")]
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenerationRate = 5f;
    [SerializeField] private float regenerationDelay = 3f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private ParticleSystem healEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip deathSound;
    
    [Header("UI")]
    [SerializeField] private GameObject healthBar;
    // Note: UI Slider removed to avoid package dependencies
    
    // Events
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent<int> OnHealed;
    public UnityEvent OnDeath;
    public UnityEvent OnInvulnerabilityStart;
    public UnityEvent OnInvulnerabilityEnd;
    
    // Properties
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvulnerable => isInvulnerable;
    public bool CanRegenerate => canRegenerate;
    
    // Private variables
    private float lastDamageTime;
    private float invulnerabilityTimer;
    private float regenerationTimer;
    
    private void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Note: UI setup removed to avoid package dependencies
    }
    
    private void Start()
    {
        // Trigger initial health event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Update()
    {
        HandleInvulnerability();
        HandleRegeneration();
    }
    
    /// <summary>
    /// Handle invulnerability timer
    /// </summary>
    private void HandleInvulnerability()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            
            if (invulnerabilityTimer <= 0f)
            {
                SetInvulnerable(false);
            }
        }
    }
    
    /// <summary>
    /// Handle health regeneration
    /// </summary>
    private void HandleRegeneration()
    {
        if (!canRegenerate || IsDead) return;
        
        if (Time.time - lastDamageTime > regenerationDelay)
        {
            regenerationTimer += Time.deltaTime;
            
            if (regenerationTimer >= 1f / regenerationRate)
            {
                Heal(1);
                regenerationTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Take damage
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead || isInvulnerable) return;
        
        // Apply damage
        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Update last damage time for regeneration
        lastDamageTime = Time.time;
        regenerationTimer = 0f;
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke(damage);
        
        // Play effects
        PlayDamageEffects();
        
        // Update UI
        UpdateHealthUI();
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Set invulnerability if damage was taken
            if (oldHealth > currentHealth)
            {
                SetInvulnerable(true);
            }
        }
    }
    
    /// <summary>
    /// Heal the entity
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHealed?.Invoke(amount);
        
        // Play effects
        PlayHealEffects();
        
        // Update UI
        UpdateHealthUI();
    }
    
    /// <summary>
    /// Set maximum health
    /// </summary>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // Note: UI update removed to avoid package dependencies
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Set current health
    /// </summary>
    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Update UI
        UpdateHealthUI();
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Set invulnerability state
    /// </summary>
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
        
        if (invulnerable)
        {
            invulnerabilityTimer = invulnerabilityTime;
            OnInvulnerabilityStart?.Invoke();
        }
        else
        {
            OnInvulnerabilityEnd?.Invoke();
        }
    }
    
    /// <summary>
    /// Enable or disable regeneration
    /// </summary>
    public void SetRegeneration(bool enabled)
    {
        canRegenerate = enabled;
    }
    
    /// <summary>
    /// Handle death
    /// </summary>
    private void Die()
    {
        // Play death effects
        PlayDeathEffects();
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Disable this component
        enabled = false;
    }
    
    /// <summary>
    /// Play damage effects
    /// </summary>
    private void PlayDamageEffects()
    {
        // Particle effect
        if (damageEffect != null)
        {
            damageEffect.Play();
        }
        
        // Sound effect
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Visual feedback (screen shake, etc.)
        // This could be expanded based on the entity type
    }
    
    /// <summary>
    /// Play heal effects
    /// </summary>
    private void PlayHealEffects()
    {
        // Particle effect
        if (healEffect != null)
        {
            healEffect.Play();
        }
        
        // Sound effect
        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }
    }
    
    /// <summary>
    /// Play death effects
    /// </summary>
    private void PlayDeathEffects()
    {
        // Sound effect
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Additional death effects could be added here
    }
    
    /// <summary>
    /// Update health UI
    /// </summary>
    private void UpdateHealthUI()
    {
        // Note: UI Slider update removed to avoid package dependencies
        
        if (healthBar != null)
        {
            // Scale health bar based on health percentage
            Vector3 scale = healthBar.transform.localScale;
            scale.x = HealthPercentage;
            healthBar.transform.localScale = scale;
        }
    }
    
    /// <summary>
    /// Get health as percentage (0-1)
    /// </summary>
    public float GetHealthPercentage()
    {
        return HealthPercentage;
    }
    
    /// <summary>
    /// Check if health is below a certain percentage
    /// </summary>
    public bool IsHealthBelow(float percentage)
    {
        return HealthPercentage < percentage;
    }
    
    /// <summary>
    /// Check if health is above a certain percentage
    /// </summary>
    public bool IsHealthAbove(float percentage)
    {
        return HealthPercentage > percentage;
    }
    
    /// <summary>
    /// Get remaining health
    /// </summary>
    public int GetRemainingHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Get missing health
    /// </summary>
    public int GetMissingHealth()
    {
        return maxHealth - currentHealth;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw health bar in scene view
        if (healthBar != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(healthBar.transform.position, healthBar.transform.localScale);
        }
    }
} 