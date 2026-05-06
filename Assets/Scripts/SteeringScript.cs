using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SteeringScript : MonoBehaviour
{
    [Header("Stats")] public float speed;
    public float maxSpeed;

    [Header("Avoid Values")] [Tooltip("If the enemy is flying it won't avoid obstacles")]
    public bool isFlying;

    [Min(1)] public Vector2 avoidLookAheadZone = new(2, 5);
    public float avoidanceForce = 1;
    public LayerMask layersToAvoid;

    [Header("References")] public Transform target;
    [SerializeField] private bool spriteIsLookingLeft;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public bool shouldAvoid;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        velocity = Vector2.ClampMagnitude((Seek(target) - Seek(target).normalized) * speed, maxSpeed);
        Debug.DrawRay(transform.position, velocity);

        if (!isFlying)
        {
            AvoidCollisions();
        }

        _rb.linearVelocity = velocity;
        if (spriteIsLookingLeft)
        {
            _spriteRenderer.flipX = velocity.x > 0;
        }
        else
        {
            _spriteRenderer.flipX = velocity.x < 0;
        }
    }

    private void AvoidCollisions()
    {
        var angle =
            Mathf.Atan2(velocity.x, velocity.y)
            * Mathf.Rad2Deg;

        var collision = Physics2D.OverlapBox(transform.position, avoidLookAheadZone, angle, layersToAvoid);

        if (collision)
        {
            var avoidanceVector = Avoid(collision.ClosestPoint(transform.position), collision.bounds.center);

            Debug.DrawRay(collision.bounds.center,
                (Vector3) collision.ClosestPoint(transform.position) - collision.bounds.center,
                Color.blue);

            velocity += avoidanceVector;
            Debug.DrawRay(transform.position, avoidanceVector, Color.red);
            Debug.DrawRay(transform.position, velocity);
        }
    }

    private void OnDrawGizmos()
    {
        var angle =
            Mathf.Atan2(velocity.x, velocity.y)
            * Mathf.Rad2Deg;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, avoidLookAheadZone);
    }

    public Vector2 Seek(Transform seekTarget) => seekTarget.position - transform.position;
    public Vector2 Flee(Transform seekTarget) => transform.position - seekTarget.position;

    public Vector2 Avoid(Vector2 contactPoint, Vector2 obstacleCenter) =>
        (contactPoint - obstacleCenter) * avoidanceForce;
}