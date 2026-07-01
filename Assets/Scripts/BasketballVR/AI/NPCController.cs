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
        private Animator _animator;
        private int _currentActionIndex = 0;
        private bool _actionsCloned = false;

        public Transform playerTransform;
        public Ball ball;
        public Transform basketHoop; // Needed for PressureAction
        public Transform handTransform; // Point where the NPC will hold the ball

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            InitializeActions();
        }

        public void SetActionSequence(List<NPCAction> newActionSequence)
        {
            _actionSequence = newActionSequence;
            _actionsCloned = false;
            InitializeActions();
        }

        private void InitializeActions()
        {
            if (_animator != null)
            {
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isWaiting", false);
            }

            if (!_actionsCloned && _actionSequence != null)
            {
                List<NPCAction> clonedSequence = new List<NPCAction>();
                foreach (var action in _actionSequence)
                {
                    if (action != null)
                    {
                        clonedSequence.Add(Instantiate(action));
                    }
                    else
                    {
                        clonedSequence.Add(null);
                    }
                }
                _actionSequence = clonedSequence;
                _actionsCloned = true;
            }

            if (_actionSequence != null)
            {
                foreach (var action in _actionSequence)
                {
                    if (action != null)
                    {
                        action.ResetState();
                    }
                }
            }

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
                if (_animator != null)
                {
                    _animator.SetBool("isRunning", false);
                    _animator.SetBool("isWaiting", false);
                }
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
        public Animator Animator => _animator;

        public NPCAction CurrentAction
        {
            get
            {
                if (_actionSequence != null && _currentActionIndex >= 0 && _currentActionIndex < _actionSequence.Count)
                {
                    return _actionSequence[_currentActionIndex];
                }
                return null;
            }
        }

        public bool AreAllActionsSuccessful()
        {
            if (_actionSequence == null || _actionSequence.Count == 0) return true;
            
            foreach (var action in _actionSequence)
            {
                if (action != null && !action.IsActionSuccessful)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
