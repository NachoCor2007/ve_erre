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

    void Update()
    {
        Vector3 newVelocity = (controllerTransform.position - lastPosition) / Time.deltaTime;
        velocity = Vector3.Lerp(velocity, newVelocity, 0.5f);
        lastPosition = controllerTransform.position;

        // if (currentBall != null)
        // {
        //     Debug.Log("Velocidad Y: " + velocity.y);
        // }

        // Si está agarrada y me muevo rápido hacia abajo → soltar para dribble
        if (currentBall != null && velocity.y < -1.0f)
        {
            lastReleaseTime = Time.time;
            currentBall.Release(velocity, this);
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
}