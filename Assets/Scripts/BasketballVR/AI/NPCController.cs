using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BasketballVR.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private List<NPCAction> _actionSequence;
        
        private NavMeshAgent _navMeshAgent;
        private int _currentActionIndex = 0;

        public Transform playerTransform;
        public Ball ball;
        public Transform basketHoop; // Needed for PressureAction
        public Transform handTransform; // Point where the NPC will hold the ball

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            InitializeActions();
        }

        private void InitializeActions()
        {
            if (_actionSequence != null && _actionSequence.Count > 0)
            {
                _currentActionIndex = 0;
                if (_actionSequence[_currentActionIndex] != null)
                {
                    _actionSequence[_currentActionIndex].Initialize(this);
                }
            }
        }

        private void Update()
        {
            if (_actionSequence == null || _actionSequence.Count == 0 || _currentActionIndex >= _actionSequence.Count)
            {
                return;
            }

            NPCAction currentAction = _actionSequence[_currentActionIndex];
            if (currentAction == null)
            {
                // Skip null actions in the sequence
                _currentActionIndex++;
                if (_currentActionIndex < _actionSequence.Count && _actionSequence[_currentActionIndex] != null)
                {
                    _actionSequence[_currentActionIndex].Initialize(this);
                }
                return;
            }

            currentAction.Execute(this);

            if (currentAction.IsFinished(this))
            {
                _currentActionIndex++;
                if (_currentActionIndex < _actionSequence.Count && _actionSequence[_currentActionIndex] != null)
                {
                    _actionSequence[_currentActionIndex].Initialize(this);
                }
            }
            
            if (playerTransform != null)
            {
                Vector3 lookAtPosition = playerTransform.position;
                lookAtPosition.y = transform.position.y;
                transform.LookAt(lookAtPosition);
            }
        }

        public NavMeshAgent NavMeshAgent => _navMeshAgent;
    }
}
