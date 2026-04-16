using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class PathNode
{
    public readonly WalkableTile tile;
    public float distance;
    public float cost;
    public float priority;
    public PathNode parent;
    public PathNode[] neighbors;
    public bool isSelected;

    public PathNode(WalkableTile t, float d, float c, float pr, PathNode p)
    {
        tile = t;
        distance = d;
        cost = c;
        priority = pr;
        parent = p;
    }
}

public class PathfindingScript
{
    private readonly WalkableTile[,] _map;
    private readonly PathNode[,] _pathMap;
    private Vector3Int _targetPosition;
    private const int MaxIterations = 100000;
    public HashSet<PathNode> debugPath = new HashSet<PathNode>();

    public PathfindingScript(WalkableTile[,] map)
    {
        _map = map;
        _pathMap = new PathNode[_map.GetLength(0), _map.GetLength(1)];
    }

    public List<PathNode> FindPathToTarget(Vector3Int startingPosition, Vector3Int targetPosition)
    {
        //create the first node
        var distance = Vector3.Distance(startingPosition, targetPosition);
        var cost = 0f;
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, cost, distance,
            null);

        //instantiate class related variables
        var path = new List<PathNode> { currentNode };
        _targetPosition = targetPosition;
        var iteration = 0;

        while (currentNode.tile.position != targetPosition && iteration++ < MaxIterations)
        {
            currentNode.neighbors = GetNeighbors(currentNode, currentNode.cost + cost);

            var cheapestChoice = FindCheapestChoice();

            //if the there is no good path, mark path as useless and go back
            if (cheapestChoice.priority >= currentNode.priority)
            {
                path.Remove(currentNode);
                currentNode.priority = float.MaxValue;
                currentNode = currentNode.parent;
                cost -= 0.5f;
            }
            else
            {
                path.Add(cheapestChoice);
                cheapestChoice.parent = currentNode;
                currentNode = cheapestChoice;
                cost += 0.5f;
            }
        }

        return path;
    }

    public IEnumerator DrawPathToTarget(Vector3Int startingPosition, Vector3Int targetPosition, float waitTime)
    {
        debugPath.Clear();
        
        //create the first node
        var distance = Vector3.Distance(startingPosition, targetPosition);
        var cost = 0f;
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, cost, distance,
            null);
        currentNode.isSelected = true;

        //instantiate class related variables
        debugPath.Add(currentNode);
        _targetPosition = targetPosition;
        var iteration = 0;

        yield return new WaitForSeconds(waitTime);

        while (currentNode.tile.position != targetPosition && iteration++ < MaxIterations)
        {
            currentNode.neighbors = GetNeighbors(currentNode, cost);

            foreach (var currentNodeNeighbor in currentNode.neighbors)
            {
                debugPath.Add(currentNodeNeighbor);
            }

            yield return new WaitForSeconds(waitTime);

            var cheapestChoice = FindCheapestChoice();

            //if the there is no good path, mark path as useless and go back
            if (cheapestChoice.priority >= currentNode.priority)
            {
                currentNode.isSelected = false;
                currentNode.priority = float.MaxValue;
                currentNode = currentNode.parent;
                cost -= 0.5f;
            }
            else
            {
                cheapestChoice.isSelected = true;
                cheapestChoice.parent = currentNode;
                currentNode = cheapestChoice;
                cost += 0.5f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private PathNode FindCheapestChoice()
    {
        var cheapestValue = float.MaxValue;
        PathNode cheapestChoice = null;

        for (var i = 0; i < _pathMap.GetLength(0); i++)
        {
            for (var j = 0; j < _pathMap.GetLength(1); j++)
            {
                var value = _pathMap[i, j];
                if (value != null && value.priority < cheapestValue)
                {
                    cheapestValue = value.priority;
                    cheapestChoice = value;
                }
            }
        }

        return cheapestChoice;
    }

    private PathNode[] GetNeighbors(PathNode currentNode, float cost)
    {
        var array = new PathNode[8];
        array[0] = GeneratePathNode(currentNode, currentNode.tile.position.x, currentNode.tile.position.y + 1, cost);
        array[1] = GeneratePathNode(currentNode, currentNode.tile.position.x + 1, currentNode.tile.position.y + 1,
            cost + 0.2f);
        array[2] = GeneratePathNode(currentNode, currentNode.tile.position.x + 1, currentNode.tile.position.y, cost);
        array[3] = GeneratePathNode(currentNode, currentNode.tile.position.x + 1, currentNode.tile.position.y - 1,
            cost + 0.2f);
        array[4] = GeneratePathNode(currentNode, currentNode.tile.position.x, currentNode.tile.position.y - 1, cost);
        array[5] = GeneratePathNode(currentNode, currentNode.tile.position.x - 1, currentNode.tile.position.y - 1,
            cost + 0.2f);
        array[6] = GeneratePathNode(currentNode, currentNode.tile.position.x - 1, currentNode.tile.position.y, cost);
        array[7] = GeneratePathNode(currentNode, currentNode.tile.position.x - 1, currentNode.tile.position.y + 1,
            cost + 0.2f);
        return array;
    }

    private PathNode GeneratePathNode(PathNode parentNode, int x, int y, float cost)
    {
        if (x < 0)
        {
            x++;
        }
        else if (x >= _map.GetLength(0))
        {
            x = _map.GetLength(0) - 1;
        }

        if (y < 0)
        {
            y++;
        }
        else if (y >= _map.GetLength(1))
        {
            y = _map.GetLength(1) - 1;
        }

        cost = 0;
        var tile = _map[x, y];

        var distance = tile.isWalkable ? Vector3.Distance(tile.position, _targetPosition) : float.MaxValue;
        var priority = distance + cost;

        var pathPosition = _pathMap[x, y];
        if (pathPosition != null)
        {
            return pathPosition;
        }

        var newPath = new PathNode(tile, distance, cost, priority, parentNode);
        _pathMap[x, y] = newPath;

        return newPath;
    }
}