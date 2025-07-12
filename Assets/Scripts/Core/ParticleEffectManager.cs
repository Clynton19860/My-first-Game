using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all particle effects in the game.
/// Provides easy-to-use methods for creating various effects.
/// </summary>
public class ParticleEffectManager : MonoBehaviour
{
    [Header("Weapon Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject bulletImpactPrefab;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject shellCasingPrefab;
    
    [Header("Explosion Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject firePrefab;
    
    [Header("Environmental Effects")]
    [SerializeField] private GameObject dustEffectPrefab;
    [SerializeField] private GameObject sparkEffectPrefab;
    [SerializeField] private GameObject bloodEffectPrefab;
    
    [Header("Settings")]
    [SerializeField] private int maxParticleEffects = 50;
    [SerializeField] private float defaultEffectLifetime = 3f;
    
    // Object pooling
    private Queue<GameObject> effectPool = new Queue<GameObject>();
    private List<GameObject> activeEffects = new List<GameObject>();
    
    // Singleton instance
    public static ParticleEffectManager Instance { get; private set; }
    
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
        // Create default effects if none assigned
        CreateDefaultEffects();
    }
    
    /// <summary>
    /// Create default particle effects
    /// </summary>
    private void CreateDefaultEffects()
    {
        if (muzzleFlashPrefab == null)
        {
            muzzleFlashPrefab = CreateDefaultMuzzleFlash();
        }
        
        if (bulletImpactPrefab == null)
        {
            bulletImpactPrefab = CreateDefaultBulletImpact();
        }
        
        if (explosionPrefab == null)
        {
            explosionPrefab = CreateDefaultExplosion();
        }
    }
    
    /// <summary>
    /// Create muzzle flash effect
    /// </summary>
    public void CreateMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        GameObject effect = GetEffectFromPool(muzzleFlashPrefab);
        if (effect != null)
        {
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.SetActive(true);
            
            // Auto-destroy after effect duration
            StartCoroutine(DestroyEffectAfterDelay(effect, 0.1f));
        }
    }
    
    /// <summary>
    /// Create bullet impact effect
    /// </summary>
    public void CreateBulletImpact(Vector3 position, Vector3 normal, Material surfaceMaterial = null)
    {
        // Impact particles
        GameObject impactEffect = GetEffectFromPool(bulletImpactPrefab);
        if (impactEffect != null)
        {
            impactEffect.transform.position = position;
            impactEffect.transform.rotation = Quaternion.LookRotation(normal);
            impactEffect.SetActive(true);
            
            // Adjust effect color based on surface material
            if (surfaceMaterial != null)
            {
                AdjustEffectColor(impactEffect, surfaceMaterial.color);
            }
            
            StartCoroutine(DestroyEffectAfterDelay(impactEffect, defaultEffectLifetime));
        }
        
        // Bullet hole decal
        if (bulletHolePrefab != null)
        {
            GameObject bulletHole = GetEffectFromPool(bulletHolePrefab);
            if (bulletHole != null)
            {
                bulletHole.transform.position = position + normal * 0.01f; // Slight offset
                bulletHole.transform.rotation = Quaternion.LookRotation(normal);
                bulletHole.SetActive(true);
                
                // Bullet holes last longer
                StartCoroutine(DestroyEffectAfterDelay(bulletHole, 30f));
            }
        }
    }
    
    /// <summary>
    /// Create explosion effect
    /// </summary>
    public void CreateExplosion(Vector3 position, float radius = 5f)
    {
        // Main explosion
        GameObject explosion = GetEffectFromPool(explosionPrefab);
        if (explosion != null)
        {
            explosion.transform.position = position;
            explosion.transform.localScale = Vector3.one * (radius / 5f);
            explosion.SetActive(true);
            
            StartCoroutine(DestroyEffectAfterDelay(explosion, defaultEffectLifetime));
        }
        
        // Smoke effect
        if (smokePrefab != null)
        {
            GameObject smoke = GetEffectFromPool(smokePrefab);
            if (smoke != null)
            {
                smoke.transform.position = position + Vector3.up * 2f;
                smoke.SetActive(true);
                
                StartCoroutine(DestroyEffectAfterDelay(smoke, defaultEffectLifetime * 2f));
            }
        }
        
        // Fire effect
        if (firePrefab != null)
        {
            GameObject fire = GetEffectFromPool(firePrefab);
            if (fire != null)
            {
                fire.transform.position = position;
                fire.SetActive(true);
                
                StartCoroutine(DestroyEffectAfterDelay(fire, defaultEffectLifetime * 1.5f));
            }
        }
    }
    
    /// <summary>
    /// Create dust effect
    /// </summary>
    public void CreateDustEffect(Vector3 position, Vector3 direction)
    {
        if (dustEffectPrefab != null)
        {
            GameObject dust = GetEffectFromPool(dustEffectPrefab);
            if (dust != null)
            {
                dust.transform.position = position;
                dust.transform.rotation = Quaternion.LookRotation(direction);
                dust.SetActive(true);
                
                StartCoroutine(DestroyEffectAfterDelay(dust, defaultEffectLifetime));
            }
        }
    }
    
    /// <summary>
    /// Create spark effect
    /// </summary>
    public void CreateSparkEffect(Vector3 position, Vector3 direction)
    {
        if (sparkEffectPrefab != null)
        {
            GameObject spark = GetEffectFromPool(sparkEffectPrefab);
            if (spark != null)
            {
                spark.transform.position = position;
                spark.transform.rotation = Quaternion.LookRotation(direction);
                spark.SetActive(true);
                
                StartCoroutine(DestroyEffectAfterDelay(spark, defaultEffectLifetime * 0.5f));
            }
        }
    }
    
    /// <summary>
    /// Create blood effect
    /// </summary>
    public void CreateBloodEffect(Vector3 position, Vector3 direction)
    {
        if (bloodEffectPrefab != null)
        {
            GameObject blood = GetEffectFromPool(bloodEffectPrefab);
            if (blood != null)
            {
                blood.transform.position = position;
                blood.transform.rotation = Quaternion.LookRotation(direction);
                blood.SetActive(true);
                
                StartCoroutine(DestroyEffectAfterDelay(blood, defaultEffectLifetime));
            }
        }
    }
    
    /// <summary>
    /// Create shell casing effect
    /// </summary>
    public void CreateShellCasing(Vector3 position, Vector3 velocity)
    {
        if (shellCasingPrefab != null)
        {
            GameObject shell = GetEffectFromPool(shellCasingPrefab);
            if (shell != null)
            {
                shell.transform.position = position;
                shell.SetActive(true);
                
                // Add physics to shell
                Rigidbody rb = shell.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = velocity;
                    rb.angularVelocity = Random.insideUnitSphere * 10f;
                }
                
                StartCoroutine(DestroyEffectAfterDelay(shell, 5f));
            }
        }
    }
    
    /// <summary>
    /// Get effect from pool or create new one
    /// </summary>
    private GameObject GetEffectFromPool(GameObject prefab)
    {
        if (prefab == null) return null;
        
        // Try to get from pool
        if (effectPool.Count > 0)
        {
            GameObject effect = effectPool.Dequeue();
            if (effect != null)
            {
                return effect;
            }
        }
        
        // Create new effect
        GameObject newEffect = Instantiate(prefab);
        activeEffects.Add(newEffect);
        
        // Limit active effects
        if (activeEffects.Count > maxParticleEffects)
        {
            GameObject oldestEffect = activeEffects[0];
            activeEffects.RemoveAt(0);
            if (oldestEffect != null)
            {
                Destroy(oldestEffect);
            }
        }
        
        return newEffect;
    }
    
    /// <summary>
    /// Return effect to pool
    /// </summary>
    private void ReturnEffectToPool(GameObject effect)
    {
        if (effect == null) return;
        
        effect.SetActive(false);
        effectPool.Enqueue(effect);
        
        // Limit pool size
        if (effectPool.Count > maxParticleEffects)
        {
            GameObject excessEffect = effectPool.Dequeue();
            if (excessEffect != null)
            {
                Destroy(excessEffect);
            }
        }
    }
    
    /// <summary>
    /// Adjust effect color based on surface material
    /// </summary>
    private void AdjustEffectColor(GameObject effect, Color surfaceColor)
    {
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
        
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startColor = surfaceColor;
        }
    }
    
