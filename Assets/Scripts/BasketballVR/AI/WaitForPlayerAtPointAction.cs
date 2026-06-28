using UnityEngine;
using Unity.XR.CoreUtils;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "WaitForPlayerAtPointAction", menuName = "BasketballVR/AI/WaitForPlayerAtPointAction")]
    public class WaitForPlayerAtPointAction : NPCAction
    {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private GameObject _pointerPrefab;
        [SerializeField] private float _detectionThreshold = 1.0f;

        private GameObject _spawnedPointer;
        private Transform _playerTransform;
        private bool _hasFinished = false;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _hasFinished = false;
            npc.NavMeshAgent.isStopped = true; // Stop moving during wait

            // Resolve player transform by finding the XROrigin component directly
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                _playerTransform = xrOrigin.transform;
            }
            else
            {
                _playerTransform = npc.playerTransform;
            }

            if (_pointerPrefab != null)
            {
                // Instantiate the pointer at the target position using the prefab's rotation
                _spawnedPointer = Instantiate(_pointerPrefab, _targetPosition, _pointerPrefab.transform.rotation);
                
                // Add the helper component to lock position/rotation and ensure parenting cleanup
                var locker = _spawnedPointer.AddComponent<FixedWorldTransform>();
                locker.position = _targetPosition;
                locker.rotation = _pointerPrefab.transform.rotation;

                // Parent to the NPC controller transform so it is cleaned up when the NPC is destroyed
                _spawnedPointer.transform.SetParent(npc.transform, true);
            }
            else
            {
                Debug.LogWarning("WaitForPlayerAtPointAction: Pointer prefab is not assigned.");
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
            if (_hasFinished)
            {
                return true;
            }

            if (_playerTransform == null)
            {
                // Try again in case it wasn't found during Initialize
                var xrOrigin = FindFirstObjectByType<XROrigin>();
                if (xrOrigin != null)
                {
                    _playerTransform = xrOrigin.transform;
                }
                else
                {
                    _playerTransform = npc.playerTransform;
                }
            }

            if (_playerTransform == null)
            {
                return false;
            }

            // Project player position onto the target horizontal plane to make detection robust
            Vector3 playerPos = _playerTransform.position;
            Vector3 targetPos = _targetPosition;
            playerPos.y = targetPos.y;

            bool reached = Vector3.Distance(playerPos, targetPos) <= _detectionThreshold;

            if (reached)
            {
                CleanupPointer();
                npc.NavMeshAgent.isStopped = false; // Resume movement
                _hasFinished = true;
                return true;
            }

            return false;
        }

        private void CleanupPointer()
        {
            if (_spawnedPointer != null)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    DestroyImmediate(_spawnedPointer);
                    return;
                }
#endif
                Destroy(_spawnedPointer);
                _spawnedPointer = null;
            }
        }

        private void OnDisable()
        {
            CleanupPointer();
        }
    }

    /// <summary>
    /// Helper component to lock the spawned pointer to target world values,
    /// avoiding rotation/movement when parent NPC moves, while preserving auto-cleanup on NPC destroy.
    /// </summary>
    public class FixedWorldTransform : MonoBehaviour
    {
        public Vector3 position;
        public Quaternion rotation;

        private void LateUpdate()
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
