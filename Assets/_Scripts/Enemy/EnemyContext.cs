using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyContext
{
    public EnemyCore Core { get; }
    public BaseEnemyConfig Shared { get; }
    public Transform Root { get; }
    public Transform Eye { get;}
    public PlayerDetector Detector { get; }
    public LineOfSight LOS { get; }
    public NavMeshAgent Agent { get; } // null if not mobile
    public EnemySignals Signals { get; }
    public EnemyContext(
    EnemyCore core,
    BaseEnemyConfig shared,
    Transform root,
    Transform eye,
    PlayerDetector detector,
    LineOfSight los,
    NavMeshAgent agent,
    EnemySignals signals)
    {
        Core = core;
        Shared = shared;
        Root = root;
        Eye = eye;
        Detector = detector;
        LOS = los;
        Agent = agent;
        Signals = signals;
    }
}
