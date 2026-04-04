using UnityEngine;

[DisallowMultipleComponent]
public class EnemyEliteShooter : MonoBehaviour
{
    [Header("Elite Tuning")]
    [SerializeField] private float shootInterval = 1.1f;
    [SerializeField] private float projectileSpeed = 9f;
    [SerializeField] private float projectileLifeTime = 4f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float shootRange = 7f;
    [SerializeField] private float preferredRange = 5.2f;
    [SerializeField] private float retreatRange = 3.4f;
    [SerializeField] private float approachSpeed = 1.2f;
    [SerializeField] private float retreatSpeed = 1.3f;
    [SerializeField] private float strafeSpeed = 1.1f;
    [SerializeField] private float strafeSwitchInterval = 1.15f;
    [SerializeField] private float obstacleProbeDistance = 0.42f;
    [SerializeField] private float obstacleProbeRadius = 0.28f;
    [SerializeField] private float eliteScaleMultiplier = 1.28f;
    [SerializeField] private Color eliteTint = new Color(1f, 0.72f, 0.28f, 1f);
    [SerializeField] private int bonusMaxHealth = 4;
    [SerializeField] private int bonusBaseDamage = 1;

    private Transform player;
    private float shootTimer;
    private bool initialized;
    private float strafeTimer;
    private int strafeDirection = 1;

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private EnemyStatusEffects statusEffects;
    private EnemyPathfinder enemyPathfinder;
    private Rigidbody2D rb;

    void Awake()
    {
        CacheVisuals();
        rb = GetComponent<Rigidbody2D>();
        enemyPathfinder = GetComponent<EnemyPathfinder>();
    }

    void Update()
    {
        if (!initialized)
        {
            InitializeElite();
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        shootTimer -= Time.deltaTime;

        if (statusEffects == null)
        {
            statusEffects = GetComponent<EnemyStatusEffects>();
        }

        if (statusEffects != null && statusEffects.IsFrozen)
        {
            RestorePathfinderIfNeeded();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (player == null)
        {
            return;
        }

        Vector2 delta = player.position - transform.position;
        float distance = delta.magnitude;
        bool hasLineOfSight = HasLineOfSightToPlayer();

        UpdateMovementMode(delta, distance, hasLineOfSight);

        if (shootTimer > 0f || distance > shootRange || !hasLineOfSight)
        {
            return;
        }

        Shoot(delta.normalized);
        shootTimer = shootInterval;
    }

    public void InitializeElite()
    {
        if (initialized)
        {
            return;
        }

        CacheVisuals();
        ApplyEliteVisuals();
        transform.localScale *= eliteScaleMultiplier;

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth += bonusMaxHealth;
            enemyHealth.experienceReward += 2;
        }

        EnemyDamage enemyDamage = GetComponent<EnemyDamage>();
        if (enemyDamage != null)
        {
            enemyDamage.baseDamage += bonusBaseDamage;
        }

        initialized = true;
        shootTimer = shootInterval * 0.65f;
        strafeTimer = strafeSwitchInterval;
        Debug.Log("Elite shooter initialized: " + gameObject.name);
    }

    void Shoot(Vector2 direction)
    {
        GameObject projectile = new GameObject("EnemyProjectile");
        projectile.transform.position = transform.position + (Vector3)(direction * 0.25f);
        projectile.AddComponent<SpriteRenderer>();
        projectile.AddComponent<Rigidbody2D>();
        projectile.AddComponent<BoxCollider2D>();

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.Initialize(direction, projectileDamage, projectileSpeed, projectileLifeTime, eliteTint, transform);
        Debug.Log("Elite shooter fired: " + gameObject.name);
    }

    void CacheVisuals()
    {
        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            return;
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    void ApplyEliteVisuals()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = Color.Lerp(originalColors[i], eliteTint, 0.45f);
            }
        }
    }

    void UpdateMovementMode(Vector2 delta, float distance, bool hasLineOfSight)
    {
        if (rb == null)
        {
            return;
        }

        if (!hasLineOfSight || distance > preferredRange + 0.8f)
        {
            RestorePathfinderIfNeeded();
            return;
        }

        if (enemyPathfinder != null && enemyPathfinder.enabled)
        {
            enemyPathfinder.enabled = false;
        }

        Vector2 directionToPlayer = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.right;

        if (distance < retreatRange)
        {
            rb.velocity = ResolveSafeVelocity(-directionToPlayer * retreatSpeed, Vector2.zero);
            return;
        }

        if (distance > preferredRange + 0.45f)
        {
            rb.velocity = ResolveSafeVelocity(directionToPlayer * approachSpeed, Vector2.zero);
            return;
        }

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeTimer = strafeSwitchInterval;
            strafeDirection = Random.value < 0.5f ? -1 : 1;
        }

        Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x);
        Vector2 primaryStrafe = perpendicular * (strafeDirection * strafeSpeed);
        Vector2 reverseStrafe = -primaryStrafe;
        Vector2 fallbackRetreat = -directionToPlayer * (retreatSpeed * 0.8f);
        rb.velocity = ResolveSafeVelocity(primaryStrafe, reverseStrafe, fallbackRetreat);
    }

    void RestorePathfinderIfNeeded()
    {
        if (enemyPathfinder != null && !enemyPathfinder.enabled)
        {
            enemyPathfinder.enabled = true;
        }
    }

    Vector2 ResolveSafeVelocity(params Vector2[] candidateVelocities)
    {
        foreach (Vector2 candidate in candidateVelocities)
        {
            if (!IsMoveBlocked(candidate))
            {
                return candidate;
            }
        }

        return Vector2.zero;
    }

    bool IsMoveBlocked(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        Vector2 direction = velocity.normalized;
        float castDistance = obstacleProbeDistance + (velocity.magnitude * Time.deltaTime);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, obstacleProbeRadius, direction, castDistance);

        foreach (RaycastHit2D hit in hits)
        {
            Collider2D hitCollider = hit.collider;
            if (hitCollider == null)
            {
                continue;
            }

            if (hitCollider.isTrigger)
            {
                continue;
            }

            if (hitCollider.transform.root == transform.root)
            {
                continue;
            }

            if (hitCollider.GetComponentInParent<PlayerHealth>() != null)
            {
                continue;
            }

            if (hitCollider.GetComponentInParent<EnemyHealth>() != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    bool HasLineOfSightToPlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector2 origin = transform.position;
        Vector2 target = player.position;
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin + direction * 0.2f, direction, distance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform.root == transform.root)
            {
                continue;
            }

            if (hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.collider.GetComponentInParent<PlayerHealth>() != null)
            {
                return true;
            }

            if (hit.collider.GetComponentInParent<EnemyHealth>() != null)
            {
                continue;
            }

            return false;
        }

        return false;
    }
}
