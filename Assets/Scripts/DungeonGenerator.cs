using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using Random = System.Random;

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
    public List<GameObject> Stairs;
    public UnityEvent onGenerationFinished;
    public int sizeX;
    public int sizeY;

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

    [SerializeField] private GameObject safeRoom;

    [HideInInspector] public Tilemap grassMap;
    [HideInInspector] public Tilemap roomsMap;

    private Dictionary<HashSet<Vector3Int>, HashSet<Vector3Int>> _wallPositions;
    private Random _random;
    private Tilemap _outerWall;
    private List<HashSet<Vector3Int>> _rooms;

    private void Start()
    {
        _random = new Random();
        _rooms = new List<HashSet<Vector3Int>>();
        _wallPositions = new Dictionary<HashSet<Vector3Int>, HashSet<Vector3Int>>();
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        GenerateGrassMap();
        GenerateOuterWall();
        GenerateSafeRoom();

        roomsMap = GenerateTilemap("wall map");

        for (var i = 0; i < numberOfRooms; i++)
        {
            GenerateRoom();
        }

        ExpandRooms();
        MergeRooms();
        FillWallList();
        GenerateWalkableTiles();
        AddStairs();
        DrawRooms();

        grassMap.GetComponent<TilemapRenderer>().sortingOrder = 1;

        onGenerationFinished?.Invoke();
    }

    private void DrawRooms()
    {
        Destroy(roomsMap.gameObject);

        for (var index = 0; index < _rooms.Count; index++)
        {
            var room = _rooms[index];
            var tilemap = GenerateTilemap($"Room {index + 1}");

            foreach (var vector3Int in room)
            {
                tilemap.SetTile(vector3Int, wallTile);
            }

            var tileCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var rb = tilemap.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            tilemap.gameObject.AddComponent<CompositeCollider2D>();

            tilemap.CompressBounds();
            tilemap.GetComponent<TilemapRenderer>().sortingOrder = 2;

            foreach (var stair in _tempStairsList.Where(stair => stair.Value == index))
            {
                stair.Key.transform.SetParent(tilemap.transform);
            }
        }
    }

    private void ClearDungeon()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        _rooms.Clear();
        _wallPositions.Clear();
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


    private void GenerateOuterWall()
    {
        _outerWall = GenerateTilemap("outer wall map");

        for (var i = 0; i < sizeX; i++)
        {
            for (var j = 0; j < sizeY; j++)
            {
                _outerWall.SetTile(new Vector3Int(i, j), wallTile);
            }
        }

        _outerWall.gameObject.AddComponent<TilemapCollider2D>().compositeOperation =
            Collider2D.CompositeOperation.Merge;
        _outerWall.GetComponent<TilemapRenderer>().sortingOrder = 3;
        _outerWall.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        _outerWall.gameObject.AddComponent<CompositeCollider2D>();
    }

    private void GenerateSafeRoom()
    {
        Instantiate(safeRoom, new Vector3(sizeX / 2, -4),
            transform.localRotation).transform.SetParent(_outerWall.transform);
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

    private readonly Dictionary<GameObject, int> _tempStairsList = new();

    /// <summary>
    /// This methode is horrible, i'm sorry
    /// </summary>
    private void AddStairs()
    {
        var roomIndex = 0;
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
                        Stairs.Add(stairs);
                        instantiatedStairs++;
                        distanceBetweenStairs = 0;
                        walkableTiles[first.x, first.y].isWalkable = true;
                        _tempStairsList.Add(stairs, roomIndex);
                    }
                    else if (first.x - 3 > 0 && !SpaceIsOccupied(room, i, 3, -1, 0))
                    {
                        var stairs = Instantiate(leftStairs, transform);
                        stairs.transform.position = new Vector3(fourth.x, fourth.y);
                        Stairs.Add(stairs);
                        instantiatedStairs++;
                        distanceBetweenStairs = 0;
                        walkableTiles[fourth.x, fourth.y].isWalkable = true;
                        _tempStairsList.Add(stairs, roomIndex);
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
                        walkableTiles[second.x, second.y].isWalkable = true;
                        Stairs.Add(stairs);
                        _tempStairsList.Add(stairs, roomIndex);
                    }
                }
            }

            roomIndex++;
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
        var randomPosition =
            new Vector3Int(_random.Next(0, (int) (sizeX * 0.8f)), _random.Next(0, (int) (sizeY * 0.8f)));
        var room = new HashSet<Vector3Int> {randomPosition};

        for (var i = 0; i < maxSteps; i++)
        {
            randomPosition = TakeAStep(randomPosition);
            room.Add(randomPosition);
        }

        foreach (var vector3Int in room)
        {
            roomsMap.SetTile(vector3Int, wallTile);
        }

        _rooms.Add(room);
    }

    private Vector3Int TakeAStep(Vector3Int position)
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

        return position;
    }

    private Tilemap GenerateTilemap(string tilemapName)
    {
        var objectMap = new GameObject(tilemapName);
        objectMap.transform.SetParent(transform);

        var tilemap = objectMap.AddComponent<Tilemap>();
        objectMap.AddComponent<TilemapRenderer>();
        return tilemap;
    }

    private bool _drawWalkableTiles;

    public void DrawWalkableTiles()
    {
        _drawWalkableTiles = !_drawWalkableTiles;
    }

    private void OnDrawGizmos()
    {
        if (_drawWalkableTiles)
        {
            foreach (var walkableTile in walkableTiles)
            {
                Gizmos.color = walkableTile.isWalkable ? Color.green : Color.red;
                Gizmos.DrawWireCube(new Vector3(walkableTile.position.x + 0.5f, walkableTile.position.y + 0.5f),
                    new Vector3(1f, 1f));
            }
        }
    }
}