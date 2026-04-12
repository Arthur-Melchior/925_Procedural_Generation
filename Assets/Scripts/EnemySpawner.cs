using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemiesList;

    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            var enemy = enemiesList[Random.Range(0, enemiesList.Count)];
            Instantiate(enemy);

            yield return new WaitForSeconds(0.5f);
        }
    }
}