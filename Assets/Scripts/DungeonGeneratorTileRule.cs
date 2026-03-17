using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

[RequireComponent(typeof(Grid))]
public class DungeonGeneratorTileRule : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int minSizeX;
    [SerializeField] private int minSizeY;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;

    [Tooltip("If the scaled down room is smaller than the min size the room won't be created")] [SerializeField]
    private float roomsScale;

    private int _roomIndex;


    private void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        var grassMap = GenerateGrassMap();
        var dungeonCutter = new DungeonCutter();
        var roomBinaryTree = new Node {cutType = CutType.Vertical, room = grassMap};
        var leaves = new List<Node>();
        dungeonCutter.Cut(roomBinaryTree, minSizeX, minSizeY, leaves, roomsScale);

        foreach (var leaf in leaves)
        {
            GenerateRoom(leaf.room);
        }
    }

    private BoundsInt GenerateGrassMap()
    {
        var tilemap = GenerateTilemap("grass map");

        for (var i = 0; i < sizeX; i++)
        {
            for (var j = 0; j < sizeY; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j), grassTile);
            }
        }

        return tilemap.cellBounds;
    }

    private Tilemap GenerateTilemap(string tilemapName)
    {
        var objectMap = new GameObject(tilemapName);
        objectMap.transform.SetParent(transform);

        var tilemap = objectMap.AddComponent<Tilemap>();
        objectMap.AddComponent<TilemapRenderer>();
        return tilemap;
    }

    public TileBase test;

    private void GenerateRoom(BoundsInt bounds)
    {
        var tilemap = GenerateTilemap($"room {++_roomIndex}");

        for (var i = bounds.x; i < bounds.xMax; i++)
        {
            for (var j = bounds.y; j < bounds.yMax; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j), wallTile);
            }
        }
        
        tilemap.CompressBounds();

        //choisis une direction aléatoire
        // var random = new Random().Next(0, 4);
        // if (random == 0)
        // {
        // }
        var position = new Vector3Int(tilemap.cellBounds.x + tilemap.cellBounds.size.x / 2, tilemap.cellBounds.y);
        tilemap.SetTile(position, test);
    }
}