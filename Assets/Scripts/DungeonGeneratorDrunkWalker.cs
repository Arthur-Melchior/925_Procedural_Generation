using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Grid))]
public class DungeonGeneratorDrunkWalker : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int maxSteps;
    [SerializeField] [Min(1)] private int numberOfRooms;
    [SerializeField] [Min(1)] private int growthIterations;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private GameObject leftStairs;
    [SerializeField] private GameObject rightStairs;
    [SerializeField] private GameObject bottomStairs;

    private System.Random _random;
    private Tilemap _grassMap;
    private Tilemap _roomsMap;
    private List<HashSet<Vector3Int>> _rooms;

    private void Start()
    {
        _random = new System.Random();
        _rooms = new List<HashSet<Vector3Int>>();
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        GenerateGrassMap();

        _roomsMap = GenerateTilemap("wall map");

        for (var i = 0; i < numberOfRooms; i++)
        {
            MakeRoom();
        }

        ExpandRooms();
        MergeRooms();
        AddStairs();

        _roomsMap.CompressBounds();
        _roomsMap.GetComponent<TilemapRenderer>().sortingOrder = 1;
    }

    private void ClearDungeon()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
            _rooms.Clear();
        }
    }

    private void AddStairs()
    {
        foreach (var room in _rooms)
        {
            var stairCreated = false;
            var currentIteration = 0;
            var maxIterations = 100000;
            var numberOfStairs = room.Count < 100 ? 1 : room.Count / 100;
            var stairIteration = 0;

            while (!stairCreated && currentIteration < maxIterations && stairIteration != numberOfStairs)
            {
                var randomDirection = _random.Next(0, 4);
                var position = room.ElementAt(_random.Next(0, room.Count));
                switch (randomDirection)
                {
                    case 0:
                    {
                        while (_roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!_roomsMap.GetTile(new Vector3Int(position.x - 1, position.y))
                                && !_roomsMap.GetTile(new Vector3Int(position.x - 2, position.y))
                                && !_roomsMap.GetTile(new Vector3Int(position.x - 3, position.y))
                                && _roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && _roomsMap.GetTile(new Vector3Int(position.x, position.y + 1)))
                            {
                                var stair = Instantiate(leftStairs, _roomsMap.gameObject.transform, true);
                                stair.transform.position = position;
                                stairCreated = true;
                                stairIteration++;
                            }

                            position.x--;
                            currentIteration++;
                        }

                        break;
                    }
                    case 1:
                    {
                        while (_roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!_roomsMap.GetTile(new Vector3Int(position.x, position.y - 1))
                                && !_roomsMap.GetTile(new Vector3Int(position.x, position.y - 2))
                                && !_roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && _roomsMap.GetTile(new Vector3Int(position.x - 1, position.y))
                                && _roomsMap.GetTile(new Vector3Int(position.x + 1, position.y)))
                            {
                                var stair = Instantiate(bottomStairs, _roomsMap.gameObject.transform, true);
                                stair.transform.position = new Vector3(position.x, position.y + 1);
                                stairCreated = true;
                                stairIteration++;
                            }

                            position.y--;
                            currentIteration++;
                        }

                        break;
                    }
                    case 2:
                    {
                        while (_roomsMap.GetTile(new Vector3Int(position.x, position.y)) &&
                               currentIteration < maxIterations)
                        {
                            if (!_roomsMap.GetTile(new Vector3Int(position.x + 1, position.y))
                                && !_roomsMap.GetTile(new Vector3Int(position.x + 2, position.y))
                                && !_roomsMap.GetTile(new Vector3Int(position.x + 3, position.y))
                                && _roomsMap.GetTile(new Vector3Int(position.x, position.y - 3))
                                && _roomsMap.GetTile(new Vector3Int(position.x, position.y + 1)))
                            {
                                var stair = Instantiate(rightStairs, _roomsMap.gameObject.transform, true);
                                stair.transform.position = new Vector3(position.x + 1, position.y);
                                stairCreated = true;
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

    public void ExpandRooms()
    {
        for (var i = 0; i < growthIterations; i++)
        {
            foreach (var room in _rooms)
            {
                //the tempList is there to avoid changing the collection of positions
                var tempList = new List<Vector3Int>();

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
                }

                foreach (var vector3Int in tempList)
                {
                    room.Add(vector3Int);
                }
            }
        }
    }

    private void ExtendTile(Vector3Int position, List<Vector3Int> room)
    {
        _roomsMap.SetTile(position, wallTile);
        room.Add(position);
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
                    var newRoom = room.Concat(nextRoom).ToHashSet();
                    _rooms.Remove(room);
                    _rooms.Remove(nextRoom);
                    _rooms.Add(newRoom);
                    currentRoomIndex = -1;
                    break;
                }
            }
        }
    }

    public void MakeRoom()
    {
        var startingPosition = new Vector3Int(_random.Next(20, sizeX - 20), _random.Next(20, sizeY - 20));
        var room = new HashSet<Vector3Int>();

        for (var i = 0; i < maxSteps; i++)
        {
            startingPosition = TakeAStep(startingPosition, room);
        }

        foreach (var vector3Int in room)
        {
            _roomsMap.SetTile(vector3Int, wallTile);
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