using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureSupportComponents();
        FloorMapController.EnsureInstance();
    }

    void Start()
    {
        Debug.Log("Player HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Player took damage: " + damage + ", HP: " + currentHealth);

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
