using UnityEngine;
using UnityEngine.UI;

public class DDAOpacityController : MonoBehaviour
{
    public EEGOscReceiver eegReceiver; // Reference to your EEG manager
    public float maxAlpha = 0.9f;      // cap at 90%

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if (eegReceiver == null)
        {
            Debug.LogError("EEG Receiver not assigned!");
        }
    }

    void Update()
    {
        if (eegReceiver != null)
        {
            // assuming eegRatio is already normalized 0–1
            float targetAlpha = Mathf.Clamp(eegReceiver.eegRatio, 0f, 1f) * maxAlpha;

            Color c = img.color;
            c.a = targetAlpha;
            c.r = Mathf.Lerp(0.5f, 1f, eegReceiver.eegRatio);
            c.g = Mathf.Lerp(0.5f, 0.1f, eegReceiver.eegRatio);
            img.color = c;
        }
    }
}
