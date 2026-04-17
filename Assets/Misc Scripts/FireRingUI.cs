using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws an animated fire ring around the stamina arc.
/// Created automatically by StaminaBarUI — no manual setup needed.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class FireRingUI : MaskableGraphic
{
    [Header("Ring geometry")]
    public float outerRadius = 84f;
    public float innerRadius = 72f;

    [Header("Spike settings")]
    [Range(8, 48)] public int   spikeCount    = 22;
    public float                spikeMinExtra = 8f;
    public float                spikeMaxExtra = 22f;

    [Header("Animation")]
    public float animSpeed = 4f;

    [Header("Colors")]
    public Color innerFireColor = new Color(1f, 0.85f, 0.1f, 0.9f);
    public Color outerFireColor = new Color(0.9f, 0.15f, 0.02f, 0f);

    private bool  active  = false;
    private float timer   = 0f;
    private float[] noiseOffsets;

    // Pre-allocate noise offsets so each spike has its own phase
    protected override void Awake()
    {
        base.Awake();
        noiseOffsets = new float[64];
        for (int i = 0; i < noiseOffsets.Length; i++)
            noiseOffsets[i] = Random.Range(0f, TAU);
    }

    public void SetActive(bool value)
    {
        active = value;
        gameObject.SetActive(value);
    }

    void Update()
    {
        if (!active) return;
        timer += Time.deltaTime * animSpeed;
        SetVerticesDirty();
    }

    // -------------------------------------------------------------------------
    // Arc span — must mirror StaminaBarUI (π → 0 through bottom)
    // -------------------------------------------------------------------------
    private const float ArcStart = Mathf.PI;
    private const float ArcEnd   = 0f;
    private const float TAU      = Mathf.PI * 2f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!active) return;

        // Draw a series of radial spike triangles along the arc
        for (int i = 0; i < spikeCount; i++)
        {
            float t = i / (float)(spikeCount - 1);
            // Map t → angle along the arc
            float angle = Mathf.Lerp(ArcStart, ArcEnd, t);

            // Animated noise for spike height
            float n1 = Mathf.Sin(timer * 1.3f + noiseOffsets[i % noiseOffsets.Length]);
            float n2 = Mathf.Cos(timer * 2.1f + noiseOffsets[(i + 7) % noiseOffsets.Length] * 0.7f);
            float noise = (n1 * 0.6f + n2 * 0.4f); // range roughly -1..1

            float spikeHeight = Mathf.Lerp(spikeMinExtra, spikeMaxExtra,
                                            Mathf.InverseLerp(-1f, 1f, noise));

            float tipR  = outerRadius + spikeHeight;
            float baseR = innerRadius;

            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Perpendicular for base spread
            float perpCos = Mathf.Cos(angle + Mathf.PI * 0.5f);
            float perpSin = Mathf.Sin(angle + Mathf.PI * 0.5f);

            // Spread at base proportional to arc segment width
            float arcStep  = Mathf.PI / (spikeCount - 1);
            float baseSpread = baseR * Mathf.Sin(arcStep * 0.5f);
            baseSpread = Mathf.Min(baseSpread, 7f);

            Vector3 tip = new Vector3(cos * tipR, sin * tipR, 0f);
            Vector3 bl  = new Vector3(cos * baseR - perpCos * baseSpread,
                                      sin * baseR - perpSin * baseSpread, 0f);
            Vector3 br  = new Vector3(cos * baseR + perpCos * baseSpread,
                                      sin * baseR + perpSin * baseSpread, 0f);

            // Fade alpha toward tip and also vary per spike
            float baseAlpha = Mathf.Lerp(0.65f, 0.9f, Mathf.InverseLerp(-1f, 1f, n1));
            Color tipCol  = new Color(outerFireColor.r, outerFireColor.g, outerFireColor.b, 0f);
            Color baseCol = new Color(innerFireColor.r, innerFireColor.g, innerFireColor.b, baseAlpha);

            int idx = vh.currentVertCount;
            vh.AddVert(tip, tipCol,  Vector2.zero);
            vh.AddVert(bl,  baseCol, Vector2.zero);
            vh.AddVert(br,  baseCol, Vector2.zero);
            vh.AddTriangle(idx, idx + 1, idx + 2);
        }
    }
}
