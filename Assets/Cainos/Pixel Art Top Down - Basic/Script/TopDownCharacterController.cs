using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Cainos.Pixel_Art_Top_Down___Basic.Script
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class TopDownCharacterController : MonoBehaviour
    {
        private static readonly int IsDodging = Animator.StringToHash("IsDodging");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int DirectionX = Animator.StringToHash("DirectionX");
        private static readonly int DirectionY = Animator.StringToHash("DirectionY");

        [SerializeField] private float speed = 3f;
        [SerializeField] private float dodgeMultiplier = 2f;
        [SerializeField] private float dodgeTimer = 0.2f;

        private float _dodgeMultiplier;
        private float _dodgeTimer;
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
            if (_dodgeTimer > 0)
            {
                _dodgeTimer -= Time.fixedDeltaTime;
            }
            else
            {
                _dodgeMultiplier = 1f;
                _animator.SetBool(IsDodging, false);
            }

            _rigidbody2D.linearVelocity = speed * _dodgeMultiplier * _direction;

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
            if (!ctx.performed || _dodgeTimer > 0) return;

            _dodgeMultiplier = dodgeMultiplier;
            _dodgeTimer = dodgeTimer;

            _animator.SetBool(IsDodging, true);
        }
    }
}