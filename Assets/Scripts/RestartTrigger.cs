using UnityEngine;
using UnityEngine.Events;

public class RestartTrigger : MonoBehaviour
{
    public UnityEvent onBallEnter;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger activated by: " + other.gameObject.name);

        if (other.CompareTag("Ball"))
        {
            onBallEnter.Invoke();
        }
    }
}
