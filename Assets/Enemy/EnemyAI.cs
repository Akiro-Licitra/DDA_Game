using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float followRange = 35f;
    public float stopRange   = 7f;
    public float roamRadius  = 21f;
    public float roamSpeed   = 12f;
    public float followSpeed = 12f;

    [Header("Collision Settings")]
    public float bounceResistance  = 0.25f;
    public float maxKnockbackForce = 1000f;

    [Header("Glow")]
    public Color rapidFireGlowColor = new Color(1f, 0.08f, 0.02f, 1f); // red

    // Exposed so EnemyShooter can check without a separate field
    public bool IsKnockedback { get; private set; } = false;

    private NavMeshAgent navMeshAgent;
    private Rigidbody    rb;
    private Vector3      roamTarget;
    private GlowEffect   glow;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb           = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // GlowEffect is optional — only added if you put it on the prefab
        glow = GetComponent<GlowEffect>();

        SetNewRoamTarget();
    }

    void Update()
    {
        UpdateGlow();

        if (IsKnockedback) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= followRange)
        {
            if (dist > stopRange)
            {
                navMeshAgent.speed = followSpeed;
                navMeshAgent.SetDestination(player.position);
            }
            else
            {
                navMeshAgent.ResetPath();
            }
        }
        else
        {
            Roam();
        }
    }

    private void UpdateGlow()
    {
        if (glow == null) return;

        bool rapidFire = EEGStressManager.Instance != null
                         && EEGStressManager.Instance.RapidFireActive;
        glow.SetGlow(rapidFire, rapidFireGlowColor);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerBall"))
        {
            Vector3 dir      = (transform.position - collision.transform.position).normalized;
            float   strength = collision.relativeVelocity.magnitude;
            ApplyKnockback(dir, strength);
        }
    }

    private void ApplyKnockback(Vector3 direction, float strength)
    {
        float effectiveForce = Mathf.Min(strength * (1f - bounceResistance), maxKnockbackForce);
        navMeshAgent.enabled = false;
        rb.AddForce(direction * effectiveForce, ForceMode.Impulse);
        IsKnockedback = true;
        Invoke(nameof(ResetAfterKnockback), 0.5f);
    }

    private void ResetAfterKnockback()
    {
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        navMeshAgent.enabled = true;
        IsKnockedback = false;
        navMeshAgent.Warp(transform.position);
    }

    private void Roam()
    {
        if (navMeshAgent.remainingDistance < 0.5f)
            SetNewRoamTarget();

        navMeshAgent.speed = roamSpeed;
        navMeshAgent.SetDestination(roamTarget);
    }

    private void SetNewRoamTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius + transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas);
        roamTarget = hit.position;
    }
}