using System;
using UnityEngine;
using BasketballVR.AI;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "WinCondition", menuName = "ScriptableObjects/WinCondition")]
  public class WinCondition : ScriptableObject
  {
    [SerializeField] private String _conditionDescription;

    public bool CheckIfDone()
    {
      NPCController[] npcsInScene = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
      
      foreach (var npc in npcsInScene)
      {
        if (!npc.AreAllActionsSuccessful())
        {
          return false;
        }
      }
      return true;
    }
  }
}
