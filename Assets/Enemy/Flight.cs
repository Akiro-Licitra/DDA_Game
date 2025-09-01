using UnityEngine;
using UnityEngine.AI;

public class Flight : MonoBehaviour
{
    public float floatHeight = 2f;
    public float floatAmplitude = 0.5f; // vertical oscillation range
    public float floatFrequency = 1f;   // speed of oscillation

    private NavMeshAgent agent;
    private float initialY;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        initialY = transform.position.y;
        agent.updateUpAxis = false;     // Optional: disable Y-axis updates if needed
        agent.updatePosition = true;
    }

    void Update()
    {
        // Make enemy float by oscillating Y position
        float newY = initialY + floatAmplitude * Mathf.Sin(Time.time * floatFrequency);

        // Set new position with NavMeshAgent controlled X,Z and floating Y
        Vector3 pos = agent.nextPosition;
        pos.y = newY;
        transform.position = pos;
    }
}
