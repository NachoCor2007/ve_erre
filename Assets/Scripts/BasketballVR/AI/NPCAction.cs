using UnityEngine;

namespace BasketballVR.AI
{
    public abstract class NPCAction : ScriptableObject
    {
        [SerializeField] protected string _description;
        [SerializeField] protected bool _showInUI = true;

        public virtual string Description
        {
            get => !string.IsNullOrEmpty(_description) ? _description : name;
            set => _description = value;
        }

        public virtual bool ShowInUI
        {
            get => _showInUI;
            set => _showInUI = value;
        }

        public bool IsActionSuccessful { get; set; }

        public virtual void Initialize(NPCController npc) 
        { 
            IsActionSuccessful = false;
        }

        public virtual void ResetState()
        {
            IsActionSuccessful = false;
        }

        public abstract void Execute(NPCController npc);
        public abstract bool IsFinished(NPCController npc);
    }
}
