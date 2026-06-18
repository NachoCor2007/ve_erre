using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class PlayerMovementRestriction : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The movement provider component whose speed will be controlled. If not set, will automatically find one in the scene.")]
    private ContinuousMoveProvider moveProvider;

    [SerializeField]
    [Tooltip("The hand controllers to monitor. If empty, will automatically find all HandController instances in the scene.")]
    private HandController[] handControllers;

    [Header("Settings")]
    [SerializeField]
    [Tooltip("The maximum time (in seconds) the player can hold the ball before movement is disabled.")]
    private float maxHoldTime = 3.0f;

    [SerializeField]
    [Tooltip("How fast the speed transitions (units per second) to the target speed. Higher values mean faster transitions.")]
    private float speedTransitionRate = 4.0f;

    [Header("Debug Info (Read Only)")]
    [SerializeField]
    private bool isHoldingBall;
    [SerializeField]
    private float holdTimer;
    [SerializeField]
    private float initialMoveSpeed;

    private bool wasHoldingBall;

    private void Start()
    {
        // Automatically find the movement provider if not assigned
        if (moveProvider == null)
        {
            moveProvider = FindFirstObjectByType<ContinuousMoveProvider>();
            if (moveProvider == null)
            {
                Debug.LogWarning("[PlayerMovementRestriction] No ContinuousMoveProvider found in the scene.");
            }
        }

        // Cache the initial move speed
        if (moveProvider != null)
        {
            initialMoveSpeed = moveProvider.moveSpeed;
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

    private void OnDisable()
    {
        // Restore initial speed when this script is disabled
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = initialMoveSpeed;
        }
    }

    private void Update()
    {
        // Check if any hand controller is holding the ball
        isHoldingBall = CheckIfHoldingBall();

        float targetSpeed = initialMoveSpeed;

        if (isHoldingBall)
        {
            // Transition: Player just grabbed the ball
            if (!wasHoldingBall)
            {
                BallController ball = GetHeldBall();
                if (ball != null)
                {
                    // If the ball has not bounced since it was last released,
                    // we resume the timer rather than resetting it.
                    if (ball.lastHandThatThrew != null)
                    {
                        Debug.Log($"[PlayerMovementRestriction] Ball caught without a bounce. Timer resumed from: {holdTimer:F2}s.");
                    }
                    else
                    {
                        holdTimer = 0f;
                        Debug.Log("[PlayerMovementRestriction] Ball grabbed after bounce or new possession. Timer reset to 0.");
                    }
                }
                else
                {
                    holdTimer = 0f;
                }

                wasHoldingBall = true;
            }

            // Increment timer while holding the ball
            holdTimer += Time.deltaTime;

            // Timer runs out -> target speed becomes 0
            if (holdTimer >= maxHoldTime)
            {
                targetSpeed = 0f;
            }
        }
        else
        {
            // Transition: Player just released the ball (e.g. shot, passed, or dribbled)
            if (wasHoldingBall)
            {
                wasHoldingBall = false;
                Debug.Log($"[PlayerMovementRestriction] Ball released. Target speed restored. Cached timer: {holdTimer:F2}s.");
            }
        }

        // Softly transition the speed
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = Mathf.MoveTowards(moveProvider.moveSpeed, targetSpeed, speedTransitionRate * Time.deltaTime);
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

    private BallController GetHeldBall()
    {
        if (handControllers == null) return null;

        foreach (var hand in handControllers)
        {
            if (hand != null)
            {
                BallController ball = hand.GetCurrentBall();
                if (ball != null)
                {
                    return ball;
                }
            }
        }
        return null;
    }
}
