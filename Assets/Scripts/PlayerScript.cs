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

    public bool hasKey;
    public int currentRoomIndex;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float dodgeForce = 2f;
    [SerializeField] private float dodgeDuration = 0.2f;

    private float _dodgeForce;
    private float _dodgeDuration;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _direction;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_dodgeDuration > 0)
        {
            _dodgeDuration -= Time.fixedDeltaTime;
        }
        else
        {
            _dodgeForce = 1f;
            _animator.SetBool(IsDodging, false);
        }

        _rigidbody2D.linearVelocity = speed * _dodgeForce * _direction;

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
        if (!ctx.performed || _dodgeDuration > 0) return;

        _dodgeForce = dodgeForce;
        _dodgeDuration = dodgeDuration;

        _animator.SetBool(IsDodging, true);
    }
}