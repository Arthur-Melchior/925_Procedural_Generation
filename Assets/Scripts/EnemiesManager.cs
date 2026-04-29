using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class EnemiesManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private Transform target;
    private PathfindingScript _pathfindingScript;
    private Coroutine _pathCalculating;

    public void InstantiateVariables()
    {
        _pathfindingScript = new PathfindingScript(map.walkableTiles);
    }

    private List<PathNode> _path;
    private Task<List<PathNode>> _currentPathTask;

    public async Task<List<PathNode>> GetPathAsync(Vector3 position)
    {
        if (_path != null)
            return _path;
        
        if (_currentPathTask != null)
            return await _currentPathTask;
        
        _currentPathTask = CalculatePathAsync(position);
        _path = await _currentPathTask;

        _currentPathTask = null;
        return _path;
    }

    private async Task<List<PathNode>> CalculatePathAsync(Vector3 position)
    {
        await Task.Yield();

        return _pathfindingScript.FindPathToTarget(
            new Vector3Int((int)position.x, (int)position.y, 0),
            new Vector3Int((int)target.position.x, (int)target.position.y, 0)
        );
    }
    
    [Space]
    [SerializeField] private float waitTime;
    public void DrawPathToTarget()
    {
        StartCoroutine(_pathfindingScript.DrawPathToTarget(
            new Vector3Int((int)transform.position.x, (int)transform.position.y),
            new Vector3Int((int)target.transform.position.x, (int)target.transform.position.y), waitTime));
    }

    private void OnDrawGizmos()
    {
        if (_path is { Count: > 0 })
        {
            foreach (var pathNode in _path)
            {
                Gizmos.DrawWireCube(pathNode.tile.position, new Vector3(1, 1));
                DrawThreeValues(pathNode.tile.position, pathNode.distance, pathNode.cost, pathNode.priority);
            }
        }

        if (_pathfindingScript?.debugPath == null) return;

        for (var index = 0; index < _pathfindingScript.debugPath.Count; index++)
        {
            var pathNode = _pathfindingScript.debugPath.ElementAt(index);

            Gizmos.color = pathNode.isSelected ? Color.aquamarine : Color.white;

            Gizmos.DrawWireCube(pathNode.tile.position, new Vector3(1, 1));
            DrawThreeValues(pathNode.tile.position, pathNode.distance, pathNode.cost, pathNode.priority);
        }

        var selectedPath = _pathfindingScript.debugPath.Where(p => p.isSelected).ToArray();
        for (var i = 0; i < selectedPath.Count(); i++)
        {
            if (i < selectedPath.Count() - 1)
            {
                Debug.DrawLine(selectedPath.ElementAt(i).tile.position, selectedPath.ElementAt(i + 1).tile.position);
            }
        }
    }
    
    private void DrawThreeValues(Vector3 center, float v1, float v2, float v3)
    {
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 10;
        style.alignment = TextAnchor.MiddleCenter;

        float offset = 0.15f;

        UnityEditor.Handles.Label(center + Vector3.up * offset, v1.ToString(), style);
        // UnityEditor.Handles.Label(center + Vector3.down * offset + Vector3.left * offset, v2.ToString(), style);
        UnityEditor.Handles.Label(center + Vector3.down * offset, v3.ToString(), style);
#endif
    }

    public void ClearPath()
    {
        _path = null;
    }
}