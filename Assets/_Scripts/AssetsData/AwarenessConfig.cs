using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Configs/Awareness")]
public class AwarenessConfig : ScriptableObject
{
    [Header("Policy")]
    public bool instantAlert = false;                  // if true: no delay, aggro immediately on acquire+LOS
    public bool gateAggroUntilAlerted = true;         // if true: EnemyCore waits for awareness=Alerted before forwarding OnPlayerDetected
    [Range(0.1f, 1f)] public float hotZoneFraction = 0.7f; // inner ring (e.g., 70% of radius)
    public bool hotZoneInstantFill = true;            // inside hot zone + LOS => set awareness=1 immediately
    public float hotZoneGainBoost = 3f;               // if not instant, multiply gain inside hot zone

    [Header("Rates (per second)")]
    public float baseRisePerSecond = 1.0f;
    public float decayWhenNoTarget = 0.7f;
    public float decayInRadiusNoLOS = 0.35f;
    public float occludedGainMultiplier = 0.25f;

    [Header("Distance shaping")]
    // x: proximity (0 near, 1 edge), y: multiplier
    public AnimationCurve proximityToGain = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Thresholds")]
    [Range(0, 1)] public float suspiciousThreshold = 0.35f;
    [Range(0, 1)] public float alertThreshold = 1.0f;
    public float graceSecondsBeforeAlert = 0.2f;      // anticipation before broadcasting

    [Header("Behavior")]
    public bool autoBroadcastGlobalOnAlert = true;    // raise squad alert on full
    public bool requireLOSForGain = false;            // if true, no gain without LOS
}
