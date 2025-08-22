using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Configs/Movement/Grounded")]
public class GroundMovementConfig : MovementConfig
{
    public float moveSpeed = 5f;
    public float gravity = -35f;
    public float leashX = 30f;
    public float laneZ = 0f;
    public float stopDistance = 0.5f;
}
