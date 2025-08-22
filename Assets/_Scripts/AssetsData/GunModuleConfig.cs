using UnityEngine;

[CreateAssetMenu(fileName = "GunModuleConfig", menuName = "Enemies/Configs/Attack/Gun")]
public class GunModuleConfig : ScriptableObject
{
    public float fireRate = 2f;
    public int burstCount = 1;
    public float burstInterval = 0.1f;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 3f;
    public bool flattenAimToSideView = true;
}
