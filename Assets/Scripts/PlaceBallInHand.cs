using UnityEngine;

public class PlaceBallInHand : MonoBehaviour
{
    public BallController ball;
    public HandController rightHand;

    void Start()
    {
        if (ball != null && rightHand != null)
        {
            // Directly assign the ball to the hand, forcing the grab
            rightHand.GrabBall(ball, true);
        }
        
        // The script's job is done, so it destroys itself.
        Destroy(this);
    }
}
