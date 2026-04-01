using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 3f;
    public bool appliesFrost;
    public int frostStacksPerHit = 1;
    public float frostSlowMultiplier = 0.6f;
    public float frostDuration = 1.5f;
    public int frostFreezeThreshold = 3;
    public float frostFreezeDuration = 1.25f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            if (appliesFrost)
            {
                EnemyStatusEffects statusEffects = other.GetComponentInParent<EnemyStatusEffects>();
                if (statusEffects == null)
                {
                    GameObject statusTarget = enemyHealth != null ? enemyHealth.gameObject : other.transform.root.gameObject;
                    statusEffects = statusTarget.AddComponent<EnemyStatusEffects>();
                }

                statusEffects.ApplyFrost(
                    frostStacksPerHit,
                    frostDuration,
                    frostSlowMultiplier,
                    frostFreezeThreshold,
                    frostFreezeDuration);
            }

            Destroy(gameObject);
        }
    }
}
