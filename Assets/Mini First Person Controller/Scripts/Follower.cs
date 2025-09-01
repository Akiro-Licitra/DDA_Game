using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform playerCamera;
    public float baseDistance = 1f;       // How far in front of camera horizontally
    public float maxVerticalOffset = 0.5f; // Max height offset when looking fully up/down

    void Update()
    {
        if (playerCamera == null) return;

        // Get the camera's pitch angle (rotation around X axis)
        float pitch = playerCamera.eulerAngles.x;

        // Convert pitch angle from 0-360 to -180 to 180
        if (pitch > 180) pitch -= 360;

        // Normalize pitch between -1 (looking down) and 1 (looking up)
        float normalizedPitch = Mathf.Clamp(pitch / 90f, -1f, 1f);

        // Calculate vertical offset based on pitch
        float verticalOffset = normalizedPitch * maxVerticalOffset;

        // Position the ShootPoint in front of camera + vertical offset
        Vector3 forwardPos = playerCamera.position + playerCamera.forward * baseDistance;
        Vector3 adjustedPos = new Vector3(forwardPos.x, playerCamera.position.y + verticalOffset, forwardPos.z);

        transform.position = adjustedPos;

        // Optional: match rotation
        transform.rotation = playerCamera.rotation;
    }
}

