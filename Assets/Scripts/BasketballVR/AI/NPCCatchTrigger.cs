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
        private float _lastCatchTime; // Añadimos un cooldown para evitar que agarre la pelota inmediatamente tras soltarla/tirarla.

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
            // Evitar loop de agarrar la pelota exactamente cuando fue lanzada desde la misma mano
            if (Time.time - _lastCatchTime < 0.5f) return;

            // Check if the object that entered the trigger is the ball
            if (other.CompareTag("Ball"))
            {
                var ballController = other.GetComponent<BallController>();
                if (ballController != null && !ballController.isHeld)
                {
                    _lastCatchTime = Time.time;
                    // Tell the ball to be grabbed by the NPC's hand
                    ballController.holdLocalOffset = Vector3.zero;
                    ballController.Grab(_npcController.handTransform);
                }
            }
        }

        // Método auxiliar para informarle al CatchTrigger que la pelota acaba de ser lanzada, para forzar cooldown.
        public void NotifyBallReleased()
        {
            _lastCatchTime = Time.time;
        }
    }
}
