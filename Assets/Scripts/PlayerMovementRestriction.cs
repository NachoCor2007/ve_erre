using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerMovementRestriction : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The movement provider component to enable/disable. If not set, will automatically find one in the scene.")]
    private DynamicMoveProvider moveProvider;

    [SerializeField]
    [Tooltip("The hand controllers to monitor. If empty, will automatically find all HandController instances in the scene.")]
    private HandController[] handControllers;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("The maximum time (in seconds) the player can hold the ball before movement is disabled.")]
    private float maxHoldTime = 3.0f;

    [Header("Debug Info (Read Only)")]
    [SerializeField]
    private bool isHoldingBall;
    [SerializeField]
    private float holdTimer;

    private bool wasHoldingBall;

    private void Start()
    {
        // Automatically find the movement provider if not assigned
        if (moveProvider == null)
        {
            moveProvider = FindFirstObjectByType<DynamicMoveProvider>();
            if (moveProvider == null)
            {
                Debug.LogWarning("[PlayerMovementRestriction] No DynamicMoveProvider found in the scene.");
            }
        }

        // Automatically find hand controllers if not assigned
        if (handControllers == null || handControllers.Length == 0)
        {
            handControllers = FindObjectsByType<HandController>(FindObjectsSortMode.None);
            if (handControllers == null || handControllers.Length == 0)
            {
                Debug.LogWarning("[PlayerMovementRestriction] No HandController instances found in the scene.");
            }
        }
    }

    private void Update()
    {
        // Check if any hand controller is holding the ball
        isHoldingBall = CheckIfHoldingBall();

        if (isHoldingBall)
        {
            // Transition: Player just grabbed the ball
            if (!wasHoldingBall)
            {
                holdTimer = 0f;
                SetMovementEnabled(true);
                wasHoldingBall = true;
                Debug.Log("[PlayerMovementRestriction] Player grabbed the ball. Timer started.");
            }

            // Increment timer
            holdTimer += Time.deltaTime;

            // Timer runs out -> Disable movement
            if (holdTimer >= maxHoldTime)
            {
                if (moveProvider != null && moveProvider.enabled)
                {
                    SetMovementEnabled(false);
                    Debug.Log("[PlayerMovementRestriction] 3 seconds holding time exceeded! Player movement disabled.");
                }
            }
        }
        else
        {
            // Transition: Player just released the ball (e.g. shot, passed, or dribbled)
            if (wasHoldingBall)
            {
                holdTimer = 0f;
                SetMovementEnabled(true);
                wasHoldingBall = false;
                Debug.Log("[PlayerMovementRestriction] Player released the ball. Movement re-enabled.");
            }
        }
    }

    private bool CheckIfHoldingBall()
    {
        if (handControllers == null) return false;

        foreach (var hand in handControllers)
        {
            if (hand != null && hand.GetCurrentBall() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void SetMovementEnabled(bool enable)
    {
        if (moveProvider != null)
        {
            moveProvider.enabled = enable;
        }
    }
}
