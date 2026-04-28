using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject key;
    [SerializeField] private DungeonGenerator map;

    public void MovePlayerToSpawn()
    {
        var spawnPoint = GameObject.Find("Spawn Point");
        player.position = spawnPoint.transform.position;
    }

    public void PutKeyOnMap()
    {
        var x = Random.Range(0, map.sizeX);
        var y = Random.Range(0, map.sizeY);
        var position = map.grassMap.CellToWorld(new Vector3Int(x, y));
        Instantiate(key, position, new Quaternion(0, 0, 0, 0));
    }
}