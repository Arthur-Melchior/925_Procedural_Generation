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

    [Header("Pathfinding")] [SerializeField]
    private SteeringScript steeringScript;

    [SerializeField] private float stopingDistance = 1f;
    public int currentRoomIndex;
    public int pathIndex;

    [Header("Parry")] 
    public bool canParry;
    [SerializeField] private float parriedBulletSpeedMultiplier = 0.5f;
    [SerializeField] private Color parriedBulletColor = Color.red;

    [Header("Explosion")] 
    public bool explodeOnDeath;
    [SerializeField] private Vector2 explosionSize = new(1, 1);
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
    private ContactFilter2D _explosionFilter;
    private int _playerLayer;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _explosionFilter.SetLayerMask(LayerMask.GetMask("Enemy", "Flying Enemy", "Player"));
        _playerLayer = LayerMask.NameToLayer("Player");
    }

    private IEnumerator FollowPath()
    {
        steeringScript.enabled = false;

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

    private void Attack()
    {
        var angle =
            Mathf.Atan2(steeringScript.velocity.x, steeringScript.velocity.y)
            * Mathf.Rad2Deg;

        var hit = Physics2D.OverlapBox(transform.position, _boxCollider2D.size, angle, _playerLayer);

        if (hit)
        {
            hit.gameObject.GetComponent<PlayerScript>().TakeDamage(attackDamage);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            var bulletScript = other.gameObject.GetComponent<BulletScript>();
            if (canParry && !bulletScript.isSuperParried)
            {
                Parry(other.gameObject);
            }
            else
            {
                Die();
            }
        }
    }

    private void Parry(GameObject bullet)
    {
        var bulletScript = bullet.GetComponent<BulletScript>();
        bulletScript.isParried = true;
        bulletScript.speed *= parriedBulletSpeedMultiplier;
        bullet.gameObject.GetComponent<SpriteRenderer>().color = parriedBulletColor;
        bullet.transform.rotation = Quaternion.Euler(bullet.transform.eulerAngles.x, bullet.transform.eulerAngles.y,
            bullet.transform.eulerAngles.z + 180f);
        _animator.Play("Parry");
    }

    private bool _isDead;

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        _animator.SetTrigger(Death);
        if (explodeOnDeath)
        {
            explosionVFX.Play();
            var hits = new List<Collider2D>();
            Physics2D.OverlapBox(transform.position, explosionSize, 0, _explosionFilter, hits);

            foreach (var hit in hits)
            {
                var gO = hit.gameObject;

                if (gO != gameObject && gO.CompareTag("Enemy"))
                {
                    gO.GetComponent<EnemyScript>().Die();
                }

                if (gO.CompareTag("Player"))
                {
                    gO.GetComponent<PlayerScript>().TakeDamage(explosionDamage);
                }
            }
        }

        onDeath?.Invoke();
    }

    public void TurnOffAnimator() => _animator.enabled = false;
}