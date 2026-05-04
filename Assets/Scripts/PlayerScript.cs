using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerScript : MonoBehaviour
{
    private static readonly int IsDodging = Animator.StringToHash("IsDodging");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int DirectionX = Animator.StringToHash("DirectionX");
    private static readonly int DirectionY = Animator.StringToHash("DirectionY");
    private static readonly int Hit = Animator.StringToHash("Hit");

    public bool hasKey;
    public int currentRoomIndex;
    public float health = 100f;
    public bool isInvulnerable;
    public float invulnerabilityDuration = 1f;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float dodgeForce = 2f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;
    [SerializeField] private float getHitRecoil = 2f;

    [HideInInspector] public bool isDodging;

    private float _dodgeForce;
    private float _dodgeDuration;
    private float _dodgeDelta;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _direction;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _dodgeDelta = dodgeCooldown;
    }

    private void Update()
    {
        _dodgeDelta += Time.deltaTime;

        if (_dodgeDuration > 0)
        {
            _dodgeDuration -= Time.deltaTime;
        }
        else
        {
            _dodgeForce = 1f;
            isDodging = false;
            _animator.SetBool(IsDodging, false);
        }

        if (!isInvulnerable)
        {
            _rigidbody2D.linearVelocity = speed * _dodgeForce * _direction;
        }

        _animator.SetBool(IsMoving, _direction.magnitude > 0);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var value = ctx.ReadValue<Vector2>();
        _direction = value.normalized;

        _animator.SetFloat(DirectionX, value.x);
        _animator.SetFloat(DirectionY, value.y);
    }

    public void OnDodge(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || _dodgeDuration > 0 || _dodgeDelta < dodgeCooldown) return;

        _dodgeForce = dodgeForce;
        _dodgeDuration = dodgeDuration;
        _dodgeDelta = 0;
        isDodging = true;

        _animator.SetBool(IsDodging, true);
    }

    public void TakeDamage(float attackDamage, Vector3 origin)
    {
        if (isInvulnerable || isDodging) return;
        health -= attackDamage;
        _rigidbody2D.AddForce((transform.position - origin).normalized * getHitRecoil, ForceMode2D.Impulse);
        _animator.SetTrigger(Hit);
        StartCoroutine(TemporaryInvulnerability());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(5, other.transform.position);
        }
    }

    private IEnumerator TemporaryInvulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }
}