using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

internal struct DungeonRoom
{
    public GameObject prefab;
    public int sizeX;
    public int sizeY;
}

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    [SerializeField] private int minSizeX;
    [SerializeField] private int minSizeY;
    [SerializeField] private TileBase defaultTile;
    [SerializeField] private List<GameObject> rooms;
    [SerializeField] private float distanceBetweenRoomsX;
    [SerializeField] private float distanceBetweenRoomsY;
    [SerializeField] private bool visualiseGeneration;
    private readonly List<DungeonRoom> _dungeonRoomsList = new();

    private void Start()
    {
        // PopulateDungeonRoomsList();
        // DrawDefault();
        // DrawRooms();
        var node = new Node
            {cutType = CutType.Horizontal, room = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(sizeX, sizeY))};
        if (visualiseGeneration)
        {
            var cutter = gameObject.AddComponent<DungeonCutterVisualiser>();
            StartCoroutine(cutter.CutPerFrame(node, minSizeX, minSizeY));
        }
        else
        {
            var dungeonCutter = new DungeonCutter();
            dungeonCutter.Cut(node, minSizeX, minSizeY);
        }
    }

    private void DrawRooms()
    {
        var distanceSinceLastRoomX = 0f;
        var distanceSinceLastRoomY = 0f;
        for (var i = 0; i < sizeX; i++)
        {
            var randomNumber = Mathf.RoundToInt(Random.Range(0, _dungeonRoomsList.Count));
            var randomRoom = _dungeonRoomsList[randomNumber];
            for (var j = 0; j < sizeY; j++)
            {
                distanceSinceLastRoomY++;
            }

            distanceSinceLastRoomX++;
        }
    }

    private void PopulateDungeonRoomsList()
    {
        foreach (var room in rooms)
        {
            var dungeonRoom = new DungeonRoom
            {
                prefab = room
            };
            for (var i = 0; i < room.transform.childCount; i++)
            {
                var child = room.transform.GetChild(i);
                if (child.TryGetComponent<Tilemap>(out var tilemap))
                {
                    if (tilemap.cellBounds.x > dungeonRoom.sizeX)
                    {
                        dungeonRoom.sizeX = tilemap.cellBounds.x;
                    }

                    if (tilemap.cellBounds.y > dungeonRoom.sizeY)
                    {
                        dungeonRoom.sizeY = tilemap.cellBounds.y;
                    }
                }
            }

            _dungeonRoomsList.Add(dungeonRoom);
        }
    }

    private void DrawDefault()
    {
        if (transform.Find("default layer"))
        {
            return;
        }

        var defaultLayer = new GameObject("default layer")
        {
            transform =
            {
                parent = transform
            }
        };

        var defaultTilemap = defaultLayer.AddComponent<Tilemap>();
        defaultLayer.AddComponent<TilemapRenderer>();
        defaultLayer.AddComponent<Grid>();

        var defaultPosition = defaultTilemap.WorldToCell(transform.position);

        for (var i = 0; i < sizeX; i++)
        {
            defaultPosition.x = i;
            for (var j = 0; j < sizeY; j++)
            {
                defaultPosition.y = j;
                defaultTilemap.SetTile(defaultPosition, defaultTile);
            }
        }
    }
}