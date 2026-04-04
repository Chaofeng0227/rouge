using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class EnemyProjectile : MonoBehaviour
{
    private static Sprite fallbackSprite;
    private static Sprite enemyProjectileSprite;

    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 4f;

    private Vector2 direction;
    private Transform ownerRoot;
    private Rigidbody2D rb;
    private bool hasResolvedHit;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(0.4f, 0.24f);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetProjectileSprite();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 20;

        transform.localScale = new Vector3(0.75f, 0.66f, 1f);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (hasResolvedHit || boxCollider == null)
        {
            return;
        }

        Vector2 center = boxCollider.bounds.center;
        Vector2 size = boxCollider.bounds.size * 0.85f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z);

        foreach (Collider2D hit in hits)
        {
            ResolveHit(hit);
            if (hasResolvedHit)
            {
                break;
            }
        }
    }

    public void Initialize(Vector2 shootDirection, int projectileDamage, float projectileSpeed, float projectileLifeTime, Color projectileColor, Transform owner)
    {
        direction = shootDirection.sqrMagnitude > 0.0001f ? shootDirection.normalized : Vector2.right;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        ownerRoot = owner != null ? owner.root : null;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = projectileColor;
        }

        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ResolveHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ResolveHit(other);
    }

    void ResolveHit(Collider2D other)
    {
        if (hasResolvedHit || other == null)
        {
            return;
        }

        if (ownerRoot != null && other.transform.root == ownerRoot)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            hasResolvedHit = true;
            playerHealth.TakeDamage(damage);
            Debug.Log("Enemy projectile hit player for " + damage);
            Destroy(gameObject);
            return;
        }

        if (other.GetComponentInParent<EnemyHealth>() != null)
        {
            return;
        }

        if (!other.isTrigger)
        {
            hasResolvedHit = true;
            Destroy(gameObject);
        }
    }

    static Sprite GetFallbackSprite()
    {
        if (fallbackSprite == null)
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
        }

        return fallbackSprite;
    }

    static Sprite GetProjectileSprite()
    {
        if (enemyProjectileSprite == null)
        {
            enemyProjectileSprite = Resources.Load<Sprite>("EnemyProjectiles/PlasmaPulsePP1");
        }

        return enemyProjectileSprite != null ? enemyProjectileSprite : GetFallbackSprite();
    }
}
