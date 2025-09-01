using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;       
    public float spawnDelay = 15f;        
    public int maxAliveEnemies = 3;     
    public int maxTotalEnemies = 7;      

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int enemiesSpawned = 0;
    private float spawnTimer = 0f;

    void Update()
    {
        // Clean up nulls from list in case enemies got destroyed outside
        activeEnemies.RemoveAll(item => item == null);

        // Check if can spawn more
        if (enemiesSpawned < maxTotalEnemies && activeEnemies.Count < maxAliveEnemies)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnDelay;
            }
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        activeEnemies.Add(enemy);
        enemiesSpawned++;
    }
}

