using UnityEngine;

[CreateAssetMenu(fileName = "ChargeAttackModuleConfig", menuName = "Enemies/Configs/Attack/Charge")]
public class ChargeAttackModuleConfig : ScriptableObject
{
    public float speedMultiplier = 6f;
    public float stopDistance = 0.5f;
    public bool killPlayerOnBodyTouch = true;
}
