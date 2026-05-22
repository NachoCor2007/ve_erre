using System.Collections.Generic;
using BasketballVR.AI;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "Location", menuName = "ScriptableObjects/Location")]
  public class Location : ScriptableObject
  {
    [SerializeField] private Vector3 _position;

    public Vector3 Position => _position;
  }
}
