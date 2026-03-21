using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureHealthBar();
    }

    void Start()
    {
        Debug.Log("玩家初始血量: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("玩家受到伤害: " + damage + "，当前血量: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("玩家血量为 0，游戏结束");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void EnsureHealthBar()
    {
        if (GetComponent<OverheadHealthBar>() == null)
        {
            gameObject.AddComponent<OverheadHealthBar>();
        }
    }
}
