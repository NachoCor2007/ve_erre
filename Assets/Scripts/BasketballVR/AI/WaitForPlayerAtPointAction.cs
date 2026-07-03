using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "WaitForPlayerAtPointAction", menuName = "BasketballVR/AI/WaitForPlayerAtPointAction")]
    public class WaitForPlayerAtPointAction : NPCAction
    {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private GameObject _pointerPrefab;
        [SerializeField] private float _detectionThreshold = 1.0f;
        [SerializeField] private AudioClip _triggerSound;
        [SerializeField] private float _soundVolume = 1.0f;
        [SerializeField] private float _soundMinDistance = 5.0f;
        [SerializeField] private float _soundMaxDistance = 100.0f;

        private GameObject _spawnedPointer;
        private Transform _playerTransform;
        private bool _hasFinished = false;

        public override string Description
        {
            get => !string.IsNullOrEmpty(_description) ? _description : "Desplazarse hacia la posición indicada";
        }

        private Transform ResolvePlayerTransform(NPCController npc)
        {
            if (Camera.main != null)
            {
                return Camera.main.transform;
            }

            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                return xrOrigin.Camera != null ? xrOrigin.Camera.transform : xrOrigin.transform;
            }

            return npc != null ? npc.playerTransform : null;
        }

        public override void ResetState()
        {
            base.ResetState();
            _hasFinished = false;
            _spawnedPointer = null;
            _playerTransform = null;
        }

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _hasFinished = false;
            npc.NavMeshAgent.isStopped = true; // Stop moving during wait

            _playerTransform = ResolvePlayerTransform(npc);

            if (_triggerSound != null)
            {
                PlaySoundAtPoint(_triggerSound, _targetPosition, _soundMinDistance, _soundMaxDistance, _soundVolume);
            }

            if (_pointerPrefab != null)
            {
                // Instantiate the pointer at the target position using the prefab's rotation
                _spawnedPointer = Instantiate(_pointerPrefab, _targetPosition, _pointerPrefab.transform.rotation);
                
                // Ensure the pointer is not solid by setting colliders as triggers
                Collider[] colliders = _spawnedPointer.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.isTrigger = true;
                }

                // Add the helper component to lock position/rotation and ensure parenting cleanup
                var locker = _spawnedPointer.AddComponent<FixedWorldTransform>();
                locker.position = _targetPosition;
                locker.rotation = _pointerPrefab.transform.rotation;

                // Add the dashed guide line component
                var guide = _spawnedPointer.AddComponent<DashedGuideLine>();
                guide.playerTransform = _playerTransform;
                guide.targetPosition = _targetPosition;

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
                _playerTransform = ResolvePlayerTransform(npc);
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

        private void PlaySoundAtPoint(AudioClip clip, Vector3 position, float minDistance, float maxDistance, float volume)
        {
            if (clip == null) return;
            GameObject go = new GameObject("TempAudio_" + clip.name);
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f; // 3D sound
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.volume = volume;
            source.Play();
            Destroy(go, clip.length);
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

    /// <summary>
    /// Dynamically draws a dashed guide line composed of flat road-style rectangles 
    /// between the pointer destination and the player's feet.
    /// </summary>
    public class DashedGuideLine : MonoBehaviour
    {
        public Transform playerTransform;
        public Vector3 targetPosition;

        [Header("Dash Settings")]
        [SerializeField] private float _dashLength = 0.3f;
        [SerializeField] private float _dashWidth = 0.06f;
        [SerializeField] private float _dashSpacing = 0.3f;
        [SerializeField] private float _heightOffset = 0.015f; // Slight height to prevent z-fighting with the court floor
        [SerializeField] private Material _dashMaterial;

        private List<GameObject> _dashes = new List<GameObject>();

        private void Start()
        {
            // Attempt to retrieve a matching material from the pointer so colors/glow are cohesive
            if (_dashMaterial == null)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    _dashMaterial = renderer.sharedMaterial;
                }
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            Vector3 startPoint = targetPosition;
            Vector3 endPoint = playerTransform.position;
            // Align Y to the target floor level
            endPoint.y = startPoint.y;

            float distance = Vector3.Distance(startPoint, endPoint);
            float step = _dashLength + _dashSpacing;
            int requiredDashes = Mathf.Max(0, Mathf.FloorToInt(distance / step));

            // Dynamically manage the pool of dash objects
            while (_dashes.Count < requiredDashes)
            {
                GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = $"RoadDash_{_dashes.Count}";
                
                // Remove collider so dashes don't interfere with physics or player movement
                Destroy(dash.GetComponent<Collider>());

                if (_dashMaterial != null)
                {
                    var renderer = dash.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = _dashMaterial;
                    }
                }

                // Parent to the pointer so it automatically deletes when the pointer is cleaned up
                dash.transform.SetParent(transform, true);
                _dashes.Add(dash);
            }

            Vector3 direction = (endPoint - startPoint).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction, Vector3.up) : Quaternion.identity;

            for (int i = 0; i < _dashes.Count; i++)
            {
                GameObject dash = _dashes[i];
                if (i < requiredDashes)
                {
                    dash.SetActive(true);
                    float t = (i * step + _dashLength / 2f) / distance;
                    Vector3 pos = Vector3.Lerp(startPoint, endPoint, t);
                    pos.y += _heightOffset;

                    dash.transform.position = pos;
                    dash.transform.rotation = rotation;
                    dash.transform.localScale = new Vector3(_dashWidth, 0.002f, _dashLength);
                }
                else
                {
                    dash.SetActive(false);
                }
            }
        }
    }
}
