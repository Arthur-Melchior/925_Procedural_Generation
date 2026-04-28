using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemiesList;
    public float spawnRate = 0.5f;

    public void StartSpawn()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            var enemy = enemiesList[Random.Range(0, enemiesList.Count)];
            Instantiate(enemy);

            yield return new WaitForSeconds(spawnRate);
        }
    }
}