using System;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PlayerDetector))]
public class EnemyCore : MonoBehaviour, IActivatable, IResettable, IStompable
{
    public enum EnemyState { Inactive, Idle, Aggro, Attack }

    [SerializeField] private BaseEnemyConfig shared;

    public BaseEnemyConfig Shared => shared;
    public EnemyState State { get; private set; }
    public bool IsActive { get; private set; }

    // Alert flags
    public bool IsAlerted { get; private set; }
    public bool IgnoreLOS { get; private set; }
    public bool PersistentAggro { get; private set; }
    public bool CanBeStompKilled => shared && shared.canBeStompKilled;

    [SerializeField] private Transform eye_trans;
    public Transform Eye => eye_trans;

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;

    private IEnemyModule[] _modules;
    private IEnemyInit[] _inits;
    private EnemySignals _signals;
    private EnemyContext _ctx;

    private PlayerDetector _detector;
    private AwarenessModule _awareness;
    private NavMeshAgent _agent;

    private Transform _pendingPlayer;

    private bool isInitialized;
    public void Init()
    {
        _spawnPos = transform.position;
        _spawnRot = transform.rotation;

        _signals = new EnemySignals();

        // Gather modules and sort by [ModuleOrder]
        _modules = GetComponents<IEnemyModule>();
        _inits = GetComponents<MonoBehaviour>().OfType<IEnemyInit>().ToArray();

        Array.Sort(_inits, CompareByOrder);
        Array.Sort(_modules, CompareByOrder);

        // Validate declared inter-module requirements (optional)
        ValidateRequirements();

        _detector = GetComponent<PlayerDetector>();
        _awareness = GetComponent<AwarenessModule>();
        _agent = GetComponent<NavMeshAgent>();

        _ctx = new EnemyContext(
            core: this,
            shared: shared,
            root: transform,
            eye: eye_trans,
            detector: _detector,
            los: GetComponent<LineOfSight>(),
            agent: _agent,
            signals: _signals
        );

        // Centralized init (no Awake/Start inside modules)
        foreach (var init in _inits) init.Initialize(_ctx);
        
        /*
        if (_detector != null)
        {
            _detector.Bind(this);
        }
        */
        
        if (_awareness) _awareness.FullyAlerted += HandleFullyAlerted;

        GameEvents.OnPlayerDied += HandlePlayerDied;
        GameEvents.OnGlobalAlert += HandleGlobalAlert;

        SetActive(shared ? shared.startsActive : true);
        SetState(IsActive ? EnemyState.Idle : EnemyState.Inactive);

        // Core listens to signals (so modules can raise without knowing Core)
        _signals.PlayerDetected += OnPlayerDetected;
        _signals.PlayerLost += OnPlayerLost;
        _signals.Alerted += HandleGlobalAlert;

        isInitialized = true;
        //_signals.ResetRequested += ResetToSpawn;
    }
    void OnDestroy()
    {
        if (!isInitialized) return;

        GameEvents.OnPlayerDied -= HandlePlayerDied;
        GameEvents.OnGlobalAlert -= HandleGlobalAlert;

        if (_awareness) _awareness.FullyAlerted -= HandleFullyAlerted;

        _signals.PlayerDetected -= OnPlayerDetected;
        _signals.PlayerLost -= OnPlayerLost;
        _signals.Alerted -= HandleGlobalAlert;
        // Allow modules to unhook
        foreach (var init in _inits) init.Shutdown();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        if (_detector) _detector.enabled = active;

        _signals.RaiseActiveChanged(active);

        foreach (var m in _modules) m.OnActiveChanged(active);
        SetState(active ? EnemyState.Idle : EnemyState.Inactive);
        if (!active) ClearAlertFlags();
    }

    void SetState(EnemyState s)
    {
        if (State == s) return;
        var old = State;
        foreach (var m in _modules) m.OnStateExit(old);
        State = s;
        foreach (var m in _modules) m.OnStateEnter(s);
        _signals.RaiseStateChanged(old, s);
    }

