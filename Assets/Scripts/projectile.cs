using UnityEngine;

public class projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    private bool hitSomething;
    private Vector2 direction = Vector2.right;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude > 0.0001f)
        {
            direction = newDirection.normalized;
        }
    }

    private void Update()
    {
        if (!hitSomething)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit();
    }

    private void HandleHit()
    {
        if (hitSomething)
        {
            return;
        }

        hitSomething = true;
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        Destroy(gameObject);
    }

    private void deactivate()
    {
        gameObject.SetActive(false);
    }
}