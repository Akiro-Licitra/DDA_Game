using UnityEngine;

/// <summary>
/// Reusable pulsing emissive glow. Attach to any GameObject with a Renderer.
/// Call SetGlow(true, color) to activate, SetGlow(false) to deactivate.
///
/// Requires the material to use a shader that supports _EmissionColor
/// (Standard, URP/Lit, or any custom shader with that property).
/// Make sure "Emission" is enabled on the material in the Inspector —
/// the script will enable it at runtime automatically.
/// </summary>
public class GlowEffect : MonoBehaviour
{
    [Header("Pulse settings")]
    [Tooltip("Minimum emission intensity multiplier")]
    public float minIntensity = 0.6f;
    [Tooltip("Maximum emission intensity multiplier")]
    public float maxIntensity = 2.2f;
    [Tooltip("Pulses per second")]
    public float pulseSpeed   = 2.8f;

    [Header("Debug — set in Inspector to force a color on")]
    public bool  debugForceOn    = false;
    public Color debugForceColor = Color.white;

    // -------------------------------------------------------------------------
    private Renderer[]  renderers;
    private Material[]  instanceMaterials; // owned copies so we don't taint shared assets
    private bool        glowActive = false;
    private Color       glowColor  = Color.white;
    private float       pulseTimer = 0f;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // Clone materials so we modify instances, not shared assets
        var mats = new System.Collections.Generic.List<Material>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                m.EnableKeyword("_EMISSION");
                mats.Add(m);
            }
        }
        instanceMaterials = mats.ToArray();
    }

    void Update()
    {
        bool shouldGlow = glowActive || (debugForceOn);
        Color activeColor = debugForceOn ? debugForceColor : glowColor;

        if (!shouldGlow)
        {
            SetEmission(Color.black);
            return;
        }

        pulseTimer += Time.deltaTime * pulseSpeed;
        // Smooth sine pulse between min and max intensity
        float t         = (Mathf.Sin(pulseTimer * Mathf.PI * 2f) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        SetEmission(activeColor * intensity);
    }

    /// <summary>
    /// Activate or deactivate the glow. Safe to call every frame — does nothing
    /// if the state hasn't changed.
    /// </summary>
    public void SetGlow(bool active, Color color = default)
    {
        glowActive = active;
        if (active && color != default)
            glowColor = color;
    }

    private void SetEmission(Color emission)
    {
        foreach (var m in instanceMaterials)
        {
            if (m != null)
                m.SetColor(EmissionColorID, emission);
        }
    }

    void OnDestroy()
    {
        // Release cloned material instances
        foreach (var m in instanceMaterials)
            if (m != null) Destroy(m);
    }
}