using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour, IPlayerKinematics
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    [Range(0f, 1f)] public float airControl = 0.6f;

    [Header("Jump")]
    public float jumpForce = 9f;
    public int maxAirJumps = 0;

    [Header("Better Jump Feel")]
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    [Tooltip("Multiplier for gravity while falling (>1 = faster fall).")]
    public float fallMultiplier = 2.2f;
    [Tooltip("Multiplier for gravity when jump is released early.")]
    public float lowJumpMultiplier = 2.0f;
    [Tooltip("Optional extra snappiness when releasing jump (scales current upward speed).")]
    [Range(0.0f, 1.0f)] public float jumpCutMultiplier = 0.6f;
    [Tooltip("Clamp maximum downward speed (m/s). 0 = no clamp.")]
    public float maxFallSpeed = 20f;

    [Header("Side-view Lane")]
    public bool lockToLaneZ = true;
    public float laneZ = 0f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckOffset = 0.05f;

    public Vector3 Velocity => _rb ? _rb.linearVelocity : Vector3.zero;
    [SerializeField] private Vector3 _vel;

    Rigidbody _rb;
    CapsuleCollider _col;

    bool _isGrounded;
    float _coyoteTimer;
    float _jumpBufferTimer;
    bool _jumpHeld;
    int _airJumpsUsed;

    Vector3 spawnPos;

    void Awake()
    {
        spawnPos = transform.position;
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();

        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.freezeRotation = true;

        ApplyLaneConstraint();

        GameEvents.OnPlayerDied += PlayerKilled;

    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerDied += PlayerKilled;
    }

    void OnValidate() => ApplyLaneConstraint();

    void ApplyLaneConstraint()
    {
        if (!_rb) _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _rb.constraints = lockToLaneZ
            ? RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ
            : RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (lockToLaneZ && Mathf.Abs(_rb.position.z - laneZ) > 0.0001f)
        {
            var p = _rb.position; p.z = laneZ; _rb.position = p;
        }

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpBufferTimer = jumpBuffer; // start buffer
            _jumpHeld = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _jumpHeld = false;
            if (_rb.linearVelocity.y > 0f && jumpCutMultiplier < 1f)
            {
                var v = _rb.linearVelocity;
                v.y *= jumpCutMultiplier;
                _rb.linearVelocity = v;
            }
        }
        _vel = _rb.linearVelocity;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        GroundCheck();

        if (!_isGrounded) _coyoteTimer -= dt; else _coyoteTimer = coyoteTime;
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= dt;

        if (_jumpBufferTimer > 0f && CanJump())
        {
            DoJump();
            _jumpBufferTimer = 0f;
        }

        float h = Input.GetAxisRaw("Horizontal");
        Vector3 vel = _rb.linearVelocity;

        float control = _isGrounded ? 1f : airControl;
        float targetX = h * moveSpeed;
        vel.x = Mathf.Lerp(vel.x, targetX, control);

        if (lockToLaneZ) vel.z = 0f;

        if (vel.y < 0f)
        {
            vel.y += Physics.gravity.y * (fallMultiplier - 1f) * dt;
        }
        else if (vel.y > 0f && !_jumpHeld)
        {
            vel.y += Physics.gravity.y * (lowJumpMultiplier - 1f) * dt;
        }

        // Terminal fall clamp
        if (maxFallSpeed > 0f && vel.y < -maxFallSpeed)
            vel.y = -maxFallSpeed;

        _rb.linearVelocity = vel;
    }

    void GroundCheck()
    {
        Vector3 center = _rb.position + Vector3.up * (_col.radius + groundCheckOffset);
        float castDist = (_col.height * 0.5f) - _col.radius + groundCheckOffset;

        bool wasGrounded = _isGrounded;
        _isGrounded = Physics.SphereCast(
            center,
            _col.radius * 0.95f,
            Vector3.down,
            out _,
            castDist,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (_isGrounded)
        {
            _airJumpsUsed = 0;
            if (!wasGrounded && _rb.linearVelocity.y < 0f)
            {
                var v = _rb.linearVelocity; v.y = 0f; _rb.linearVelocity = v;
            }
        }
    }

    bool CanJump()
    {
        if (_isGrounded || _coyoteTimer > 0f) return true;
        return _airJumpsUsed < Mathf.Max(0, maxAirJumps);
    }

    void DoJump()
    {
        var v = _rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;
        v.y = jumpForce;
        _rb.linearVelocity = v;

        if (!(_isGrounded || _coyoteTimer > 0f))
            _airJumpsUsed++;
    }

    private void PlayerKilled()
    {
        gameObject.SetActive(false);
        Invoke(nameof(ResetPlayer), 0.5f);
    }

    private void ResetPlayer()
    {
        transform.position = spawnPos;
        gameObject.SetActive(true);
    }
}
