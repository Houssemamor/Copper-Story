using UnityEngine;

// Rotates 180° when hit by player projectile
public class WoodWall : MonoBehaviour
{
    [SerializeField] private float rotationAngle = 180f;
    [SerializeField] private float rotationSpeed = 360f; // deg/s

    private bool rotating;
    private float targetRotation;
    private Quaternion startRotation;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var projectile = other.GetComponent<projectile>();
        if (projectile == null)
        {
            return;
        }

        Rotate();
    }

    private void Rotate()
    {
        if (rotating)
        {
            return;
        }

        rotating = true;
        startRotation = transform.rotation;
        targetRotation = startRotation.eulerAngles.z + rotationAngle;
    }

    private void Update()
    {
        if (!rotating)
        {
            return;
        }

        float currentZ = transform.rotation.eulerAngles.z;
        float step = rotationSpeed * Time.deltaTime;
        float newZ = Mathf.MoveTowardsAngle(currentZ, targetRotation, step);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);

        if (Mathf.Approximately(Mathf.DeltaAngle(currentZ, targetRotation), 0f))
        {
            rotating = false;
        }
    }
}