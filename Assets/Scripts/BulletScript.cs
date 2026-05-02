using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BulletScript : MonoBehaviour
{
    public bool isSuper;

    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxLifeTime = 10f;
    [SerializeField] private int maxSuperBulletPenetrations = 3;
    [SerializeField] private float swayIntensity = 30f;

    private int _numberOfPenetrations;
    private int _penIterations;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        transform.parent = null;
        var sign = Random.value > 0.5f ? 1 : -1;
        transform.localRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y,
            transform.eulerAngles.z + sign * Random.Range(0, swayIntensity / 2));
        Destroy(gameObject, maxLifeTime);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isSuper)
        {
            _numberOfPenetrations = maxSuperBulletPenetrations;
        }

        if (_penIterations < _numberOfPenetrations)
        {
            _penIterations++;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}