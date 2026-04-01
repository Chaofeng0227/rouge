using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("生成点位 (在预制体中用空物体占位)")]
    public Transform[] enemySpawnPoints;
    public Transform[] propSpawnPoints;
    public Transform centerSpawnPoint;

    [Header("可生成的资源池")]
    public GameObject[] enemyPrefabs;
    public GameObject[] propPrefabs;
    public GameObject chestPrefab;

    [Header("传送门配置")]
    public GameObject portalPrefab;

    [Header("道具生成概率")]
    [Range(0f, 1f)] public float propSpawnChance = 0.5f;

    // --- 新增：刷怪数量与排版配置 ---
    [Header("怪物生成配置")]
    public int minEnemies = 2;           // 最少刷几只怪
    public int maxEnemies = 5;           // 最多刷几只怪
    public float spawnRadius = 1.5f;     // 在生成点附近随机散开的范围，防止重叠

    public void InitializeRoom(bool isStartRoom, bool isEdgeRoom, bool isEndRoom)
    {
        if (isStartRoom) return;

        if (isEndRoom)
        {
            if (centerSpawnPoint != null && portalPrefab != null)
            {
                Instantiate(portalPrefab, centerSpawnPoint.position, Quaternion.identity, transform);
            }

            SpawnEnemies(true);
            return;
        }

        if (isEdgeRoom)
        {
            if (centerSpawnPoint != null && chestPrefab != null)
            {
                Instantiate(chestPrefab, centerSpawnPoint.position, Quaternion.identity, transform);
            }
            SpawnEnemies(true);
        }
        else
        {
            SpawnEnemies(false);
            SpawnProps();
        }
    }

    private void SpawnEnemies(bool isHardMode)
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 1. 掷骰子决定这个房间要刷多少只怪
        int targetEnemyCount = Random.Range(minEnemies, maxEnemies + 1);

        // 如果是困难模式（比如终点房、宝箱房），就在随机出的数量上再额外加点怪！
        if (isHardMode)
        {
            targetEnemyCount += 2;
        }

        // 2. 循环生成目标数量的怪物
        for (int i = 0; i < targetEnemyCount; i++)
        {
            // 随机选一个你预先摆好的生成点位作为“中心点”
            Transform basePoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            if (basePoint == null) continue;

            // 随机选一个怪物图鉴里的怪物
            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // 【核心技巧】：在中心点附近生成一个随机的圆形偏移量，避免怪物像叠罗汉一样挤在同一个像素上
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = basePoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            Instantiate(randomEnemy, spawnPos, Quaternion.identity, transform);
        }
    }

    private void SpawnProps()
    {
        if (propSpawnPoints == null || propSpawnPoints.Length == 0) return;
        if (propPrefabs == null || propPrefabs.Length == 0) return;

        foreach (Transform spawnPoint in propSpawnPoints)
        {
            if (spawnPoint == null) continue;

            if (Random.value < propSpawnChance)
            {
                GameObject randomProp = propPrefabs[Random.Range(0, propPrefabs.Length)];
                Instantiate(randomProp, spawnPoint.position, Quaternion.identity, transform);
            }
        }
    }
}