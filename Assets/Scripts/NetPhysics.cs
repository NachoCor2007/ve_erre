using UnityEngine;

/// <summary>
/// Activates physics (Cloth simulation) on the basketball net when a ball passes through.
/// This script should be attached to the Net mesh GameObject itself.
/// It creates its own trigger zone automatically at runtime.
/// </summary>
public class NetPhysics : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How long (seconds) the cloth simulation stays active after a ball enters.")]
    public float activeTime = 2.5f;

    [Tooltip("Multiplier applied to the ball's velocity to push the net.")]
    public float forceMultiplier = 0.3f;

    [Tooltip("Size of the trigger detection zone (radius).")]
    public float triggerRadius = 0.3f;

    [Tooltip("Offset of the trigger zone relative to this object (local space).")]
    public Vector3 triggerOffset = new Vector3(0f, 0.3f, 0f);

    private Cloth netCloth;
    private float deactivateTimer = -1f;
    private GameObject triggerObject;

    void Start()
    {
        // Get or add the Cloth component
        netCloth = GetComponent<Cloth>();

        if (netCloth == null)
        {
            Debug.LogWarning("[NetPhysics] No Cloth component found on " + gameObject.name + ". Adding one.");
            netCloth = gameObject.AddComponent<Cloth>();
        }

        // Configure cloth physics
        netCloth.damping = 0.3f;
        netCloth.stretchingStiffness = 0.8f;
        netCloth.bendingStiffness = 0.5f;
        netCloth.externalAcceleration = Vector3.zero;
        netCloth.randomAcceleration = Vector3.zero;

        // Cloth starts disabled (static net, no performance cost)
        netCloth.enabled = false;

        // Create the trigger zone as a child object
        CreateTriggerZone();
    }

    void CreateTriggerZone()
    {
        triggerObject = new GameObject("NetTrigger");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localPosition = triggerOffset;
        triggerObject.transform.localRotation = Quaternion.identity;
        triggerObject.transform.localScale = Vector3.one;

        // Add a sphere collider as trigger
        SphereCollider trigger = triggerObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = triggerRadius;

        // Add a Rigidbody (kinematic) so OnTriggerEnter works
        Rigidbody rb = triggerObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add the trigger handler
        NetTriggerHandler handler = triggerObject.AddComponent<NetTriggerHandler>();
        handler.netPhysics = this;
    }

    void Update()
    {
        if (deactivateTimer > 0f)
        {
            deactivateTimer -= Time.deltaTime;

            // Gradually reduce external force for a natural settling
            if (netCloth != null && netCloth.enabled)
            {
                float t = Mathf.Clamp01(deactivateTimer / activeTime);
                netCloth.externalAcceleration *= t;
            }

            if (deactivateTimer <= 0f)
            {
                DeactivateCloth();
            }
        }
    }

    public void OnBallEntered(Collider ballCollider)
    {
        if (netCloth == null) return;

        // Enable cloth simulation
        netCloth.enabled = true;

        // Apply force based on ball velocity
        Rigidbody ballRb = ballCollider.GetComponent<Rigidbody>();
        if (ballRb == null)
            ballRb = ballCollider.GetComponentInParent<Rigidbody>();

        if (ballRb != null)
        {
            Vector3 ballVelocity = ballRb.linearVelocity;
            netCloth.externalAcceleration = ballVelocity * forceMultiplier;
            Debug.Log("[NetPhysics] Ball entered net! Velocity: " + ballVelocity);
        }

        // Reset timer
        deactivateTimer = activeTime;
    }

    public void OnBallExited(Collider ballCollider)
    {
        if (netCloth != null)
        {
            netCloth.externalAcceleration = Vector3.zero;
        }
    }

    private void DeactivateCloth()
    {
        if (netCloth != null)
        {
            netCloth.externalAcceleration = Vector3.zero;
            netCloth.enabled = false;
        }
    }
}
