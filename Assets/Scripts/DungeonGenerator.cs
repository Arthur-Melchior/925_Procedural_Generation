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
    [SerializeField] private int roomScale;
    [SerializeField] private bool visualiseGeneration;

    private void Start()
    {
        var node = new Node
            {CutType = CutType.Horizontal, Room = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(sizeX, sizeY))};
        if (visualiseGeneration)
        {
            var cutter = gameObject.AddComponent<DungeonCutterVisualiser>();
            StartCoroutine(cutter.CutPerFrame(node, minSizeX, minSizeY, roomScale));
        }
        else
        {
            var dungeonCutter = new DungeonCutter();
            dungeonCutter.Cut(node, minSizeX, minSizeY, new List<Node>(), roomScale);
        }
    }
}