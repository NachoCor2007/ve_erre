using UnityEngine;

/// <summary>
/// Detects when a ball enters the trigger zone and notifies NetPhysics.
/// This component is automatically added by NetPhysics at runtime.
/// </summary>
public class NetTriggerHandler : MonoBehaviour
{
    [HideInInspector]
    public NetPhysics netPhysics;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && netPhysics != null)
        {
            netPhysics.OnBallEntered(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && netPhysics != null)
        {
            netPhysics.OnBallExited(other);
        }
    }
}
