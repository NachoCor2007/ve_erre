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
    [SerializeField] private string _playName;
    [SerializeField] private string _finalStepMessage = "¡Encestá!";

    public List<NPCConfig> NpcConfigs => _npcConfigs;
    public Location PlayerLocation => _playerLocation;
    public WinCondition WinCondition => _winCondition;
    public GameObject BallPrefab => _ballPrefab;
    public string PlayName => _playName;
    public string FinalStepMessage => _finalStepMessage;

    /// <summary>
    /// Generates a formatted string representing the play name
    /// and a flat list of actions across all configured NPCs, ending with the final step message.
    /// </summary>
    public string Display()
    {
      var sb = new System.Text.StringBuilder();
      sb.AppendLine($"{_playName}");
      sb.AppendLine();
      sb.AppendLine("Pasos de la jugada:");

      int actionCounter = 1;
      if (_npcConfigs != null)
      {
        for (int i = 0; i < _npcConfigs.Count; i++)
        {
          var npcConfig = _npcConfigs[i];
          if (npcConfig != null && npcConfig.ActionSequence != null)
          {
            for (int j = 0; j < npcConfig.ActionSequence.Count; j++)
            {
              var action = npcConfig.ActionSequence[j];
              if (action != null && action.ShowInUI)
              {
                sb.AppendLine($"{actionCounter}. {action.Description}");
                actionCounter++;
              }
            }
          }
        }
      }

      if (!string.IsNullOrEmpty(_finalStepMessage))
      {
        sb.AppendLine($"{actionCounter}. {_finalStepMessage}");
      }

      return sb.ToString();
    }
  }
}
