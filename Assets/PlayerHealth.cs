<<<<<<< HEAD
ï»¿using UnityEngine;
=======
using UnityEngine;
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

<<<<<<< HEAD
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureHealthBar();
    }

    void Start()
    {
        Debug.Log("çŽ©å®¶åˆå§‹è¡€é‡: " + currentHealth);
=======
    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Íæ¼Ò³õÊ¼ÑªÁ¿: " + currentHealth);
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

<<<<<<< HEAD
        Debug.Log("çŽ©å®¶å—åˆ°ä¼¤å®³: " + damage + "ï¼Œå½“å‰è¡€é‡: " + currentHealth);
=======
        Debug.Log("Íæ¼ÒÊÜµ½ÉËº¦: " + damage + "£¬µ±Ç°ÑªÁ¿: " + currentHealth);
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
<<<<<<< HEAD
        Debug.Log("çŽ©å®¶è¡€é‡ä¸º 0ï¼Œæ¸¸æˆç»“æŸ");
=======
        Debug.Log("Íæ¼ÒÑªÁ¿Îª 0£¬ÓÎÏ·½áÊø");
>>>>>>> 1a1f7825e7bd6fed254ceb9dfb3453bdc7c54dda

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
