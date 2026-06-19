using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using BasketballVR.AI;

namespace Manager
{
    public class PlayManager : MonoBehaviour
    {
        [SerializeField] private List<Play> _playsList;
        [SerializeField] private int _currentPlayIndex;
        [SerializeField] private HandController _playerHand;
        [SerializeField] private GameObject _playsMenuUIReference;
        [SerializeField] private GameObject _restartUIReference;
        [SerializeField] private GameObject _completedPlayUIReference;
        [SerializeField] private GameObject _playerMovementReference;
        [SerializeField] private Vector3 _defaultPlayerLocation = Vector3.zero;

        List<Play> GetPlaysList()
        {
            return _playsList;
        }
        
        int GetCurrentPlayIndex()
        {
            return _currentPlayIndex;
        }
        
        void SelectPlay(int playIndex)
        {
            _currentPlayIndex = playIndex;
        }

        private List<GameObject> _instantiatedNpcs = new List<GameObject>();
        private GameObject _instantiatedBall;


        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
            #if UNITY_ANDROID && !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            #endif
        }

        public Play GetCurrentPlay()
        {
            if (_playsList != null && _currentPlayIndex >= 0 && _currentPlayIndex < _playsList.Count)
            {
                return _playsList[_currentPlayIndex];
            }
            return null;
        }

        public void SetUpPlay()
        {
            _playsMenuUIReference.SetActive(false);
            _restartUIReference.SetActive(false);
            _completedPlayUIReference.SetActive(false);
            _playerMovementReference.SetActive(true);

            if (_playsList == null || _playsList.Count == 0 || _currentPlayIndex < 0 || _currentPlayIndex >= _playsList.Count)
                return;

            Play play = _playsList[_currentPlayIndex];

            Unity.XR.CoreUtils.XROrigin playerOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (playerOrigin != null && play.PlayerLocation != null)
            {
                playerOrigin.transform.position = play.PlayerLocation.Position;
            }

            // Reset hand/shooting controllers to clear velocity spikes from teleportation
            if (_playerHand != null)
            {
                _playerHand.ResetState();
                var shootingController = _playerHand.GetComponentInChildren<ShootingController>();
                if (shootingController != null)
                {
                    shootingController.ResetState();
                }
            }
            else
            {
                HandController[] hands = FindObjectsByType<HandController>(FindObjectsSortMode.None);
                foreach (var hand in hands)
                {
                    if (hand != null)
                    {
                        hand.ResetState();
                        var shootingController = hand.GetComponentInChildren<ShootingController>();
                        if (shootingController != null)
                        {
                            shootingController.ResetState();
                        }
                    }
                }
            }

            Ball instantiatedBallComponent = null;
            if (play.BallPrefab != null)
            {
                _instantiatedBall = Instantiate(play.BallPrefab);
                Debug.Log($"Ball instantiated from prefab: {_instantiatedBall.name}");
                instantiatedBallComponent = _instantiatedBall.GetComponent<Ball>();
                
                BallController ballController = _instantiatedBall.GetComponent<BallController>();
                
                if (_playerHand != null && ballController != null)
                {
                    Debug.Log("Attaching ball directly to the provided HandController.");
                    _playerHand.GrabBall(ballController, true);
                }
                else if (ballController != null)
                {
                    Debug.Log("HandController reference not set in PlayManager. Attempting to attach to first found HandController.");
                    HandController handController = FindFirstObjectByType<HandController>();
                    if (handController != null)
                    {
                        handController.GrabBall(ballController, true);
                    }
                    else
                    {
                        Debug.LogError("No HandController found in scene to hold the ball.");
                    }
                }

                if (ballController == null)
                {
                    Debug.LogError("BallController component not found on ball prefab.");
                }
            }
            else
            {
                Debug.LogWarning("Play.BallPrefab is null.");
            }

            if (play.NpcConfigs != null)
            {
                foreach (var npcConfig in play.NpcConfigs)
                {
                    if (npcConfig.NpcPrefab != null && npcConfig.NpcLocation != null)
                    {
                        GameObject npc = Instantiate(npcConfig.NpcPrefab, npcConfig.NpcLocation.Position, Quaternion.identity);
                        _instantiatedNpcs.Add(npc);
                        NPCController controller = npc.GetComponent<NPCController>();
                        if (controller != null)
                        {
                            controller.SetActionSequence(npcConfig.ActionSequence);
                            if (playerOrigin != null)
                            {
                                controller.playerTransform = playerOrigin.transform;
                            }
                            if (instantiatedBallComponent != null)
                            {
                                controller.ball = instantiatedBallComponent;
                            }
                        }
                    }
                }
            }
        }

        public void RestartPlay()
        {
            CleanUpPlay();
            SetUpPlay();
        }

        public void EndPlay()
        {
            Debug.Log("Play is successfully done! Requirements met inside PlayManager.");

            CleanUpPlay();

            Unity.XR.CoreUtils.XROrigin playerOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (playerOrigin != null)
            {
                playerOrigin.transform.position = _defaultPlayerLocation;
            }

            _playsMenuUIReference.SetActive(true);
            _restartUIReference.SetActive(false);
            _completedPlayUIReference.SetActive(false);
            _playerMovementReference.SetActive(false);
        }

        private void CleanUpPlay()
        {
            foreach (var npc in _instantiatedNpcs)
            {
                Destroy(npc);
            }
            _instantiatedNpcs.Clear();

            if (_instantiatedBall != null)
            {
                Destroy(_instantiatedBall);
                _instantiatedBall = null;
            }

            // Clean up player controllers during play reset
            if (_playerHand != null)
            {
                _playerHand.ResetState();
                var shootingController = _playerHand.GetComponentInChildren<ShootingController>();
                if (shootingController != null)
                {
                    shootingController.ResetState();
                }
            }
            else
            {
                HandController[] hands = FindObjectsByType<HandController>(FindObjectsSortMode.None);
                foreach (var hand in hands)
                {
                    if (hand != null)
                    {
                        hand.ResetState();
                        var shootingController = hand.GetComponentInChildren<ShootingController>();
                        if (shootingController != null)
                        {
                            shootingController.ResetState();
                        }
                    }
                }
            }
        }
    }
}
