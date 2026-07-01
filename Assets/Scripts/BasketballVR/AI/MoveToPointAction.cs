using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "MoveToPointAction", menuName = "BasketballVR/AI/MoveToPointAction")]
    public class MoveToPointAction : NPCAction
    {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _stoppingDistance = 0.1f;

        public override string Description
        {
            get => !string.IsNullOrEmpty(_description) ? _description : "Esperar a posicionamiento del compañero";
        }

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            npc.NavMeshAgent.SetDestination(_targetPosition);

            if (npc.Animator != null)
            {
                npc.Animator.SetBool("isRunning", true);
                npc.Animator.SetBool("isWaiting", false);
            }
        }

        public override void Execute(NPCController npc)
        {
            if (IsFinished(npc))
            {
                IsActionSuccessful = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            // Check if the agent is on a NavMesh and has a path
            if (!npc.NavMeshAgent.isOnNavMesh || npc.NavMeshAgent.pathPending)
            {
                return false;
            }

            bool finished = npc.NavMeshAgent.remainingDistance <= npc.NavMeshAgent.stoppingDistance + _stoppingDistance;
            if (finished)
            {
                if (npc.Animator != null)
                {
                    npc.Animator.SetBool("isRunning", false);
                }
            }
            return finished;
        }
    }
}