    /// <summary>
    /// Destroy effect after delay
    /// </summary>
    private System.Collections.IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnEffectToPool(effect);
    }
    
    /// <summary>
    /// Create default muzzle flash effect
    /// </summary>
    private GameObject CreateDefaultMuzzleFlash()
    {
        GameObject muzzleFlash = new GameObject("DefaultMuzzleFlash");
        
        // Create particle system
        ParticleSystem ps = muzzleFlash.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = 0.1f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = Color.yellow;
        main.maxParticles = 20;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 20)
        });
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        
        return muzzleFlash;
    }
    
    /// <summary>
    /// Create default bullet impact effect
    /// </summary>
    private GameObject CreateDefaultBulletImpact()
    {
        GameObject impact = new GameObject("DefaultBulletImpact");
        
        // Create particle system
        ParticleSystem ps = impact.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.1f;
        main.startColor = Color.gray;
        main.maxParticles = 15;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 15)
        });
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        return impact;
    }
    
    /// <summary>
    /// Create default explosion effect
    /// </summary>
    private GameObject CreateDefaultExplosion()
    {
        GameObject explosion = new GameObject("DefaultExplosion");
        
        // Create particle system
        ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 1f;
        main.startSpeed = 10f;
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.5f, 0f); // Orange color
        main.maxParticles = 50;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 50)
        });
        
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        return explosion;
    }
    
    /// <summary>
    /// Clear all active effects
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (GameObject effect in activeEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeEffects.Clear();
        
        while (effectPool.Count > 0)
        {
            GameObject effect = effectPool.Dequeue();
            if (effect != null)
            {
                Destroy(effect);
            }
        }
    }
} 