using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("基础属性 (第1层数值)")]
    public int maxHealth = 3;
    public int experienceReward = 1;

    [Header("肉鸽成长配置")]
    public int healthIncreasePerLevel = 2; // 每下一层，血量增加多少
    public int expIncreasePerLevel = 1;    // 每下一层，经验掉落增加多少

    private int currentHealth;

    // 保持你原有的属性，完美兼容 OverheadHealthBar 和 EnemyStagedDirector
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        // Awake 阶段先确保血条组件存在
        EnsureHealthBar();
    }

    void Start()
    {
        // 1. 寻找地牢生成器，获取当前层数
        DungeonGenerator generator = Object.FindFirstObjectByType<DungeonGenerator>();

        if (generator != null)
        {
            // 2. 计算需要增加的额外属性 (第1层不增加，第2层增加1次，以此类推)
            int extraHealth = (generator.currentLevel - 1) * healthIncreasePerLevel;
            int extraExp = (generator.currentLevel - 1) * expIncreasePerLevel;

            // 3. 叠加到怪物的面板上
            maxHealth += extraHealth;
            experienceReward += extraExp;
        }

        // 4. 属性计算完毕后，同步当前血量为最大的满血状态
        SyncCurrentHealthToMax();
    }

    public void SyncCurrentHealthToMax()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log(gameObject.name + " took damage: " + damage + ", HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 现在掉落的经验球也会随着层数越来越肥了！
        ExperienceOrb.Spawn(transform.position, experienceReward);

        Destroy(gameObject);
    }

    void EnsureHealthBar()
    {
        if (GetComponent<OverheadHealthBar>() == null)
        {
            gameObject.AddComponent<OverheadHealthBar>();
        }
    }
}