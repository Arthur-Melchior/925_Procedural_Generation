using System.Collections;
using UnityEngine;

public class DungeonCutterVisualiser : MonoBehaviour
{
    private const float MaxIterations = 10000f;
    private float _currentIteration;

    public IEnumerator CutPerFrame(Node node, int minSizeX, int minSizeY, int roomScale)
    {
        BoundsInt leftNodeRoom;
        BoundsInt rightNodeRoom;
        CutType nextCutType;

        if (node.CutType == CutType.Vertical)
        {
            var xMiddle = node.Room.size.x / 2;
            leftNodeRoom = new BoundsInt(node.Room.min, new Vector3Int(xMiddle, node.Room.size.y));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.Room.x + xMiddle, node.Room.yMin),
                new Vector3Int(xMiddle, node.Room.size.y));
            nextCutType = CutType.Horizontal;
        }
        else
        {
            var yMiddle = node.Room.size.y / 2;
            leftNodeRoom = new BoundsInt(node.Room.min, new Vector3Int(node.Room.size.x, yMiddle));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.Room.xMin, node.Room.y + yMiddle),
                new Vector3Int(node.Room.size.x, yMiddle));
            nextCutType = CutType.Vertical;
        }
        
        leftNodeRoom.size = new Vector3Int(leftNodeRoom.size.x / 10 * 9, leftNodeRoom.size.y / 10 * roomScale);
        rightNodeRoom.size =
            new Vector3Int(rightNodeRoom.size.x / 10 * roomScale, rightNodeRoom.size.y / 10 * roomScale);

        //reposition rooms in the center
        leftNodeRoom.position += new Vector3Int((int) (leftNodeRoom.center.x - node.Room.center.x),
            (int) (leftNodeRoom.center.y - node.Room.center.x));
        rightNodeRoom.position = new Vector3Int((int) (rightNodeRoom.center.x - node.Room.center.x),
            (int) (rightNodeRoom.center.y - node.Room.center.x));

        _currentIteration++;

        Debug.Log(_currentIteration);
        Debug.Log(node.Room);

        Debug.DrawLine(node.Room.min, new Vector3(node.Room.xMax, node.Room.y), Color.red, 1000);
        Debug.DrawLine(node.Room.min, new Vector3(node.Room.x, node.Room.yMax), Color.red, 1000);
        Debug.DrawLine(new Vector3(node.Room.x, node.Room.yMax), node.Room.max, Color.red, 1000);
        Debug.DrawLine(new Vector3(node.Room.xMax, node.Room.y), node.Room.max, Color.red, 1000);

        // ⬇️ attendre la frame suivante
        yield return null;

        if (leftNodeRoom.size.x < minSizeX || leftNodeRoom.size.y < minSizeY || _currentIteration > MaxIterations)
        {
            yield break;
        }

        node.LeftNode = new Node {CutType = nextCutType, Room = leftNodeRoom};
        node.RightNode = new Node {CutType = nextCutType, Room = rightNodeRoom};

        yield return StartCoroutine(CutPerFrame(node.LeftNode, minSizeX, minSizeY, roomScale));
        yield return StartCoroutine(CutPerFrame(node.RightNode, minSizeX, minSizeY, roomScale));
    }
}