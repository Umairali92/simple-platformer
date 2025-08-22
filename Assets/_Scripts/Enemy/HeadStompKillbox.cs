using UnityEngine;

public class HeadStompKillbox : MonoBehaviour
{
    [SerializeField] private EnemyCore core;
    [SerializeField] private float minDownwardVel = -4f;

    void Reset() { core = GetComponentInParent<EnemyCore>(); }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!core || !core.CanBeStompKilled) return;

        float vy = 0f;
        
        if (other.TryGetComponent<IPlayerKinematics>(out var kin)) vy = kin.Velocity.y;
        else { var rb = other.attachedRigidbody; if (rb) vy = rb.linearVelocity.y; }

        if (vy <= minDownwardVel)
        {
            core.OnStomped();
        }
    }
}