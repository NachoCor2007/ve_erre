using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "StealBallAction", menuName = "BasketballVR/AI/StealBallAction")]
    public class StealBallAction : NPCAction
    {
        [SerializeField] private float _stealDistance = 1.0f;
        [SerializeField] private float _moveSpeedMultiplier = 1.5f;

        private float _originalSpeed;
        private bool _ballStolen;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _originalSpeed = npc.NavMeshAgent.speed;
            npc.NavMeshAgent.speed *= _moveSpeedMultiplier;
            _ballStolen = false;
        }

        public override void Execute(NPCController npc)
        {
            if (npc.ball == null || _ballStolen) return;

            npc.NavMeshAgent.SetDestination(npc.ball.transform.position);

            if (Vector3.Distance(npc.transform.position, npc.ball.transform.position) < _stealDistance)
            {
                npc.ball.Steal();
                _ballStolen = true;
                npc.NavMeshAgent.speed = _originalSpeed; // Reset speed
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            return _ballStolen;
        }
    }
}
