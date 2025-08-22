using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Enemies/Locomotion/NavAgent Locomotion Config")]
public class NavMeshAgentMovementConfig : ScriptableObject
{
    [Header("Speed")]
    [Tooltip("If > 0, this value overwrites NavMeshAgent.speed every frame. If 0, the agent's own speed is used.")]
    [Min(0f)] public float speed = 0f;

    [Header("Pathing")]
    [Tooltip("How many times per second to refresh SetDestination while chasing. Higher = tighter pathing, more CPU.")]
    [Range(1f, 20f)] public float repathHz = 6f;

    [Tooltip("If horizontal (XZ) distance to target is <= this value, stop repathing and let melee/killboxes handle contact.")]
    [Min(0f)] public float stopDistance = 0.5f;

    [Header("2.5D Lane")]
    [Tooltip("If true, constrain movement to a fixed Z lane (useful for 2.5D).")]
    public bool lockZ = true;

    [Tooltip("Z value for the lane when lockZ is enabled. The controller should clamp/teleport Z to this value.")]
    public float laneZ = 0f;

    [Header("Return Home")]
    [Tooltip("If true, when the player is lost (and aggro isn’t persistent), the agent returns to its spawn/origin.")]
    public bool returnToSpawnOnLost = true;

    [Tooltip("Consider 'arrived' when distance to the spawn/origin is within this tolerance.")]
    [Min(0f)] public float arriveTolerance = 0.05f;

    [Header("Agent Setup")]
    [Tooltip("If true, apply the below agent options automatically on activation (recommended for consistent behavior).")]
    public bool setAgentOptions = true;

    [Tooltip("If true, the agent traverses Off-Mesh Links automatically. Disable if you have a custom jump/climb handler.")]
    public bool autoTraverseOffMeshLink = false;

    [Tooltip("Obstacle avoidance quality used when setAgentOptions is true. NoObstacleAvoidance = cheapest, best for lanes.")]
    public ObstacleAvoidanceType avoidance = ObstacleAvoidanceType.NoObstacleAvoidance;

    [Tooltip("If true, set NavMeshAgent.angularSpeed = 0 (no physical turning). Useful when you flip sprites or rotate manually.")]
    public bool zeroAngularSpeed = true;

    [Tooltip("If true, set NavMeshAgent.updateRotation = false so rotation is handled by your code/animator.")]
    public bool disableUpdateRotation = true;

    [Tooltip("If true, overwrite NavMeshAgent.stoppingDistance with the value below (applied only when setAgentOptions is true).")]
    public bool overrideStoppingDistance = false;

    [Tooltip("Value to use for NavMeshAgent.stoppingDistance when overrideStoppingDistance is enabled.")]
    [Min(0f)] public float stoppingDistance = 0f;

    [Header("Quality of Life")]
    [Tooltip("If true, on activation the agent will adopt any global alert target so newly-enabled enemies start chasing immediately.")]
    public bool adoptGlobalAlertOnActivate = true;

    [Tooltip("If true, flip/face along +X or -X according to current X velocity (for 2D sprites/2.5D characters).")]
    public bool faceVelocityX = false;
}