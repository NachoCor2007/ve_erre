using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "Play", menuName = "ScriptableObjects/Play")]
  public class Play : ScriptableObject
  {
    [SerializeField] private List<NPCConfig> _npcConfigs;
    [SerializeField] private Location _playerLocation;
    [SerializeField] private WinCondition _winCondition;
    [SerializeField] private GameObject _ballPrefab;

    public List<NPCConfig> NpcConfigs => _npcConfigs;
    public Location PlayerLocation => _playerLocation;
    public WinCondition WinCondition => _winCondition;
    public GameObject BallPrefab => _ballPrefab;
  }
}
