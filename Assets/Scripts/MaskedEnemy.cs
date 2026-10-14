using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Level 4 "Masked" enemy.
// - 10 hit points: each player projectile removes one.
// - Flashes white twice (0.2 s per flash) when hit.
// - Fires green projectiles in a straight horizontal line toward the player,
//   at a height the player can jump over.
// - Defeating it wins the level.
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class MaskedEnemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHitPoints = 10;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float firstShotDelay = 1.5f;
    [SerializeField] private float shotInterval = 2f;
    [SerializeField] private float projectileSpeed = 6f;
    [Tooltip("Horizontal distance from the enemy's centre where shots spawn.")]
    [SerializeField] private float fireOffsetX = 1.2f;
    [Tooltip("Min height of the shot above the enemy's feet.")]
    [SerializeField] private float minFireHeightAboveFeet = 0f;
    [Tooltip("Max height of the shot above the enemy's feet.")]
    [SerializeField] private float maxFireHeightAboveFeet = 1f;
    [SerializeField] private Color projectileColor = new Color(0.2f, 1f, 0.2f, 1f);

    [Header("Hit Flash")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private int flashCount = 2;
    [Tooltip("Optional. If left empty, Shader.Find(\"Sprites/WhiteFlash\") is used.")]
    [SerializeField] private Shader flashShader;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer flashOverlay;
    private Collider2D bodyCollider;
    private PlayerMovement player;
    private int hitPoints;
    private float shotTimer;
    private bool defeated;
    private Coroutine flashRoutine;

    public int HitPoints => hitPoints;
    public int MaxHitPoints => maxHitPoints;
    public bool IsDefeated => defeated;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();

        hitPoints = maxHitPoints;
        shotTimer = firstShotDelay;

        // The enemy still falls onto the floor, but player shots can't push it sideways.
        body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

        CreateFlashOverlay();
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnableWinTimer();
        }
    }

    private void Update()
    {
        if (defeated || !GameManager.IsPlaying)
        {
            return;
        }

        shotTimer -= Time.deltaTime;
        if (shotTimer <= 0f)
        {
            Shoot();
            shotTimer = shotInterval;
        }
    }

    // Runs after the Animator, so the overlay always matches the current animation frame.
    private void LateUpdate()
    {
        if (flashOverlay == null || spriteRenderer == null)
        {
            return;
        }

        flashOverlay.sprite = spriteRenderer.sprite;
        flashOverlay.flipX = spriteRenderer.flipX;
        flashOverlay.flipY = spriteRenderer.flipY;
    }

    public void TakeHit()
    {
        if (defeated)
        {
            return;
        }

        hitPoints = Mathf.Max(0, hitPoints - 1);

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashWhite());

        if (hitPoints == 0)
        {
            Defeat();
        }
    }

    private IEnumerator FlashWhite()
    {
        // Make sure we don't inherit a stuck "on" state from an interrupted flash.
        flashOverlay.enabled = false;

        for (int i = 0; i < flashCount; i++)
        {
            flashOverlay.enabled = true;
            yield return new WaitForSeconds(flashDuration);

            flashOverlay.enabled = false;
            yield return new WaitForSeconds(flashDuration);
        }

        flashRoutine = null;
    }

    private void Defeat()
    {
        defeated = true;
        StopAllCoroutines();
        flashRoutine = null;

        // Hide the enemy and stop it from being hit again. Done without deactivating
        // the object, since this runs inside a physics collision callback.
        flashOverlay.enabled = false;
        spriteRenderer.enabled = false;
        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Win(SceneManager.GetActiveScene().name);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
        }
        if (player == null)
        {
            return;
        }

        // Straight horizontal shot toward the player's side.
        float dirX = player.transform.position.x >= transform.position.x ? 1f : -1f;

        // Randomize shot height so the player must react (jump or crouch).
        float heightJitter = Random.Range(minFireHeightAboveFeet, maxFireHeightAboveFeet);

        GameObject shot = Instantiate(projectilePrefab, GetFirePosition(dirX, heightJitter), Quaternion.identity);
        shot.SetActive(true);

        SpriteRenderer shotRenderer = shot.GetComponentInChildren<SpriteRenderer>();
        if (shotRenderer != null)
        {
            shotRenderer.color = projectileColor;
        }

        Vector3 shotScale = shot.transform.localScale;
        shotScale.x = Mathf.Abs(shotScale.x) * dirX;
        shot.transform.localScale = shotScale;

        projectile shotProjectile = shot.GetComponent<projectile>();
        if (shotProjectile != null)
        {
            shotProjectile.SetOwner(gameObject);
            shotProjectile.SetSpeed(projectileSpeed);
            shotProjectile.SetDirection(new Vector2(dirX, 0f));
        }
    }

    private Vector3 GetFirePosition(float dirX, float heightJitter)
    {
        float feetY = bodyCollider != null ? bodyCollider.bounds.min.y : transform.position.y;
        return new Vector3(
            transform.position.x + dirX * fireOffsetX,
            feetY + heightJitter,
            0f);
    }

    private void CreateFlashOverlay()
    {
        // A white copy of the enemy's current sprite, drawn on top while flashing.
        var overlayObject = new GameObject("HitFlashOverlay");
        overlayObject.transform.SetParent(transform, false);

        flashOverlay = overlayObject.AddComponent<SpriteRenderer>();
        flashOverlay.sortingLayerID = spriteRenderer.sortingLayerID;
        flashOverlay.sortingOrder = spriteRenderer.sortingOrder + 1;
        flashOverlay.color = Color.white;

        Shader shader = flashShader != null ? flashShader : Shader.Find("Sprites/WhiteFlash");
        if (shader != null)
        {
            flashOverlay.sharedMaterial = new Material(shader);
        }
        else
        {
            Debug.LogWarning(
                "MaskedEnemy: WhiteFlash shader not found. Assign it to 'flashShader' " +
                "in the inspector, or place it in a Resources folder and include it in " +
                "Always Included Shaders so Shader.Find works in a build.");
        }

        flashOverlay.enabled = false;
    }
}