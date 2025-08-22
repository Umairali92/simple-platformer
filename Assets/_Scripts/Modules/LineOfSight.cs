using UnityEngine;

[ModuleOrder(-20)]
public class LineOfSight : MonoBehaviour, IEnemyInit
{
    [Header("Config (falls back to EnemyContext.Shared)")]
    [SerializeField] private BaseEnemyConfig config;

    [Header("Origin")]
    [SerializeField] private Transform eye;   // if not set, uses ctx.Root

    [Header("LoS Robustness")]
    [Tooltip("Treat LoS as visible if ANY of these points are unobstructed. 1=closest only; 3=head/center/feet.")]
    [Range(1, 8)] public int extraSamples = 3;

    [Tooltip("Multiplier of half-height used to sample head/feet (0.5..0.98).")]
    [Range(0.5f, 0.98f)] public float sampleMargin = 0.85f;

    [Tooltip("Use SphereCast instead of Raycast to avoid thin-gap misses.")]
    public bool useSphereCast = true;

    [Range(0.01f, 0.5f)] public float sphereRadius = 0.15f;

    [Tooltip("Ignore very near hits to avoid self/surface acne.")]
    [Range(0f, 0.1f)] public float skin = 0.01f;

    [Header("Sampling Options")]
    [Tooltip("If target has a Collider, use exact head/feet from collider; otherwise use bounds.")]
    public bool preferCapsuleEndpoints = true;

    private EnemyContext _ctx;

    [Header("Debug")]
    public bool debugRays = false;
    public float debugDuration = 0.05f;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        if (!config) config = _ctx.Shared;
        if (!eye) eye = _ctx.Core.Eye != null ? _ctx.Core.Eye : transform;
    }

    public void Shutdown() { }

    public bool HasLineOfSight(Transform target)
    {
        if (!target) return false;
        var col = target.GetComponent<Collider>();
        Vector3 from = eye.position;

        // Fallback to center if no collider
        if (!col)
            return Clear(from, target.position);

        // Candidate points
        var b = col.bounds;
        Vector3 center = b.center;
        Vector3 up = 0.85f * b.extents.y * Vector3.up;
        Vector3 down = 0.85f * b.extents.y * Vector3.down;

        Vector3 pClosest = col.ClosestPoint(from);
        if ((pClosest - from).sqrMagnitude < 1e-6f) pClosest = center; // guard if inside

        Vector3 pHead = center + up;
        Vector3 pTorso = center;
        Vector3 pFeet = center + down;

        Vector3 horiz = (center - from); horiz.y = 0f; horiz = horiz.sqrMagnitude > 1e-6f ? horiz.normalized : Vector3.right;
        Vector3 pNearShoulder = center + 0.8f * Mathf.Min(b.extents.x, b.extents.z) * horiz;

        if (Clear(from, pClosest)) return true;
        if (extraSamples >= 2 && Clear(from, pHead)) return true;
        if (extraSamples >= 3 && Clear(from, pTorso)) return true;
        if (extraSamples >= 4 && Clear(from, pFeet)) return true;
        if (extraSamples >= 5 && Clear(from, pNearShoulder)) return true;

        return false;
    }

    bool Clear(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 1e-5f) return true;
        dir /= dist;

        float maxDist = Mathf.Max(0f, dist - skin * 2f);
        Vector3 start = from + dir * skin;

        if (useSphereCast)
        {
            return !Physics.SphereCast(start, sphereRadius, dir, out var _,
                                       maxDist, config ? config.occluders : ~0,
                                       QueryTriggerInteraction.Ignore);
        }
        else
        {
            return !Physics.Raycast(start, dir, maxDist, config ? config.occluders : ~0,
                                    QueryTriggerInteraction.Ignore);
        }
    }

#if UNITY_EDITOR
    public void DebugDraw(Transform target, bool visible)
    {
        Color c = visible ? Color.green : Color.red;
        Debug.DrawLine(eye ? eye.position : transform.position, target.position, c, 0.05f);
    }
#endif
}

