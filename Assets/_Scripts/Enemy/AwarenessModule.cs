using UnityEngine;

public enum AwarenessStage { Calm, Suspicious, Alerted }
[RequireModule(typeof(PlayerDetector))]
[ModuleOrder(-5)]
public class AwarenessModule : MonoBehaviour, IEnemyModule, IEnemyInit
{
    [SerializeField] private BaseEnemyConfig config;
    [SerializeField] private Transform eye;

    public float Awareness01 { get; private set; }
    public AwarenessStage Stage { get; private set; } = AwarenessStage.Calm;

    public bool GatesAggro => config && config.gateAggroUntilAlerted;
    public bool IsAlerted => Stage == AwarenessStage.Alerted;

    public event System.Action<float> AwarenessChanged;
    public event System.Action<AwarenessStage> StageChanged;
    public event System.Action<Transform> FullyAlerted;

    private EnemyContext _ctx;
    private Transform _player;
    private float _nextTick;
    private float _tickInterval;
    private float _grace;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        if (!config) config = _ctx.Shared;
        if (!eye) eye = transform;

        _tickInterval = 1f / Mathf.Max(1, config ? config.senseHz : 10);

        // Subscribe to detector samples via context
        _ctx.Detector.Sampled += OnSample;

        // Reset + active change via signals
        _ctx.Signals.ResetRequested += OnReset;
        _ctx.Signals.ActiveChanged += a => { /* if needed */ };
    }

    public void Shutdown()
    {
        if (_ctx != null && _ctx.Detector != null)
            _ctx.Detector.Sampled -= OnSample;
        if (_ctx != null) _ctx.Signals.ResetRequested -= OnReset;
    }


    // IEnemyModule (minimal to integrate reset/alerts)
    public void OnActiveChanged(bool active) { }
    public void OnStateEnter(EnemyCore.EnemyState s) { }
    public void OnStateExit(EnemyCore.EnemyState s) { }
    public void OnPlayerDetected(Transform player) { _player = player; }
    public void OnPlayerLost(Transform player)
    {
        if (!_ctx.Core.PersistentAggro && _player == player)
            _player = null;
    }
    public void OnAlerted(GameEvents.AlertMessage msg) { SetAwareness(1f, forceStage: true); }
    public void OnReset()
    {
        _player = null;
        _grace = 0f;
        SetAwareness(0f, forceStage: true);
    }

    void OnSample(DetectionSample s)
    {
        if (_ctx.Core.IgnoreLOS) return;

        // Rate limit (same cadence as detection)
        if (Time.time < _nextTick) return;
        _nextTick = Time.time + _tickInterval;

        // Bind current candidate
        _player = s.player;

        // INSTANT policy?
        if (config && config.instantAlert && s.inRadius && s.hasLOS)
        {
            SetAwareness(1f, forceStage: true);
            MaybeBroadcast();
            FullyAlerted?.Invoke(_player);
            return;
        }

        // GRADUAL policy
        float delta = ComputeDelta(s);
        if (Mathf.Abs(delta) > 0.0001f)
            SetAwareness(Mathf.Clamp01(Awareness01 + delta * _tickInterval));

        var newStage = CalcStage(Awareness01);
        if (newStage != Stage) SetStage(newStage);

        if (Stage == AwarenessStage.Alerted && _player != null)
        {
            _grace += _tickInterval;
            if (_grace >= (config ? config.graceSecondsBeforeAlert : 0.2f))
            {
                MaybeBroadcast();
                FullyAlerted?.Invoke(_player);
                _grace = 0f; // fire once
            }
        }
        else _grace = 0f;
    }

    float ComputeDelta(DetectionSample s)
    {
        if (!s.inRadius || s.player == null) // fully out → decay
            return -(config ? config.decayWhenNoTarget : 0.7f);

        // LOS policy
        bool hasLOS = s.hasLOS;
        if (config && config.requireLOSForGain && !hasLOS)
            return -(config ? config.decayInRadiusNoLOS : 0.35f);

        // Distance shaping
        float R = Mathf.Max(0.0001f, s.radius);
        float proximity = 1f - Mathf.Clamp01(s.distance / R); // 0..1 (1 = at enemy)
        float shaped = config ? config.proximityToGain.Evaluate(proximity) : proximity;

        // Hot zone rule (≤ 0.7R by default)
        float hot = config ? config.hotZoneFraction : 0.7f;
        bool inHotZone = s.distance <= (R * hot);

        if (hasLOS && inHotZone && config)
        {
            if (config.hotZoneInstantFill) { SetAwareness(1f); return 0f; }
            shaped *= Mathf.Max(1f, config.hotZoneGainBoost);
        }

        float rise = (config ? config.baseRisePerSecond : 1f) * shaped * (hasLOS ? 1f : (config ? config.occludedGainMultiplier : 0.25f));
        return rise;
    }

    AwarenessStage CalcStage(float a)
    {
        if (config == null) return a >= 1f ? AwarenessStage.Alerted :
                                 (a >= 0.35f ? AwarenessStage.Suspicious : AwarenessStage.Calm);
        if (a >= config.alertThreshold - 1e-4f) return AwarenessStage.Alerted;
        if (a >= config.suspiciousThreshold - 1e-4f) return AwarenessStage.Suspicious;
        return AwarenessStage.Calm;
    }

    void SetAwareness(float v, bool forceStage = false)
    {
        float old = Awareness01;
        Awareness01 = Mathf.Clamp01(v);
        if (!Mathf.Approximately(old, Awareness01))
            AwarenessChanged?.Invoke(Awareness01);

        if (forceStage)
        {
            var ns = CalcStage(Awareness01);
            if (ns != Stage) SetStage(ns);
        }
    }

    void SetStage(AwarenessStage s)
    {
        Stage = s;
        StageChanged?.Invoke(Stage);
    }

    void MaybeBroadcast()
    {
        if (!config || !config.autoBroadcastGlobalOnAlert || !_ctx.Core.Shared) return;
        GameEvents.RaiseGlobalAlert(
            transform,
            -1f,
            _ctx.Core.Shared.squadId,
            ignoreLOS: true,
            persistentAggro: true
        );
    }
}
