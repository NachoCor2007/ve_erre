using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "ReceiveBallAction", menuName = "BasketballVR/AI/ReceiveBallAction")]
    public class ReceiveBallAction : NPCAction
    {
        private bool _hasReceivedBall = false;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _hasReceivedBall = false;
            npc.NavMeshAgent.isStopped = true; // The NPC waits in place
        }

        public override void Execute(NPCController npc)
        {
            if (_hasReceivedBall || npc.ball == null)
            {
                return;
            }

            // The actual catch logic is now handled by NPCCatchTrigger.
            // We just need to check if the ball is now being held by the NPC.
            var ballController = npc.ball.GetComponent<BallController>();
            if (ballController != null && ballController.isHeld && ballController.holdPoint == npc.handTransform)
            {
                _hasReceivedBall = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            // The action is finished once the ball has been received.
            if (_hasReceivedBall)
            {
                npc.NavMeshAgent.isStopped = false; // Allow movement for the next action
                return true;
            }
            return false;
        }
    }
}
