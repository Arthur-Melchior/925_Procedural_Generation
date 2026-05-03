using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyScript : MonoBehaviour
{
    private static readonly int AttackAnimation = Animator.StringToHash("Attack");
    private static readonly int Death = Animator.StringToHash("Death");

    [Header("Stats")] [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRange;

    [Header("Movement")] [SerializeField] private SteeringScript steeringScript;
    [SerializeField] private float stopingDistance = 1f;
    public int currentRoomIndex;
    public int pathIndex;

    [Header("Explosion")] [SerializeField] private bool explodeOnDeath;
    [SerializeField] private Vector2 explosionSize = new Vector2(1, 1);
    [SerializeField] private float explosionDamage = 10f;
    [SerializeField] private ParticleSystem explosionVFX;

    [Header("References")] public Transform target;
    public PlayerScript player;
    public EnemiesManager enemiesManager;

    [Header("Events")] public UnityEvent onDeath;

    private Animator _animator;
    private List<PathNode> _path;
    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private bool _isFollowingPath;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
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
            var newPosition = Vector3.Lerp(_rigidbody2D.position, _path[pathIndex].Tile.Position,
                steeringScript.speed * Time.deltaTime);
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
            _animator.SetTrigger(AttackAnimation);
        }

        if (!steeringScript.isFlying && currentRoomIndex != player.currentRoomIndex && !_isFollowingPath)
        {
            _isFollowingPath = true;
            _path = await enemiesManager.GetPathAsync(transform.position);
            StartCoroutine(FollowPath());
        }
    }

    public void Attack()
    {
        var angle = Mathf.Atan2(steeringScript.velocity.x, steeringScript.velocity.y) * Mathf.Rad2Deg;
        var hits = Physics2D.BoxCastAll(transform.position, _boxCollider2D.size, angle, steeringScript.velocity,
            _boxCollider2D.size.x);
        foreach (var raycastHit2D in hits)
        {
            if (raycastHit2D.transform.gameObject.CompareTag("Player"))
            {
                raycastHit2D.transform.gameObject.GetComponent<PlayerScript>().TakeDamage(attackDamage);
                break;
            }
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
        _animator.SetTrigger(Death);
        if (explodeOnDeath)
        {
            explosionVFX.Play();
            var hits = Physics2D.BoxCastAll(transform.position, explosionSize, 0, Vector2.zero);
            foreach (var raycastHit2D in hits)
            {
                var gO = raycastHit2D.transform.gameObject;
                if (gO != gameObject && gO.CompareTag("Enemy"))
                {
                    gO.GetComponent<EnemyScript>().Die();
                }
                else if (gO.CompareTag("Player"))
                {
                    gO.GetComponent<PlayerScript>().TakeDamage(explosionDamage);
                }
            }
        }

        onDeath?.Invoke();
    }

    public void TurnOffAnimator() => _animator.enabled = false;
    
}