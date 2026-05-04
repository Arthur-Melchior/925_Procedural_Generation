using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BulletScript : MonoBehaviour
{
    public bool isSuper;
    public bool isParried;
    public bool isSuperParried;

    public float speed = 1f;
    public int maxSuperBulletPenetrations = 3;
    public float swayIntensity = 30f;
    [SerializeField] private float maxLifeTime = 10f;

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
        if (isParried)
        {
            isParried = false;
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }
        
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