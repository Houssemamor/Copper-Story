using UnityEngine;

// Health pickup collectible - adds health on player contact
public class HealthCollectible : MonoBehaviour
{
    [SerializeField] private float healthValue = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        var health = collision.GetComponent<Health>();
        if (health != null)
        {
            health.AddHealth(healthValue);
            gameObject.SetActive(false);
        }
    }
}