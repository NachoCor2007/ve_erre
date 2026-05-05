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
        private bool IsPlayerHoldingBall() => false; 
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
            // This action is a waiting state, so no execution logic is needed.
        }

        public override bool IsFinished(NPCController npc)
        {
            bool result = false;
            switch (_trigger)
            {
                case WaitTrigger.PlayerHasBall:
                    result = IsPlayerHoldingBall();
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
