using UnityEngine;

/// <summary>
/// Attach to the player's bullet/ball prefab.
/// Glows green when the player is in high-stress mode (tier >= 2),
/// which is the same state that increases bullet damage.
///
/// Requires a GlowEffect component on this GameObject (added automatically).
/// </summary>
[RequireComponent(typeof(GlowEffect))]
public class PlayerBulletGlow : MonoBehaviour
{
    [Header("Color")]
    public Color highStressColor = new Color(0.1f, 1f, 0.2f, 1f); // bright green

    private GlowEffect glow;

    void Awake()
    {
        glow = GetComponent<GlowEffect>();
    }

    void Update()
    {
        bool shouldGlow = false;

        if (EEGStressManager.Instance != null)
            shouldGlow = EEGStressManager.Instance.StressTier >= 2
                         || EEGStressManager.Instance.DebugHighStress;

        glow.SetGlow(shouldGlow, highStressColor);
    }
}