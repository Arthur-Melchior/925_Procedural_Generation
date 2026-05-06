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

    [Min(1)] public float avoidDistance = 1;
    public float avoidanceForce = 1;
    public LayerMask layersToAvoid;

    [Header("References")] public Transform target;
    [SerializeField] private bool spriteIsLookingLeft;
    [HideInInspector] public Vector2 velocity;

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
        var ahead = velocity * avoidDistance;
        var angle =
            Mathf.Atan2(velocity.x, velocity.y)
            * Mathf.Rad2Deg;
        var test = Physics2D.BoxCast(transform.position, ahead, angle, velocity, 1);
        DrawBoxCast(transform.position, ahead, angle, velocity, 1, Color.bisque);
        if (test)
        {
            Debug.Log(test.point);
            Debug.Log(test.centroid);
        }

        var collision = Physics2D.Raycast(transform.position, ahead, ahead.magnitude, layersToAvoid);

        Debug.DrawRay(transform.position, ahead, Color.aquamarine);

        if (collision)
        {
            var avoidanceVector = Avoid(collision.point, collision.collider.bounds.center);

            Debug.DrawRay(collision.collider.bounds.center, (Vector3)collision.point - collision.collider.bounds.center,
                Color.blue);

            velocity += avoidanceVector;
            Debug.DrawRay(transform.position, avoidanceVector, Color.red);
            Debug.DrawRay(transform.position, velocity);
        }
    }

    void DrawBoxCast(
        Vector2 origin,
        Vector2 size,
        float angle,
        Vector2 direction,
        float distance,
        Color color)
    {
        Vector2 endPosition = origin + direction.normalized * distance;

        DrawRotatedRectangle(origin, size, angle, color);
        DrawRotatedRectangle(endPosition, size, angle, color);

        Vector2[] startCorners = GetRectangleCorners(origin, size, angle);
        Vector2[] endCorners = GetRectangleCorners(endPosition, size, angle);

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(startCorners[i], endCorners[i], color);
        }
    }

    void DrawRotatedRectangle(
        Vector2 center,
        Vector2 size,
        float angle,
        Color color)
    {
        Vector2[] corners = GetRectangleCorners(center, size, angle);

        Debug.DrawLine(corners[0], corners[1], color);
        Debug.DrawLine(corners[1], corners[2], color);
        Debug.DrawLine(corners[2], corners[3], color);
        Debug.DrawLine(corners[3], corners[0], color);
    }

    Vector2[] GetRectangleCorners(
        Vector2 center,
        Vector2 size,
        float angle)
    {
        Vector2 half = size / 2f;

        Vector2[] corners = new Vector2[4];

        corners[0] = new Vector2(-half.x, half.y);   // TL
        corners[1] = new Vector2(half.x, half.y);    // TR
        corners[2] = new Vector2(half.x, -half.y);  // BR
        corners[3] = new Vector2(-half.x, -half.y); // BL

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = center + (Vector2)(rotation * corners[i]);
        }

        return corners;
    }

    public Vector2 Seek(Transform seekTarget) => seekTarget.position - transform.position;
    public Vector2 Flee(Transform seekTarget) => transform.position - seekTarget.position;

    public Vector2 Avoid(Vector2 contactPoint, Vector2 obstacleCenter) =>
        (contactPoint - obstacleCenter) * avoidanceForce;
}