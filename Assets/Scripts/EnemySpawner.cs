using System;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;
using Random = System.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemySo> enemiesList;
    public List<Transform> spawnPoints;
    public float enemiesBudget;

    private bool _enemiesSpawned;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_enemiesSpawned) return;
        SpawnEnemies();
        _enemiesSpawned = true;
    }

    private void SpawnEnemies()
    {
        var random = new Random();
        foreach (var spawnPoint in spawnPoints)
        {
            var rng = random.Next(enemiesList.Count);
            var enemy = enemiesList[rng];
            if (enemy.cost < enemiesBudget)
            {
                Instantiate(enemy.prefab, spawnPoint);
                enemiesBudget -= enemy.cost;
            }
        }
    }
}