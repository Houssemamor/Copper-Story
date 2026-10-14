using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private string attackTriggerName = "attack";
    [SerializeField] private InputActionAsset inputActions;

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
    }
}
