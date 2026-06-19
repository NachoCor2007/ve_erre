using UnityEngine;
using Unity.XR.CoreUtils;

public class HandController : MonoBehaviour
{
    public Transform controllerTransform;
    
    [Header("Grip Settings")]
    public Vector3 grabOffset = new Vector3(0f, -0.08f, 0f);

    private Vector3 lastPosition;
    public Vector3 velocity;

    private Vector3 lastLocalPosition;
    public Vector3 localVelocity;

    private BallController currentBall;
    private float lastReleaseTime;
    private float grabCooldown = 0.2f;

    [Header("Dribble Assist")]
    public HandController otherHand;

    private XROrigin xrOrigin;

    private void Start()
    {
        xrOrigin = FindFirstObjectByType<XROrigin>();
        lastPosition = controllerTransform != null ? controllerTransform.position : transform.position;

        if (controllerTransform != null)
        {
            lastLocalPosition = xrOrigin != null ? xrOrigin.transform.InverseTransformPoint(controllerTransform.position) : controllerTransform.localPosition;
        }

        if (otherHand == null)
        {
            HandController[] hands = FindObjectsByType<HandController>(FindObjectsSortMode.None);
            foreach (var hand in hands)
            {
                if (hand != this)
                {
                    otherHand = hand;
                    break;
                }
            }
        }
    }

    void Update()
    {
        Vector3 newVelocity = (controllerTransform.position - lastPosition) / Time.deltaTime;
        velocity = Vector3.Lerp(velocity, newVelocity, 0.5f);
        lastPosition = controllerTransform.position;

        // Calculate local velocity (relative to XR Origin) to avoid locomotion interference
        Vector3 localPos = controllerTransform.position;
        if (xrOrigin != null)
        {
            localPos = xrOrigin.transform.InverseTransformPoint(controllerTransform.position);
        }
        else
        {
            localPos = controllerTransform.localPosition;
        }

        Vector3 newLocalVelocity = (localPos - lastLocalPosition) / Time.deltaTime;
        localVelocity = Vector3.Lerp(localVelocity, newLocalVelocity, 0.5f);
        lastLocalPosition = localPos;

        // Si está agarrada en esta mano y me muevo rápido hacia abajo → soltar para dribble (check uses localVelocity)
        if (currentBall != null && currentBall.holdPoint == controllerTransform && localVelocity.y < -1.0f)
        {
            lastReleaseTime = Time.time;

            // Check if doing a crossover (throwing towards the other hand)
            bool isCrossover = false;
            if (otherHand != null)
            {
                Vector3 toOtherHandLocal = (otherHand.controllerTransform.position - controllerTransform.position).normalized;
                if (xrOrigin != null)
                {
                    Vector3 otherHandLocalPos = xrOrigin.transform.InverseTransformPoint(otherHand.controllerTransform.position);
                    toOtherHandLocal = (otherHandLocalPos - localPos).normalized;
                }

                float alignment = Vector3.Dot(localVelocity.normalized, toOtherHandLocal);
                // If local velocity points towards the other hand
                isCrossover = alignment > 0.2f;
            }

            HandController target = isCrossover ? otherHand : this;

            currentBall.StartAssistedDribble(target, velocity); // Release with world velocity!
            currentBall = null;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            BallController ball = other.GetComponent<BallController>();

            // Si no está agarrada → agarrar automáticamente
            if (!ball.isHeld && currentBall == null)
            {
                if (ball.lastHandThatThrew == this) return;

                if (Time.time - lastReleaseTime > grabCooldown && localVelocity.y > -0.5f)
                {
                    ball.holdLocalOffset = grabOffset;
                    ball.Grab(controllerTransform);
                    currentBall = ball;
                }
            }
        }
    }

    public void GrabBall(BallController ball, bool force = false)
    {
        if (force || (currentBall == null && Time.time - lastReleaseTime > grabCooldown))
        {
            ball.holdLocalOffset = grabOffset;
            ball.Grab(controllerTransform);
            currentBall = ball;
        }
    }

    public BallController GetCurrentBall()
    {
        return currentBall;
    }

    public void ClearBall()
    {
        currentBall = null;
        lastReleaseTime = Time.time;
    }

    public void ResetState()
    {
        velocity = Vector3.zero;
        localVelocity = Vector3.zero;
        if (controllerTransform != null)
        {
            lastPosition = controllerTransform.position;
            lastLocalPosition = xrOrigin != null ? xrOrigin.transform.InverseTransformPoint(controllerTransform.position) : controllerTransform.localPosition;
        }
        currentBall = null;
        lastReleaseTime = Time.time;
    }
}