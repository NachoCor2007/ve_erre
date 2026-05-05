using UnityEngine;

namespace BasketballVR.AI
{
    /// <summary>
    /// This component acts as the physical presence for the NPC to detect and interact with the ball.
    /// It uses a trigger collider to detect when the ball is nearby and tells the NPCController to "catch" it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class NPCCatchTrigger : MonoBehaviour
    {
        private NPCController _npcController;

        private void Awake()
        {
            _npcController = GetComponentInParent<NPCController>();
            if (_npcController == null)
            {
                Debug.LogError("NPCCatchTrigger must be a child of an object with an NPCController component.");
            }

            // Ensure the collider is set to be a trigger
            var col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                Debug.LogWarning("Collider on NPCCatchTrigger was not set to 'Is Trigger'. Forcing it to true.", this);
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the object that entered the trigger is the ball
            if (other.CompareTag("Ball"))
            {
                var ballController = other.GetComponent<BallController>();
                if (ballController != null && !ballController.isHeld)
                {
                    // Tell the ball to be grabbed by the NPC's hand
                    ballController.Grab(_npcController.handTransform);
                }
            }
        }
    }
}

