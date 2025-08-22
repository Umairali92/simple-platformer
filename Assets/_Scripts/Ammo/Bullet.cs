using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _totalLifetime = 5f;
    [SerializeField] private Vector3 _dir = Vector3.forward;

    [Header("Filtering")]
    [Tooltip("Only these layers will register as hits.")]
    [SerializeField] private LayerMask _hitLayers = ~0;
    [Tooltip("Ignore collisions with objects tagged the same as this (e.g. the shooter). Leave blank to ignore none.")]
    [SerializeField] private string _ignoreTag = "";

    private float _elapsedLifetime;
    private float _laneZ;
    private Rigidbody _rb;
    private Collider _col;

    public void Fire(Vector3 pos, Vector3 dir, float speed, float lifetime)
    {
        _laneZ = pos.z;
        transform.position = pos;
        _dir = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.forward;
        _speed = speed;
        _totalLifetime = lifetime;
        _elapsedLifetime = 0f;
        gameObject.SetActive(true);

        if (!_rb.isKinematic)
        {
            _rb.linearVelocity = _dir * _speed;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        if (_rb.isKinematic)
        {
            _col.isTrigger = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            _col.isTrigger = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void OnEnable()
    {
        _elapsedLifetime = 0f;
    }

    private void Update()
    {
        // Manual movement for kinematic bullets
        if (_rb.isKinematic)
        {
            transform.position += _speed * Time.deltaTime * _dir;
        }

        // Keep bullet locked to its lane Z if desired
        var p = transform.position;
        p.z = _laneZ;
        transform.position = p;

        _elapsedLifetime += Time.deltaTime;
        if (_elapsedLifetime >= _totalLifetime)
            Deactivate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_rb.isKinematic) return;
        if (!IsValidHit(other.gameObject)) return;

        HandleHit(other.gameObject, contactPoint: transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_rb.isKinematic) return;
        if (!IsValidHit(collision.gameObject)) return;

        Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        HandleHit(collision.gameObject, contactPoint: hitPoint);
    }

    private bool IsValidHit(GameObject other)
    {
        if (!string.IsNullOrEmpty(_ignoreTag) && other.CompareTag(_ignoreTag))
            return false;

        int otherLayerMask = 1 << other.layer;
        if ((_hitLayers.value & otherLayerMask) == 0)
            return false;

        if (other == this.gameObject)
            return false;

        return true;
    }

    private void HandleHit(GameObject hitObject, Vector3 contactPoint)
    {
        Debug.Log($"Bullet hit: {hitObject.name} (layer: {LayerMask.LayerToName(hitObject.layer)}) at {contactPoint}");

        if (hitObject.CompareTag("Player"))
        {
            GameEvents.RaisePlayerDied();
        }
        // TODO: Add other reactions here (damage enemies, props, shields, etc.)
        Deactivate();
    }

    private void Deactivate()
    {
        if (_rb && !_rb.isKinematic) _rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + _dir.normalized * 0.5f);
    }
#endif
}
