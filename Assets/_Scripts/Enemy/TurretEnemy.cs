using UnityEngine;
using System.Collections;

[ModuleOrder(12)] // after sensing/awareness; no locomotion needed
[RequireComponent(typeof(Rigidbody))]
public class TurretEnemy : MonoBehaviour, IEnemyModule, IEnemyInit
{
    [Header("Configs")]
    [SerializeField] private GunModuleConfig gun;        // fireRate, burstCount, burstInterval, bulletSpeed, bulletLifetime, flattenAimToSideView (optional)
    [SerializeField] private BaseEnemyConfig coreOverride; // optional; falls back to ctx.Shared

    [Header("Scene Refs")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform barrelPivot;      // rotates to aim
    [SerializeField] private Bullet bulletPrefab;

    [Header("Pooling")]
    [SerializeField, Min(1)] private int poolSize = 16;

    [Header("Aiming")]
    [Tooltip("Clamp aiming to side-view (ignore Z). If GunModuleConfig has a flag, that takes precedence.")]
    public bool flattenAimToSideView = true;
    [Tooltip("If > 0, smoothly rotate the barrel toward target (deg/sec). 0 = snap.")]
    public float rotateSpeedDegPerSec = 0f;

    // --- runtime ---
    private EnemyContext _ctx;
    private BaseEnemyConfig _cfg;
    private LineOfSight _los;
    private Rigidbody _rb;

    private Bullet[] _pool;
    private int _idx;

    private Transform _player;
    private bool _active;

    // ------------- Init / Shutdown -------------
    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        _cfg = coreOverride ? coreOverride : ctx.Shared;
        _los = _ctx.LOS ? _ctx.LOS : GetComponent<LineOfSight>();
        _rb = GetComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.constraints = RigidbodyConstraints.FreezeAll;

        // Pool
        _pool = new Bullet[Mathf.Max(1, poolSize)];
        for (int i = 0; i < _pool.Length; i++)
        {
            _pool[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform);
            _pool[i].gameObject.SetActive(false);
        }
    }

    public void Shutdown()
    {

    }

    // ------------- IEnemyModule -------------
    public void OnActiveChanged(bool active)
    {
        _active = active;
        if (!active)
        {
            StopAllCoroutines();
            DeactivatePool();
            _player = null;
        }
    }

    public void OnStateEnter(EnemyCore.EnemyState s) { }
    public void OnStateExit(EnemyCore.EnemyState s) { }

    public void OnPlayerDetected(Transform player)
    {
        _player = player;
        StopAllCoroutines();
        StartCoroutine(FireLoop());
    }

    public void OnPlayerLost(Transform player)
    {
        if (_ctx.Core.PersistentAggro) return;
        if (_player == player)
        {
            StopAllCoroutines();
            _player = null;
        }
    }

    public void OnAlerted(GameEvents.AlertMessage msg)
    {
        _player = msg.target;
    }

    public void OnReset()
    {
        StopAllCoroutines();
        _player = null;
        DeactivatePool();
    }

    // ------------- Update: aim only -------------
    void Update()
    {
        if (!_player || !barrelPivot) return;

        Vector3 toTarget = _player.position - barrelPivot.position;
        toTarget.z = 0f;
        if (toTarget.sqrMagnitude < 1e-8f) return;

        float zAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        zAngle += 90f;

        Quaternion desired = Quaternion.Euler(0f, 0f, zAngle);

        if (rotateSpeedDegPerSec > 0f)
            barrelPivot.rotation = Quaternion.RotateTowards(
                barrelPivot.rotation, desired, rotateSpeedDegPerSec * Time.deltaTime);
        else
            barrelPivot.rotation = desired;
    }

    IEnumerator FireLoop()
    {
        float rate = Mathf.Max(0.01f, gun ? gun.fireRate : 2f);
        WaitForSeconds wait = new WaitForSeconds(1f / rate);

        while (_active && _player != null)
        {
            bool canShoot = true;

            // Respect LOS unless globally told to ignore it
            if (!_ctx.Core.IgnoreLOS && _cfg && _cfg.requireLOSOnSustain)
            {
                if (_los) canShoot = _los.HasLineOfSight(_player);
            }

            if (canShoot)
            {
                if (gun && gun.burstCount > 1) yield return StartCoroutine(Burst());
                else FireOne();
            }

            yield return wait;
        }
    }

    IEnumerator Burst()
    {
        float gap = Mathf.Max(0.01f, gun ? gun.burstInterval : 0.1f);
        WaitForSeconds bi = new WaitForSeconds(gap);

        int count = gun ? gun.burstCount : 3;
        for (int i = 0; i < count; i++)
        {
            FireOne();
            yield return bi;
        }
    }

    void FireOne()
    {
        var b = NextBullet();

        // Choose target and optionally clamp to lane Z
        Vector3 target = _player ? _player.position : (muzzle ? muzzle.position + muzzle.up : transform.position + Vector3.up);
        if (gun && gun.flattenAimToSideView || flattenAimToSideView) target.z = (muzzle ? muzzle.position.z : transform.position.z);

        // Project onto XY (remove Z component only)
        Vector3 dir = Vector3.ProjectOnPlane(target - (muzzle ? muzzle.position : transform.position), Vector3.forward);
        if (dir.sqrMagnitude < 1e-6f) dir = barrelPivot ? barrelPivot.up : Vector3.up;

        Vector3 spawn = muzzle ? muzzle.position : transform.position;
        if (gun && gun.flattenAimToSideView || flattenAimToSideView) spawn.z = (muzzle ? muzzle.position.z : transform.position.z);

        float speed = gun ? gun.bulletSpeed : 20f;
        float life = gun ? gun.bulletLifetime : 3f;

        b.Fire(spawn, dir, speed, life);

#if UNITY_EDITOR
        Debug.DrawLine(spawn, spawn + dir.normalized * 3f, Color.green, 0.25f);
#endif
    }

    Bullet NextBullet()
    {
        // Simple round-robin: return next inactive; if all active, reuse index 0
        for (int i = 0; i < _pool.Length; i++)
        {
            _idx = (_idx + 1) % _pool.Length;
            if (!_pool[_idx].gameObject.activeSelf) return _pool[_idx];
        }
        return _pool[0];
    }

    void DeactivatePool()
    {
        if (_pool == null) return;
        foreach (var b in _pool)
            if (b) b.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameEvents.RaisePlayerDied();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            GameEvents.RaisePlayerDied();
    }
}
