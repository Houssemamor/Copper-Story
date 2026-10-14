using UnityEngine;

// Tracks the player's health and triggers Game Over when it reaches zero
// or when the player falls out of the level.
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerableTime = 1f;
    [SerializeField] private float killHeight = -15f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private float invulnerableTimer;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (IsDead || !GameManager.IsPlaying)
        {
            return;
        }

        if (invulnerableTimer > 0f)
        {
            invulnerableTimer -= Time.deltaTime;
        }

        if (transform.position.y < killHeight)
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0 || !GameManager.IsPlaying || invulnerableTimer > 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        invulnerableTimer = invulnerableTime;

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        CurrentHealth = 0;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    [ContextMenu("Test: Take 1 Damage")]
    private void TestDamage()
    {
        TakeDamage(1);
    }

    [ContextMenu("Test: Die Instantly")]
    private void TestDie()
    {
        Die();
    }
}
