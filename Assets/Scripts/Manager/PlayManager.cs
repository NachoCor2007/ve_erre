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

            if (_playsList == null || _playsList.Count == 0 || _currentPlayIndex < 0 || _currentPlayIndex >= _playsList.Count)
                return;

            Play play = _playsList[_currentPlayIndex];
            
            // if (play.WinCondition == null)
            // {
            //     return;
            // }

            Unity.XR.CoreUtils.XROrigin playerOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (playerOrigin != null && play.PlayerLocation != null)
            {
                playerOrigin.transform.position = play.PlayerLocation.Position;
            }

            Ball instantiatedBall = null;
            if (play.BallPrefab != null)
            {
                GameObject ballObj = Instantiate(play.BallPrefab);
                Debug.Log($"Ball instantiated from prefab: {ballObj.name}");
                instantiatedBall = ballObj.GetComponent<Ball>();
                
                BallController ballController = ballObj.GetComponent<BallController>();
                
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
                        NPCController controller = npc.GetComponent<NPCController>();
                        if (controller != null)
                        {
                            controller.SetActionSequence(npcConfig.ActionSequence);
                            if (playerOrigin != null)
                            {
                                controller.playerTransform = playerOrigin.transform;
                            }
                            if (instantiatedBall != null)
                            {
                                controller.ball = instantiatedBall;
                            }
                        }
                    }
                }
            }
        }

        public void EndPlay()
        {
            Debug.Log("Play is successfully done! Requirements met inside PlayManager.");

            Unity.XR.CoreUtils.XROrigin playerOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (playerOrigin != null)
            {
                playerOrigin.transform.position = _defaultPlayerLocation;
            }

            _playsMenuUIReference.SetActive(true);
        }
    }
}
