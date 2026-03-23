using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 0.5f;
    public int maxEnemyCount = 20;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemyCount)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (EnemyStageDirector.Instance != null)
        {
            EnemyStageDirector.Instance.ApplyStageToEnemy(enemy);
        }
    }
}
