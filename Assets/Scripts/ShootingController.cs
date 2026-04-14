using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ShootingController : MonoBehaviour
{
    public Transform shootPoint; // referencia al controller
    public InputActionProperty triggerAction;
    
    private BallController currentBall;
    private bool isShooting = false;
    bool wasPressedLastFrame = false;
    public HandController handController;

    void Update()
    {
        currentBall = handController.GetCurrentBall();
        
        bool pressed = triggerAction.action.ReadValue<float>() > 0.1f;

        if (pressed && !wasPressedLastFrame)
            Debug.Log("TRIGGER DOWN");

        if (!pressed && wasPressedLastFrame)
            Debug.Log("TRIGGER UP");

        // 🟢 SOLO cuando empieza a apretar
        if (pressed && !wasPressedLastFrame && currentBall != null)
        {
            isShooting = true;
            currentBall.Grab(shootPoint);
        }

        // 🔴 SOLO cuando suelta
        if (!pressed && wasPressedLastFrame && isShooting && currentBall != null)
        {
            Shoot();
            isShooting = false;
            currentBall = null;
        }

        wasPressedLastFrame = pressed;
    }

    void Shoot()
    {
        Debug.Log("DISPARANDO: " + currentBall);

        Rigidbody rb = currentBall.rb;

        currentBall.isHeld = false;
        currentBall.holdPoint = null;

        rb.isKinematic = false;

        // fuerza hacia adelante + un poco hacia arriba
        Vector3 shootVelocity =
            shootPoint.forward * 8f +
            shootPoint.up * 2f;

        rb.linearVelocity = shootVelocity;
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
