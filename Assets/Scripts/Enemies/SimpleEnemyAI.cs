using UnityEngine;
using System.Collections;

/// <summary>
/// Simple enemy AI that doesn't require NavMesh.
/// Provides basic movement, detection, and attack behavior.
/// </summary>
[RequireComponent(typeof(Health))]
public class SimpleEnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 20;
    
    [Header("Patrol Settings")]
    [SerializeField] private bool canPatrol = true;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float patrolWaitTime = 2f;
    
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] public bool isRangedEnemy = false;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private AudioClip[] alertSounds;
    
    // Components
    private Health health;
    private Transform player;
    
    // AI State
    private EnemyAIState currentState = EnemyAIState.Patrol;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float lastAttackTime;
    private bool isDead = false;
    
    // Events
    public System.Action<SimpleEnemyAI> OnEnemyDeath;
    public System.Action<SimpleEnemyAI> OnEnemyAlerted;
    
    // Properties
    public EnemyAIState CurrentState => currentState;
    public bool IsDead => isDead;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    
    private void Awake()
    {
        health = GetComponent<Health>();
        startPosition = transform.position;
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
            health.OnHealthChanged.AddListener(OnHealthChanged);
        }
        else
        {
            Debug.LogWarning($"SimpleEnemyAI: Health component not found on {gameObject.name}. Adding one.");
            health = gameObject.AddComponent<Health>();
            health.SetMaxHealth(100);
        }
    }
    
    private void Start()
    {
        // Set initial patrol target
        SetNewPatrolTarget();
    }
    
    private void Update()
    {
        if (isDead || player == null) return;
        
        UpdateAI();
    }
    
    /// <summary>
    /// Main AI update loop
    /// </summary>
    private void UpdateAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is detected
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != EnemyAIState.Chase && currentState != EnemyAIState.Attack)
            {
                SetState(EnemyAIState.Chase);
                OnEnemyAlerted?.Invoke(this);
                PlayAlertSound();
            }
        }
        else
        {
            if (currentState == EnemyAIState.Chase || currentState == EnemyAIState.Attack)
            {
                SetState(EnemyAIState.Patrol);
            }
        }
        
        // Update current state
        switch (currentState)
        {
            case EnemyAIState.Patrol:
                UpdatePatrol();
                break;
            case EnemyAIState.Chase:
                UpdateChase();
                break;
            case EnemyAIState.Attack:
                UpdateAttack();
                break;
        }
    }
    
    /// <summary>
    /// Update patrol behavior
    /// </summary>
    private void UpdatePatrol()
    {
        if (!canPatrol) return;
        
        // Move towards patrol target
        Vector3 direction = (patrolTarget - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            // Rotate towards target
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            
            // Move forward
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        
        // Check if reached patrol target
        if (Vector3.Distance(transform.position, patrolTarget) < 1f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }
    
    /// <summary>
    /// Update chase behavior
    /// </summary>
    private void UpdateChase()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            SetState(EnemyAIState.Attack);
        }
        else
        {
            // Move towards player
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            
            // Move forward
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Update attack behavior
    /// </summary>
    private void UpdateAttack()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange)
        {
            SetState(EnemyAIState.Chase);
            return;
        }
        
        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        
        // Attack if cooldown is ready
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }
    }
    
    /// <summary>
    /// Perform attack action
    /// </summary>
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // Play attack sound
        PlayAttackSound();
        
        // Perform attack based on enemy type
        if (isRangedEnemy)
        {
            PerformRangedAttack();
        }
        else
        {
            PerformMeleeAttack();
        }
    }
    
    /// <summary>
    /// Perform melee attack
    /// </summary>
    private void PerformMeleeAttack()
    {
        if (attackPoint == null) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, 1.5f, playerLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            Health playerHealth = hitCollider.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Enemy hit player for {damage} damage!");
            }
        }
    }
    
    /// <summary>
    /// Perform ranged attack
    /// </summary>
    private void PerformRangedAttack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;
        
        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        
        if (projectileComponent != null)
        {
            projectileComponent.SetDamage(damage);
            projectileComponent.Initialize(damage, playerLayer);
        }
        
        Debug.Log("Enemy fired projectile!");
    }
    
    /// <summary>
    /// Set new patrol target
    /// </summary>
    private void SetNewPatrolTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        patrolTarget = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
    
    /// <summary>
    /// Wait at patrol point
    /// </summary>
    private IEnumerator WaitAtPatrolPoint()
    {
        yield return new WaitForSeconds(patrolWaitTime);
        SetNewPatrolTarget();
    }
    
    /// <summary>
    /// Set AI state
    /// </summary>
    private void SetState(EnemyAIState newState)
    {
        currentState = newState;
        Debug.Log($"Enemy {gameObject.name} state changed to: {newState}");
    }
    
    /// <summary>
    /// Handle health changes
    /// </summary>
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"Enemy {gameObject.name} health: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Handle death
    /// </summary>
    private void OnDeath()
    {
        isDead = true;
        
        // Play death effects
        if (deathEffect != null)
        {
            deathEffect.Play();
        }
        
        PlayDeathSound();
        
        // Trigger death event
        OnEnemyDeath?.Invoke(this);
        
        // Add score
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddKillScore(null);
        }
        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay(2f));
    }
    
    /// <summary>
    /// Play attack sound
    /// </summary>
    private void PlayAttackSound()
    {
        if (audioSource != null && attackSounds.Length > 0)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    /// <summary>
    /// Play death sound
    /// </summary>
    private void PlayDeathSound()
    {
        if (audioSource != null && deathSounds.Length > 0)
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    /// <summary>
    /// Play alert sound
    /// </summary>
    private void PlayAlertSound()
    {
        if (audioSource != null && alertSounds.Length > 0)
        {
            AudioClip clip = alertSounds[Random.Range(0, alertSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    /// <summary>
    /// Destroy enemy after delay
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Draw gizmos for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Patrol radius
        if (canPatrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, patrolRadius);
        }
        
        // Attack point
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 1.5f);
        }
    }
}

/// <summary>
/// Enemy AI states
/// </summary>
public enum EnemyAIState
{
    Patrol,
    Chase,
    Attack
} 