using UnityEngine;

[ModuleOrder(5)]
public class AlertEnemy : MonoBehaviour, IEnemyModule, IEnemyInit
{
    [Header("Config (optional override; falls back to EnemyContext.Shared)")]
    [SerializeField] private BaseEnemyConfig configOverride;

    [Tooltip("Log when broadcasting (useful while wiring).")]
    public bool logBroadcast = false;

    private EnemyContext _ctx;
    private BaseEnemyConfig _cfg;
    private int _squadId;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        _cfg = configOverride ? configOverride : ctx.Shared;
        _squadId = _ctx.Core.Shared ? _ctx.Core.Shared.squadId : -1;
    }

    public void Shutdown() { }

    // -------- IEnemyModule --------
    public void OnActiveChanged(bool active) { }
    public void OnStateEnter(EnemyCore.EnemyState s) { }
    public void OnStateExit(EnemyCore.EnemyState s) { }
    public void OnReset() {  }

    public void OnPlayerDetected(Transform player)
    {
        if (_cfg == null || player == null) return;

        if (logBroadcast) Debug.Log($"[{name}] Broadcasting GLOBAL ALERT for {player.name}");

        GameEvents.RaiseGlobalAlert(
            player,
            _cfg.alertRadius,                  
            _squadId,                         
            _cfg.ignoreLOSWhenAlerted,         
            _cfg.persistentAggroUntilPlayerDies
        );
    }

    public void OnPlayerLost(Transform player)  { }

    public void OnAlerted(GameEvents.AlertMessage msg)  { }
}
