using UnityEngine;
using UnityEngine.InputSystem;

namespace Cainos.Pixel_Art_Top_Down___Basic.Script
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class TopDownCharacterController : MonoBehaviour
    {
        public float speed;

        private Animator _animator;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _direction;
        private float dodgeMultiplier = 1f;
        private float dodgeTimer = 0f;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
        

        private void FixedUpdate()
        {
            if (dodgeTimer > 0)
            {
                dodgeTimer -= Time.fixedDeltaTime;
            }
            else
            {
                dodgeMultiplier = 1f;
            }

            _rigidbody2D.linearVelocity = speed * dodgeMultiplier * _direction;

            _animator.SetBool("IsMoving", _direction.magnitude > 0);
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<Vector2>();
            _direction = value.normalized;

            _animator.SetFloat("DirectionX", value.x);
            _animator.SetFloat("DirectionY", value.y);
        }

        public void OnDodge(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            dodgeMultiplier = 2.2f;
            dodgeTimer = 0.2f;
        }
    }
}