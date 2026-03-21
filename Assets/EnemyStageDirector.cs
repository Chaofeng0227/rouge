using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStageDirector : MonoBehaviour
{
    public static EnemyStageDirector Instance { get; private set; }

    [SerializeField] private float secondsPerStage = 30f;

    public int CurrentStage => Mathf.Max(0, Mathf.FloorToInt(Time.timeSinceLevelLoad / secondsPerStage));

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _ = LevelUpUI.Instance;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ApplyStageToEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        int stage = CurrentStage;
        if (stage <= 0)
        {
            return;
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth += stage;
            enemyHealth.SyncCurrentHealthToMax();
        }

        EnemyMoveToPlayer enemyMove = enemy.GetComponent<EnemyMoveToPlayer>();
        if (enemyMove != null)
        {
            enemyMove.moveSpeed += stage * 0.15f;
        }

        EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
        if (enemyDamage != null)
        {
            enemyDamage.damage += stage / 3;
        }
    }
}
