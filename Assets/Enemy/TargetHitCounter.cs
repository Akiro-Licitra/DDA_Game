using UnityEngine;

public class TargetHitCounter : MonoBehaviour
{
    public int targetHP = 6;      // HP instead of hits
    private EEGOscReceiver eeg;       // reference to your global manager

    private void Start()
    {
        // Finds the eegManager script in the scene
        eeg = FindObjectOfType<EEGOscReceiver>();

        if (eeg == null)
        {
            Debug.LogError("TargetHitCounter: No eegManager found in the scene!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckBall(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckBall(other.gameObject);
    }

    private void CheckBall(GameObject obj)
    {
        if (!obj.CompareTag("PlayerBall"))
            return;

        // Read the global damage value
        int damage = eeg.ballDamage;

        // Apply damage
        targetHP -= damage;

        Debug.Log($"Ball hit: Damage {damage}. HP remaining: {targetHP}");

        if (targetHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
