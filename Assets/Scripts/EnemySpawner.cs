using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemiesList;
    [SerializeField] [Min(1)] private int enemiesPerSpawn;
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private TopDownCharacterController player;
    [SerializeField] private EnemiesManager enemiesManager;
    [SerializeField] private float spawnRateIncrementation = 0.1f;
    private Camera _camera;
    public float spawnRate = 2f;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void StartSpawn()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            var possibleSpawnPoints = map.walkableTiles.Cast<WalkableTile>()
                .Where(tile =>
                {
                    var viewportPos = _camera.WorldToViewportPoint(tile.position);
                    var visible = viewportPos is { z: > 0, x: > 0 and < 1, y: > 0 and < 1 };

                    return tile.roomIndex == player.currentRoomIndex && tile.isWalkable && !visible;
                }).ToArray();

            if (possibleSpawnPoints.Length == 0)
            {
                possibleSpawnPoints =
                    map.walkableTiles.Cast<WalkableTile>().Where(tile => tile.roomIndex == 0).ToArray();
            }


            for (var i = 0; i < enemiesPerSpawn; i++)
            {
                var enemy = enemiesList[Random.Range(0, enemiesList.Count)];

                var enemyScript = enemy.GetComponent<EnemyScript>();
                enemyScript.player = player;
                enemyScript.target = player.transform;
                enemyScript.enemiesManager = enemiesManager;
                enemyScript.currentRoomIndex = player.currentRoomIndex;

                var steeringScript = enemy.GetComponent<SteeringScript>();
                steeringScript.target = player.transform;

                var position = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Length)].position;

                Instantiate(enemy, position, new Quaternion(0, 0, 0, 0));
                enemyScript.onDeath.AddListener(IncreaseSpawnRate);
            }

            yield return new WaitForSeconds(1 / spawnRate);
        }
    }

    public void IncreaseSpawnRate()
    {
        spawnRate += spawnRateIncrementation;
    }
}