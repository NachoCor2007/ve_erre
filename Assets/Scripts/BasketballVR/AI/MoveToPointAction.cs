using UnityEngine;
using UnityEngine.AI;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "MoveToPointAction", menuName = "BasketballVR/AI/MoveToPointAction")]
    public class MoveToPointAction : NPCAction
    {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _stoppingDistance = 0.1f;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            npc.NavMeshAgent.SetDestination(_targetPosition);
        }

        public override void Execute(NPCController npc)
        {
            // The movement is handled by NavMeshAgent, so nothing to do here.
        }

        public override bool IsFinished(NPCController npc)
        {
            // Check if the agent is on a NavMesh and has a path
            if (!npc.NavMeshAgent.isOnNavMesh || npc.NavMeshAgent.pathPending)
            {
                return false;
            }

            return npc.NavMeshAgent.remainingDistance <= npc.NavMeshAgent.stoppingDistance + _stoppingDistance;
        }
    }
}
