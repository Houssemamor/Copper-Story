using UnityEngine;

// Attach this component to the visible wooden wall root. The wall's own collider
// (or a collider on a child) rotates together with the sprite when hit by a projectile.
[DisallowMultipleComponent]
public class WoodWall : MonoBehaviour
{
    [SerializeField] private float rotationAngle = -90f;
    [SerializeField] private float rotationSpeed = 360f;

    private Rigidbody2D body;
    private bool rotating;
    private float targetRotation;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.useFullKinematicContacts = true;

        // A rotating barrier must have solid collision geometry.
        foreach (Collider2D wallCollider in GetComponentsInChildren<Collider2D>(true))
        {
            if (wallCollider != null)
            {
                wallCollider.isTrigger = false;
            }
        }
    }

    public void HitByProjectile()
    {
        if (rotating)
        {
            return;
        }

        rotating = true;
        targetRotation = body.rotation + rotationAngle;
    }

    private void FixedUpdate()
    {
        if (!rotating || body == null)
        {
            return;
        }

        float nextRotation = Mathf.MoveTowardsAngle(
            body.rotation,
            targetRotation,
            Mathf.Max(0f, rotationSpeed) * Time.fixedDeltaTime);

        body.MoveRotation(nextRotation);

        if (Mathf.Abs(Mathf.DeltaAngle(nextRotation, targetRotation)) <= 0.01f)
        {
            body.MoveRotation(targetRotation);
            rotating = false;
        }
    }
}
