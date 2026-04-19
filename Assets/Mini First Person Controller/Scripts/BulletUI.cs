using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Row of bullet slots in the bottom-left corner.
///   White  = ball available.
///   Gray   = ball in use.
///   Green outline on available slots = high-stress / green glow mode is active.
///
/// Each slot is two stacked Images:
///   - Border (slightly larger, behind) — transparent normally, green when glowing
///   - Fill   (on top)                 — white or gray
/// </summary>
public class BulletUI : MonoBehaviour
{
    [Header("References")]
    public PlayerShooter shooter;

    [Header("Layout")]
    public float slotWidth    = 24f;
    public float slotHeight   = 10f;
    public float slotSpacing  = 5f;
    public float edgePadding  = 20f;

    [Header("Colors")]
    public Color availableColor  = new Color(1f,    1f,    1f,    1f);
    public Color usedColor       = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color outlineColor    = new Color(0.1f,  1f,    0.2f,  1f);   // green
    [Tooltip("How many pixels the border image extends beyond the fill on each side")]
    public float outlineWidth    = 2.5f;

    // -------------------------------------------------------------------------
    private Image[] fillSlots;
    private Image[] borderSlots;
    private int     lastMaxBalls  = -1;
    private int     lastBallsUsed = -1;
    private bool    lastGlowState = false;

    void Start()
    {
        if (shooter == null)
            shooter = FindFirstObjectByType<PlayerShooter>();

        if (shooter == null)
        {
            Debug.LogError("BulletUI: No PlayerShooter found in the scene!");
            return;
        }

        BuildSlots(shooter.maxBalls);
    }

    void Update()
    {
        if (shooter == null) return;

        if (shooter.maxBalls != lastMaxBalls)
            BuildSlots(shooter.maxBalls);

        bool glowActive = EEGStressManager.Instance != null &&
                          (EEGStressManager.Instance.StressTier >= 2 ||
                           EEGStressManager.Instance.DebugHighStress);

        bool stateChanged = shooter.BallsUsed != lastBallsUsed
                            || glowActive      != lastGlowState;

        if (stateChanged)
            RefreshSlots(shooter.BallsUsed, glowActive);
    }

    // -------------------------------------------------------------------------
    private void BuildSlots(int count)
    {
        if (fillSlots != null)
            foreach (var s in fillSlots)
                if (s != null) Destroy(s.transform.parent.gameObject);

        fillSlots   = new Image[count];
        borderSlots = new Image[count];

        for (int i = 0; i < count; i++)
        {
            // --- Container (purely for grouping, no visuals) ---
            var container = new GameObject($"Slot_{i}", typeof(RectTransform));
            container.transform.SetParent(transform, false);

            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.zero;
            crt.pivot     = Vector2.zero;
            crt.sizeDelta = new Vector2(slotWidth, slotHeight);
            float xPos    = edgePadding + i * (slotWidth + slotSpacing);
            crt.anchoredPosition = new Vector2(xPos, edgePadding);

            // --- Border image (behind, slightly larger) ---
            var borderGO  = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGO.transform.SetParent(container.transform, false);
            var brt = borderGO.GetComponent<RectTransform>();
            brt.anchorMin        = Vector2.zero;
            brt.anchorMax        = Vector2.one;
            brt.pivot            = new Vector2(0.5f, 0.5f);
            // Expand outward by outlineWidth on all sides
            brt.offsetMin        = new Vector2(-outlineWidth, -outlineWidth);
            brt.offsetMax        = new Vector2( outlineWidth,  outlineWidth);
            var borderImg        = borderGO.GetComponent<Image>();
            borderImg.color      = Color.clear;
            borderSlots[i]       = borderImg;

            // --- Fill image (on top, exact slot size) ---
            var fillGO  = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(container.transform, false);
            var frt = fillGO.GetComponent<RectTransform>();
            frt.anchorMin   = Vector2.zero;
            frt.anchorMax   = Vector2.one;
            frt.offsetMin   = Vector2.zero;
            frt.offsetMax   = Vector2.zero;
            var fillImg     = fillGO.GetComponent<Image>();
            fillImg.color   = availableColor;
            fillSlots[i]    = fillImg;
        }

        // Anchor this root RectTransform to the bottom-left
        var selfRt = GetComponent<RectTransform>();
        if (selfRt != null)
        {
            selfRt.anchorMin        = Vector2.zero;
            selfRt.anchorMax        = Vector2.zero;
            selfRt.pivot            = Vector2.zero;
            selfRt.anchoredPosition = Vector2.zero;
            float totalW = count * slotWidth + (count - 1) * slotSpacing + edgePadding * 2f;
            selfRt.sizeDelta = new Vector2(totalW, slotHeight + edgePadding * 2f);
        }

        lastMaxBalls  = count;
        lastBallsUsed = -1;
        lastGlowState = false;
        RefreshSlots(shooter.BallsUsed, false);
    }

    // -------------------------------------------------------------------------
    private void RefreshSlots(int used, bool glowActive)
    {
        for (int i = 0; i < fillSlots.Length; i++)
        {
            if (fillSlots[i]   == null) continue;
            if (borderSlots[i] == null) continue;

            bool slotUsed      = i < used;
            fillSlots[i].color = slotUsed ? usedColor : availableColor;

            // Outline: only on available slots, only while glow is active
            borderSlots[i].color = (!slotUsed && glowActive) ? outlineColor : Color.clear;
        }

        lastBallsUsed = used;
        lastGlowState = glowActive;
    }
}