using System.Collections.Generic;
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

    [Header("怪物生成配置")]
    public int minEnemies = 2;
    public int maxEnemies = 5;
    public float spawnRadius = 1.5f;
    [Range(0f, 1f)] public float eliteSpawnChance = 0.18f;
    [Range(0f, 1f)] public float hardRoomEliteSpawnChance = 0.72f;

    public void InitializeRoom(bool isStartRoom, bool isEdgeRoom, bool isEndRoom, bool suppressPortal = false)
    {
        if (isStartRoom)
        {
            return;
        }

        if (isEndRoom)
        {
            if (!suppressPortal && centerSpawnPoint != null && portalPrefab != null)
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

    void SpawnEnemies(bool isHardMode)
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        List<GameObject> spawnedEnemies = new List<GameObject>();
        int targetEnemyCount = Random.Range(minEnemies, maxEnemies + 1);

        if (isHardMode)
        {
            targetEnemyCount += 2;
        }

        for (int i = 0; i < targetEnemyCount; i++)
        {
            Transform basePoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            if (basePoint == null) continue;

            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = basePoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject spawnedEnemy = Instantiate(randomEnemy, spawnPos, Quaternion.identity, transform);
            spawnedEnemies.Add(spawnedEnemy);
        }

        if (spawnedEnemies.Count == 0)
        {
            return;
        }

        float chance = isHardMode ? hardRoomEliteSpawnChance : eliteSpawnChance;
        if (Random.value <= chance)
        {
            GameObject eliteEnemy = spawnedEnemies[Random.Range(0, spawnedEnemies.Count)];
            PromoteToElite(eliteEnemy);
        }
    }

    void PromoteToElite(GameObject enemy)
    {
        if (enemy == null || enemy.GetComponent<EnemyEliteShooter>() != null)
        {
            return;
        }

        EnemyEliteShooter eliteShooter = enemy.AddComponent<EnemyEliteShooter>();
        eliteShooter.InitializeElite();
    }

    void SpawnProps()
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
