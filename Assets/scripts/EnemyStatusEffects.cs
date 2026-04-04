using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStatusEffects : MonoBehaviour
{
    [SerializeField] private Color chilledTint = new Color(0.62f, 0.82f, 1f, 1f);
    [SerializeField] private Color frozenTint = new Color(0.78f, 0.92f, 1f, 1f);

    private SpriteRenderer[] spriteRenderers;
    private Color[] baseColors;
    private int chillStacks;
    private float chillTimer;
    private float frozenTimer;
    private float slowMultiplier = 1f;

    public bool IsFrozen => frozenTimer > 0f;
    public bool IsChilled => !IsFrozen && chillTimer > 0f;
    public float MoveSpeedMultiplier => IsFrozen ? 0f : (IsChilled ? slowMultiplier : 1f);
    public int ChillStacks => chillStacks;

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            baseColors[i] = spriteRenderers[i].color;
        }
    }

    void Update()
    {
        if (frozenTimer > 0f)
        {
            frozenTimer -= Time.deltaTime;
            if (frozenTimer <= 0f)
            {
                frozenTimer = 0f;
                RestoreBaseVisuals();
            }

            return;
        }

        if (chillTimer > 0f)
        {
            chillTimer -= Time.deltaTime;
            if (chillTimer <= 0f)
            {
                chillTimer = 0f;
                chillStacks = 0;
                slowMultiplier = 1f;
                RestoreBaseVisuals();
            }
        }
    }

    public void ApplyFrost(int stacksToAdd, float chillDuration, float chilledMoveMultiplier, int freezeThreshold, float freezeDuration)
    {
        if (GetComponent<BossController>() != null)
        {
            return;
        }

        if (IsFrozen)
        {
            frozenTimer = Mathf.Max(frozenTimer, freezeDuration);
            ApplyTint(frozenTint);
            return;
        }

        chillStacks += Mathf.Max(1, stacksToAdd);
        chillTimer = Mathf.Max(chillTimer, chillDuration);
        slowMultiplier = Mathf.Clamp(chilledMoveMultiplier, 0.05f, 1f);

        if (chillStacks >= Mathf.Max(1, freezeThreshold))
        {
            chillStacks = 0;
            chillTimer = 0f;
            slowMultiplier = 1f;
            frozenTimer = Mathf.Max(0.1f, freezeDuration);
            ApplyTint(frozenTint);
            return;
        }

        ApplyTint(chilledTint);
    }

    void RestoreBaseVisuals()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = baseColors[i];
            }
        }
    }

    void ApplyTint(Color tint)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = tint;
            }
        }
    }
}
