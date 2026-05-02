using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringScript : MonoBehaviour
{
    public Transform target;
    public float speed;
    public float maxSpeed;
    public bool isFlying;
    [Min(1)] public float aheadDistance = 1;
    public float avoidanceForce = 1;
    public LayerMask layerMask;
    private Rigidbody2D _rb;
    private Vector2 _velocity;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _velocity = Vector2.ClampMagnitude(Seek(target) * speed, maxSpeed);
        Debug.DrawRay(transform.position, _velocity);

        if (!isFlying)
        {
            AvoidCollisions();
        }

        _rb.linearVelocity = _velocity;
    }

    private void AvoidCollisions()
    {
        var ahead = _velocity * aheadDistance;
        var collision = Physics2D.Raycast(transform.position, ahead, ahead.magnitude, layerMask);

        Debug.DrawRay(transform.position, ahead, Color.aquamarine);

        if (collision)
        {
            var avoidanceVector = Avoid(collision.centroid, collision.collider.bounds.center);
            
            Debug.DrawRay((Vector2) transform.position + ahead, avoidanceVector, Color.red);
            Debug.DrawRay(collision.centroid, ahead, Color.blue);
            
            _velocity += avoidanceVector;
            Debug.DrawRay(transform.position, _velocity);
        }
    }

    public Vector2 Seek(Transform seekTarget) => seekTarget.position - transform.position;
    public Vector2 Flee(Transform seekTarget) => transform.position - seekTarget.position;
    public Vector2 Avoid(Vector2 velocity, Vector2 obstacle) => (velocity - obstacle) * avoidanceForce;
}