using UnityEngine;

public class DunkHelper : MonoBehaviour
{
    [Header("Helper Forces")]
    [Tooltip("Horizontal force pulling the ball towards the center of the hoop.")]
    [SerializeField] private float _helperForceStrength = 2.0f;
    [Tooltip("Downward force pulling the ball into the hoop, only applied when the ball is falling or near-horizontal.")]
    [SerializeField] private float _downwardForceStrength = 0.5f;
    [Tooltip("Tag used to identify the basketball.")]
    [SerializeField] private string _ballTag = "Ball";

    [Header("Debug Settings")]
    [SerializeField] private bool _enableDebugLogs = false;
    [SerializeField] private bool _drawDebugLines = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(_ballTag))
        {
            BallController ball = other.GetComponent<BallController>();
            // If the ball is being held by the player or NPC, do not apply helper forces
            if (ball != null && !ball.isHeld)
            {
                Rigidbody ballRb = ball.rb != null ? ball.rb : other.attachedRigidbody;
                if (ballRb != null)
                {
                    Vector3 targetPos = transform.position;
                    Vector3 ballPos = ball.transform.position;

                    // Calculate horizontal direction to the center of the hoop (ignoring Y height difference)
                    Vector3 horizontalDir = new Vector3(targetPos.x - ballPos.x, 0f, targetPos.z - ballPos.z);
                    float distance = horizontalDir.magnitude;

                    // Only apply helper forces if the ball is falling (moving downwards) 
                    // and is physically at or above the hoop's rim level (with a small buffer)
                    if (ballRb.linearVelocity.y < 0f && ballPos.y >= targetPos.y - 0.15f)
                    {
                        // Calculate force vector
                        Vector3 forceVector = horizontalDir.normalized * _helperForceStrength;
                        forceVector.y = -_downwardForceStrength;

                        // Apply the force to the Rigidbody
                        ballRb.AddForce(forceVector, ForceMode.Force);

                        if (_drawDebugLines)
                        {
                            Debug.DrawLine(ballPos, ballPos + forceVector, Color.green, 0.1f);
                            Debug.DrawLine(ballPos, targetPos, Color.yellow, 0.1f);
                        }

                        if (_enableDebugLogs)
                        {
                            Debug.Log($"[DunkHelper] Applying guiding force {forceVector} to ball. Distance to center: {distance:F2}");
                        }
                    }
                }
            }
        }
    }
}
