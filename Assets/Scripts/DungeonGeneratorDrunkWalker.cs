using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Grid))]
public class DungeonGeneratorDrunkWalker : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int maxSteps;
    [SerializeField] [Min(1)] private int growthIteration;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;

    private System.Random _random;
    private List<Vector3Int> _drunkWalkerPath;
    private Tilemap _grassMap;
    private Tilemap _drunkMap;

    private void Start()
    {
        _random = new System.Random();
        _drunkWalkerPath = new List<Vector3Int>();
        GenerateMap();
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
            _drunkWalkerPath.Clear();
        }

        GenerateGrassMap();
        DrunkWalkerMarch();
        ExtendDrunkWalkerPath();
        _drunkMap.CompressBounds();
        _drunkMap.GetComponent<TilemapRenderer>().sortingOrder = 1;
    }

    private void ExtendDrunkWalkerPath()
    {
        for (int i = 0; i < growthIteration; i++)
        {
            foreach (var position in _drunkWalkerPath)
            {
                _drunkMap.SetTile(new Vector3Int(position.x + i, position.y), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x + i, position.y + i), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x, position.y + i), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x - i, position.y + i), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x - i, position.y), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x - i, position.y - i), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x, position.y - i), wallTile);
                _drunkMap.SetTile(new Vector3Int(position.x + i, position.y - i), wallTile);
            }
        }
    }

    private void DrunkWalkerMarch()
    {
        _drunkMap = GenerateTilemap("wall map");
        var startingPosition = new Vector3Int(_random.Next(0, sizeX + 1), _random.Next(0, sizeY + 1));

        for (var i = 0; i < maxSteps; i++)
        {
            startingPosition = TakeAStep(startingPosition);
        }
    }

    private Vector3Int TakeAStep(Vector3Int position)
    {
        //choisir une direction
        var direction = _random.Next(0, 4);

        switch (direction)
        {
            case 0 when position.y + 1 < sizeY:
                position.y++;
                break;
            case 1 when position.x + 1 < sizeX:
                position.x++;
                break;
            case 2 when position.y - 1 > 0:
                position.y--;
                break;
            case 3 when position.x - 1 > 0:
                position.x--;
                break;
        }

        _drunkWalkerPath.Add(position);
        _drunkMap.SetTile(position, wallTile);
        return position;
    }

    private BoundsInt GenerateGrassMap()
    {
        _grassMap = GenerateTilemap("grass map");

        for (var i = 0; i < sizeX; i++)
        {
            for (var j = 0; j < sizeY; j++)
            {
                _grassMap.SetTile(new Vector3Int(i, j), grassTile);
            }
        }

        return _grassMap.cellBounds;
    }

    private Tilemap GenerateTilemap(string tilemapName)
    {
        var objectMap = new GameObject(tilemapName);
        objectMap.transform.SetParent(transform);

        var tilemap = objectMap.AddComponent<Tilemap>();
        objectMap.AddComponent<TilemapRenderer>();
        return tilemap;
    }
}