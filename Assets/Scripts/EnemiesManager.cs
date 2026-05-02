using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private Transform target;
    private PathfindingScript _pathfindingScript;
    private Coroutine _pathCalculating;

    public void InstantiateVariables()
    {
        _pathfindingScript = new PathfindingScript(map.WalkableTiles);
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
        Transform stair = null;
        var distance = float.MaxValue;

        foreach (var s in map.stairs)
        {
            var d = Vector2.Distance(target.position, s.transform.position);
            if (d < distance)
            {
                stair = s.transform;
                distance = d;
            }
        }

        return await _pathfindingScript.FindPathToTarget(
            new Vector3Int((int)position.x, (int)position.y, 0),
            new Vector3Int((int)stair!.position.x, (int)stair.position.y, 0)
        );
    }
    
    public void ClearPath()
    {
        _currentPathTask = null;
        _path = null;
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
                Gizmos.DrawWireCube(pathNode.Tile.Position, new Vector3(1, 1));
                DrawThreeValues(pathNode.Tile.Position, pathNode.Distance, pathNode.Cost, pathNode.Priority);
            }
        }

        if (_pathfindingScript?.DebugPath == null) return;

        for (var index = 0; index < _pathfindingScript.DebugPath.Count; index++)
        {
            var pathNode = _pathfindingScript.DebugPath.ElementAt(index);

            Gizmos.color = pathNode.IsSelected ? Color.aquamarine : Color.white;

            Gizmos.DrawWireCube(pathNode.Tile.Position, new Vector3(1, 1));
            DrawThreeValues(pathNode.Tile.Position, pathNode.Distance, pathNode.Cost, pathNode.Priority);
        }

        var selectedPath = _pathfindingScript.DebugPath.Where(p => p.IsSelected).ToArray();
        for (var i = 0; i < selectedPath.Count(); i++)
        {
            if (i < selectedPath.Count() - 1)
            {
                Debug.DrawLine(selectedPath.ElementAt(i).Tile.Position, selectedPath.ElementAt(i + 1).Tile.Position);
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
}