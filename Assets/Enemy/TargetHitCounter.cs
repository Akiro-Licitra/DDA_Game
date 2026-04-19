using UnityEngine;

/// <summary>
/// Reads damage from the incoming PlayerBall rather than from EEG directly.
/// No reference to EEGOscReceiver needed here.
/// </summary>
public class TargetHitCounter : MonoBehaviour
{
    public int targetHP = 6;

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
        if (!obj.CompareTag("PlayerBall")) return;

        PlayerBall ball = obj.GetComponent<PlayerBall>();
        int damage = (ball != null) ? ball.damage : 1;

        targetHP -= damage;
        Debug.Log($"Hit: {damage} dmg. HP remaining: {targetHP}");

        if (targetHP <= 0)
            Destroy(gameObject);
    }
}