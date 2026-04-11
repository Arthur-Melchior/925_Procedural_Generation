using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class WalkableTile
{
    public Vector3Int position;
    public bool isWalkable;

    public WalkableTile(Vector3Int position, bool isWalkable)
    {
        this.position = position;
        this.isWalkable = isWalkable;
    }
}

[RequireComponent(typeof(Grid))]
public class DungeonGenerator : MonoBehaviour
{
    public WalkableTile[,] walkableTiles;
    public UnityEvent onGenerationFinished;

    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int maxSteps;
    [SerializeField] [Min(1)] private int numberOfRooms;
    [SerializeField] [Min(1)] private int growthIterations;
    [SerializeField] [Min(1)] private int tilePerStair = 200;
    [SerializeField] [Min(3)] private int spaceBetweenStairs = 10;

    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private GameObject leftStairs;
    [SerializeField] private GameObject rightStairs;
    [SerializeField] private GameObject bottomStairs;

    [HideInInspector] public Tilemap grassMap;
    [HideInInspector] public Tilemap roomsMap;

    private Dictionary<HashSet<Vector3Int>, HashSet<Vector3Int>> _wallPositions;
    private System.Random _random;
    private List<HashSet<Vector3Int>> _rooms;

    private void Start()
    {
        _random = new System.Random();
        _rooms = new List<HashSet<Vector3Int>>();
        _wallPositions = new Dictionary<HashSet<Vector3Int>, HashSet<Vector3Int>>();
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
        FillWallList();
        AddStairs();
        GenerateWalkableTiles();

        roomsMap.CompressBounds();
        roomsMap.GetComponent<TilemapRenderer>().sortingOrder = 1;

        onGenerationFinished?.Invoke();
    }

    private void FillWallList()
    {
        foreach (var room in _rooms)
        {
            _wallPositions.Add(room, new HashSet<Vector3Int>());
            foreach (var position in room)
            {
                //if one neighbor is empty
                if (!roomsMap.HasTile(new Vector3Int(position.x, position.y + 1))
                    || !roomsMap.HasTile(new Vector3Int(position.x + 1, position.y + 1))
                    || !roomsMap.HasTile(new Vector3Int(position.x + 1, position.y))
                    || !roomsMap.HasTile(new Vector3Int(position.x + 1, position.y - 1))
                    || !roomsMap.HasTile(new Vector3Int(position.x, position.y - 1))
                    || !roomsMap.HasTile(new Vector3Int(position.x - 1, position.y - 1))
                    || !roomsMap.HasTile(new Vector3Int(position.x - 1, position.y))
                    || !roomsMap.HasTile(new Vector3Int(position.x - 1, position.y + 1))
                   )
                {
                    _wallPositions[room].Add(position);
                }
            }
        }
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
        foreach (var room in _wallPositions)
        {
            //how many stairs should there be in this room
            var numberOfStairs = _rooms.First(r => r == room.Key).Count / tilePerStair;
            if (numberOfStairs < 1)
            {
                numberOfStairs = 1;
            }

            var instantiatedStairs = 0;
            var distanceBetweenStairs = spaceBetweenStairs;
            for (var i = 0;
                 i < room.Value.Count - 3 && instantiatedStairs < numberOfStairs;
                 i++, distanceBetweenStairs++)
            {
                if (distanceBetweenStairs < spaceBetweenStairs) continue;

                var first = room.Value.ElementAt(i);
                var second = room.Value.ElementAt(i + 1);
                var third = room.Value.ElementAt(i + 2);
                var fourth = room.Value.ElementAt(i + 3);

                //if three walls align
                if (first.x == second.x &&
                    first.x == third.x &&
                    first.x == fourth.x)
                {
                    //if there is no room three tiles in front
                    //and is inbound
                    if (first.x + 3 < sizeX && !SpaceIsOccupied(room, i, 3, 1, 0))
                    {
                        var stairs = Instantiate(rightStairs, transform);
                        stairs.transform.position = new Vector3(first.x + 1, first.y);
                        instantiatedStairs++;
                        distanceBetweenStairs = 0;
                    }
                    else if (first.x - 3 > 0 && !SpaceIsOccupied(room, i, 3, -1, 0))
                    {
                        var stairs = Instantiate(leftStairs, transform);
                        stairs.transform.position = new Vector3(fourth.x, fourth.y);
                        instantiatedStairs++;
                        distanceBetweenStairs = 0;
                    }
                }
                else if (first.y == second.y &&
                         first.y == third.y &&
                         first.y == fourth.y)
                {
                    if (first.y - 3 > 0 && !SpaceIsOccupied(room, i, 3, 0, -1))
                    {
                        var stairs = Instantiate(bottomStairs, transform);
                        stairs.transform.position = new Vector3(second.x, second.y + 1);
                        instantiatedStairs++;
                        distanceBetweenStairs = 0;
                    }
                }
            }
        }
    }

    private bool SpaceIsOccupied(KeyValuePair<HashSet<Vector3Int>, HashSet<Vector3Int>> room, int index, int size,
        int x, int y)
    {
        var first = room.Value.ElementAt(index);
        var second = room.Value.ElementAt(index + 1);
        var third = room.Value.ElementAt(index + 2);
        var fourth = room.Value.ElementAt(index + 3);

        for (var i = 1; i < size + 1; i++)
        {
            if (roomsMap.HasTile(new Vector3Int(first.x + x * i, first.y + y * i)) ||
                roomsMap.HasTile(new Vector3Int(second.x + x * i, second.y + y * i)) ||
                roomsMap.HasTile(new Vector3Int(third.x + x * i, third.y + y * i)) ||
                roomsMap.HasTile(new Vector3Int(fourth.x + x * i, fourth.y + y * i))
               ) return true;
        }

        return false;
    }

    private void GenerateWalkableTiles()
    {
        walkableTiles = new WalkableTile[sizeX, sizeY];

        foreach (var tile in grassMap.cellBounds.allPositionsWithin)
        {
            walkableTiles[tile.x, tile.y] = new WalkableTile(tile, true);
        }

        foreach (var wall in _wallPositions)
        {
            foreach (var vector3Int in wall.Value)
            {
                walkableTiles[vector3Int.x, vector3Int.y].isWalkable = false;
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
        if (position.x < 0 || position.x > sizeX || position.y < 0 || position.y > sizeY)
        {
            return;
        }

        roomsMap.SetTile(position, wallTile);
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

    public void GenerateRoom()
    {
        var startingPosition =
            new Vector3Int(_random.Next(0, (int) (sizeX * 0.8f)), _random.Next(0, (int) (sizeY * 0.8f)));
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