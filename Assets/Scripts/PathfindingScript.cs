using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class PathNode
{
    public readonly WalkableTile Tile;
    public float Distance;
    public float Cost;
    public float Priority;
    public readonly PathNode Parent;
    public PathNode[] Neighbors;
    public bool isDeadEnd;
    public bool IsSelected;

    public PathNode(WalkableTile t, float d, float c, float pr, PathNode p)
    {
        Tile = t;
        Distance = d;
        Cost = c;
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
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, 0, distance, null);

        var path = new List<PathNode>();
        var iteration = 0;

        try
        {
            //Find the path
            while (currentNode.Tile.Position != targetPosition && iteration++ < MaxIterations)
            {
                currentNode.Neighbors = GetNeighbors(currentNode, currentNode.Cost + 0.5f);
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
        var currentNode = new PathNode(_map[startingPosition.x, startingPosition.y], distance, 0, distance, null);
        currentNode.IsSelected = true;

        //instantiate class related variables
        DebugPath.Add(currentNode);
        var iteration = 0;

        yield return new WaitForSeconds(waitTime);

        while (currentNode.Tile.Position != targetPosition && iteration++ < MaxIterations)
        {
            currentNode.Neighbors = GetNeighbors(currentNode, currentNode.Cost + 0.5f);
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

    private PathNode[] GetNeighbors(PathNode currentNode, float cost)
    {
        var array = new PathNode[8];
        array[0] = GeneratePathNode(currentNode, currentNode.Tile.Position.x, currentNode.Tile.Position.y + 1, cost);
        array[1] = GeneratePathNode(currentNode, currentNode.Tile.Position.x + 1, currentNode.Tile.Position.y + 1,
            cost + 0.4f);
        array[2] = GeneratePathNode(currentNode, currentNode.Tile.Position.x + 1, currentNode.Tile.Position.y, cost);
        array[3] = GeneratePathNode(currentNode, currentNode.Tile.Position.x + 1, currentNode.Tile.Position.y - 1,
            cost + 0.4f);
        array[4] = GeneratePathNode(currentNode, currentNode.Tile.Position.x, currentNode.Tile.Position.y - 1, cost);
        array[5] = GeneratePathNode(currentNode, currentNode.Tile.Position.x - 1, currentNode.Tile.Position.y - 1,
            cost + 0.4f);
        array[6] = GeneratePathNode(currentNode, currentNode.Tile.Position.x - 1, currentNode.Tile.Position.y, cost);
        array[7] = GeneratePathNode(currentNode, currentNode.Tile.Position.x - 1, currentNode.Tile.Position.y + 1,
            cost + 0.4f);
        return array;
    }

    private PathNode GeneratePathNode(PathNode parentNode, int x, int y, float cost)
    {
        cost = 0;
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
        var priority = distance + cost;

        var existingNode = _pathNodeMap[x, y];
        if (existingNode != null)
        {
            if (!existingNode.isDeadEnd)
            {
                existingNode.Distance = distance;
                existingNode.Cost = cost;
                existingNode.Priority = priority;
            }

            return existingNode;
        }

        var newNode = new PathNode(tile, distance, cost, priority, parentNode);
        _pathNodeMap[x, y] = newNode;

        return newNode;
    }
}