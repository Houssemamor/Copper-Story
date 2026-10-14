using UnityEngine;

// Core health system adapted from nickbota/Unity-Platformer-Episode-7
// Float-based health with animator triggers for hurt/die states
public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth = 3f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => startingHealth;

    private Animator animator;
    private bool dead;
    private float invulnerableTimer;
    [SerializeField] private float invulnerableTime = 1f;

    private void Awake()
    {
        CurrentHealth = startingHealth;
        animator = GetComponent<Animator>();
        dead = false;
    }

    private void Update()
    {
        if (invulnerableTimer > 0f)
        {
            invulnerableTimer -= Time.deltaTime;
        }

        if (transform.position.y < -15f)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (dead || damage <= 0f || invulnerableTimer > 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0f, startingHealth);
        invulnerableTimer = invulnerableTime;

        if (CurrentHealth > 0f)
        {
            animator?.SetTrigger("hurt");
        }
        else
        {
            Die();
        }
    }

    public void AddHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + value, 0f, startingHealth);
    }

    public void Die()
    {
        if (dead)
        {
            return;
        }

        dead = true;
        CurrentHealth = 0f;
        animator?.SetTrigger("die");

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        var attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = startingHealth;
        dead = false;
        invulnerableTimer = 0f;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }

        var attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = true;
        }
    }

    [ContextMenu("Test: Take 1 Damage")]
    private void TestDamage()
    {
        TakeDamage(1f);
    }

    [ContextMenu("Test: Die Instantly")]
    private void TestDie()
    {
        Die();
    }
}