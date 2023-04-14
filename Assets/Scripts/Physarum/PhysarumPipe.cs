using UnityEngine;

public class PhysarumPipe : MonoBehaviour
{
    public InitAgentsType initType;
    public float resolution;
    public float population;

    public void Dispose() => Destroy(gameObject);

    // IT is destroyed after received
}
