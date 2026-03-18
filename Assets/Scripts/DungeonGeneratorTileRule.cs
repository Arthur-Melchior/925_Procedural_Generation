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
    [SerializeField] private int roomMaxSizeX;
    [SerializeField] private int roomMaxSizeY;
    [SerializeField] private int roomMinSizeX;
    [SerializeField] private int roomMinSizeY;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase doorTile;

    private int _roomIndex;


    private void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        var grassMap = GenerateGrassMap();
        var dungeonCutter = new DungeonCutter();
        var roomBinaryTree = new Node { CutType = CutType.Vertical, Room = grassMap };
        var leaves = new List<Node>();
        dungeonCutter.Cut(roomBinaryTree, roomMaxSizeX, roomMaxSizeY, leaves);

        foreach (var leaf in leaves)
        {
            leaf.Room = ResizeRoom(leaf.Room);
            GenerateRoom(leaf.Room);
        }
    }

    private BoundsInt ResizeRoom(BoundsInt room)
    {
        var tempRoom = room;
        var randomValueX = new Random().Next(1, 10) * 0.1f;
        var randomValueY = new Random().Next(1, 10) * 0.1f;

        tempRoom.size = new Vector3Int((int)(tempRoom.size.x * randomValueX), (int)(tempRoom.size.y * randomValueY));
        
        if (tempRoom.size.x < roomMinSizeX && tempRoom.size.y < roomMinSizeY)
        {
            return room;
        }

        if (tempRoom.size.x < roomMinSizeX && tempRoom.size.y > roomMinSizeY)
        {
            tempRoom.size = new Vector3Int(room.size.x, tempRoom.size.y);
            tempRoom.position += new Vector3Int((int)(room.center.x - tempRoom.center.x),
                (int)(room.center.y - tempRoom.center.y));
            return tempRoom;
        }

        if (tempRoom.size.x > roomMinSizeX && tempRoom.size.y < roomMinSizeY)
        {
            tempRoom.size = new Vector3Int(tempRoom.size.x, room.size.y);
            tempRoom.position += new Vector3Int((int)(room.center.x - tempRoom.center.x),
                (int)(room.center.y - tempRoom.center.y));
            return tempRoom;
        }

        tempRoom.position += new Vector3Int((int)(room.center.x - tempRoom.center.x),
            (int)(room.center.y - tempRoom.center.y));

        return tempRoom;
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
        objectMap.AddComponent<TilemapRenderer>().sortingOrder = _roomIndex;
        return tilemap;
    }

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

        //add door
        var position = new Vector3Int(tilemap.cellBounds.x + tilemap.cellBounds.size.x / 2, tilemap.cellBounds.y);
        tilemap.SetTile(position, doorTile);
    }
}