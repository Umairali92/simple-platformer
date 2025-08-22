using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[ModuleOrder(12)]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ChargerEnemy : MonoBehaviour, IEnemyModule, IEnemyInit
{
    [Header("Kill Settings")]
    [Tooltip("If true, also kill via body trigger collisions (requires a trigger collider).")]
    [SerializeField] private bool killOnTrigger = true;

    [SerializeField] private Rigidbody _rb;

    // --- runtime ---
    private EnemyContext _ctx;
    private EnemyLocomotion _locomotion;
    private Transform _player;
    private bool _active;

    [Header("For Debugging")]
    [SerializeField] private float distanceToPlayer;

    // ---------- Init ----------
    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        _locomotion = GetComponent<EnemyLocomotion>();
        _rb = GetComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Shutdown() { }

    // ---------- IEnemyModule ----------
    public void OnActiveChanged(bool active)
    {
        _active = active;
        if (!_active)
        {
            _player = null;
            if (_locomotion) _locomotion.ExternalSpeedMultiplier = 1f;
        }
    }

    public void OnStateEnter(EnemyCore.EnemyState s) { }
    public void OnStateExit(EnemyCore.EnemyState s) { }

    public void OnPlayerDetected(Transform player)
    {
        _player = player;
    }

    public void OnPlayerLost(Transform player)
    {
        if (_ctx.Core.PersistentAggro) return;
        if (_player == player)
        {
            _player = null;
            if (_locomotion) _locomotion.ExternalSpeedMultiplier = 1f;
        }
    }

    public void OnAlerted(GameEvents.AlertMessage msg)
    {
        _player = msg.target ? msg.target : _player;
    }

    public void OnReset()
    {
        _player = null;
    }

    void Update()
    {
        if (!_active || !_player) return;

        distanceToPlayer = DistanceXZ(transform.position, _player.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!killOnTrigger) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.activeInHierarchy)
            {
                GameEvents.RaisePlayerDied();
            }
        }
    }

    // ---------- Utils ----------
    static float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
