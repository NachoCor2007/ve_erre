using UnityEngine;
using UnityEngine.UI;
using ScriptableObjects;

namespace BasketballVR.UI
{
    public class ContextualMenuDisplay : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private Text titleText;

        /// <summary>
        /// Renders the play's details and actions list inside the titleText component.
        /// </summary>
        public void RenderPlay(Play play)
        {
            if (play == null) return;

            // Automatically resolve titleText if not assigned in Inspector
            if (titleText == null)
            {
                Transform titleTransform = transform.Find("ContextualContainer/Background Panel/Title");
                if (titleTransform != null)
                {
                    titleText = titleTransform.GetComponent<Text>();
                }
                else
                {
                    titleText = GetComponentInChildren<Text>(true);
                }
            }

            if (titleText != null)
            {
                titleText.text = play.Display();
            }
            else
            {
                Debug.LogWarning("[ContextualMenuDisplay] Text component for rendering the play was not found.", this);
            }
        }
    }
}
