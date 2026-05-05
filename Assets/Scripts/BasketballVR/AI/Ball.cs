using UnityEngine;

namespace BasketballVR.AI
{
    public class Ball : MonoBehaviour
    {
        public delegate void StolenAction();
        public event StolenAction? OnStolen;

        public void Steal()
        {
            OnStolen?.Invoke();
        }
    }
}
