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

    public HandController lastHandThatThrew;

    [Header("Assisted Dribble Settings")]
    [SerializeField]
    private float dribbleReturnTime = 0.45f; // Time in seconds for the ball to rise back to the hand
    [SerializeField]
    private float catchSnapDistance = 0.35f; // Distance threshold to snap the ball back to the hand

    private bool _isAssistedDribbling = false;
    private bool _hasBounced = false;
    private HandController _targetHand;
    private float _dribbleStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isHeld && holdPoint != null)
        {
            Vector3 targetPosition = holdPoint.TransformPoint(holdLocalOffset);
            transform.position = targetPosition;
        }
        else if (_isAssistedDribbling && _hasBounced && _targetHand != null)
        {
            // Safety timeout to prevent soft-locks if the ball misses or gets stuck
            if (Time.time - _dribbleStartTime > 2.0f)
            {
                _isAssistedDribbling = false;
                Debug.Log("[BallController] Assisted dribble timed out. Returning to free physics.");
                return;
            }

            // Calculate distance to target hand
            float distance = Vector3.Distance(transform.position, _targetHand.controllerTransform.position);

            // If close enough and ball is moving upwards or close to it, snap catch it!
            if (distance < catchSnapDistance && rb.linearVelocity.y > -1f)
            {
                _isAssistedDribbling = false;
                _targetHand.GrabBall(this, true);
                Debug.Log($"[BallController] Ball snapped back to hand: {_targetHand.name}");
            }
        }
        else if (_isAssistedDribbling && !_hasBounced && Time.time - _dribbleStartTime > 2.0f)
        {
            // Safety timeout if it never bounced
            _isAssistedDribbling = false;
            Debug.Log("[BallController] Assisted dribble timed out before bounce.");
        }
    }

    public void Grab(Transform hand)
    {
        isHeld = true;
        holdPoint = hand;
        rb.isKinematic = true;
        _isAssistedDribbling = false; // Reset dribble state on grab
        
        HandController handController = hand.GetComponentInParent<HandController>();
        if (handController != null && handController != lastHandThatThrew)
        {
            lastHandThatThrew = null;
        }
    }

    public void Release(Vector3 velocity, HandController hand = null)
    {
        isHeld = false;
        holdPoint = null;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        lastHandThatThrew = hand;
        _isAssistedDribbling = false; // Reset dribble state on release (like shooting)
    }

    public void StartAssistedDribble(HandController targetHand, Vector3 initialVelocity)
    {
        _isAssistedDribbling = true;
        _hasBounced = false;
        _targetHand = targetHand;
        _dribbleStartTime = Time.time;

        isHeld = false;
        holdPoint = null;
        rb.isKinematic = false;
        rb.linearVelocity = initialVelocity;
        lastHandThatThrew = targetHand.otherHand; // Set last hand to the throwing hand to prevent early recaptures
    }

    void OnCollisionEnter(Collision collision)
    {
        // Enforce bounce reset
        lastHandThatThrew = null;

        if (_isAssistedDribbling && !_hasBounced && _targetHand != null)
        {
            _hasBounced = true;

            // Calculate the velocity needed to bounce from current floor position to target hand
            Vector3 startPos = transform.position;
            Vector3 targetPos = _targetHand.controllerTransform.position;
            Vector3 gravity = Physics.gravity;

            // Projectile motion formula: target = start + v*t + 0.5*g*t^2
            // Solve for v: v = (target - start - 0.5*g*t^2) / t
            Vector3 requiredVelocity = (targetPos - startPos - 0.5f * gravity * dribbleReturnTime * dribbleReturnTime) / dribbleReturnTime;

            // Apply the calculated velocity
            rb.linearVelocity = requiredVelocity;
            Debug.Log($"[BallController] Assisted bounce calculated. Target hand: {_targetHand.name}, Bounce Velocity: {requiredVelocity}");
        }
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
