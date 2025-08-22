using UnityEngine;

[CreateAssetMenu(fileName = "BaseEnemyConfig", menuName = "Enemies/Configs/BaseEnemyConfig")]
public class BaseEnemyConfig : ScriptableObject
{
    // ---------------- Lifecycle / Identity ----------------
    [Header("Lifecycle")]
    [Tooltip("If true, this enemy starts in the Active state; otherwise it starts Inactive until enabled by gameplay.")]
    public bool startsActive = true;

    [Tooltip("If true, the player can kill this enemy by stomping its head.")]
    public bool canBeStompKilled = true;

    [Tooltip("Delay (seconds) before the enemy resets to its spawn state when the player dies or a global reset occurs.")]
    [Min(0f)] public float resetDelay = 0.1f;


    // ---------------- Detection / Sensing ----------------
    [Header("Detection")]
    [Tooltip("Radius (world units) of the initial proximity check used to find the player. Awareness then ramps based on distance/LOS.")]
    [Min(0.01f)] public float detectRadius = 8f;

    [Tooltip("How often (per second) the enemy samples for detection/LOS. Higher values = more reactive but more CPU.")]
    [Range(1, 30)] public int senseHz = 10;

    [Tooltip("Require a clear line of sight to ACQUIRE a target. If false, proximity alone can acquire.")]
    public bool requireLOSOnAcquire = true;

    [Tooltip("Require a clear line of sight to KEEP a target. If true, losing LOS causes 'OnPlayerLost' unless globally alerted.")]
    public bool requireLOSOnSustain = true;

    [Tooltip("Physics layers considered solid for line-of-sight checks (ray/sphere casts).")]
    public LayerMask occluders;

    [Tooltip("Physics layers considered as 'player' candidates for the initial radius scan.")]
    public LayerMask playerMask;


    // ---------------- Awareness (ramp-up model) ----------------
    [Header("Awareness")]
    [Tooltip("If true, awareness jumps to 100% instantly when player is in radius AND has LOS (no ramp).")]
    public bool instantAlert = false;

    [Tooltip("If true, EnemyCore will NOT forward OnPlayerDetected to behavior modules until awareness reaches 'Alerted'.")]
    public bool gateAggroUntilAlerted = true;

    [Tooltip("Inner fraction of the detection radius that counts as a 'hot zone'. Example: 0.7 means inside 70% of radius.")]
    [Range(0.1f, 1f)] public float hotZoneFraction = 0.7f;

    [Tooltip("If true, entering the hot zone with LOS instantly fills awareness to 100%. If false, uses 'hotZoneGainBoost'.")]
    public bool hotZoneInstantFill = true;

    [Tooltip("Multiplier to awareness gain rate while in hot zone (used only if 'hotZoneInstantFill' is false).")]
    [Min(0f)] public float hotZoneGainBoost = 3f;

    [Tooltip("When awareness reaches 'Alerted', automatically broadcast a global alert to the squad (see Alert Mode).")]
    public bool autoBroadcastGlobalOnAlert = true;

    [Tooltip("If true, awareness does not rise without LOS. If false, awareness can still rise occluded (scaled by 'occludedGainMultiplier').")]
    public bool requireLOSForGain = false;

    [Header("Awareness • Rates (per second)")]
    [Tooltip("Base rise rate of awareness when conditions allow (scaled by distance curve and LOS multipliers).")]
    [Min(0f)] public float baseRisePerSecond = 1.0f;

    [Tooltip("Decay rate when NO player is in detection radius.")]
    [Min(0f)] public float decayWhenNoTarget = 0.7f;

    [Tooltip("Decay rate when player is in radius but WITHOUT LOS (only used when 'requireLOSForGain' is true, or when occluded).")]
    [Min(0f)] public float decayInRadiusNoLOS = 0.35f;

    [Tooltip("Multiplier applied to gain when occluded (if 'requireLOSForGain' is false). Example: 0.25 = gain at 25% speed while occluded.")]
    [Min(0f)] public float occludedGainMultiplier = 0.25f;

    [Header("Awareness • Distance shaping")]
    [Tooltip("Maps proximity (0 at radius edge → 1 at enemy) to a multiplier for awareness gain. Default eases in near the enemy.")]
    public AnimationCurve proximityToGain = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Awareness • Thresholds")]
    [Tooltip("Awareness >= this value transitions into the 'Suspicious' stage.")]
    [Range(0f, 1f)] public float suspiciousThreshold = 0.35f;

    [Tooltip("Awareness >= this value transitions into the 'Alerted' stage.")]
    [Range(0f, 1f)] public float alertThreshold = 1.0f;

    [Tooltip("Seconds to wait after becoming 'Alerted' before broadcasting (gives a brief anticipation window).")]
    [Min(0f)] public float graceSecondsBeforeAlert = 0.2f;


    // ---------------- Coordination / Squads ----------------
    [Header("Coordination")]
    [Tooltip("Enemies with the same Squad ID respond to each other's alerts. Use -1 for 'all squads'.")]
    public int squadId = 0;


    // ---------------- Alert Mode (global behaviors) ----------------
    [Header("Alert Mode")]
    [Tooltip("Radius (world units) for global alert broadcast. Use -1 to alert everyone (all distances).")]
    public float alertRadius = 60f;

    [Tooltip("When alerted, enemies ignore LOS requirements (Detector/LOS sustain checks are bypassed).")]
    public bool ignoreLOSWhenAlerted = true;

    [Tooltip("When alerted, enemies keep aggro until the player dies (EnemyCore.PersistentAggro).")]
    public bool persistentAggroUntilPlayerDies = true;
}