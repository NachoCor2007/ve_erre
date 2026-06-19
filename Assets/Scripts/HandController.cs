using UnityEngine;

public class HandController : MonoBehaviour
{
    public Transform controllerTransform;
    
    [Header("Grip Settings")]
    public Vector3 grabOffset = new Vector3(0f, -0.08f, 0f);

    private Vector3 lastPosition;
    public Vector3 velocity;

    private BallController currentBall;
    private float lastReleaseTime;
    private float grabCooldown = 0.2f;

    [Header("Dribble Assist")]
    public HandController otherHand;

    private void Start()
    {
        lastPosition = controllerTransform != null ? controllerTransform.position : transform.position;

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

        // if (currentBall != null)
        // {
        //     Debug.Log("Velocidad Y: " + velocity.y);
        // }

        // Si está agarrada en esta mano y me muevo rápido hacia abajo → soltar para dribble
        if (currentBall != null && currentBall.holdPoint == controllerTransform && velocity.y < -1.0f)
        {
            lastReleaseTime = Time.time;

            // Check if doing a crossover (throwing towards the other hand)
            bool isCrossover = false;
            if (otherHand != null)
            {
                Vector3 toOtherHand = (otherHand.controllerTransform.position - controllerTransform.position).normalized;
                float alignment = Vector3.Dot(velocity.normalized, toOtherHand);
                // If velocity points towards the other hand
                isCrossover = alignment > 0.2f;
            }

            HandController target = isCrossover ? otherHand : this;

            currentBall.StartAssistedDribble(target, velocity);
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

                if (Time.time - lastReleaseTime > grabCooldown && velocity.y > -0.5f)
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
        if (controllerTransform != null)
        {
            lastPosition = controllerTransform.position;
        }
        currentBall = null;
        lastReleaseTime = Time.time;
    }
}