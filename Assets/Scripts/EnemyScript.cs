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
    public int currentRoomIndex;
    public Transform target;
    public TopDownCharacterController player;
    public EnemiesManager enemiesManager;

    [SerializeField] private float attackRange;
    [SerializeField] private float speed;
    [SerializeField] private SteeringScript steeringScript;
    [SerializeField] private float stopingDistance = 1f;
    public UnityEvent onDeath;

    private Animator _animator;
    private List<PathNode> _path;
    private Rigidbody2D _rigidbody2D;
    private bool _isFollowingPath;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public int index;

    private IEnumerator FollowPath()
    {
        steeringScript.enabled = false;

        while (index < _path.Count && currentRoomIndex != player.currentRoomIndex)
        {
            transform.position = Vector3.Lerp(transform.position, _path[index].tile.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _path[index].tile.position) < stopingDistance)
            {
                index++;
            }

            yield return null;
        }

        steeringScript.enabled = true;
        _isFollowingPath = false;
    }

    private async void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < attackRange)
        {
            _animator.SetTrigger("Attack");
        }

        if (currentRoomIndex != player.currentRoomIndex && !_isFollowingPath)
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
            _animator.SetTrigger("Death");
            onDeath?.Invoke();
        }
    }
}