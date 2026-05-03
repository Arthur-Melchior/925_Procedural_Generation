using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringScript : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float maxSpeed;
    
    [Header("Avoid Values")]
    [Tooltip("If the enemy is flying it won't avoid obstacles")] public bool isFlying;
    [Min(1)] public float avoidDistance = 1;
    public float avoidanceForce = 1;
    public LayerMask layersToAvoid;
    
    [Header("References")]
    public Transform target;
    
    [HideInInspector] public Vector2 velocity;
    
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        velocity = Vector2.ClampMagnitude(Seek(target) * speed, maxSpeed);
        Debug.DrawRay(transform.position, velocity);

        if (!isFlying)
        {
            AvoidCollisions();
        }

        _rb.linearVelocity = velocity;
    }

    private void AvoidCollisions()
    {
        var ahead = velocity * avoidDistance;
        var collision = Physics2D.Raycast(transform.position, ahead, ahead.magnitude, layersToAvoid);

        Debug.DrawRay(transform.position, ahead, Color.aquamarine);

        if (collision)
        {
            var avoidanceVector = Avoid(collision.centroid, collision.collider.bounds.center);
            
            Debug.DrawRay((Vector2) transform.position + ahead, avoidanceVector, Color.red);
            Debug.DrawRay(collision.centroid, ahead, Color.blue);
            
            velocity += avoidanceVector;
            Debug.DrawRay(transform.position, velocity);
        }
    }

    public Vector2 Seek(Transform seekTarget) => seekTarget.position - transform.position;
    public Vector2 Flee(Transform seekTarget) => transform.position - seekTarget.position;
    public Vector2 Avoid(Vector2 velocity, Vector2 obstacle) => (velocity - obstacle) * avoidanceForce;
}