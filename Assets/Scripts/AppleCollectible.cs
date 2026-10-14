using UnityEngine;

/// <summary>
/// Numbered apple pickup used by the Level 3 sequence puzzle.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AppleCollectible : MonoBehaviour
{
    [SerializeField, Range(1, 3)] private int appleNumber = 1;

    private bool collected;
    public int AppleNumber => appleNumber;

    private void Reset()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }

        CreateNumberLabel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other == null || other.GetComponentInParent<PlayerMovement>() == null)
        {
            return;
        }

        AppleSequenceManager sequenceManager = FindFirstObjectByType<AppleSequenceManager>();
        if (sequenceManager == null)
        {
            Debug.LogWarning("Apple pickup has no AppleSequenceManager in this scene.", this);
            return;
        }

        sequenceManager.TryCollect(this);
    }

    public void MarkCollected()
    {
        if (collected)
        {
            return;
        }

        collected = true;
        gameObject.SetActive(false);
    }

    private void CreateNumberLabel()
    {
        GameObject labelObject = new GameObject("Apple Number Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0.38f, -0.1f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = appleNumber.ToString();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 64;
        label.characterSize = 0.09f;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;

        Font builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtInFont != null)
        {
            label.font = builtInFont;
        }

        MeshRenderer labelRenderer = labelObject.GetComponent<MeshRenderer>();
        SpriteRenderer appleRenderer = GetComponent<SpriteRenderer>();
        if (labelRenderer != null && appleRenderer != null)
        {
            labelRenderer.sortingLayerID = appleRenderer.sortingLayerID;
            labelRenderer.sortingOrder = appleRenderer.sortingOrder + 1;
        }
    }
}
