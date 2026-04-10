using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BulletScript : MonoBehaviour
{
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxLifeTime = 10f;
    [SerializeField] private int numberOfPenetrations = 3;
    public bool isSuper;
    private int _penIterations;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (bulletSprite)
        {
            _spriteRenderer.sprite = bulletSprite;
        }

        transform.parent = null;

        Destroy(gameObject, maxLifeTime);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += -transform.up * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_penIterations < numberOfPenetrations)
        {
            _penIterations++;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}