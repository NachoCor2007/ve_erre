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

    void Update()
    {
        _currentBall = handController.GetCurrentBall();

        if (_currentBall != null)
        {
            Debug.Log("PELOTA EN MANO: " + _currentBall);
        }
        
        bool pressed = triggerAction.action.ReadValue<float>() > 0.1f;

        // if (pressed && !wasPressedLastFrame)
        //     Debug.Log("TRIGGER DOWN");

        // if (!pressed && wasPressedLastFrame)
        //     Debug.Log("TRIGGER UP");

        // 🟢 SOLO cuando empieza a apretar
        if (pressed && !_wasPressedLastFrame && _currentBall != null)
        {
            _isShooting = true;
            _currentBall.Grab(shootPoint);
        }

        // 🔴 SOLO cuando suelta
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

        handController.ClearBall();

        Rigidbody rb = _currentBall.rb;

        // fuerza hacia adelante + un poco hacia arriba
        Vector3 shootVelocity =
            shootPoint.forward * 8f +
            shootPoint.up * 2f;

        _currentBall.Release(shootVelocity, handController);
    }

    // Detectar pelota cercana (igual que tu grab)
    // void OnTriggerStay(Collider other)
    // {
    //     if (other.CompareTag("Ball"))
    //     {
    //         if (currentBall == null)
    //         {
    //             currentBall = other.GetComponent<BallController>();
    //         }
    //     }
    // }
}
