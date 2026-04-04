using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    private const string BossVisualResourcePath = "BossVisuals/WoodenAarakocraBoss";

    [Header("Boss Stats")]
    [SerializeField] private int baseBossHealth = 80;
    [SerializeField] private int bossExperienceReward = 20;
    [SerializeField] private float bossScaleMultiplier = 2.25f;
    [SerializeField] private Color bossTint = new Color(1f, 0.48f, 0.2f, 1f);
    [SerializeField] private float bossVisualScale = 0.24f;
    [SerializeField] private Vector3 bossVisualOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Boss Movement")]
    [SerializeField] private float activationRange = 11f;
    [SerializeField] private float moveSpeed = 1.9f;
    [SerializeField] private float preferredRange = 5.8f;
    [SerializeField] private float retreatRange = 3.6f;
    [SerializeField] private float strafeSpeed = 1.15f;
    [SerializeField] private float strafeSwitchInterval = 1.2f;
    [SerializeField] private float obstacleProbeDistance = 0.48f;
    [SerializeField] private float obstacleProbeRadius = 0.34f;

    [Header("Projectile Look")]
    [SerializeField] private Vector2 phaseOneProjectileScale = new Vector2(0.82f, 0.62f);
    [SerializeField] private Vector2 phaseTwoProjectileScale = new Vector2(0.9f, 0.68f);
    [SerializeField] private Vector2 phaseThreeProjectileScale = new Vector2(0.88f, 0.66f);

    private EnemyHealth enemyHealth;
    private EnemyPathfinder enemyPathfinder;
    private EnemyDamage enemyDamage;
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;
    private Transform player;
    private bool initialized;
    private bool hasDied;
    private float radialCooldown;
    private float fanCooldown;
    private float spinOffset;
    private float strafeTimer;
    private int strafeDirection = 1;
    private Vector3 spawnPosition;
    private GameObject bossVisualInstance;

    private enum BossPhase
    {
        PhaseOne,
        PhaseTwo,
        PhaseThree
    }

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyPathfinder = GetComponent<EnemyPathfinder>();
        enemyDamage = GetComponent<EnemyDamage>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        spawnPosition = transform.position;
    }

    void Start()
    {
        if (enemyHealth != null)
        {
            enemyHealth.SyncCurrentHealthToMax();
        }
    }

    void Update()
    {
        if (!initialized || hasDied)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        radialCooldown -= Time.deltaTime;
        fanCooldown -= Time.deltaTime;
        spinOffset += Time.deltaTime * 22f;

        if (player == null || enemyHealth == null || enemyHealth.CurrentHealth <= 0)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        bool isActivated = distanceToPlayer <= activationRange;
        bool hasLineOfSight = isActivated && HasLineOfSightToPlayer();

        UpdateMovement(toPlayer, distanceToPlayer, isActivated, hasLineOfSight);

        if (!isActivated || !hasLineOfSight)
        {
            return;
        }

        BossPhase currentPhase = GetCurrentPhase();

        if (radialCooldown <= 0f)
        {
            FireRadialPattern(currentPhase);
            radialCooldown = GetRadialCooldown(currentPhase);
        }

        if (currentPhase != BossPhase.PhaseOne && fanCooldown <= 0f)
        {
            FireAimedFan(currentPhase);
            fanCooldown = GetFanCooldown(currentPhase);
        }
    }

    public void InitializeBoss()
    {
        if (initialized)
        {
            return;
        }

        if (enemyHealth != null)
        {
            enemyHealth.maxHealth = baseBossHealth;
            enemyHealth.experienceReward = bossExperienceReward;
        }

        if (enemyPathfinder != null)
        {
            enemyPathfinder.enabled = false;
        }

        if (enemyDamage != null)
        {
            enemyDamage.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        AttachBossVisual();
        transform.localScale *= bossScaleMultiplier;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        radialCooldown = 1.2f;
        fanCooldown = 2.6f;
        strafeTimer = strafeSwitchInterval;
        initialized = true;
    }

    public void HandleDeath()
    {
        if (hasDied)
        {
            return;
        }

        hasDied = true;
        ExperienceOrb.Spawn(transform.position, bossExperienceReward);
        VictoryUI.Show();
        Destroy(gameObject);
    }

    void UpdateMovement(Vector2 toPlayer, float distanceToPlayer, bool isActivated, bool hasLineOfSight)
    {
        if (rb == null)
        {
            return;
        }

        if (!isActivated)
        {
            Vector2 toSpawn = spawnPosition - transform.position;
            if (toSpawn.magnitude > 0.15f)
            {
                rb.velocity = ResolveSafeVelocity(toSpawn.normalized * (moveSpeed * 0.75f), Vector2.zero);
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (!hasLineOfSight)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 directionToPlayer = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.right;

        if (distanceToPlayer < retreatRange)
        {
            rb.velocity = ResolveSafeVelocity(-directionToPlayer * moveSpeed, Vector2.zero);
            return;
        }

        if (distanceToPlayer > preferredRange + 0.5f)
        {
            rb.velocity = ResolveSafeVelocity(directionToPlayer * moveSpeed, Vector2.zero);
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
        rb.velocity = ResolveSafeVelocity(primaryStrafe, reverseStrafe, Vector2.zero);
    }

    BossPhase GetCurrentPhase()
    {
        float healthRatio = enemyHealth.MaxHealth > 0 ? (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth : 1f;
        if (healthRatio > 0.66f)
        {
            return BossPhase.PhaseOne;
        }

        if (healthRatio > 0.33f)
        {
            return BossPhase.PhaseTwo;
        }

        return BossPhase.PhaseThree;
    }

    void FireRadialPattern(BossPhase phase)
    {
        int projectileCount = 12;
        float projectileSpeed = 5.2f;
        int damage = 1;
        Color projectileColor = new Color(1f, 0.55f, 0.22f, 1f);
        Vector2 projectileScale = phaseOneProjectileScale;

        switch (phase)
        {
            case BossPhase.PhaseTwo:
                projectileCount = 18;
                projectileSpeed = 6f;
                projectileColor = new Color(1f, 0.35f, 0.22f, 1f);
                projectileScale = phaseTwoProjectileScale;
                break;
            case BossPhase.PhaseThree:
                projectileCount = 16;
                projectileSpeed = 6.1f;
                damage = 2;
                projectileColor = new Color(1f, 0.18f, 0.18f, 1f);
                projectileScale = phaseThreeProjectileScale;
                break;
        }

        float angleStep = 360f / projectileCount;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = spinOffset + (angleStep * i);
            SpawnBossProjectile(DirectionFromAngle(angle), projectileSpeed, damage, projectileColor, projectileScale, 5.6f);
        }

        if (phase == BossPhase.PhaseThree)
        {
            int secondaryRingCount = 8;
            float secondaryAngleStep = 360f / secondaryRingCount;
            for (int i = 0; i < secondaryRingCount; i++)
            {
                float angle = spinOffset + 12f + (secondaryAngleStep * i);
                SpawnBossProjectile(DirectionFromAngle(angle), projectileSpeed * 0.82f, damage, projectileColor, projectileScale, 3.4f);
            }
        }
    }

    void FireAimedFan(BossPhase phase)
    {
        if (player == null)
        {
            return;
        }

        Vector2 baseDirection = (player.position - transform.position).normalized;
        float spread = phase == BossPhase.PhaseThree ? 13f : 11f;
        int count = phase == BossPhase.PhaseThree ? 5 : 5;
        float speed = phase == BossPhase.PhaseThree ? 6.6f : 6.2f;
        int damage = phase == BossPhase.PhaseThree ? 2 : 1;
        Color color = phase == BossPhase.PhaseThree ? new Color(1f, 0.2f, 0.3f, 1f) : new Color(1f, 0.72f, 0.28f, 1f);
        Vector2 projectileScale = phase == BossPhase.PhaseThree ? phaseThreeProjectileScale : phaseTwoProjectileScale;

        int half = count / 2;
        for (int i = -half; i <= half; i++)
        {
            Vector2 direction = Rotate(baseDirection, spread * i);
            SpawnBossProjectile(direction, speed, damage, color, projectileScale, 4.8f);
        }
    }

    float GetRadialCooldown(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.PhaseOne: return 3.6f;
            case BossPhase.PhaseTwo: return 3.0f;
            default: return 2.35f;
        }
    }

    float GetFanCooldown(BossPhase phase)
    {
        return phase == BossPhase.PhaseThree ? 2.85f : 3.25f;
    }

    void SpawnBossProjectile(Vector2 direction, float speed, int damage, Color color, Vector2 scale, float lifeTime)
    {
        GameObject projectile = new GameObject("BossProjectile");
        projectile.transform.position = transform.position + (Vector3)(direction * 0.55f);
        projectile.AddComponent<SpriteRenderer>();
        projectile.AddComponent<Rigidbody2D>();
        projectile.AddComponent<CircleCollider2D>();

        BossProjectile bossProjectile = projectile.AddComponent<BossProjectile>();
        bossProjectile.Initialize(direction, speed, damage, lifeTime, color, scale, transform);
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
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin + direction * 0.25f, direction, distance);

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

    static Vector2 DirectionFromAngle(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos).normalized;
    }

    void AttachBossVisual()
    {
        if (bossVisualInstance != null)
        {
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(BossVisualResourcePath);
        if (visualPrefab == null)
        {
            Debug.LogWarning("BossController: boss visual prefab not found at Resources/" + BossVisualResourcePath);
            return;
        }

        bossVisualInstance = Instantiate(visualPrefab, transform);
        bossVisualInstance.name = "BossVisual";
        bossVisualInstance.transform.localPosition = bossVisualOffset;
        bossVisualInstance.transform.localRotation = Quaternion.identity;
        bossVisualInstance.transform.localScale = Vector3.one * bossVisualScale;

        SpriteRenderer[] visualRenderers = bossVisualInstance.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer visualRenderer in visualRenderers)
        {
            if (visualRenderer != null)
            {
                visualRenderer.sortingOrder = Mathf.Max(visualRenderer.sortingOrder, 8);
            }
        }
    }
}
