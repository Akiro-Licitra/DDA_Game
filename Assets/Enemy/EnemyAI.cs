using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float followRange = 35f;
    public float stopRange = 7f;
    public float roamRadius = 21f;
    public float roamSpeed = 12f;
    public float followSpeed = 12f;

    [Header("Collision Settings")]
    public float bounceResistance = 0.25f; // 0 = no resistance (full knockback), 1 = full resistance (no knockback)
    public float maxKnockbackForce = 1000f; // Adjusted for reasonable force

    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    private Vector3 roamTarget;
    private bool isKnockedback = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // Set to false to allow physics interactions
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Ensure continuous collision detection
        SetNewRoamTarget();
    }

    void Update()
    {
        if (isKnockedback) return; // Don't move while being knocked back

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= followRange)
        {
            if (distanceToPlayer > stopRange)
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerBall"))
        {
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            float impactStrength = collision.relativeVelocity.magnitude;
            ApplyKnockback(knockbackDirection, impactStrength);
        }
    }

    private void ApplyKnockback(Vector3 direction, float strength)
    {
        // Calculate knockback force based on resistance
        float effectiveForce = Mathf.Min(strength * (1f - bounceResistance), maxKnockbackForce);

        // Disable NavMeshAgent temporarily
        navMeshAgent.enabled = false;

        // Apply force
        rb.AddForce(direction * effectiveForce, ForceMode.Impulse);

        isKnockedback = true;
        Invoke(nameof(ResetAfterKnockback), 0.5f); // Resume AI after 0.5 seconds
    }

    private void ResetAfterKnockback()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        navMeshAgent.enabled = true;
        isKnockedback = false;

        // Warp to current position to prevent NavMesh issues
        navMeshAgent.Warp(transform.position);
    }

    private void Roam()
    {
        if (navMeshAgent.remainingDistance < 0.5f)
        {
            SetNewRoamTarget();
        }
        navMeshAgent.speed = roamSpeed;
        navMeshAgent.SetDestination(roamTarget);
    }

    private void SetNewRoamTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas);
        roamTarget = hit.position;
    }
}
