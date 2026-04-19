using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;

    // Base rates
    private const float BaseSprintDrain   = 33f;  // per second while sprinting
    private const float BaseRegenRate     = 20f;  // per second while not sprinting
    private const float RegenLockDuration = 1.5f; // seconds before regen resumes after hitting 0

    // Stress-mode multipliers
    private const float StressDrainMultiplier = 3f;   // triple drain
    private const float StressRegenMultiplier = 0.5f; // 50% slower regen

    private float regenLockTimer  = 0f;   // counts down after stamina hits 0
    private bool  regenLocked     = false;

    [Header("EEG Stress Mode")]
    // Assign in Inspector, or find at runtime
    public EEGOscReceiver eegReceiver;

    private const float StressModeDuration = 15f;
    private float stressModeTimer  = 0f;
    private bool  inStressMode     = false;
    private int   lastKnownStressMode = 0;

    Rigidbody rigidbody;

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        if (eegReceiver == null)
            eegReceiver = FindFirstObjectByType<EEGOscReceiver>();
    }

    void Update()
    {
        UpdateStressMode();
        UpdateStamina();
    }

    void FixedUpdate()
    {
        // Sprinting requires stamina > 0
        bool staminaAvailable = currentStamina > 0f;
        IsRunning = canRun && staminaAvailable && Input.GetKey(runningKey);

        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();

        Vector2 targetVelocity = new Vector2(
            Input.GetAxis("Horizontal") * targetMovingSpeed,
            Input.GetAxis("Vertical")   * targetMovingSpeed
        );

        rigidbody.linearVelocity = transform.rotation * new Vector3(
            targetVelocity.x,
            rigidbody.linearVelocity.y,
            targetVelocity.y
        );
    }

    // -------------------------------------------------------------------------
    // Stress Mode
    // -------------------------------------------------------------------------

    private void UpdateStressMode()
    {
        // Delegate entirely to EEGStressManager, which handles both high-stress
        // (tier >= 2) and the unfocus StaminaDrain modifier.
        if (EEGStressManager.Instance != null)
        {
            inStressMode = EEGStressManager.Instance.StaminaDrainActive;
            // stressModeTimer is no longer needed here — manager owns the clock.
            // Keep it in sync for any external UI reading IsInStressMode.
            stressModeTimer = EEGStressManager.Instance.ModifierTimeRemaining;
            return;
        }

        // Fallback: original edge-detect logic if manager is not present
        if (eegReceiver == null) return;
        int currentStressModeValue = eegReceiver.stressMode;
        if (currentStressModeValue != 0 && currentStressModeValue != lastKnownStressMode)
        {
            inStressMode    = true;
            stressModeTimer = StressModeDuration;
        }
        lastKnownStressMode = currentStressModeValue;
        if (inStressMode)
        {
            stressModeTimer -= Time.deltaTime;
            if (stressModeTimer <= 0f)
            {
                inStressMode    = false;
                stressModeTimer = 0f;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Stamina
    // -------------------------------------------------------------------------

    private void UpdateStamina()
    {
        float drainRate = BaseSprintDrain * (inStressMode ? StressDrainMultiplier : 1f);
        float regenRate = BaseRegenRate   * (inStressMode ? StressRegenMultiplier : 1f);

        if (IsRunning)
        {
            // Drain stamina while actively sprinting
            currentStamina -= drainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;

                // Lock regeneration for 1.5 s
                regenLocked    = true;
                regenLockTimer = RegenLockDuration;
            }
        }
        else
        {
            // Handle the regen lock countdown
            if (regenLocked)
            {
                regenLockTimer -= Time.deltaTime;
                if (regenLockTimer <= 0f)
                {
                    regenLocked    = false;
                    regenLockTimer = 0f;
                }
            }

            // Regenerate only when not locked and below max
            if (!regenLocked && currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina  = Mathf.Min(currentStamina, maxStamina);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Optional public helpers for UI
    // -------------------------------------------------------------------------

    /// <summary> 0–1 fill fraction for a stamina bar. </summary>
    public float StaminaFraction => currentStamina / maxStamina;

    /// <summary> Whether the 15-second EEG stress window is active. </summary>
    public bool IsInStressMode => inStressMode;

    /// <summary> Remaining seconds in the current stress window (0 if inactive). </summary>
    public float StressModeTimeRemaining => stressModeTimer;
}