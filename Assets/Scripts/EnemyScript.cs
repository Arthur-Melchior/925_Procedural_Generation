using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class EnemyScript : MonoBehaviour
{
    [SerializeField] private float attackRange;
    [SerializeField] private Transform target;
    [SerializeField] private DungeonGenerator map;
    public UnityEvent onDeath;

    private Animator _animator;
    private PathfindingScript _pathfindingScript;
    private List<PathNode> _path;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        map.onGenerationFinished.AddListener(() => _pathfindingScript = new PathfindingScript(map.walkableTiles));
    }

    public void GoToTarget()
    {
        _path = _pathfindingScript.FindPathToTarget(
            new Vector3Int((int) transform.position.x, (int) transform.position.y),
            new Vector3Int((int) target.position.x, (int) target.position.y));
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange)
        {
            _animator.SetTrigger("Attack");
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
}