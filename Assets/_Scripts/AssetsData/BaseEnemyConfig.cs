using UnityEngine;

[CreateAssetMenu(fileName = "BaseEnemyConfig", menuName = "Enemies/Configs/BaseEnemyConfig")]
public class BaseEnemyConfig : ScriptableObject
{
    [Header("Lifecycle")]
    public bool startsActive = true;
    public bool canBeStompKilled = true;
    public float resetDelay = 0.1f;

    [Header("Detection (radius only; details in DetectionConfig)")]
    public float detectRadius = 8f;
    [Range(1, 30)] public int senseHz = 10;
    public bool requireLOSOnAcquire = true;
    public bool requireLOSOnSustain = true;
    public LayerMask occluders;
    public LayerMask playerMask;

    [Header("Awareness")]
    public bool instantAlert = false;                  // if true: no delay, aggro immediately on acquire+LOS
    public bool gateAggroUntilAlerted = true;         // if true: EnemyCore waits for awareness=Alerted before forwarding OnPlayerDetected
    [Range(0.1f, 1f)] public float hotZoneFraction = 0.7f; // inner ring (e.g., 70% of radius)
    public bool hotZoneInstantFill = true;            // inside hot zone + LOS => set awareness=1 immediately
    public float hotZoneGainBoost = 3f;               // if not instant, multiply gain inside hot zone
    public bool autoBroadcastGlobalOnAlert = true;    // raise squad alert on full
    public bool requireLOSForGain = false;            // if true, no gain without LOS

    [Header("Awareness - Rates (Per Second)")]
    public float baseRisePerSecond = 1.0f;
    public float decayWhenNoTarget = 0.7f;
    public float decayInRadiusNoLOS = 0.35f;
    public float occludedGainMultiplier = 0.25f;

    [Header("Awareness - Distance shaping")]
    public AnimationCurve proximityToGain = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Awareness - Thresholds")]
    [Range(0, 1)] public float suspiciousThreshold = 0.35f;
    [Range(0, 1)] public float alertThreshold = 1.0f;
    public float graceSecondsBeforeAlert = 0.2f;      // anticipation before broadcasting

    [Header("Coordination")]
    public int squadId = 0;

    [Header("Alert Mode")]
    public float alertRadius = 60f;
    public bool ignoreLOSWhenAlerted = true;
    public bool persistentAggroUntilPlayerDies = true;
}
