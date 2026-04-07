using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private const string DeathSfxResourcePath = "Sfx/BloodyPunch";

    [Header("Base Stats")]
    public int maxHealth = 3;
    public int experienceReward = 1;

    [Header("Floor Scaling")]
    public int healthIncreasePerLevel = 2;
    public int expIncreasePerLevel = 1;

    [Header("Audio")]
    [SerializeField] private float deathSfxVolume = 0.65f;

    private int currentHealth;
    private static AudioClip cachedDeathSfx;

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

        PlayDeathSfx();
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

    void PlayDeathSfx()
    {
        if (cachedDeathSfx == null)
        {
            cachedDeathSfx = Resources.Load<AudioClip>(DeathSfxResourcePath);
        }

        if (cachedDeathSfx != null)
        {
            AudioSource.PlayClipAtPoint(cachedDeathSfx, transform.position, deathSfxVolume);
        }
    }
}
