using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringScript : MonoBehaviour
{
    public Transform target;
    public float speed;
    public float maxSpeed;
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var direction = Vector3.ClampMagnitude(Seek(target) * speed, maxSpeed);
        Debug.DrawRay(transform.position, direction);
        _rb.linearVelocity = direction;
    }

    public Vector3 Seek(Transform seekTarget) => seekTarget.position - transform.position;
    public Vector3 Flee(Transform seekTarget) => transform.position - seekTarget.position;
}