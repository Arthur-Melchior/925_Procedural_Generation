using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameObjectFloatPair
{
    public GameObject gameObject;

    [Tooltip(
        "Spawn chances compared to the sum of all spawn forces \n Ex : sum = 1 + 2 + 3 = 6 => 3 = 50% chance to spawn (6/3), 2 = 33.3%, 1 = 16.6%")]
    public float spawnForce;
}

public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The enemy and it's spawn force")]
    public GameObjectFloatPair[] enemiesList;

    [SerializeField] private int maximumNumberOfEnemies = 100;
    [SerializeField] [Min(1)] private int enemiesPerSpawn = 3;
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private PlayerScript player;
    [SerializeField] private EnemiesManager enemiesManager;
    [SerializeField] private float spawnRateIncrementation = 0.01f;
    public float spawnRate = 1f;
    private int _currentNumberOfEnemies;
    private Camera _camera;
    private float _spawnRatio;

    private void Start()
    {
        _camera = Camera.main;
        _spawnRatio = 1 / enemiesList.Sum(enemy => enemy.spawnForce);
    }

    public void StartSpawn()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            if (_currentNumberOfEnemies < maximumNumberOfEnemies)
            {
                var possibleSpawnPoints = map.WalkableTiles.Cast<WalkableTile>()
                    .Where(tile =>
                    {
                        var viewportPos = _camera.WorldToViewportPoint(tile.Position);
                        var visible = viewportPos is {z: > 0, x: > 0 and < 1, y: > 0 and < 1};

                        return tile.RoomIndex == player.currentRoomIndex && tile.IsWalkable && !visible;
                    }).ToArray();

                if (possibleSpawnPoints.Length == 0)
                {
                    possibleSpawnPoints =
                        map.WalkableTiles.Cast<WalkableTile>().Where(tile => tile.RoomIndex == 0).ToArray();
                }

                for (var i = 0; i < enemiesPerSpawn; i++)
                {
                    var enemy = RandomlyChooseEnemy();
                    var enemyScript = enemy.GetComponent<EnemyScript>();
                    var steeringScript = enemy.GetComponent<SteeringScript>();

                    var randomTile = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Length)];
                    enemyScript.player = player;
                    enemyScript.target = player.transform;
                    enemyScript.enemiesManager = enemiesManager;
                    enemyScript.currentRoomIndex = randomTile.RoomIndex;

                    steeringScript.target = player.transform;

                    var instance = Instantiate(enemy, randomTile.Position, new Quaternion(0, 0, 0, 0));
                    var instanceScript = instance.GetComponent<EnemyScript>();
                    instanceScript.onDeath.AddListener(experience =>
                    {
                        IncreaseSpawnRate();
                        _currentNumberOfEnemies--;
                        player.playerStats.experience += experience * spawnRate;
                    });

                    _currentNumberOfEnemies++;
                }
            }

            yield return new WaitForSeconds(1 / spawnRate);
        }
    }

    private GameObject RandomlyChooseEnemy()
    {
        var randomValue = Random.value;

        var probSum = 0f;
        foreach (var enemy in enemiesList)
        {
            var probability = enemy.spawnForce * _spawnRatio;
            if (randomValue <= probability + probSum)
            {
                return enemy.gameObject;
            }

            probSum += probability;
        }

        return null;
    }

    public void IncreaseSpawnRate()
    {
        spawnRate += spawnRateIncrementation;
    }
}