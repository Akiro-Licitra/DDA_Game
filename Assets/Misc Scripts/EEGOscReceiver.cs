using UnityEngine;
using OscJack;
using System.Collections.Generic; // needed for Queue

public class EEGOscReceiver : MonoBehaviour
{
    public int listenPort = 6969;
    OscServer server;

    // This stores the raw EEG ratio received from Python
    private float rawEegRatio = 0f;

    // Smoothed EEG ratio
    public float eegRatio = 0f;

    // Sliding window for smoothing
    public int smoothingWindowSize = 10; // number of frames to average over
    private Queue<float> ratioWindow = new Queue<float>();

    void Start()
    {
        server = new OscServer(listenPort);

        server.MessageDispatcher.AddCallback(
            "/eeg_ratio",
            (string address, OscDataHandle data) =>
            {
                rawEegRatio = data.GetElementAsFloat(0);
                // Add to smoothing window
                ratioWindow.Enqueue(rawEegRatio);
                if (ratioWindow.Count > smoothingWindowSize)
                    ratioWindow.Dequeue();

                // Compute average for smoothing
                float sum = 0f;
                foreach (var val in ratioWindow)
                    sum += val;
                eegRatio = sum / ratioWindow.Count;

                Debug.Log("Received EEG ratio: " + rawEegRatio + " | Smoothed: " + eegRatio);
            }
        );
    }

    void Update()
    {
        // Example: scale this object based on the smoothed EEG ratio
        transform.localScale = Vector3.one * (1.0f + eegRatio);
    }

    void OnDestroy()
    {
        server.Dispose();
    }
}
