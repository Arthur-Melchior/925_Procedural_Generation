using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
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

        _path = _pathfindingScript.FindPathToTarget(
            new Vector3Int((int)transform.position.x, (int)transform.position.y),
            new Vector3Int((int)target.transform.position.x, (int)target.transform.position.y));


        var index = 0;

        while (index < _path.Count)
        {
            for (var i = 1; i < _path.Count - 1; i++)
            {
                Debug.DrawLine(_path[i - 1].tile.position, _path[i].tile.position);
            }
            
            transform.position = Vector3.Lerp(transform.position,_path[index].tile.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _path[index].tile.position) < 1)
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
}