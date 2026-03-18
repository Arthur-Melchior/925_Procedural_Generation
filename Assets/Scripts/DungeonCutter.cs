using System;
using System.Collections.Generic;
using UnityEngine;

public enum CutType
{
    Vertical,
    Horizontal
}

public class Node
{
    public CutType CutType;
    public BoundsInt Room;
    public Node LeftNode;
    public Node RightNode;
}

public class DungeonCutter
{
    private const float MaxIterations = 10000f;
    private float _currentIteration;

    public void Cut(Node node, int maxSizeX, int maxSizeY, List<Node> leaves)
    {
        BoundsInt leftNodeRoom;
        BoundsInt rightNodeRoom;
        CutType nexCutType;

        if (node.CutType == CutType.Vertical)
        {
            var xMiddle = node.Room.size.x / 2;
            leftNodeRoom = new BoundsInt(node.Room.min, new Vector3Int(xMiddle, node.Room.size.y));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.Room.x + xMiddle, node.Room.yMin),
                new Vector3Int(xMiddle, node.Room.size.y));
            nexCutType = CutType.Horizontal;
        }
        else
        {
            var yMiddle = node.Room.size.y / 2;
            leftNodeRoom = new BoundsInt(node.Room.min, new Vector3Int(node.Room.size.x, yMiddle));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.Room.xMin, node.Room.y + yMiddle),
                new Vector3Int(node.Room.size.x, yMiddle));
            nexCutType = CutType.Vertical;
        }

        _currentIteration++;

        Debug.DrawLine(node.Room.min, new Vector3(node.Room.xMax, node.Room.y), Color.red, 1000);
        Debug.DrawLine(node.Room.min, new Vector3(node.Room.x, node.Room.yMax), Color.red, 1000);
        Debug.DrawLine(new Vector3(node.Room.x, node.Room.yMax), node.Room.max, Color.red, 1000);
        Debug.DrawLine(new Vector3(node.Room.xMax, node.Room.y), node.Room.max, Color.red, 1000);

        if (leftNodeRoom.size.x < maxSizeX || leftNodeRoom.size.y < maxSizeY || _currentIteration > MaxIterations)
        {
            leaves.Add(node);
            return;
        }

        node.LeftNode = new Node {CutType = nexCutType, Room = leftNodeRoom};
        node.RightNode = new Node {CutType = nexCutType, Room = rightNodeRoom};

        Cut(node.LeftNode, maxSizeX, maxSizeY, leaves);
        Cut(node.RightNode, maxSizeX, maxSizeY, leaves);
    }
}