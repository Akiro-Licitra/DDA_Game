using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a half-moon stamina arc entirely in code — no sprites required.
/// Attach to a GameObject that has a CanvasRenderer.
/// The fire effect is handled by the child FireRingUI component.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class StaminaBarUI : MaskableGraphic
{
    [Header("Arc geometry")]
    [Range(40f, 200f)] public float outerRadius = 70f;
    [Range(10f, 190f)] public float innerRadius  = 48f;
    [Range(16, 128)]   public int   segments     = 64;

    [Header("Colors")]
    public Color fullColor     = new Color(1f, 0.85f, 0.05f, 1f);  // gold
    public Color emptyColor    = new Color(1f, 0.85f, 0.05f, 0.18f); // dim ghost

    [Header("Runtime — bind these")]
    // Drag FirstPersonMovement here in the Inspector, OR it will be found at runtime
    public FirstPersonMovement player;

    // Optional child for the fire ring — auto-created if not assigned
    [SerializeField] private FireRingUI fireRing;

    // -------------------------------------------------------------------------
    // Arc math
    // -------------------------------------------------------------------------
    // The half-moon spans from π (left tip) → 0 (right tip) going THROUGH
    // the bottom (i.e. the "U" shape open at the top).
    // fillFrac = 1 → full arc; fillFrac = 0 → empty.
    // Depletion trims symmetrically from both tips inward toward the bottom,
    // which reads as "top to bottom" on each side just like Genshin/BotW.

    private const float ArcStart = Mathf.PI;   // left tip
    private const float ArcEnd   = 0f;         // right tip
    private const float ArcSpan  = Mathf.PI;   // 180°

    private float currentFill = 1f;

    protected override void Start()
    {
        base.Start();
        if (player == null)
            player = FindFirstObjectByType<FirstPersonMovement>();

        EnsureFireRing();
    }

    void Update()
    {
        if (player == null) return;

        float targetFill = player.StaminaFraction;
        // Smooth visual a little so the bar doesn't snap
        currentFill = Mathf.MoveTowards(currentFill, targetFill, Time.deltaTime * 4f);
        SetVerticesDirty();

        // Fire ring is active whenever stamina drain is penalised —
        // covers both high-stress (tier 2) and the unfocus StaminaDrain modifier.
        bool fireActive = (EEGStressManager.Instance != null)
                          ? EEGStressManager.Instance.StaminaDrainActive
                          : player.IsInStressMode;
        if (fireRing != null)
            fireRing.SetActive(fireActive);
    }

    // -------------------------------------------------------------------------
    // Procedural mesh
    // -------------------------------------------------------------------------
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // ---- Ghost (full arc, dim) ----
        AddArcRing(vh, 1f, emptyColor);
        // ---- Fill (current stamina) ----
        if (currentFill > 0.001f)
            AddArcRing(vh, Mathf.Clamp01(currentFill), fullColor);
    }

    private void AddArcRing(VertexHelper vh, float fillFrac, Color col)
    {
        float filledSpan = ArcSpan * fillFrac;
        float margin     = (ArcSpan - filledSpan) * 0.5f;
        float aStart     = ArcStart - margin;   // trim from left tip
        float aEnd       = ArcEnd   + margin;   // trim from right tip

        int baseIndex = vh.currentVertCount;
        int segs = Mathf.Max(4, Mathf.RoundToInt(segments * fillFrac));
        if (segs < 2) segs = 2;

        for (int i = 0; i <= segs; i++)
        {
            float t     = i / (float)segs;
            // Going counterclockwise from aStart → through bottom → aEnd
            float angle = Mathf.Lerp(aStart, aEnd, t);
            float cos   = Mathf.Cos(angle);
            float sin   = Mathf.Sin(angle);

            // Outer vertex
            vh.AddVert(new Vector3(cos * outerRadius, sin * outerRadius, 0f),
                       col, Vector2.zero);
            // Inner vertex
            vh.AddVert(new Vector3(cos * innerRadius, sin * innerRadius, 0f),
                       col, Vector2.zero);
        }

        // Quads between consecutive pairs
        for (int i = 0; i < segs; i++)
        {
            int o0 = baseIndex + i * 2;
            int i0 = o0 + 1;
            int o1 = o0 + 2;
            int i1 = o0 + 3;

            // Two triangles per quad
            vh.AddTriangle(o0, o1, i0);
            vh.AddTriangle(o1, i1, i0);
        }
    }

    // -------------------------------------------------------------------------
    // Fire ring auto-setup
    // -------------------------------------------------------------------------
    private void EnsureFireRing()
    {
        if (fireRing != null) return;

        var go = new GameObject("FireRing");
        go.transform.SetParent(transform, false);
        fireRing = go.AddComponent<FireRingUI>();
        fireRing.outerRadius = outerRadius + 14f;
        fireRing.innerRadius = outerRadius + 2f;
        fireRing.SetActive(false);
    }
}