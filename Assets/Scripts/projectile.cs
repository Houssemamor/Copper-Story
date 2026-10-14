using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private bool hitSomething;
    private Vector2 direction = Vector2.right;
    private GameObject owner;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // A dynamic body reliably reports contacts with static and kinematic level geometry.
        // Gravity is disabled so the projectile travels in a straight line.
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        Destroy(gameObject, lifetime);
    }

    public void SetOwner(GameObject ownerObject)
    {
        owner = ownerObject;
        if (owner == null || boxCollider == null)
        {
            return;
        }

        // Prevent the projectile from colliding with the player that fired it,
        // including any colliders attached to child objects.
        Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null && ownerCollider != boxCollider)
            {
                Physics2D.IgnoreCollision(boxCollider, ownerCollider, true);
            }
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude > 0.0001f)
        {
            direction = newDirection.normalized;
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!hitSomething && rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            HandleContact(collision.collider);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other);
    }

    private void HandleContact(Collider2D other)
    {
        if (hitSomething || other == null)
        {
            return;
        }

        // Safety check in case this projectile is spawned before owner collisions are ignored.
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // The visible wall may have its collider on a child, so search upward in the hierarchy.
        WoodWall wall = other.GetComponentInParent<WoodWall>();
        if (wall != null)
        {
            wall.HitByProjectile();
        }

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
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Destroy(gameObject);
    }
}