    public void OnPlayerDetected(Transform player)
    {
        if (!IsActive) return;
        if (_awareness && _awareness.GatesAggro && !_awareness.IsAlerted && !IsAlerted)
        {
            _pendingPlayer = player;
            SetState(EnemyState.Aggro);
            return;
        }
        SetState(EnemyState.Aggro);
        foreach (var m in _modules) m.OnPlayerDetected(player);
    }

    public void OnPlayerLost(Transform player)
    {
        if (!IsActive) return;
        if (PersistentAggro) return; // stay aggro when globally alerted
        foreach (var m in _modules) m.OnPlayerLost(player);
        SetState(EnemyState.Idle);
        ClearAlertFlags();
    }

    void HandleGlobalAlert(GameEvents.AlertMessage msg)
    {
        if (!IsActive) return;

        Debug.Log($"{gameObject.name}"+ msg.ToString());
        if (msg.squadId >= 0 && shared && shared.squadId != msg.squadId) 
        {
            Debug.Log($"Squad ID Does Not Match : Target Squad ID :  {msg.squadId}  : Enemy Squad ID : {shared.squadId}");
            return;
        }
        float distanceFromBroadcast = Vector3.Distance(transform.position, msg.target.position);
        if (msg.radius >= 0f && distanceFromBroadcast > msg.radius) 
        {
            Debug.Log($"Alert Distance is Bigger : Distance Allowed :  {msg.radius}  : Current Distance : {distanceFromBroadcast}");
            return;
        }

        IsAlerted = true;
        IgnoreLOS = msg.ignoreLOS;
        PersistentAggro = msg.persistentAggro;

        SetState(EnemyState.Aggro);
        foreach (var m in _modules) m.OnAlerted(msg);
    }

    void HandleFullyAlerted(Transform player)
    {
        // If we had a pending player, now forward detection to modules
        if (_pendingPlayer == null) _pendingPlayer = player;
        if (_pendingPlayer != null)
        {
            IsAlerted = true;
            IgnoreLOS = shared.persistentAggroUntilPlayerDies;
            PersistentAggro = shared.persistentAggroUntilPlayerDies;
            foreach (var m in _modules) m.OnPlayerDetected(_pendingPlayer);
            _pendingPlayer = null;
        }
    }

    void HandlePlayerDied()
    {
        Invoke(nameof(ResetToSpawn), shared ? shared.resetDelay : 0.1f);
    }

    public void ResetToSpawn()
    {
        gameObject.SetActive(true);
        _signals.RaiseReset();
        foreach (var m in _modules) m.OnReset();
        ClearAlertFlags();
        SetActive(shared ? shared.startsActive : true);
        transform.SetPositionAndRotation(_spawnPos, _spawnRot);
    }

    void ClearAlertFlags()
    {
        IsAlerted = false;
        IgnoreLOS = false;
        PersistentAggro = false;
    }

    public void OnStomped()
    {
        if (!CanBeStompKilled) return;
        gameObject.SetActive(false);
    }

    static int CompareByOrder(object a, object b)
    {
        int GetOrder(object o)
        {
            var attr = o.GetType().GetCustomAttributes(typeof(ModuleOrderAttribute), true).FirstOrDefault() as ModuleOrderAttribute;
            return attr?.Order ?? 0;
        }
        return GetOrder(a).CompareTo(GetOrder(b));
    }

    void ValidateRequirements()
    {
        var types = GetComponents<MonoBehaviour>().Select(c => c.GetType()).ToHashSet();
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            var reqs = comp.GetType().GetCustomAttributes(typeof(RequireModuleAttribute), true)
                .Cast<RequireModuleAttribute>();
            foreach (var r in reqs)
            {
                if (!types.Contains(r.ModuleType))
                    Debug.LogError($"{name}: {comp.GetType().Name} requires {r.ModuleType.Name} but it's missing.", this);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (shared)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, shared.detectRadius);
        }
    }
#endif
}