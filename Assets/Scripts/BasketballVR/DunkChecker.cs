using UnityEngine;
using ScriptableObjects;
using Manager;

namespace BasketballVR
{
    public class DunkChecker : MonoBehaviour
    {
        [Tooltip("The tag of the ball to compare against.")]
        [SerializeField] private string _ballTag = "Ball";
        [SerializeField] private PlayManager _playManager;
        [SerializeField] private GameObject _playCompletedUIReference;
        [SerializeField] private HoopGlowController _glowController;

        private int _ballGoingDownHash;
        private Rigidbody _trackedBallRb;
        private bool _glowStarted = false;

        private void Awake()
        {
            _playManager = FindFirstObjectByType<PlayManager>();
            if (_playManager == null)
            {
                Debug.LogError("PlayManager not found in the scene. Please ensure there is a PlayManager present.");
            }
        }

        private void Start()
        {
            // Search in parent hierarchy first, then fall back to a global search in the scene
            _glowController = GetComponentInParent<HoopGlowController>();
            if (_glowController == null)
            {
                _glowController = FindFirstObjectByType<HoopGlowController>();
            }
            
            if (_glowController == null)
            {
                Debug.LogWarning("HoopGlowController not found in parent hierarchy or scene. Emissive glow will not be triggered.");
            }
        }

        private void Update()
        {
            WinCondition currentWinCondition = GetWinCondition();
            if (currentWinCondition != null)
            {
                bool isDone = currentWinCondition.CheckIfDone();
                if (isDone && !_glowStarted)
                {
                    if (_glowController != null)
                    {
                        _glowController.StartGlow();
                    }
                    _glowStarted = true;
                }
                else if (!isDone && _glowStarted)
                {
                    if (_glowController != null)
                    {
                        _glowController.StopGlow();
                    }
                    _glowStarted = false;
                }
            }
            else
            {
                if (_glowStarted)
                {
                    if (_glowController != null)
                    {
                        _glowController.StopGlow();
                    }
                    _glowStarted = false;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_ballTag))
            {
                Rigidbody rb = other.attachedRigidbody;
                
                // When entering, if going down, record it.
                if (rb != null && rb.linearVelocity.y < 0f)
                {
                    _trackedBallRb = rb;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_ballTag) && _trackedBallRb != null && other.attachedRigidbody == _trackedBallRb)
            {
                // When exiting, check if it's STILL going down
                if (_trackedBallRb.linearVelocity.y < 0f)
                {
                    WinCondition currentWinCondition = GetWinCondition();
                    if (currentWinCondition != null && currentWinCondition.CheckIfDone())
                    {
                        other.gameObject.SetActive(false);
                        EndPlay();
                    }
                }
                _trackedBallRb = null; // Reset tracker
            }
        }

        private WinCondition GetWinCondition()
        {
            PlayManager playManager = FindFirstObjectByType<PlayManager>();
            if (playManager != null)
            {
                Play currentPlay = playManager.GetCurrentPlay();
                if (currentPlay != null)
                {
                    return currentPlay.WinCondition;
                }
            }
            return null;
        }

        private void EndPlay()
        {
            _glowController.StopGlow();
            _playCompletedUIReference.SetActive(true);
        }
    }
}
