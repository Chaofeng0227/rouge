using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public float hurtInvincibilityDuration = 0.5f;
    private int currentHealth;
    private bool isDead;
    private float hurtInvincibilityTimer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureSupportComponents();
        FloorMapController.EnsureInstance();
        BackgroundMusicController.EnsureInstance();
        PauseMenuUI.EnsureInstance();
    }

    void Start()
    {
        Debug.Log("Player HP: " + currentHealth);
    }

    void Update()
    {
        if (hurtInvincibilityTimer > 0f)
        {
            hurtInvincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || hurtInvincibilityTimer > 0f)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Player took damage: " + damage + ", HP: " + currentHealth);
        hurtInvincibilityTimer = hurtInvincibilityDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void IncreaseMaxHealth(int amount, int healAmount)
    {
        if (amount <= 0)
        {
            return;
        }

        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Debug.Log("Player died.");
        GameOverUI.Show();
    }

    void EnsureSupportComponents()
    {
        if (GetComponent<OverheadHealthBar>() == null)
        {
            gameObject.AddComponent<OverheadHealthBar>();
        }

        if (GetComponent<PlayerProgression>() == null)
        {
            gameObject.AddComponent<PlayerProgression>();
        }
    }
}
