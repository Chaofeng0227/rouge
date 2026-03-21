using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public float attackInterval = 1f;

    private float attackTimer = 0f;

    void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
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