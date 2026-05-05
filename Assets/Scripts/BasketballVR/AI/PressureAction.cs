using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "PressureAction", menuName = "BasketballVR/AI/PressureAction")]
    public class PressureAction : NPCAction
    {
        [SerializeField] private float _duration = 5f;
        [SerializeField] private float _distanceFromPlayer = 2f;

        private float _startTime;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _startTime = Time.time;
        }

        public override void Execute(NPCController npc)
        {
            if (npc.playerTransform == null || npc.basketHoop == null) return;

            Vector3 playerToHoopDir = (npc.basketHoop.position - npc.playerTransform.position).normalized;
            playerToHoopDir.y = 0; // Keep the calculation on the XZ plane

            Vector3 targetPosition = npc.playerTransform.position + playerToHoopDir * _distanceFromPlayer;
            
            npc.NavMeshAgent.SetDestination(targetPosition);
        }

        public override bool IsFinished(NPCController npc)
        {
            return Time.time - _startTime >= _duration;
        }
    }
}
