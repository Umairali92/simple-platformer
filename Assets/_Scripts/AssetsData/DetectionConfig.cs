using UnityEngine;

[CreateAssetMenu(fileName = "DetectionConfig", menuName = "Enemies/Configs/Detection")]
public class DetectionConfig : ScriptableObject
{
    [Range(1, 30)] public int senseHz = 10;
    public bool requireLOSOnAcquire = true;
    public bool requireLOSOnSustain = true;
    public LayerMask occluders;
    public LayerMask playerMask;
}
