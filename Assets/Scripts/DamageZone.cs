using UnityEngine;

// Add to enemies, spikes or other hazards. The hazard needs a Collider2D.
// Enable "Is Trigger" for pass-through damage, or leave it off to damage on touch.
public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool killInstantly;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void TryDamage(Collider2D other)
    {
        var health = other.GetComponentInParent<Health>();
        if (health == null)
        {
            return;
        }

        if (killInstantly)
        {
            health.Die();
        }
        else
        {
            health.TakeDamage(damage);
        }
    }
}