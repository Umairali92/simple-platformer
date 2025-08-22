using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public float xOffset = 0f;

    [Header("Smoothing")]
    [Tooltip("Lower = snappier, Higher = smoother.")]
    public float smoothTime = 0.15f;
    private float _xVelocity; // used by SmoothDamp

    [Header("Optional Dead Zone")]
    [Tooltip("Width (in world units) around the camera where the target can move without panning. 0 = off.")]
    public float deadZoneWidth = 0f;

    [Header("Optional Bounds")]
    public bool useBounds = false;
    public float minX = -999f;
    public float maxX = 999f;

    [Header("Side-view Lane Lock")]
    public bool lockZ = true;
    public float laneZ = 0f;

    void LateUpdate()
    {
        if (!target) return;

        // Desired X (with optional dead zone)
        float desiredX = target.position.x + xOffset;

        if (deadZoneWidth > 0f)
        {
            float half = deadZoneWidth * 0.5f;
            float left = transform.position.x - half;
            float right = transform.position.x + half;

            if (desiredX < left) desiredX = left;
            if (desiredX > right) desiredX = right;
        }

        if (useBounds)
            desiredX = Mathf.Clamp(desiredX, minX, maxX);

        // Smoothly move X only
        float newX = Mathf.SmoothDamp(transform.position.x, desiredX, ref _xVelocity, smoothTime);

        var pos = transform.position;
        pos.x = newX;
        if (lockZ) pos.z = laneZ; // keep camera/object on the lane
        transform.position = pos;
    }
}
