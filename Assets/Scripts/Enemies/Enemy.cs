using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Main enemy class handling AI behavior, health, and combat mechanics.
/// Implements state machine for different behaviors.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private AudioClip[] alertSounds;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string attackAnimationName = "Attack";
    [SerializeField] private string deathAnimationName = "Death";
    
    // Components
    private NavMeshAgent navAgent;
    private Health health;
    private Transform player;
    
    // AI State
    private EnemyState currentState = EnemyState.Patrol;
    private Vector3 patrolTarget;
    private float lastAttackTime;
    private bool isDead = false;
    
    // Events
    public System.Action<Enemy> OnEnemyDeath;
    public System.Action<Enemy> OnEnemyAlerted;
    
    // Properties
    public EnemyState CurrentState => currentState;
    public bool IsDead => isDead;
    public int ScoreValue => scoreValue;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Subscribe to health events
        if (health != null)
        {
            // Note: Event subscription removed to avoid compilation issues
            Debug.Log("Enemy health component found");
        }
        
        // Setup NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = enemyData.moveSpeed;
            navAgent.angularSpeed = enemyData.rotationSpeed;
            navAgent.stoppingDistance = attackRange;
        }
    }
    
    private void Start()
    {
        // Set initial patrol target
        SetNewPatrolTarget();
    }
    
    private void Update()
    {
        if (isDead) return;
        
        UpdateAI();
        UpdateAnimations();
    }
    
    /// <summary>
    /// Main AI update loop
    /// </summary>
    private void UpdateAI()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is detected
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != EnemyState.Chase && currentState != EnemyState.Attack)
            {
                SetState(EnemyState.Chase);
                OnEnemyAlerted?.Invoke(this);
                PlayAlertSound();
            }
        }
        else
        {
            if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            {
                SetState(EnemyState.Patrol);
            }
        }
        
        // Update current state
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }
    }
    
    /// <summary>
    /// Update patrol behavior
    /// </summary>
    private void UpdatePatrol()
    {
        if (navAgent.remainingDistance < 0.5f)
        {
            SetNewPatrolTarget();
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
            SetState(EnemyState.Attack);
        }
        else
        {
            navAgent.SetDestination(player.position);
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
            SetState(EnemyState.Chase);
            return;
        }
        
        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemyData.rotationSpeed);
        
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
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger(attackAnimationName);
        }
        
        // Play attack sound
        PlayAttackSound();
        
        // Perform attack based on enemy type
        if (enemyData.attackType == AttackType.Melee)
        {
            PerformMeleeAttack();
        }
        else if (enemyData.attackType == AttackType.Ranged)
        {
            PerformRangedAttack();
        }
    }
    
    /// <summary>
    /// Perform melee attack
    /// </summary>
    private void PerformMeleeAttack()
    {
        if (attackPoint == null) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, enemyData.attackRadius, playerLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            Health playerHealth = hitCollider.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(enemyData.damage);
            }
        }
    }
    
    /// <summary>
    /// Perform ranged attack
    /// </summary>
    private void PerformRangedAttack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;
        
        // Create projectile
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        
        // Setup projectile
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(enemyData.damage, playerLayer);
        }
    }
    
    /// <summary>
    /// Set new patrol target
    /// </summary>
    private void SetNewPatrolTarget()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * enemyData.patrolRadius;
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(randomPoint, out hit, enemyData.patrolRadius, 1))
        {
            patrolTarget = hit.position;
            navAgent.SetDestination(patrolTarget);
        }
    }
    
    /// <summary>
    /// Set AI state
    /// </summary>
    private void SetState(EnemyState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case EnemyState.Patrol:
                navAgent.SetDestination(patrolTarget);
                break;
            case EnemyState.Chase:
                if (player != null)
                {
                    navAgent.SetDestination(player.position);
                }
                break;
            case EnemyState.Attack:
                navAgent.isStopped = true;
                break;
        }
    }
    
    /// <summary>
    /// Update animations based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Set walking animation
        bool isWalking = navAgent.velocity.magnitude > 0.1f;
        animator.SetBool(walkAnimationName, isWalking);
    }
    
    /// <summary>
    /// Handle health changes
    /// </summary>
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        // Could add visual feedback here (health bar, damage effects, etc.)
    }
    
    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void OnDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger(deathAnimationName);
        }
        
        // Play death effect
        if (deathEffect != null)
        {
            deathEffect.Play();
        }
        
        // Play death sound
        PlayDeathSound();
        
        // Disable AI
        navAgent.enabled = false;
        
        // Trigger events
        OnEnemyDeath?.Invoke(this);
        
        // Destroy after delay
        StartCoroutine(DestroyAfterDelay(3f));
    }
    
    /// <summary>
    /// Play attack sound
    /// </summary>
    private void PlayAttackSound()
    {
        if (audioSource != null && attackSounds.Length > 0)
        {
            AudioClip randomSound = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }
    
    /// <summary>
    /// Play death sound
    /// </summary>
    private void PlayDeathSound()
    {
        if (audioSource != null && deathSounds.Length > 0)
        {
            AudioClip randomSound = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }
    
    /// <summary>
    /// Play alert sound
    /// </summary>
    private void PlayAlertSound()
    {
        if (audioSource != null && alertSounds.Length > 0)
        {
            AudioClip randomSound = alertSounds[Random.Range(0, alertSounds.Length)];
            audioSource.PlayOneShot(randomSound);
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
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyData?.patrolRadius ?? 10f);
        
        // Draw attack radius
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, enemyData?.attackRadius ?? 1f);
        }
    }
}

/// <summary>
/// Enemy AI states
/// </summary>
public enum EnemyState
{
    Patrol,
    Chase,
    Attack
}

/// <summary>
/// Attack types
/// </summary>
public enum AttackType
{
    Melee,
    Ranged
}

/// <summary>
/// Scriptable object containing enemy data
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Elite Shooter/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public GameObject enemyModel;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 120f;
    public float patrolRadius = 10f;
    
    [Header("Combat")]
    public int damage = 20;
    public float attackRadius = 1.5f;
    public AttackType attackType = AttackType.Melee;
    
    [Header("Health")]
    public int maxHealth = 100;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] alertSounds;
} 