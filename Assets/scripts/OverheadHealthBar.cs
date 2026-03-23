using UnityEngine;

[DisallowMultipleComponent]
public class OverheadHealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 barSize = new Vector2(1.2f, 0.14f);
    [SerializeField] private float verticalPadding = 0.35f;
    [SerializeField] private int sortingOffset = 10;
    [SerializeField] private Color playerFillColor = new Color(0.22f, 0.84f, 0.39f, 1f);
    [SerializeField] private Color enemyFillColor = new Color(0.92f, 0.24f, 0.24f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);

    private static Sprite whiteSprite;

    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;
    private SpriteRenderer targetRenderer;

    private Transform barRoot;
    private Transform fillTransform;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        targetRenderer = GetComponent<SpriteRenderer>();

        if (playerHealth == null && enemyHealth == null)
        {
            enabled = false;
            return;
        }

        CreateVisuals();
        UpdateVisuals();
    }

    private void LateUpdate()
    {
        if (barRoot == null)
        {
            return;
        }

        barRoot.position = transform.position + GetWorldOffset();
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        if (barRoot != null)
        {
            Destroy(barRoot.gameObject);
        }
    }

    private void CreateVisuals()
    {
        Sprite sprite = GetWhiteSprite();

        GameObject rootObject = new GameObject(name + "_HealthBar");
        barRoot = rootObject.transform;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(barRoot, false);
        backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = sprite;
        backgroundObject.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        backgroundRenderer.color = backgroundColor;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barRoot, false);
        fillTransform = fillObject.transform;
        fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = sprite;
        fillRenderer.color = CompareTag("Enemy") ? enemyFillColor : playerFillColor;

        ApplySorting();
    }

    private void ApplySorting()
    {
        if (targetRenderer == null)
        {
            return;
        }

        backgroundRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        backgroundRenderer.sortingOrder = targetRenderer.sortingOrder + sortingOffset;
        fillRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        fillRenderer.sortingOrder = targetRenderer.sortingOrder + sortingOffset + 1;
    }

    private void UpdateVisuals()
    {
        float healthPercent = GetHealthPercent();
        float fillWidth = barSize.x * healthPercent;

        fillTransform.localScale = new Vector3(fillWidth, barSize.y, 1f);
        fillTransform.localPosition = new Vector3((fillWidth - barSize.x) * 0.5f, 0f, 0f);
    }

    private float GetHealthPercent()
    {
        if (playerHealth != null)
        {
            return playerHealth.MaxHealth <= 0 ? 0f : (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        }

        if (enemyHealth != null)
        {
            return enemyHealth.MaxHealth <= 0 ? 0f : (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
        }

        return 0f;
    }

    private Vector3 GetWorldOffset()
    {
        float heightOffset = verticalPadding;

        if (targetRenderer != null)
        {
            heightOffset += targetRenderer.bounds.extents.y;
        }

        return Vector3.up * heightOffset;
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        return whiteSprite;
    }
}
