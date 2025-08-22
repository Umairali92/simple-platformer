using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyOffMeshTraverserController : MonoBehaviour
{
    public enum Mode { Parabola, Linear }

    [Header("Lane (2.5D)")]
    public float laneZ = 0f;
    public bool lockZ = true;

    [Header("Traversal Style")]
    public Mode mode = Mode.Parabola;

    [Tooltip("Extra arc height at mid-air for Parabola mode.")]
    public float arcHeight = 1.5f;
    [Tooltip("Horizontal speed along the link (m/s).")]
    public float traverseSpeed = 7f;
    [Tooltip("Rotate to face travel direction while jumping.")]
    public bool faceVelocity = false;

    NavMeshAgent _agent;
    Coroutine _traverse;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.autoTraverseOffMeshLink = false;  // <-- important
        _agent.updateRotation = false;           // we’ll rotate manually if needed
        _agent.angularSpeed = 0f;
    }

    void Update()
    {
        // Keep agent & transform on the lane each frame (belt + suspenders)
        if (lockZ)
        {
            var p = transform.position; p.z = laneZ; transform.position = p;
            var n = _agent.nextPosition; n.z = laneZ; _agent.nextPosition = n;
        }

        if (_agent.isOnOffMeshLink && _traverse == null)
            _traverse = StartCoroutine(TraverseLink());
    }

    IEnumerator TraverseLink()
    {
        var data = _agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = data.endPos + Vector3.up * _agent.baseOffset;

        start.z = end.z = laneZ;

        // Duration from horizontal distance / speed
        float horiz = Mathf.Abs(end.x - start.x) + Mathf.Abs(end.z - start.z);
        float dur = Mathf.Max(0.1f, horiz / Mathf.Max(0.01f, traverseSpeed));

        _agent.updatePosition = false; // we’ll drive transform

        float t = 0f;
        Vector3 prev = start;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float u = Mathf.Clamp01(t);

            Vector3 pos = (mode == Mode.Parabola)
                ? Parabola(start, end, arcHeight, u)
                : Vector3.Lerp(start, end, u);

            if (lockZ) pos.z = laneZ;

            if (faceVelocity)
            {
                var vel = pos - prev;
                if (vel.sqrMagnitude > 1e-6f)
                    transform.right = new Vector3(Mathf.Sign(vel.x) * 1f, 0f, 0f); // or use LookRotation on XY if you want pitch
            }

            transform.position = pos;
            _agent.nextPosition = pos; // keep agent in sync
            prev = pos;
            yield return null;
        }

        _agent.updatePosition = true;
        _agent.CompleteOffMeshLink();
        _traverse = null;
    }

    static Vector3 Parabola(Vector3 a, Vector3 b, float height, float t)
    {
        // Quadratic Bezier with apex offset = height at t=0.5
        Vector3 p = Vector3.Lerp(a, b, t);
        float arc = 4f * t * (1f - t); // 0..1 bell curve
        p.y += height * arc;
        return p;
    }
}
