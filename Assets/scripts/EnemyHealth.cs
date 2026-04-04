using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 3;
    public int experienceReward = 1;

    [Header("Floor Scaling")]
    public int healthIncreasePerLevel = 2;
    public int expIncreasePerLevel = 1;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        EnsureHealthBar();
    }

    void Start()
    {
        DungeonGenerator generator = Object.FindFirstObjectByType<DungeonGenerator>();
        if (generator != null)
        {
            int extraHealth = (generator.currentLevel - 1) * healthIncreasePerLevel;
            int extraExp = (generator.currentLevel - 1) * expIncreasePerLevel;

            maxHealth += extraHealth;
            experienceReward += extraExp;
        }

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
        BossController bossController = GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.HandleDeath();
            return;
        }

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
