using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Configs/Movement/Airborne")]
public class AirMovementConfig : MovementConfig
{
    public float moveSpeed = 4f;
    public float hoverHeight = 2f;
    public float hoverBand = 0.5f;
    public float bobAmplitude = 0.25f;
    public float bobHz = 0.6f;
    public float leashX = 30f;
    public float laneZ = 0f;
}
