using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class BossProjectile : MonoBehaviour
{
    private static Sprite projectileSprite;

    private Rigidbody2D rb;
    private Transform ownerRoot;
    private int damage;
    private bool hasResolvedHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Collider2D projectileCollider = GetComponent<Collider2D>();
        projectileCollider.isTrigger = true;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetProjectileSprite();
        spriteRenderer.sortingOrder = 22;
    }

    public void Initialize(Vector2 direction, float speed, int projectileDamage, float lifeTime, Color color, Vector2 scale, Transform owner)
    {
        ownerRoot = owner != null ? owner.root : null;
        damage = Mathf.Max(1, projectileDamage);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        transform.localScale = new Vector3(scale.x, scale.y, 1f);

        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, Mathf.Max(0.5f, lifeTime));
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

    static Sprite GetProjectileSprite()
    {
        if (projectileSprite == null)
        {
            projectileSprite = Resources.Load<Sprite>("EnemyProjectiles/PlasmaPulsePP1");
        }

        if (projectileSprite == null)
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            projectileSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
        }

        return projectileSprite;
    }
}
