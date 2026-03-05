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

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }


        private void Update()
        {
            _rigidbody2D.linearVelocity = speed * _direction;
            
            _animator.SetBool("IsMoving", _direction.magnitude > 0);
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<Vector2>();
            _direction.x = value.x;
            _direction.y = value.y;

            _animator.SetFloat("DirectionX", value.x);
            _animator.SetFloat("DirectionY", value.y);
        }
    }
}