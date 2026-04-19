using UnityEngine;
using System.Collections.Generic;

public class PlayerShooter : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform  shootPoint;
    public float      shootForce = 20f;
    public int        maxBalls   = 6;

    // Read by BulletUI
    public int BallsUsed      => activeBalls.Count;
    public int BallsRemaining => maxBalls - activeBalls.Count;

    private EEGOscReceiver   eeg;
    private List<GameObject> activeBalls = new List<GameObject>();

    void Start()
    {
        eeg = FindFirstObjectByType<EEGOscReceiver>();
        if (eeg == null)
            Debug.LogError("PlayerShooter: No EEGOscReceiver found in the scene!");
    }

    void Update()
    {
        // Clean up balls that were destroyed externally (e.g. hit something and got Destroy'd)
        activeBalls.RemoveAll(b => b == null);

        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetKeyDown(KeyCode.R))  Reload();
    }

    void Shoot()
    {
        if (ballPrefab == null || shootPoint == null) return;
        if (activeBalls.Count >= maxBalls) return;

        GameObject ball = Instantiate(ballPrefab, shootPoint.position, shootPoint.rotation);
        ball.tag = "PlayerBall";

        int damage    = (eeg != null) ? eeg.ballDamage : 1;
        PlayerBall bd = ball.GetComponent<PlayerBall>() ?? ball.AddComponent<PlayerBall>();
        bd.damage     = damage;

        Rigidbody rb = ball.GetComponent<Rigidbody>() ?? ball.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);

        activeBalls.Add(ball);
    }

    void Reload()
    {
        foreach (GameObject ball in activeBalls)
            if (ball != null) Destroy(ball);
        activeBalls.Clear();
    }
}