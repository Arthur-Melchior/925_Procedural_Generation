using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemiesList;
    [SerializeField] private int maximumNumberOfEnemies = 100;
    [SerializeField] [Min(1)] private int enemiesPerSpawn;
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private PlayerScript player;
    [SerializeField] private EnemiesManager enemiesManager;
    [SerializeField] private float spawnRateIncrementation = 0.1f;
    private Camera _camera;
    public int currentNumberOfEnemies;
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
            if (currentNumberOfEnemies < maximumNumberOfEnemies)
            {
                var possibleSpawnPoints = map.WalkableTiles.Cast<WalkableTile>()
                    .Where(tile =>
                    {
                        var viewportPos = _camera.WorldToViewportPoint(tile.Position);
                        var visible = viewportPos is { z: > 0, x: > 0 and < 1, y: > 0 and < 1 };

                        return tile.RoomIndex == player.currentRoomIndex && tile.IsWalkable && !visible;
                    }).ToArray();

                if (possibleSpawnPoints.Length == 0)
                {
                    possibleSpawnPoints =
                        map.WalkableTiles.Cast<WalkableTile>().Where(tile => tile.RoomIndex == 0).ToArray();
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

                    var position = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Length)].Position;

                    var instance = Instantiate(enemy, position, new Quaternion(0, 0, 0, 0));
                    var instanceScript = instance.GetComponent<EnemyScript>();
                    instanceScript.onDeath.AddListener(IncreaseSpawnRate);
                    instanceScript.onDeath.AddListener(() => currentNumberOfEnemies--);

                    currentNumberOfEnemies++;
                }
            }

            yield return new WaitForSeconds(1 / spawnRate);
        }
    }

    public void IncreaseSpawnRate()
    {
        spawnRate += spawnRateIncrementation;
    }
}