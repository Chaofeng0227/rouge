using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("Directional Sprites")]
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite sideSprite;
    public Sprite downDiagonalSprite;
    public Sprite upDiagonalSprite;

    [Header("Motion")]
    public float moveThreshold = 0.05f;
    public float bounceSpeed = 12f;
    public float bounceAmount = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector3 baseScale;
    private Vector2 lastFacingDirection = Vector2.down;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        baseScale = transform.localScale;
    }

    void Update()
    {
        UpdateFacing();
        UpdateBounce();
    }

    void OnDisable()
    {
        transform.localScale = baseScale;
    }

    void UpdateFacing()
    {
        Vector2 facingDirection = GetFacingDirection();
        Vector2 absoluteDirection = new Vector2(Mathf.Abs(facingDirection.x), Mathf.Abs(facingDirection.y));

        if (absoluteDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        spriteRenderer.flipX = facingDirection.x > 0.15f;

        if (absoluteDirection.y > absoluteDirection.x * 1.5f)
        {
            spriteRenderer.sprite = facingDirection.y > 0f ? upSprite : downSprite;
            return;
        }

        if (absoluteDirection.x > absoluteDirection.y * 1.5f)
        {
            spriteRenderer.sprite = sideSprite;
            return;
        }

        spriteRenderer.sprite = facingDirection.y > 0f ? upDiagonalSprite : downDiagonalSprite;
    }

    void UpdateBounce()
    {
        if (rb == null || rb.velocity.sqrMagnitude < moveThreshold * moveThreshold)
        {
            transform.localScale = baseScale;
            return;
        }

        float bounce = (Mathf.Sin(Time.time * bounceSpeed) + 1f) * 0.5f;
        float stretch = 1f + bounce * bounceAmount;
        float squash = 1f - bounce * bounceAmount * 0.45f;

        transform.localScale = new Vector3(
            baseScale.x * squash,
            baseScale.y * stretch,
            baseScale.z);
    }

    Vector2 GetFacingDirection()
    {
        if (rb != null && rb.velocity.sqrMagnitude >= moveThreshold * moveThreshold)
        {
            lastFacingDirection = rb.velocity.normalized;
            return lastFacingDirection;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = transform.position.z;

            Vector2 aimDirection = mouseWorldPosition - transform.position;
            if (aimDirection.sqrMagnitude > 0.001f)
            {
                lastFacingDirection = aimDirection.normalized;
            }
        }

        return lastFacingDirection;
    }
}
