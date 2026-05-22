using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Manager
{
    public class PlayManager : MonoBehaviour
    {
        [SerializeField] private List<Play> _playsList;
        [SerializeField] private int _currentPlayIndex;

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

        void SetUpPlay()
        {
            
        }

        void EndPlay()
        {
            
        }
    }
}
