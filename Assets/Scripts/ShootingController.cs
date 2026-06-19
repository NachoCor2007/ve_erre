using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    [Tooltip("Minimum physical velocity magnitude required to execute a throw. Below this, the shot is cancelled and returned to the hand.")]
    private float minVelocityThreshold = 1.0f;

    [Header("Velocity Tracking")]
    [SerializeField]
    [Tooltip("Number of frames to buffer for finding the peak velocity of the throw.")]
    private int velocityBufferFrames = 20;

    private List<Vector3> _velocityHistory = new List<Vector3>();

    void Update()
    {
        _currentBall = handController.GetCurrentBall();
        
        bool pressed = triggerAction.action.ReadValue<float>() > 0.1f;

        if (pressed && !_wasPressedLastFrame && _currentBall != null)
        {
            _isShooting = true;
            _velocityHistory.Clear();
            _currentBall.Grab(shootPoint);
        }

        // Buffer the velocity while the trigger is held and preparing to shoot
        if (_isShooting && handController != null)
        {
            _velocityHistory.Add(handController.velocity);
            if (_velocityHistory.Count > velocityBufferFrames)
            {
                _velocityHistory.RemoveAt(0);
            }
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
        // Get the peak velocity from our buffered history during the throw swing
        Vector3 throwVelocity = GetPeakVelocity();

        if (throwVelocity.magnitude > minVelocityThreshold)
        {
            Debug.Log("DISPARANDO: " + _currentBall);
            handController.ClearBall();
            
            Vector3 shootVelocity = throwVelocity * throwForceMultiplier + throwVelocityBoost;
            _currentBall.Release(shootVelocity, handController);
            Debug.Log($"[ShootingController] Physical throw. Peak Velocity: {throwVelocity} (Mag: {throwVelocity.magnitude:F2} m/s).");
        }
        else
        {
            // Cancel the shot and return the ball to the hand controller's normal grip
            if (handController != null && _currentBall != null)
            {
                handController.GrabBall(_currentBall, true);
                Debug.Log($"[ShootingController] Shot cancelled. Low peak velocity: {throwVelocity.magnitude:F2} m/s (Threshold: {minVelocityThreshold} m/s). Ball returned to hand.");
            }
        }
    }

    private Vector3 GetPeakVelocity()
    {
        if (_velocityHistory == null || _velocityHistory.Count == 0)
        {
            return handController != null ? handController.velocity : Vector3.zero;
        }

        Vector3 peakVelocity = Vector3.zero;
        float maxSpeed = 0f;

        foreach (Vector3 v in _velocityHistory)
        {
            float speed = v.magnitude;
            if (speed > maxSpeed)
            {
                maxSpeed = speed;
                peakVelocity = v;
            }
        }

        return peakVelocity;
    }

    public void ResetState()
    {
        _isShooting = false;
        _wasPressedLastFrame = false;
        _velocityHistory.Clear();
        _currentBall = null;
    }
}
