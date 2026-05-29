using UnityEngine;
using ScriptableObjects;
using Manager;

namespace BasketballVR
{
    public class DunkChecker : MonoBehaviour
    {
        [Tooltip("The tag of the ball to compare against.")]
        [SerializeField] private string _ballTag = "Ball";

        private int _ballGoingDownHash;
        private Rigidbody _trackedBallRb;

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
            Debug.Log("Play is successfully done! Requirements met.");
            // Further logic for ending the play goes here
        }
    }
}
