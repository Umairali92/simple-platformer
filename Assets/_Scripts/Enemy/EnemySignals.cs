using System;
using UnityEngine;

public sealed class EnemySignals
{
    // lifecycle
    public event Action<bool> ActiveChanged;
    public event Action<EnemyCore.EnemyState, EnemyCore.EnemyState> StateChanged;
    public event Action ResetRequested;

    // sensing & combat
    public event Action<Transform> PlayerDetected;
    public event Action<Transform> PlayerLost;
    public event Action<GameEvents.AlertMessage> Alerted;

    // Raise methods (internal to EnemyCore)
    internal void RaiseActiveChanged(bool a) => ActiveChanged?.Invoke(a);
    internal void RaiseStateChanged(EnemyCore.EnemyState oldS, EnemyCore.EnemyState newS) => StateChanged?.Invoke(oldS, newS);
    internal void RaiseReset() => ResetRequested?.Invoke();
    internal void RaisePlayerDetected(Transform t) => PlayerDetected?.Invoke(t);
    internal void RaisePlayerLost(Transform t) => PlayerLost?.Invoke(t);
    internal void RaiseAlerted(GameEvents.AlertMessage m) => Alerted?.Invoke(m);
}
