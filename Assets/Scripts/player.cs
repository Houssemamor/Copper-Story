using UnityEngine;
using UnityEngine.InputSystem;
 
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D body;
    private Animator anim;
    private bool grounded;
    
    [SerializeField] private InputActionAsset inputActions;
    
    
    private InputAction moveAction;
    private InputAction jumpAction;
    private string runParamName;
    private string groundedParamName;
    private string jumpParamName;
    
    private void Awake()
    {   
        //Grabs references for rigidbody and animator from game object.
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (body != null)
        {
            // Keep the rigid body active even when grounded to avoid input/connection stalls.
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        if (anim != null)
        {
            runParamName = FindAnimatorParameterName("run", AnimatorControllerParameterType.Bool);
            groundedParamName = FindAnimatorParameterName("grounded", AnimatorControllerParameterType.Bool);
            jumpParamName = FindAnimatorParameterName("jump", AnimatorControllerParameterType.Trigger);
            if (runParamName == null || groundedParamName == null || jumpParamName == null)
            {
                Debug.LogWarning("Player controller is missing required Animator parameters. Add 'run' and 'grounded' (bool) plus 'jump' (trigger) to the Animator Controller, case-sensitive matching isn't required.", this);
            }
        }
        
        if (inputActions == null)
        {
            inputActions = InputSystem.actions;
        }
        
        // Get the Move and Jump actions from the Input Actions asset
        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
                jumpAction = playerActionMap.FindAction("Jump");
            }
        }
    }
   
    private void OnEnable()
    {
        if (moveAction == null)
        {
            return;
        }
        
        // Enable the input actions
        try
        {
            moveAction.Enable();
        }
        catch (System.Exception)
        {
            // Ignore exceptions when enabling input actions
        }
        
        if (jumpAction != null)
        {
            try
            {
                jumpAction.Enable();
            }
            catch (System.Exception)
            {
                // Ignore exceptions when enabling input actions
            }
        }
    }
   
    private void OnDisable()
    {
        if (moveAction == null)
        {
            return;
        }
        
        // Disable the input actions
        try
        {
            moveAction.Disable();
        }
        catch (System.Exception)
        {
            // Ignore exceptions when disabling input actions
        }
        
        if (jumpAction != null)
        {
            try
            {
                jumpAction.Disable();
            }
            catch (System.Exception)
            {
                // Ignore exceptions when disabling input actions
            }
        }
    }
   
    private void Update()
    {
        float horizontalInput = 0f;
        bool jumpPressedThisFrame = false;

        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            horizontalInput = moveInput.x;
        }
        else
        {
            // Fallback direct keyboard read so movement/jump still work while
            // no InputActionAsset is wired (uses Input System, not legacy Input).
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                {
                    horizontalInput -= 1f;
                }
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                {
                    horizontalInput += 1f;
                }
            }
        }

        if (body != null)
        {
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
            body.WakeUp();
        }

        if (jumpAction != null)
        {
            if (jumpAction.WasPerformedThisFrame())
            {
                jumpPressedThisFrame = true;
            }
        }
        else
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
            {
                jumpPressedThisFrame = true;
            }
        }

        //Flip player when facing left/right.
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // Check for jump input
        if (grounded && jumpPressedThisFrame)
        {
            Jump();
        }

        //sets animation parameters
        if (anim != null)
        {
            if (!string.IsNullOrEmpty(runParamName))
            {
                anim.SetBool(runParamName, horizontalInput != 0);
            }
            if (!string.IsNullOrEmpty(groundedParamName))
            {
                anim.SetBool(groundedParamName, grounded);
            }
        }
        {
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
            body.WakeUp();
        }
 
        //Flip player when facing left/right.
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
 
        // Check for jump input
        if (grounded && jumpAction != null && jumpAction.WasPerformedThisFrame())
        {
            Jump();
        }
 
        //sets animation parameters
        if (anim != null)
        {
            if (!string.IsNullOrEmpty(runParamName))
            {
                anim.SetBool(runParamName, horizontalInput != 0);
            }
            if (!string.IsNullOrEmpty(groundedParamName))
            {
                anim.SetBool(groundedParamName, grounded);
            }
        }
    }
 
    private void Jump()
    {
        if (body != null)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, speed);
            body.WakeUp();
        }
        if (anim != null && !string.IsNullOrEmpty(jumpParamName))
        {
            anim.SetTrigger(jumpParamName);
        }
        grounded = false;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.contactCount > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (normal.y > 0.7f)
            {
                grounded = true;
            }
            else
            {
                // also accept ground tagged with "Ground"
                if (collision.gameObject.CompareTag("Ground"))
                {
                    grounded = true;
                }
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Continuously refresh grounded while in contact with something above (standing/landing scale), reducing stuck-in-air.
        if (collision == null || collision.contactCount <= 0)
        {
            return;
        }
        Vector2 normal = collision.contacts[0].normal;
        if (normal.y > 0.7f || collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }
    
    private string FindAnimatorParameterName(string name, AnimatorControllerParameterType type)
    {
        if (anim == null)
        {
            return null;
        }
        foreach (var p in anim.parameters)
        {
            if (p.type == type && string.Equals(p.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return p.name;
            }
        }
        return null;
    }
}
