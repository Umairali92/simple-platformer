using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnPlayerDied;
    public struct AlertMessage
    {
        public Transform target;
        public float radius;     // -1 => global
        public int squadId;      // -1 => any
        public bool ignoreLOS;
        public bool persistentAggro;
    }

    public static event Action<AlertMessage> OnGlobalAlert;

    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();

    public static void RaiseGlobalAlert(Transform _target, float radius, int squadId, bool ignoreLOS, bool persistentAggro)
        => OnGlobalAlert?.Invoke(new AlertMessage
        {
            target = _target,
            radius = radius,
            squadId = squadId,
            ignoreLOS = ignoreLOS,
            persistentAggro = persistentAggro
        });
}
