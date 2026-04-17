using UnityEngine;
using OscJack;
using System.Collections.Generic;

public class EEGOscReceiver : MonoBehaviour
{
    public int listenPort = 6969;
    OscServer server;

    // --- Simulation Toggle ---
    public bool simulateEEG = false;     // <- Turn ON to simulate EEG
    public float simulationSpeed = 0.05f; // how fast values drift

    // Raw + Smoothed EEG data
    private float rawEegRatio = 0f;
    public  float eegRatio    = 0f;

    // Smoothing
    public int smoothingWindowSize = 10;
    private Queue<float> ratioWindow = new Queue<float>();

    // Gameplay variables
    // stressMode is READ by FirstPersonMovement to trigger the 15-second stress window.
    // It changes whenever ballDamage tier changes (e.g. 0 -> 1 or 1 -> 2).
    public int stressMode  = 0;
    public int ballDamage  = 0;

    // Track last stressMode so we can emit a new value on change
    private int lastStressMode = 0;

    void Start()
    {
        if (!simulateEEG)
        {
            server = new OscServer(listenPort);

            server.MessageDispatcher.AddCallback(
                "/eeg_ratio",
                (string address, OscDataHandle data) =>
                {
                    ReceiveEEG(data.GetElementAsFloat(0));
                }
            );
        }
    }

    void Update()
    {
        if (simulateEEG)
        {
            float simulated = SimulateEegRatio();
            ReceiveEEG(simulated);
        }

        // Derive stress tier from eegRatio thresholds.
        // Adjust thresholds to taste.
        int newStressMode = ComputeStressMode(eegRatio);

        // Only write a new value when the tier actually changes,
        // so FirstPersonMovement can edge-detect the transition.
        if (newStressMode != lastStressMode)
        {
            stressMode     = newStressMode;
            lastStressMode = newStressMode;
        }

        ballDamage = stressMode + 1;

        // Example scaling (retained from original)
        transform.localScale = Vector3.one * (1.0f + eegRatio);
    }

    // -------------------------------------------------------------------------
    // Stress tier logic — edit thresholds here
    // -------------------------------------------------------------------------
    private int ComputeStressMode(float ratio)
    {
        if (ratio >= 1.2f) return 2;   // high stress
        if (ratio >= 0.8f) return 1;   // moderate stress
        return 0;                       // calm
    }

    // -------------------------------------------------------------------------
    // EEG receive + smoothing
    // -------------------------------------------------------------------------
    private void ReceiveEEG(float value)
    {
        rawEegRatio = value;

        ratioWindow.Enqueue(rawEegRatio);
        if (ratioWindow.Count > smoothingWindowSize)
            ratioWindow.Dequeue();

        float sum = 0f;
        foreach (var v in ratioWindow)
            sum += v;

        eegRatio = sum / ratioWindow.Count;
    }

    // -------------------------------------------------------------------------
    // EEG Simulation
    // -------------------------------------------------------------------------
    private float simulatedBase = 0.5f;
    private float velocity      = 0f;

    private float SimulateEegRatio()
    {
        velocity += Random.Range(-0.05f, 0.05f) * simulationSpeed;
        velocity  = Mathf.Clamp(velocity, -0.02f, 0.02f);

        simulatedBase += velocity;
        simulatedBase  = Mathf.Clamp(simulatedBase, 0.2f, 1.5f);

        return simulatedBase;
    }

    void OnDestroy()
    {
        server?.Dispose();
    }
}