using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    public Transform shootPoint; // referencia al controller
    public InputActionProperty triggerAction;
    
    private BallController _currentBall;
    private bool _isShooting = false;
    bool _wasPressedLastFrame = false;
    public HandController handController;

    [Header("Throw Settings")]
    [SerializeField]
    [Tooltip("Multiplier applied to the hand velocity when throwing.")]
    private float throwForceMultiplier = 1.3f;

    [SerializeField]
    [Tooltip("An optional velocity boost (usually upward) to make throwing feel more effortless in VR.")]
    private Vector3 throwVelocityBoost = new Vector3(0f, 1.2f, 0f);

    [SerializeField]
    [Tooltip("Minimum physical velocity magnitude required to use the physics-based throw. Below this, fallback aim throw is used.")]
    private float minVelocityThreshold = 0.5f;

    [SerializeField]
    [Tooltip("The speed of the throw when using the fallback (aim-based) throw.")]
    private float fallbackShootSpeed = 6.0f;

    void Update()
    {
        _currentBall = handController.GetCurrentBall();
        
        bool pressed = triggerAction.action.ReadValue<float>() > 0.1f;

        if (pressed && !_wasPressedLastFrame && _currentBall != null)
        {
            _isShooting = true;
            _currentBall.Grab(shootPoint);
        }

        if (!pressed && _wasPressedLastFrame && _isShooting && _currentBall != null)
        {
            Shoot();
            _isShooting = false;
            _currentBall = null;
        }

        _wasPressedLastFrame = pressed;
    }

    void Shoot()
    {
        Debug.Log("DISPARANDO: " + _currentBall);

        // Get the velocity of the hand controller
        Vector3 handVelocity = handController != null ? handController.velocity : Vector3.zero;

        handController.ClearBall();
        
        Vector3 shootVelocity;
        if (handVelocity.magnitude > minVelocityThreshold)
        {
            shootVelocity = handVelocity * throwForceMultiplier;
            Debug.Log($"[ShootingController] Physical throw. Hand Velocity: {handVelocity} (Mag: {handVelocity.magnitude:F2}).");
        }
        else
        {
            // Fallback: use controller orientation if motion is too slow
            Vector3 aimDirection = -shootPoint.right;
            shootVelocity = aimDirection * fallbackShootSpeed;
            Debug.Log($"[ShootingController] Low velocity fallback throw. Aim Direction: {aimDirection}.");
        }

        // Apply helper upward/forward boost
        shootVelocity += throwVelocityBoost;

        _currentBall.Release(shootVelocity, handController);
    }
}
