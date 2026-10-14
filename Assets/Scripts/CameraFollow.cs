using UnityEngine;

/// <summary>
/// Keeps the camera centered on the player and maintains a consistent world-height
/// view across resolutions by adjusting orthographicSize to match the reference PPU.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Vertical view (world units)")]
    [Tooltip("How many world units tall the camera view should be. 5 to 6 is typical for 2D platformers.")]
    [SerializeField] private float desiredWorldHeight = 6f;

    [Tooltip("Pixels Per Unit of your sprites (must match Sprite import settings).")]
    [SerializeField] private float pixelsPerUnit = 16f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            var player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) target = player.transform;
        }

        if (target != null)
        {
            Vector3 pos = target.position;
            pos.z = transform.position.z;
            transform.position = pos;
        }

        // Maintain constant world height regardless of screen resolution.
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = desiredWorldHeight / 2f;
        }
    }
}