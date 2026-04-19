using UnityEngine;

/// <summary>
/// Attach alongside EnemyAI. Fires pellets at the player.
/// When FastProjectile modifier is active, pellets move faster AND glow purple.
/// Requires GlowEffect on the pellet prefab.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Small sphere/capsule prefab — needs Rigidbody, Collider, GlowEffect, tag EnemyPellet")]
    public GameObject pelletPrefab;

    [Header("Firing parameters")]
    public float baseFireRate    = 2.5f;
    public float shootRange      = 20f;
    public float basePelletSpeed = 18f;

    [Header("Stress scaling")]
    public float highStressFireRateMultiplier  = 1.5f;
    public float highStressPelletSpeedMult     = 1.3f;
    public float rapidFireMultiplier           = 2.0f;
    public float fastProjectileMultiplier      = 2.0f;

    [Header("Glow colors")]
    public Color fastProjectileGlowColor = new Color(0.7f, 0.1f, 1f, 1f); // purple

    [Header("Spawn offset")]
    public float muzzleOffset = 1.2f;

    private Transform player;
    private EnemyAI   enemyAI;
    private float     fireCooldown = 0f;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null && enemyAI.player != null)
            player = enemyAI.player;
        else
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null || pelletPrefab == null) return;
        if (enemyAI != null && enemyAI.IsKnockedback) return;

        fireCooldown -= Time.deltaTime;

        if (Vector3.Distance(transform.position, player.position) > shootRange) return;

        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = ComputeFireInterval();
        }
    }

    private float ComputeFireInterval()
    {
        float multiplier = 1f;
        if (EEGStressManager.Instance != null)
        {
            if (EEGStressManager.Instance.StressTier >= 2)
                multiplier = Mathf.Max(multiplier, highStressFireRateMultiplier);
            if (EEGStressManager.Instance.RapidFireActive)
                multiplier = Mathf.Max(multiplier, rapidFireMultiplier);
        }
        return baseFireRate / multiplier;
    }

    private float ComputePelletSpeed()
    {
        float speed = basePelletSpeed;
        if (EEGStressManager.Instance != null)
        {
            if (EEGStressManager.Instance.StressTier >= 2)
                speed *= highStressPelletSpeedMult;
            if (EEGStressManager.Instance.FastProjectileActive)
                speed *= fastProjectileMultiplier;
        }
        return speed;
    }

    private void Shoot()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + toPlayer * muzzleOffset + Vector3.up * 0.5f;

        GameObject pellet = Instantiate(pelletPrefab, spawnPos, Quaternion.LookRotation(toPlayer));

        // Apply velocity
        Rigidbody pelletRb = pellet.GetComponent<Rigidbody>();
        if (pelletRb != null)
        {
            pelletRb.linearVelocity = toPlayer * ComputePelletSpeed();
            pelletRb.useGravity     = false;
        }

        // Purple glow on fast projectiles
        bool fastActive = EEGStressManager.Instance != null
                          && EEGStressManager.Instance.FastProjectileActive;
        if (fastActive)
        {
            GlowEffect pelletGlow = pellet.GetComponent<GlowEffect>();
            if (pelletGlow != null)
                pelletGlow.SetGlow(true, fastProjectileGlowColor);
        }

        Destroy(pellet, 6f);
    }
}