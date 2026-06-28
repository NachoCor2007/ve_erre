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

      if (npcsInScene == null || npcsInScene.Length == 0)
      {
        Debug.LogWarning("No NPCs found in the scene. Win condition cannot be evaluated.");
        return false;
      }
      
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
