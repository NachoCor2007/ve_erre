using System.Collections.Generic;
using BasketballVR.AI;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "NPCConfig", menuName = "ScriptableObjects/NPCConfig")]
  public class NPCConfig : ScriptableObject
  {
    [SerializeField] private GameObject _npcPrefab;
    [SerializeField] private Location _npcLocation;
    [SerializeField] private List<NPCAction> _actionSequence;

    public GameObject NpcPrefab => _npcPrefab;
    public Location NpcLocation => _npcLocation;
    public List<NPCAction> ActionSequence => _actionSequence;
  }
}
