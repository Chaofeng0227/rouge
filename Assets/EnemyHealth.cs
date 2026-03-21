<<<<<<< HEAD
ï»¿using UnityEngine;
=======
using UnityEngine;
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

<<<<<<< HEAD
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureHealthBar();
=======
    void Start()
    {
        currentHealth = maxHealth;
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

<<<<<<< HEAD
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log(gameObject.name + " å—åˆ°ä¼¤å®³: " + damage + "ï¼Œå‰©ä½™è¡€é‡: " + currentHealth);
=======
        Debug.Log(gameObject.name + " ÊÜµ½ÉËº¦: " + damage + "£¬Ê£ÓàÑªÁ¿: " + currentHealth);
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
<<<<<<< HEAD

    void EnsureHealthBar()
    {
        if (GetComponent<OverheadHealthBar>() == null)
        {
            gameObject.AddComponent<OverheadHealthBar>();
        }
    }
}
=======
}
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda
