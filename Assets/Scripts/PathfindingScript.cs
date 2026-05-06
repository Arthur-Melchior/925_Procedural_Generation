using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class PathNode
{
    public readonly WalkableTile Tile;
    public float Distance;
    public float Priority;
    public readonly PathNode Parent;
    public PathNode[] Neighbors;
    public bool isDeadEnd;
    public bool IsSelected;

    public PathNode(WalkableTile t, float d, float pr, PathNode p)
    {
        Tile = t;
        Distance = d;
        Priority = pr;
        Parent = p;
    }
}

public class PathfindingScript
{
    private readonly WalkableTile[,] _map;
    private PathNode[,] _pathNodeMap;
    private Vector3Int _targetPosition;
    private const int MaxIterations = 10000;
    public readonly HashSet<PathNode> DebugPath = new();

    public PathfindingScript(WalkableTile[,] map)
    {
        _map = map;
    }

    public Task<List<PathNode>> FindPathToTarget(Vector3Int startingPosition, Vector3Int targetPosition)
    {
        SanitizeInput(ref startingPosition);
        SanitizeInput(ref targetPosition);
        
        Debug.Log("Pathfinding started");
        _targetPosition = targetPosition;
        _pathNodeMap = new PathNode[_map.GetLength(0), _map.GetLength(1)];

        //create the first node
        var distance = Vector3.Distance(startingPosition, targetPosition);
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, distance, null);

        var path = new List<PathNode>();
        var iteration = 0;

        try
        {
            //Find the path
            while (currentNode.Tile.Position != targetPosition && iteration++ < MaxIterations)
            {
                currentNode.Neighbors = GetNeighbors(currentNode);
                var cheapestChoice = FindCheapestChoice();
                if (cheapestChoice != currentNode)
                {
                    currentNode = cheapestChoice;
                }
                else
                {
                    currentNode.isDeadEnd = true;
                    currentNode.Priority = float.MaxValue;
                }
            }

            //Retrace the path
            while (currentNode.Tile.Position != startingPosition)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        path.Reverse();

        return Task.FromResult(path);
    }

    private void SanitizeInput(ref Vector3Int position)
    {
        if (position.x < 0)
        {
            position.x = 0;
        }

        if (position.x >= _map.GetLength(0))
        {
            position.x = _map.GetLength(0) - 1;
        }
        
        if (position.y < 0)
        {
            position.y = 0;
        }

        if (position.y >= _map.GetLength(1))
        {
            position.y = _map.GetLength(1) - 1;
        }
    }

    public IEnumerator DrawPathToTarget(Vector3Int startingPosition, Vector3Int targetPosition, float waitTime)
    {
        DebugPath.Clear();
        _targetPosition = targetPosition;
        _pathNodeMap = new PathNode[_map.GetLength(0), _map.GetLength(1)];

        //create the first node
        var distance = Vector3.Distance(startingPosition, targetPosition);
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, distance, null);
        currentNode.IsSelected = true;

        //instantiate class related variables
        DebugPath.Add(currentNode);
        var iteration = 0;

        yield return new WaitForSeconds(waitTime);

        while (currentNode.Tile.Position != targetPosition && iteration++ < MaxIterations)
        {
            currentNode.Neighbors = GetNeighbors(currentNode);
            foreach (var currentNodeNeighbor in currentNode.Neighbors)
            {
                DebugPath.Add(currentNodeNeighbor);
            }

            yield return new WaitForSeconds(waitTime);

            var cheapestChoice = FindCheapestChoice();
            if (cheapestChoice != currentNode)
            {
                currentNode = cheapestChoice;
            }
            else
            {
                currentNode.isDeadEnd = true;
                currentNode.Priority = float.MaxValue;
            }

            yield return new WaitForSeconds(waitTime);
        }

        //Retrace the path
        while (currentNode.Tile.Position != startingPosition)
        {
            currentNode.IsSelected = true;
            currentNode = currentNode.Parent;
        }
    }

    private PathNode FindCheapestChoice()
    {
        var cheapestValue = float.MaxValue;
        PathNode cheapestChoice = null;

        for (var i = 0; i < _pathNodeMap.GetLength(0); i++)
        {
            for (var j = 0; j < _pathNodeMap.GetLength(1); j++)
            {
                var value = _pathNodeMap[i, j];
                if (value != null && value.Priority < cheapestValue)
                {
                    cheapestValue = value.Priority;
                    cheapestChoice = value;
                }
            }
        }

        return cheapestChoice;
    }

    private PathNode[] GetNeighbors(PathNode currentNode)
    {
        var array = new PathNode[4];
        array[0] = GeneratePathNode(currentNode, currentNode.Tile.Position.x, currentNode.Tile.Position.y + 1);
        array[1] = GeneratePathNode(currentNode, currentNode.Tile.Position.x + 1, currentNode.Tile.Position.y);
        array[2] = GeneratePathNode(currentNode, currentNode.Tile.Position.x, currentNode.Tile.Position.y -1);
        array[3] = GeneratePathNode(currentNode, currentNode.Tile.Position.x - 1, currentNode.Tile.Position.y);
        return array;
    }

    private PathNode GeneratePathNode(PathNode parentNode, int x, int y)
    {
        if (x < 0)
        {
            x = 0;
        }
        else if (x >= _map.GetLength(0))
        {
            x = _map.GetLength(0) - 1;
        }

        if (y < 0)
        {
            y = 0;
        }
        else if (y >= _map.GetLength(1))
        {
            y = _map.GetLength(1) - 1;
        }

        var tile = _map[x, y];
        var distance = tile.IsWalkable ? Vector3.Distance(tile.Position, _targetPosition) : float.MaxValue;
        var priority = distance;

        var existingNode = _pathNodeMap[x, y];
        if (existingNode != null)
        {
            if (!existingNode.isDeadEnd)
            {
                existingNode.Distance = distance;
                existingNode.Priority = priority;
            }

            return existingNode;
        }

        var newNode = new PathNode(tile, distance, priority, parentNode);
        _pathNodeMap[x, y] = newNode;

        return newNode;
    }
}