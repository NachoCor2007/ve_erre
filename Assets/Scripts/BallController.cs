using System;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Grab")]
    public bool isHeld = false;
    public Transform holdPoint;
    public Vector3 holdLocalOffset = new Vector3(0f, -0.08f, 0f);

    [Header("Dribble")]
    public float minVelocity = 0.5f;
    public float bounceMultiplier = 1.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isHeld && holdPoint != null)
        {
            Vector3 targetPosition = holdPoint.TransformPoint(holdLocalOffset);
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                10f * Time.deltaTime
            );        
        }
    }

    public void Grab(Transform hand)
    {
        isHeld = true;
        holdPoint = hand;
        rb.isKinematic = true;
    }

    public void Release(Vector3 velocity)
    {
        isHeld = false;
        holdPoint = null;
        rb.isKinematic = false;
        rb.linearVelocity = new Vector3(
            velocity.x,
            Math.Min(velocity.y, -2f),
            velocity.z
        );
    }

    public void ApplyBounce(Vector3 handVelocity)
    {
        if (handVelocity.y < -minVelocity)
        {
            rb.linearVelocity = new Vector3(
                handVelocity.x,
                Mathf.Abs(handVelocity.y) * bounceMultiplier,
                handVelocity.z
            );
        }
    }
}
