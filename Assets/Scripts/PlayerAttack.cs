using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private string attackTriggerName = "attack";
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Animator animator;
    private PlayerMovement playerMovement;
    private InputAction attackAction;
    private float cooldownRemaining;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        InputActionAsset actions = inputActions;
        if (actions == null)
        {
            actions = InputSystem.actions;
        }

        if (actions != null)
        {
            InputActionMap playerMap = actions.FindActionMap("Player");
            if (playerMap != null)
            {
                attackAction = playerMap.FindAction("Attack");
            }
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            try
            {
                attackAction.Enable();
            }
            catch (System.Exception)
            {
            }
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            try
            {
                attackAction.Disable();
            }
            catch (System.Exception)
            {
            }
        }
    }

    private void Update()
    {
        if (!GameManager.IsPlaying)
        {
            return;
        }

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (!IsAttackPressed())
        {
            return;
        }

        if (cooldownRemaining > 0f)
        {
            return;
        }

        if (playerMovement != null && !playerMovement.canAttack())
        {
            return;
        }

        PerformAttack();
    }

    private bool IsAttackPressed()
    {
        if (attackAction != null)
        {
            try
            {
                if (attackAction.WasPerformedThisFrame())
                {
                    return true;
                }
            }
            catch (System.Exception)
            {
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        if (keyboard.jKey.wasPressedThisFrame
            || keyboard.enterKey.wasPressedThisFrame)
        {
            return true;
        }

        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private void PerformAttack()
    {
        cooldownRemaining = attackCooldown;

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        if (projectilePrefab != null)
        {
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            GameObject spawnedProjectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            spawnedProjectile.SetActive(true);

            float facing = transform.localScale.x >= 0f ? 1f : -1f;
            Vector2 direction = new Vector2(facing, 0f);

            projectile projectileComponent = spawnedProjectile.GetComponent<projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.SetOwner(gameObject);
                projectileComponent.SetDirection(direction);
            }

            Vector3 projectileScale = spawnedProjectile.transform.localScale;
            projectileScale.x = Mathf.Abs(projectileScale.x) * facing;
            spawnedProjectile.transform.localScale = projectileScale;
        }

    }

}
