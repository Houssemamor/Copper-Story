using UnityEngine;

/// <summary>
/// Level 3 puzzle: collect apples in the configured order before the goal becomes usable.
/// If the wrong apple is collected, all previously collected apples respawn and the sequence restarts.
/// </summary>
public class AppleSequenceManager : MonoBehaviour
{
    [SerializeField] private int[] requiredSequence = { 2, 1, 3 };
    [SerializeField] private GameObject goalObject;

    private int nextSequenceIndex;
    private AppleCollectible[] allApples;
    private bool[] collectedFlags;
    private SpriteRenderer[] goalRenderers;
    private Collider2D[] goalColliders;
    private Animator[] goalAnimators;

    public bool IsComplete => requiredSequence != null && nextSequenceIndex >= requiredSequence.Length;

    private void Awake()
    {
        CacheGoalComponents();
    }

    private void Start()
    {
        // Find all apples in the scene and cache their initial state.
        allApples = FindObjectsByType<AppleCollectible>(FindObjectsInactive.Include);
        collectedFlags = new bool[allApples.Length];

        // Keep the goal hidden and non-interactive until the full sequence is correct.
        SetGoalEnabled(false);
    }

    public bool TryCollect(AppleCollectible apple)
    {
        if (apple == null || IsComplete || requiredSequence == null || requiredSequence.Length == 0)
        {
            return false;
        }

        int expectedNumber = requiredSequence[nextSequenceIndex];
        if (apple.AppleNumber != expectedNumber)
        {
            Debug.Log($"Level 3: wrong apple ({apple.AppleNumber}), expected {expectedNumber}. Resetting sequence.", apple);
            ResetSequence();
            return false;
        }

        apple.MarkCollected();
        nextSequenceIndex++;

        if (IsComplete)
        {
            SetGoalEnabled(true);
            Debug.Log("Level 3: correct apple sequence (2, 1, 3). Goal enabled.", this);
        }
        else
        {
            Debug.Log($"Level 3: Apple {apple.AppleNumber} collected. Next: Apple {requiredSequence[nextSequenceIndex]}.", this);
        }

        return true;
    }

    private void ResetSequence()
    {
        nextSequenceIndex = 0;

        // Respawn all apples that were collected.
        for (int i = 0; i < allApples.Length; i++)
        {
            if (collectedFlags[i] && allApples[i] != null)
            {
                allApples[i].Respawn();
                collectedFlags[i] = false;
            }
        }

        Debug.Log("Level 3: sequence reset. All apples respawned.", this);
    }

    // Called by AppleCollectible when it gets collected so we can track it.
    public void RegisterCollected(AppleCollectible apple)
    {
        if (allApples == null) return;
        for (int i = 0; i < allApples.Length; i++)
        {
            if (allApples[i] == apple)
            {
                collectedFlags[i] = true;
                break;
            }
        }
    }

    private void CacheGoalComponents()
    {
        if (goalObject == null)
        {
            goalObject = GameObject.Find("Goal");
        }

        if (goalObject == null)
        {
            Debug.LogError("AppleSequenceManager could not find a GameObject named 'Goal' in Level 3.", this);
            return;
        }

        goalRenderers = goalObject.GetComponentsInChildren<SpriteRenderer>(true);
        goalColliders = goalObject.GetComponentsInChildren<Collider2D>(true);
        goalAnimators = goalObject.GetComponentsInChildren<Animator>(true);
    }

    private void SetGoalEnabled(bool enabledState)
    {
        if (goalObject == null || goalRenderers == null || goalColliders == null || goalAnimators == null)
        {
            CacheGoalComponents();
        }

        if (goalObject == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in goalRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = enabledState;
            }
        }

        foreach (Collider2D goalCollider in goalColliders)
        {
            if (goalCollider != null)
            {
                goalCollider.enabled = enabledState;
            }
        }

        foreach (Animator animator in goalAnimators)
        {
            if (animator != null)
            {
                animator.enabled = enabledState;
            }
        }
    }
}
