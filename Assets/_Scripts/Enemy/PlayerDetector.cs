using System;
using UnityEngine;

[Serializable]
public struct DetectionSample
{
    public Transform player;
    public bool inRadius;
    public float distance;
    public float radius;
    public bool hasLOS;
}
[RequireModule(typeof(LineOfSight))]
[ModuleOrder(-10)]
public class PlayerDetector : MonoBehaviour, IEnemyInit
{
    [SerializeField] private BaseEnemyConfig config;
    [SerializeField] private LineOfSight los;
    [SerializeField] private Transform eye;

    private EnemyContext _ctx;
    private float _interval, _next;
    private readonly Collider[] _hits = new Collider[4];
    private Transform _current;

    public event System.Action<DetectionSample> Sampled;

    [Header("For Debugging Purpose")]
    [SerializeField] private DetectionSample detectionSample;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        if (!eye) eye = transform;
        if (!los) los = _ctx.LOS;
        if (!config) config = _ctx.Shared;
        _interval = 1f / Mathf.Max(1, config ? config.senseHz : 10);

        _ctx.Signals.ActiveChanged += a => enabled = a;
        _ctx.Signals.ResetRequested += HandleReset;
    }
    public void Shutdown()
    {
        if (_ctx != null)
        {
            _ctx.Signals.ActiveChanged -= a => enabled = a;
            _ctx.Signals.ResetRequested -= HandleReset;
        }
    }
    void HandleReset()
    {
        _current = null;
        _next = 0f;
    }
    void Update()
    {
        if (_ctx?.Core == null || !_ctx.Core.IsActive) return;
        if (Time.time < _next) return;
        _next = Time.time + _interval;

        float radius = _ctx.Shared ? _ctx.Shared.detectRadius : 8f;
        Vector3 eyePos = eye ? eye.position : transform.position;

        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _hits, config ? config.playerMask : ~0);
        Transform found = (count > 0) ? _hits[0].transform : null;

        bool rawLOS = false;
        float dist = float.PositiveInfinity;
        if (found)
        {
            dist = Vector3.Distance(eyePos, found.position);
            rawLOS = !los || los.HasLineOfSight(found);
        }

        detectionSample = new DetectionSample
        {
            player = found,
            inRadius = found && dist <= radius,
            distance = found ? dist : radius + 1f,
            radius = radius,
            hasLOS = rawLOS
        };

        Sampled?.Invoke(detectionSample);

        // Acquire / sustain → push through signals (loose coupling)
        if (found != null)
        {
            bool needLOS = config && config.requireLOSOnAcquire && !_ctx.Core.IgnoreLOS;
            if (!needLOS || rawLOS)
            {
                if (_current != found)
                {
                    _current = found;
                    _ctx.Signals.RaisePlayerDetected(found);
                }
            }
        }
        else if (_current != null)
        {
            _ctx.Signals.RaisePlayerLost(_current);
            _current = null;
            return;
        }

        if (_current != null && config && config.requireLOSOnSustain && !_ctx.Core.IgnoreLOS)
        {
            bool sustainLOS = !los || los.HasLineOfSight(_current);
            if (!sustainLOS)
            {
                _ctx.Signals.RaisePlayerLost(_current);
                _current = null;
            }
        }
    }
}