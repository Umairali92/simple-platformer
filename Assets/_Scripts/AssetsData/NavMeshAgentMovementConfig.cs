using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Enemies/Locomotion/NavAgent Locomotion Config")]
public class NavMeshAgentMovementConfig : ScriptableObject
{
    [Header("Speed")]
    [Tooltip("If >0, overrides agent.speed each frame. If 0, uses Agent.speed")]
    public float speed = 0f;

    [Header("Pathing")]
    [Tooltip("How often we refresh SetDestination while chasing (Hz).")]
    [Range(1f, 20f)] public float repathHz = 6f;
    [Tooltip("Stop repathing when closer than this in XZ; let killboxes handle contact.")]
    public float stopDistance = 0.5f;

    [Header("2.5D Lane")]
    public bool lockZ = true;
    public float laneZ = 0f;

    [Header("Return Home")]
    [Tooltip("When player is lost and aggro not persistent, return to spawn.")]
    public bool returnToSpawnOnLost = true;
    public float arriveTolerance = 0.05f;

    [Header("Agent Setup")]
    public bool setAgentOptions = true;
    public bool autoTraverseOffMeshLink = false; // set true if you DON'T use the custom jump traverser
    public ObstacleAvoidanceType avoidance = ObstacleAvoidanceType.NoObstacleAvoidance;
    public bool zeroAngularSpeed = true; // we don't rotate in a lane
    public bool disableUpdateRotation = true; // we rotate manually if needed
    public float? overrideStoppingDistance = null; // null = leave as-is

    [Header("Quality of Life")]
    [Tooltip("On activation, adopt a global alert target (if any) so newly-enabled enemies start chasing.")]
    public bool adoptGlobalAlertOnActivate = true;
    public bool faceVelocityX = false; // flip along +X/-X based on velocity
}