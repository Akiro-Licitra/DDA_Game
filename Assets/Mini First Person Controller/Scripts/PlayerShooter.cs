using UnityEngine;
using System.Collections.Generic; // For List

public class PlayerShooter : MonoBehaviour
{
    public GameObject ballPrefab;       
    public Transform shootPoint;        
    public float shootForce = 20f;      
    public int maxBalls = 6;            

    private List<GameObject> activeBalls = new List<GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R)) // Reload key
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (ballPrefab == null || shootPoint == null)
        {
            return;
        }

        if (activeBalls.Count >= maxBalls)
        {
            return;
        }

        // Instantiate ball
        GameObject ball = Instantiate(ballPrefab, shootPoint.position, shootPoint.rotation);

        // Ensure Rigidbody is present and set properly
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = ball.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = Vector3.zero;  // Reset velocity before applying force

        // Ensure collider exists and is not a trigger
        Collider col = ball.GetComponent<Collider>();
        if (col == null)
        {
            //Debug.LogWarning("Ball prefab missing collider! Add a non-trigger collider to the prefab.");
        }
        else if (col.isTrigger)
        {
            //Debug.LogWarning("Ball prefab collider is set to trigger! Set isTrigger to false.");
        }

        // Apply force
        rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);

        // Track this ball
        activeBalls.Add(ball);
    }


    void Reload()
    {
        foreach (GameObject ball in activeBalls)
        {
            if (ball != null)
            {
                Destroy(ball);
            }
        }

        activeBalls.Clear();
    }
}

