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
    [SerializeField] private DungeonGeneratorDrunkWalker map;
    public UnityEvent onDeath;
    private GameObject[] _stairs;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _stairs = GameObject.FindGameObjectsWithTag("Stairs");
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

    private void GoToPlayer()
    {
        if (!Mathf.Approximately(target.position.z, transform.position.z))
        {
            GoToStairs();
        }
    }

    private void GoToStairs()
    {
        var closestStair = _stairs.OrderBy(s => Vector3.Distance(target.position, s.transform.position)).First();
        FindFastestPath(closestStair);
    }

    private void FindFastestPath(GameObject pathTarget)
    {
        var startingPosition = map.grassMap.WorldToCell(transform.position);
    }
}