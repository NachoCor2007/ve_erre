using System.Collections.Generic;
using BasketballVR.AI;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "NPCConfig", menuName = "ScriptableObjects/NPCConfig")]
  public class NPCConfig : ScriptableObject
  {
    [SerializeField] private Location _npcLocation;
    [SerializeField] private List<NPCAction> _actionSequence;
  }
}
