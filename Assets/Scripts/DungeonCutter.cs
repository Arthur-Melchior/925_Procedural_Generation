using UnityEngine;

public enum CutType
{
    Vertical,
    Horizontal
}

public class Node
{
    public CutType cutType;
    public BoundsInt room;
    public Node leftNode;
    public Node rightNode;
}

public class DungeonCutter
{
    private const float MaxIterations = 10000f;
    private float _currentIteration;

    public void Cut(Node node, int minSizeX, int minSizeY)
    {
        BoundsInt leftNodeRoom;
        BoundsInt rightNodeRoom;
        CutType nexCutType;

        if (node.cutType == CutType.Vertical)
        {
            var xMiddle = node.room.size.x / 2;
            leftNodeRoom = new BoundsInt(node.room.min, new Vector3Int(xMiddle, node.room.size.y));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.room.x + xMiddle, node.room.yMin),
                new Vector3Int(xMiddle, node.room.size.y));
            nexCutType = CutType.Horizontal;
        }
        else
        {
            var yMiddle = node.room.size.y / 2;
            leftNodeRoom = new BoundsInt(node.room.min, new Vector3Int(node.room.size.x, yMiddle));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.room.xMin, node.room.y + yMiddle),
                new Vector3Int(node.room.size.x, yMiddle));
            nexCutType = CutType.Vertical;
        }

        _currentIteration++;

        Debug.Log(_currentIteration);
        Debug.Log(node.room);

        Debug.DrawLine(node.room.min, new Vector3(node.room.xMax, node.room.y), Color.red, 1000);
        Debug.DrawLine(node.room.min, new Vector3(node.room.x, node.room.yMax), Color.red, 1000);
        Debug.DrawLine(new Vector3(node.room.x, node.room.yMax), node.room.max, Color.red, 1000);
        Debug.DrawLine(new Vector3(node.room.xMax, node.room.y), node.room.max, Color.red, 1000);


        if (leftNodeRoom.size.x < minSizeX || leftNodeRoom.size.y < minSizeY || _currentIteration > MaxIterations)
        {
            return;
        }

        node.leftNode = new Node {cutType = nexCutType, room = leftNodeRoom};
        node.rightNode = new Node {cutType = nexCutType, room = rightNodeRoom};

        Cut(node.leftNode, minSizeX, minSizeY);
        Cut(node.rightNode, minSizeX, minSizeY);
    }
}