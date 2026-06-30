using UnityEngine;

namespace BasketballVR.AI
{
    public enum WaitTrigger { PlayerHasBall, PlayerStartedShot, GlobalTimer }

    [CreateAssetMenu(fileName = "WaitConditionAction", menuName = "BasketballVR/AI/WaitConditionAction")]
    public class WaitConditionAction : NPCAction
    {
        [SerializeField] private WaitTrigger _trigger;
        [SerializeField] private float _timerDuration = 5f;

        private float _startTime;

        // Placeholder for game state checks.
        // In a real project, you would get this from a GameManager or similar singleton.
        private bool IsPlayerHoldingBall(NPCController npc)
        {
            Ball ball = (npc != null) ? npc.ball : null;
            if (ball == null)
            {
                ball = FindFirstObjectByType<Ball>();
            }
            if (ball == null) return false;

            var ballController = ball.GetComponent<BallController>();
            if (ballController == null || !ballController.isHeld) return false;

            var hands = FindObjectsByType<HandController>(FindObjectsSortMode.None);
            foreach (var hand in hands)
            {
                if (hand != null)
                {
                    if (hand.GetCurrentBall() == ballController || ballController.holdPoint == hand.controllerTransform)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasPlayerStartedShot() => false;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            if (_trigger == WaitTrigger.GlobalTimer)
            {
                _startTime = Time.time;
            }
            npc.NavMeshAgent.isStopped = true; // Stop moving during wait
        }

        public override void Execute(NPCController npc)
        {
            // The wait logic is handled within the WaitCondition. 
            // The Execute method simply continues tracking until IsFinished returns true.
            if (IsFinished(npc))
            {
                IsActionSuccessful = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            bool result = false;
            switch (_trigger)
            {
                case WaitTrigger.PlayerHasBall:
                    result = IsPlayerHoldingBall(npc);
                    break;
                case WaitTrigger.PlayerStartedShot:
                    result = HasPlayerStartedShot();
                    break;
                case WaitTrigger.GlobalTimer:
                    result = Time.time - _startTime >= _timerDuration;
                    break;
            }

            if (result)
            {
                npc.NavMeshAgent.isStopped = false; // Resume movement
            }
            return result;
        }
    }
}
