using UnityEngine;
public interface IEnemyModule
{
    void OnActiveChanged(bool active);
    void OnStateEnter(EnemyCore.EnemyState s);
    void OnStateExit(EnemyCore.EnemyState s);
    void OnPlayerDetected(Transform player);
    void OnPlayerLost(Transform player);
    void OnAlerted(GameEvents.AlertMessage msg);
    void OnReset();
}