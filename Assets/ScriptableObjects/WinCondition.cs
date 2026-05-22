using System;
using System.Collections.Generic;
using BasketballVR.AI;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "WinCondition", menuName = "ScriptableObjects/WinCondition")]
  public class WinCondition : ScriptableObject
  {
    [SerializeField] private String _conditionDescription;
  }
}
