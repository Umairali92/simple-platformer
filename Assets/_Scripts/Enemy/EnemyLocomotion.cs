using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.STP;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyOffMeshTraverserController))]
[ModuleOrder(10)]
public class EnemyLocomotion : MonoBehaviour, IEnemyModule, IEnemyInit
{
    [SerializeField] private NavMeshAgentMovementConfig config;

    // Runtime
    private EnemyContext _ctx;
    private NavMeshAgent _agent;
    private BaseEnemyConfig _shared;

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;

    private Transform _target;
    private bool _active;
    private float _nextRepathAt;

    private enum Mode { Idle, Chase, Return }
    private Mode _mode = Mode.Idle;

    /// <summary>
    /// Optional runtime multiplier (e.g., Charger speed boost). Set from other modules.
    /// </summary>
    public float ExternalSpeedMultiplier { get; set; } = 1f;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        _agent = ctx.Agent ? ctx.Agent : GetComponent<NavMeshAgent>();
        _shared = ctx.Shared;

        _spawnPos = transform.position;
        _spawnRot = transform.rotation;

        if (config && config.setAgentOptions)
        {
            _agent.autoTraverseOffMeshLink = config.autoTraverseOffMeshLink;
            _agent.obstacleAvoidanceType = config.avoidance;
            if (config.zeroAngularSpeed) _agent.angularSpeed = 0f;
            if (config.disableUpdateRotation) _agent.updateRotation = false;
            if (config.overrideStoppingDistance.HasValue)
                _agent.stoppingDistance = config.overrideStoppingDistance.Value;
        }

        if (config && config.lockZ)
        {
            var p = transform.position; p.z = config.laneZ; transform.position = p;
        }

        EnsureOnNavMesh();
        _agent.nextPosition = transform.position;
    }

    public void Shutdown() { }

    public void OnActiveChanged(bool active)
    {
        _active = active;
        if (!active)
        {
            _mode = Mode.Idle;
            _target = null;
            _agent.isStopped = true;
            _agent.ResetPath();
            return;
        }
        if (_agent.isActiveAndEnabled)
            _agent.isStopped = false;
    }

    public void OnStateEnter(EnemyCore.EnemyState s) { }
    public void OnStateExit(EnemyCore.EnemyState s) { }

    public void OnPlayerDetected(Transform player)
    {
        _target = player;
        _mode = Mode.Chase;
        if (_agent.isActiveAndEnabled)
            _agent.isStopped = false;
        _nextRepathAt = 0f;
    }

    public void OnPlayerLost(Transform player)
    {
        if (_ctx.Core.PersistentAggro) return;
        if (_target == player)
        {
            _target = null;
            if (config && config.returnToSpawnOnLost)
            {
                _mode = Mode.Return;
                SetDestination(_spawnPos);
            }
            else
            {
                _mode = Mode.Idle;
                _agent.isStopped = true;
                _agent.ResetPath();
            }
        }
    }

    public void OnAlerted(GameEvents.AlertMessage msg)
    {
        if (msg.target)
        {
            _target = msg.target;
            _mode = Mode.Chase;
            if (_agent.isActiveAndEnabled)
            {
                _agent.isStopped = false;
            }
            _nextRepathAt = 0f;
        }
    }

    public void OnReset()
    {
        _target = null;
        _mode = Mode.Idle;

        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        transform.SetPositionAndRotation(_spawnPos, _spawnRot);
        if (config && config.lockZ)
        {
            var p = transform.position; p.z = config.laneZ; transform.position = p;
        }
        EnsureOnNavMesh();
        _agent.Warp(transform.position);
        ExternalSpeedMultiplier = 1f;
    }

    // ---------- Update ----------
    void Update()
    {
        if (!_active || config == null) return;

        // Lane lock belt & suspenders
        if (config.lockZ)
        {
            var p = transform.position; p.z = config.laneZ; transform.position = p;
            var n = _agent.nextPosition; n.z = config.laneZ; _agent.nextPosition = n;
        }

        switch (_mode)
        {
            case Mode.Chase:
                if (_target) TickChase();
                else _mode = Mode.Idle;
                break;

            case Mode.Return:
                TickReturn();
                break;

            case Mode.Idle:
                break;
        }

        if (config.faceVelocityX)
        {
            var v = _agent.velocity;
            if (v.sqrMagnitude > 0.0001f)
                transform.right = new Vector3(Mathf.Sign(v.x), 0f, 0f);
        }
    }

    void TickChase()
    {
        if (_agent.isOnOffMeshLink) return;

        float now = Time.time;
        if (now < _nextRepathAt) return;
        _nextRepathAt = now + 1f / Mathf.Max(0.01f, config.repathHz);

        _agent.speed = (config.speed >= 0f ? config.speed: _agent.speed) * ExternalSpeedMultiplier;

        if (DistanceXZ(transform.position, _target.position) <= config.stopDistance * 0.8f)
            return;

        SetDestination(_target.position);
    }

    void TickReturn()
    {
        if (_agent.pathPending) return;

        _agent.speed = (config.speed >= 0f ? config.speed : _agent.speed) * ExternalSpeedMultiplier;

        if (_agent.remainingDistance <= Mathf.Max(0.01f, config.arriveTolerance))
        {
            _mode = Mode.Idle;
            _agent.isStopped = true;
        }
    }

    // ---------- Helpers ----------
    void SetDestination(Vector3 world, float sampleRadius = 1.0f)
    {
        if (config && config.lockZ) world.z = config.laneZ;

        EnsureOnNavMesh();

        if (NavMesh.SamplePosition(world, out var hit, sampleRadius, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
        else
            _agent.SetDestination(world);

        _agent.isStopped = false;
    }

    /// <summary>
    /// If placed slightly off the navmesh (e.g., after reset), warp to nearest point.
    /// </summary>
    bool EnsureOnNavMesh(float searchRadius = 2f)
    {
        if (_agent.isOnNavMesh) return true;
        if (NavMesh.SamplePosition(transform.position, out var hit, searchRadius, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
            return true;
        }
        return false;
    }

    static float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
