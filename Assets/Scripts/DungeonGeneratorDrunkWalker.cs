using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class WalkableTile
{
    public Vector3Int Position;
    public bool IsWalkable;

    public WalkableTile(Vector3Int position, bool isWalkable)
    {
        Position = position;
        IsWalkable = isWalkable;
    }
}

[RequireComponent(typeof(Grid))]
public class DungeonGeneratorDrunkWalker : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int maxSteps;
    [SerializeField] [Min(1)] private int numberOfRooms;
    [SerializeField] [Min(1)] private int growthIterations;
    [SerializeField] [Min(1)] private int tilePerStair = 200;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private GameObject leftStairs;
    [SerializeField] private GameObject rightStairs;
    [SerializeField] private GameObject bottomStairs;

    [HideInInspector] public Tilemap grassMap;
    [HideInInspector] public Tilemap roomsMap;
    public HashSet<WalkableTile> WalkableTiles = new();
    private HashSet<Vector3Int> _wallPositions;

    private System.Random _random;
    private List<HashSet<Vector3Int>> _rooms;

    private void Start()
    {
        _random = new System.Random();
        _rooms = new List<HashSet<Vector3Int>>();
        _wallPositions = new HashSet<Vector3Int>();
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        GenerateGrassMap();

        roomsMap = GenerateTilemap("wall map");

        for (var i = 0; i < numberOfRooms; i++)
        {
            GenerateRoom();
        }

        ExpandRooms();
        MergeRooms();
        AddStairs();
        GenerateWalkableTiles();

        roomsMap.CompressBounds();
        roomsMap.GetComponent<TilemapRenderer>().sortingOrder = 1;
    }

    private void ClearDungeon()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
            _rooms.Clear();
            _wallPositions.Clear();
        }
    }

    private void AddStairs()
    {
        foreach (var room in _rooms)
        {
            var currentIteration = 0;
            var maxIterations = 100000;
            var stairIteration = 0;
            var numberOfStairs = room.Count / tilePerStair;
            if (numberOfStairs < 1) numberOfStairs = 1;

            while (currentIteration < maxIterations && stairIteration != numberOfStairs)
            {
                var randomDirection = _random.Next(0, 4);
                var position = room.ElementAt(_random.Next(0, room.Count));
                switch (randomDirection)
                {
                    case 0:
                    {
                        while (roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!roomsMap.GetTile(new Vector3Int(position.x - 1, position.y))
                                && !roomsMap.GetTile(new Vector3Int(position.x - 2, position.y))
                                && !roomsMap.GetTile(new Vector3Int(position.x - 3, position.y))
                                && roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && roomsMap.GetTile(new Vector3Int(position.x, position.y + 1)))
                            {
                                var stair = Instantiate(leftStairs, roomsMap.gameObject.transform, true);
                                stair.transform.position = position;
                                stairIteration++;
                            }

                            position.x--;
                            currentIteration++;
                        }

                        break;
                    }
                    case 1:
                    {
                        while (roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!roomsMap.GetTile(new Vector3Int(position.x, position.y - 1))
                                && !roomsMap.GetTile(new Vector3Int(position.x, position.y - 2))
                                && !roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && roomsMap.GetTile(new Vector3Int(position.x - 1, position.y))
                                && roomsMap.GetTile(new Vector3Int(position.x + 1, position.y)))
                            {
                                var stair = Instantiate(bottomStairs, roomsMap.gameObject.transform, true);
                                stair.transform.position = new Vector3(position.x, position.y + 1);
                                stairIteration++;
                            }

                            position.y--;
                            currentIteration++;
                        }

                        break;
                    }
                    case 2:
                    {
                        while (roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!roomsMap.GetTile(new Vector3Int(position.x + 1, position.y))
                                && !roomsMap.GetTile(new Vector3Int(position.x + 2, position.y))
                                && !roomsMap.GetTile(new Vector3Int(position.x + 3, position.y))
                                && roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && roomsMap.GetTile(new Vector3Int(position.x, position.y + 1)))
                            {
                                var stair = Instantiate(rightStairs, roomsMap.gameObject.transform, true);
                                stair.transform.position = new Vector3(position.x + 1, position.y);
                                stairIteration++;
                            }

                            position.x++;
                            currentIteration++;
                        }

                        break;
                    }
                }
            }
        }
    }

    private void GenerateWalkableTiles()
    {
        foreach (var tile in grassMap.cellBounds.allPositionsWithin)
        {
            WalkableTiles.Add(new WalkableTile(tile, true));
        }

        foreach (var wall in _wallPositions)
        {
            var w = WalkableTiles.FirstOrDefault(w => w.Position == wall);
            if (w != null)
            {
                w.IsWalkable = false;
            }
        }
    }

    public void ExpandRooms()
    {
        for (var i = 0; i < growthIterations; i++)
        {
            foreach (var room in _rooms)
            {
                //the tempList is there to avoid changing the collection of positions
                var tempList = new List<Vector3Int>();
                var tempList2 = new List<Vector3Int>();

                foreach (var position in room)
                {
                    ExtendTile(new Vector3Int(position.x, position.y + 1), tempList);
                    ExtendTile(new Vector3Int(position.x + 1, position.y + 1), tempList);
                    ExtendTile(new Vector3Int(position.x + 1, position.y), tempList);
                    ExtendTile(new Vector3Int(position.x + 1, position.y - 1), tempList);
                    ExtendTile(new Vector3Int(position.x, position.y - 1), tempList);
                    ExtendTile(new Vector3Int(position.x - 1, position.y - 1), tempList);
                    ExtendTile(new Vector3Int(position.x - 1, position.y), tempList);
                    ExtendTile(new Vector3Int(position.x - 1, position.y + 1), tempList);
                    tempList2.Add(position);
                }

                foreach (var vector3Int in tempList)
                {
                    room.Add(vector3Int);
                }

                foreach (var vector3Int in tempList2)
                {
                    _wallPositions.Remove(vector3Int);
                }
            }
        }
    }

    private void ExtendTile(Vector3Int position, List<Vector3Int> room)
    {
        roomsMap.SetTile(position, wallTile);
        room.Add(position);
        _wallPositions.Add(position);
    }

    private void MergeRooms()
    {
        for (var currentRoomIndex = 0; currentRoomIndex < _rooms.Count - 1; currentRoomIndex++)
        {
            var room = _rooms[currentRoomIndex];
            for (var nextRoomIndex = currentRoomIndex + 1; nextRoomIndex < _rooms.Count; nextRoomIndex++)
            {
                var nextRoom = _rooms[nextRoomIndex];

                if (room.Overlaps(nextRoom))
                {
                    var newRoom = Enumerable.ToHashSet(room.Concat(nextRoom));
                    _rooms.Remove(room);
                    _rooms.Remove(nextRoom);
                    _rooms.Add(newRoom);
                    currentRoomIndex = -1;
                    break;
                }
            }
        }
    }

    public void GenerateRoom()
    {
        var startingPosition =
            new Vector3Int(_random.Next(0, (int)(sizeX * 0.8f)), _random.Next(0, (int)(sizeY * 0.8f)));
        var room = new HashSet<Vector3Int>();

        for (var i = 0; i < maxSteps; i++)
        {
            startingPosition = TakeAStep(startingPosition, room);
        }

        foreach (var vector3Int in room)
        {
            roomsMap.SetTile(vector3Int, wallTile);
        }

        _rooms.Add(room);
    }

    private Vector3Int TakeAStep(Vector3Int position, HashSet<Vector3Int> room)
    {
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

        room.Add(position);

        return position;
    }

    private BoundsInt GenerateGrassMap()
    {
        grassMap = GenerateTilemap("grass map");

        for (var i = 0; i < sizeX; i++)
        {
            for (var j = 0; j < sizeY; j++)
            {
                grassMap.SetTile(new Vector3Int(i, j), grassTile);
            }
        }

        return grassMap.cellBounds;
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