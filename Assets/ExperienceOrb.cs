using UnityEngine;

[DisallowMultipleComponent]
public class ExperienceOrb : MonoBehaviour
{
    private static Sprite whiteSprite;

    [SerializeField] private float idleDriftSpeed = 0.8f;
    [SerializeField] private float attractionRange = 3f;
    [SerializeField] private float collectRange = 0.35f;
    [SerializeField] private float minAttractSpeed = 3f;
    [SerializeField] private float maxAttractSpeed = 12f;
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseAmount = 0.12f;

    private int experienceValue;
    private Vector3 idleVelocity;
    private Vector3 baseScale = Vector3.one;
    private Transform target;
    private CircleCollider2D triggerCollider;

    public static void Spawn(Vector3 position, int experienceValue)
    {
        if (experienceValue <= 0)
        {
            return;
        }

        GameObject orbObject = new GameObject("ExperienceOrb");
        orbObject.transform.position = position;

        ExperienceOrb orb = orbObject.AddComponent<ExperienceOrb>();
        orb.Initialize(experienceValue);
    }

    void Awake()
    {
        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetWhiteSprite();
        spriteRenderer.color = new Color(0.24f, 0.93f, 0.95f, 1f);
        spriteRenderer.sortingOrder = 4;

        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = collectRange;

        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        idleVelocity = (Vector3)(direction * idleDriftSpeed);
    }

    void Update()
    {
        if (target == null && PlayerProgression.Instance != null)
        {
            target = PlayerProgression.Instance.transform;
        }

        if (target == null)
        {
            transform.position += idleVelocity * Time.deltaTime;
            UpdatePulse();
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= collectRange)
        {
            Collect();
            return;
        }

        if (distance <= attractionRange)
        {
            float t = 1f - Mathf.Clamp01(distance / attractionRange);
            float speed = Mathf.Lerp(minAttractSpeed, maxAttractSpeed, t);
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position += idleVelocity * Time.deltaTime;
        }

        UpdatePulse();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerProgression>() == null)
        {
            return;
        }

        Collect();
    }

    void Initialize(int value)
    {
        experienceValue = value;
        transform.localScale = baseScale * 0.28f;
    }

    void Collect()
    {
        if (PlayerProgression.Instance != null)
        {
            PlayerProgression.Instance.AddExperience(experienceValue);
        }

        Destroy(gameObject);
    }

    void UpdatePulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * 0.28f * pulse;
    }

    static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            whiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
        }

        return whiteSprite;
    }
}
