using UnityEngine;

/// <summary>
/// Central authority for EEG-driven gameplay modifiers.
/// Place on any persistent GameObject (e.g. GameManager).
///
/// Stress tiers:
///   2 = high stress   → player bullets glow green + do more damage
///   1 = moderate
///   0 = calm
///  -1 = unfocus zone  → triggers one random unfocus modifier after sustained low ratio
///
/// Unfocus modifiers (mutually exclusive, one active at a time):
///   StaminaDrain   — triple stamina drain + fire ring on stamina bar
///   RapidFire      — enemies glow red + fire twice as fast
///   FastProjectile — enemy bullets glow purple + move twice as fast
///
/// ---- DEBUG TOGGLES (Inspector, "Debug Overrides" section) ----
/// Each toggle forces the corresponding visual/gameplay state ON regardless
/// of EEG input, so you can test every effect independently in Play mode.
/// They stack with — but don't cancel — the live EEG state.
/// </summary>
public class EEGStressManager : MonoBehaviour
{
    public static EEGStressManager Instance { get; private set; }

    [Header("References")]
    public EEGOscReceiver    eegReceiver;
    public FirstPersonMovement player;

    [Header("Unfocus detection")]
    [Tooltip("eegRatio must stay below this to build unfocus pressure")]
    public float unfocusThreshold = 0.4f;
    [Tooltip("Seconds of continuous low ratio before a modifier fires")]
    public float unfocusBuildTime = 8f;

    [Header("Modifier durations")]
    public float modifierDurationMin = 10f;
    public float modifierDurationMax = 15f;

    [Header("Cooldown between modifiers")]
    [Tooltip("Grace period after a modifier expires before another can trigger")]
    public float postModifierCooldown = 12f;

    // =========================================================================
    // DEBUG OVERRIDES
    // Each one forces its corresponding state on in Play mode.
    // Exposed as public fields so they appear in the Inspector under the header.
    // =========================================================================
    [Header("─── Debug Overrides ───────────────────")]
    [Tooltip("Force high-stress state: player bullets glow green + deal bonus damage")]
    public bool DebugHighStress      = false;

    [Tooltip("Force StaminaDrain modifier: triple stamina drain + fire ring on stamina bar")]
    public bool DebugStaminaDrain    = false;

    [Tooltip("Force RapidFire modifier: enemies glow red + fire at double rate")]
    public bool DebugRapidFire       = false;

    [Tooltip("Force FastProjectile modifier: enemy bullets glow purple + move at double speed")]
    public bool DebugFastProjectile  = false;

    // (High stress already covers bullet damage — no separate damage debug toggle needed,
    //  but DebugHighStress drives both the glow and the damage multiplier.)

    // =========================================================================
    // Public state — read by other systems
    // =========================================================================
    public enum UnfocusModifier { None, StaminaDrain, RapidFire, FastProjectile }

    public int             StressTier           { get; private set; } = 0;
    public UnfocusModifier ActiveModifier       { get; private set; } = UnfocusModifier.None;

    // Convenience bools — combine live EEG state with debug overrides
    public bool RapidFireActive      => ActiveModifier == UnfocusModifier.RapidFire      || DebugRapidFire;
    public bool FastProjectileActive => ActiveModifier == UnfocusModifier.FastProjectile  || DebugFastProjectile;
    // StaminaDrain is an UNFOCUS penalty only — high stress (tier 2) does not trigger it.
    // DebugHighStress only affects bullet glow/damage, not stamina.
    public bool StaminaDrainActive   => ActiveModifier == UnfocusModifier.StaminaDrain
                                        || DebugStaminaDrain;

    // =========================================================================
    // Private state
    // =========================================================================
    private float unfocusTimer   = 0f;
    private float modifierTimer  = 0f;
    private float cooldownTimer  = 0f;
    private UnfocusModifier lastModifier = UnfocusModifier.None;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (eegReceiver == null) eegReceiver = FindFirstObjectByType<EEGOscReceiver>();
        if (player      == null) player      = FindFirstObjectByType<FirstPersonMovement>();
    }

    void Update()
    {
        UpdateStressTier();
        UpdateUnfocusDetection();
        TickActiveModifier();
    }

    // -------------------------------------------------------------------------
    // Stress tier
    // -------------------------------------------------------------------------
    private void UpdateStressTier()
    {
        if (eegReceiver == null) return;
        float ratio = eegReceiver.eegRatio;

        if      (ratio >= 1.2f)            StressTier =  2;
        else if (ratio >= 0.8f)            StressTier =  1;
        else if (ratio >= unfocusThreshold) StressTier =  0;
        else                               StressTier = -1;

        // Debug override: force tier 2 regardless of EEG
        if (DebugHighStress && StressTier < 2)
            StressTier = 2;
    }

    // -------------------------------------------------------------------------
    // Unfocus detection
    // -------------------------------------------------------------------------
    private void UpdateUnfocusDetection()
    {
        if (StressTier >= 0)
        {
            unfocusTimer = Mathf.Max(0f, unfocusTimer - Time.deltaTime * 1.5f);
            return;
        }

        if (cooldownTimer > 0f)  { cooldownTimer -= Time.deltaTime; return; }
        if (ActiveModifier != UnfocusModifier.None) return;

        unfocusTimer += Time.deltaTime;
        if (unfocusTimer >= unfocusBuildTime)
        {
            unfocusTimer = 0f;
            TriggerUnfocusModifier();
        }
    }

    private void TriggerUnfocusModifier()
    {
        var pool = new System.Collections.Generic.List<UnfocusModifier>
        {
            UnfocusModifier.StaminaDrain,
            UnfocusModifier.RapidFire,
            UnfocusModifier.FastProjectile
        };
        pool.Remove(lastModifier);

        UnfocusModifier chosen = pool[Random.Range(0, pool.Count)];
        float duration = Random.Range(modifierDurationMin, modifierDurationMax);
        ActivateModifier(chosen, duration);
    }

    private void ActivateModifier(UnfocusModifier mod, float duration)
    {
        ActiveModifier = mod;
        modifierTimer  = duration;
        lastModifier   = mod;
        Debug.Log($"[EEGStressManager] Modifier activated: {mod} for {duration:F1}s");
    }

    // -------------------------------------------------------------------------
    // Tick active modifier
    // -------------------------------------------------------------------------
    private void TickActiveModifier()
    {
        if (ActiveModifier == UnfocusModifier.None) return;

        modifierTimer -= Time.deltaTime;
        if (modifierTimer <= 0f)
        {
            Debug.Log($"[EEGStressManager] Modifier expired: {ActiveModifier}");
            ActiveModifier = UnfocusModifier.None;
            modifierTimer  = 0f;
            cooldownTimer  = postModifierCooldown;
        }
    }

    // -------------------------------------------------------------------------
    // UI helpers
    // -------------------------------------------------------------------------
    public float ModifierTimeRemaining => Mathf.Max(0f, modifierTimer);
    public float UnfocusBuildFraction  => unfocusBuildTime > 0f
                                          ? Mathf.Clamp01(unfocusTimer / unfocusBuildTime)
                                          : 0f;
}