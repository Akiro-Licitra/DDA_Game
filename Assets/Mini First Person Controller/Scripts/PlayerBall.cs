using UnityEngine;

/// <summary>
/// Attach to the player ball prefab.
/// Damage is written once at spawn time by PlayerShooter — never changes after that.
/// Also drives the green glow via GlowEffect if present.
/// </summary>
public class PlayerBall : MonoBehaviour
{
    /// <summary>Set by PlayerShooter immediately after Instantiate.</summary>
    public int damage = 1;
}