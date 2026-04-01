using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("伤害配置 (肉鸽成长)")]
    public int baseDamage = 1;
    public int damageIncreasePerLevel = 1;

    // 【关键修复】：恢复公开的 damage 变量，让 EnemyStagedDirector 能找到它
    public int damage;

    public float attackInterval = 1f;
    private float attackTimer = 0f;
    private EnemyStatusEffects statusEffects;

    void Start()
    {
        // 计算当前层数的实际伤害
        damage = baseDamage;
        DungeonGenerator generator = Object.FindFirstObjectByType<DungeonGenerator>();

        if (generator != null)
        {
            int extraDamage = (generator.currentLevel - 1) * damageIncreasePerLevel;
            damage += extraDamage;
        }

        statusEffects = GetComponent<EnemyStatusEffects>();
    }

    void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (statusEffects == null)
        {
            statusEffects = GetComponent<EnemyStatusEffects>();
        }

        if (statusEffects != null && statusEffects.IsFrozen)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;

        if (attackTimer <= 0f)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("怪物对玩家造成伤害: " + damage);
            attackTimer = attackInterval;
        }
    }
}
