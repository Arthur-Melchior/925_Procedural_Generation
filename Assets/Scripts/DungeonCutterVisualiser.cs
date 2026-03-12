using System.Collections;
using UnityEngine;

public class DungeonCutterVisualiser : MonoBehaviour
{
    private const float MaxIterations = 10000f;
    private float _currentIteration;

    public IEnumerator CutPerFrame(Node node, int minSizeX, int minSizeY)
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

        _currentIteration++;

        Debug.Log(_currentIteration);
        Debug.Log(node.Room);
        
        yield return null;

        if (leftNodeRoom.size.x < minSizeX || leftNodeRoom.size.y < minSizeY || _currentIteration > MaxIterations)
        {
            Debug.DrawLine(node.Room.min, new Vector3(node.Room.xMax, node.Room.y), Color.red, 1000);
            Debug.DrawLine(node.Room.min, new Vector3(node.Room.x, node.Room.yMax), Color.red, 1000);
            Debug.DrawLine(new Vector3(node.Room.x, node.Room.yMax), node.Room.max, Color.red, 1000);
            Debug.DrawLine(new Vector3(node.Room.xMax, node.Room.y), node.Room.max, Color.red, 1000);
            yield break;
        }

        node.LeftNode = new Node { CutType = nextCutType, Room = leftNodeRoom };
        node.RightNode = new Node { CutType = nextCutType, Room = rightNodeRoom };

        yield return StartCoroutine(CutPerFrame(node.LeftNode, minSizeX, minSizeY));
        yield return StartCoroutine(CutPerFrame(node.RightNode, minSizeX, minSizeY));
    }
}