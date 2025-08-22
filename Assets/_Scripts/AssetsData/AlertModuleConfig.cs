using UnityEngine;

[CreateAssetMenu(fileName = "AlertModuleConfig", menuName = "Enemies/Configs/Alert")]
public class AlertModuleConfig : ScriptableObject
{
    public float alertRadius = 60f;
    public bool ignoreLOSWhenAlerted = true;
    public bool persistentAggroUntilPlayerDies = true;
}
