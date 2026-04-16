using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    public int currentLayer;

    [SerializeField] private float attackRange;
    [SerializeField] private float speed;
    [SerializeField] private Transform target;
    [SerializeField] private TopDownCharacterController player;
    [SerializeField] private DungeonGenerator map;
    [SerializeField] private SteeringScript steeringScript;
    public UnityEvent onDeath;

    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private PathfindingScript _pathfindingScript;
    private List<PathNode> _path;
    private bool _isGoingToStairs;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        map.onGenerationFinished.AddListener(() => _pathfindingScript = new PathfindingScript(map.walkableTiles));
    }

    private IEnumerator GoToClosestStairs()
    {
        _isGoingToStairs = true;
        steeringScript.shouldSeek = false;

        var closestStair = map.Stairs.OrderBy(s => Vector3.Distance(s.transform.position, target.position)).First();

        _path = _pathfindingScript.FindPathToTarget(
            new Vector3Int((int)transform.position.x, (int)transform.position.y),
            new Vector3Int((int)closestStair.transform.position.x, (int)closestStair.transform.position.y));

        var index = 0;

        while (index < _path.Count)
        {
            transform.position = Vector3.Lerp(transform.position, _path[index].tile.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _path[index].tile.position) < 3)
            {
                index++;
            }

            yield return null;
        }

        _isGoingToStairs = false;
        steeringScript.shouldSeek = true;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange)
        {
            _animator.SetTrigger("Attack");
        }

        if (currentLayer != player.currentLayer && !_isGoingToStairs)
        {
            _pathfindingScript ??= new PathfindingScript(map.walkableTiles);
            StartCoroutine(GoToClosestStairs());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            _animator.SetTrigger("Death");
            onDeath?.Invoke();
        }
    }

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
}