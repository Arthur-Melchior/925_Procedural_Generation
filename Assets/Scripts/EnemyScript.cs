using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    public int currentRoomIndex;
    public Transform target;
    public PlayerScript player;
    public EnemiesManager enemiesManager;
    public UnityEvent onDeath;
    public int pathIndex;

    [SerializeField] private float attackRange;
    [SerializeField] private SteeringScript steeringScript;
    [SerializeField] private float stopingDistance = 1f;

    private Animator _animator;
    private List<PathNode> _path;
    private Rigidbody2D _rigidbody2D;
    private bool _isFollowingPath;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }


    private IEnumerator FollowPath()
    {
        steeringScript.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
        
        var distance = float.MaxValue;
        for (var i = 0; i < _path.Count; i++)
        {
            var difference = Vector3.Distance(transform.position, _path[i].Tile.Position);
            if (difference < distance)
            {
                distance = difference;
                pathIndex = i;
            }
        }

        while (pathIndex < _path.Count && currentRoomIndex != player.currentRoomIndex)
        {
            var newPosition = Vector3.Lerp(_rigidbody2D.position, _path[pathIndex].Tile.Position, steeringScript.speed * Time.deltaTime);
            _rigidbody2D.MovePosition(newPosition);
            if (Vector3.Distance(transform.position, _path[pathIndex].Tile.Position) < stopingDistance)
            {
                pathIndex++;
            }

            yield return null;
        }
        
        
        steeringScript.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        _isFollowingPath = false;
    }

    private async void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange)
        {
            _animator.SetTrigger("Attack");
        }

        if (!steeringScript.isFlying && currentRoomIndex != player.currentRoomIndex && !_isFollowingPath)
        {
            _isFollowingPath = true;
            _path = await enemiesManager.GetPathAsync(transform.position);
            StartCoroutine(FollowPath());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Die();
        }
    }

    public void Die()
    {
        _animator.SetTrigger("Death");
        onDeath?.Invoke();
    }
}