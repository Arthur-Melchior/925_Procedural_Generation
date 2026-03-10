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

        if (node.cutType == CutType.Vertical)
        {
            var xMiddle = node.room.size.x / 2;
            leftNodeRoom = new BoundsInt(node.room.min, new Vector3Int(xMiddle, node.room.size.y));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.room.x + xMiddle, node.room.yMin),
                new Vector3Int(xMiddle, node.room.size.y));
            nextCutType = CutType.Horizontal;
        }
        else
        {
            var yMiddle = node.room.size.y / 2;
            leftNodeRoom = new BoundsInt(node.room.min, new Vector3Int(node.room.size.x, yMiddle));
            rightNodeRoom = new BoundsInt(new Vector3Int(node.room.xMin, node.room.y + yMiddle),
                new Vector3Int(node.room.size.x, yMiddle));
            nextCutType = CutType.Vertical;
        }

        _currentIteration++;

        Debug.Log(_currentIteration);
        Debug.Log(node.room);

        Debug.DrawLine(node.room.min, new Vector3(node.room.xMax, node.room.y), Color.red, 1000);
        Debug.DrawLine(node.room.min, new Vector3(node.room.x, node.room.yMax), Color.red, 1000);
        Debug.DrawLine(new Vector3(node.room.x, node.room.yMax), node.room.max, Color.red, 1000);
        Debug.DrawLine(new Vector3(node.room.xMax, node.room.y), node.room.max, Color.red, 1000);

        // ⬇️ attendre la frame suivante
        yield return null;

        if (leftNodeRoom.size.x < minSizeX || leftNodeRoom.size.y < minSizeY || _currentIteration > MaxIterations)
        {
            yield break;
        }

        node.leftNode = new Node {cutType = nextCutType, room = leftNodeRoom};
        node.rightNode = new Node {cutType = nextCutType, room = rightNodeRoom};

        yield return StartCoroutine(CutPerFrame(node.leftNode, minSizeX, minSizeY));
        yield return StartCoroutine(CutPerFrame(node.rightNode, minSizeX, minSizeY));
    }
}