using UnityEngine;

namespace BasketballVR.AI
{
    public abstract class NPCAction : ScriptableObject
    {
        public bool IsActionSuccessful { get; set; }

        public virtual void Initialize(NPCController npc) 
        { 
            IsActionSuccessful = false;
        }
        public abstract void Execute(NPCController npc);
        public abstract bool IsFinished(NPCController npc);
    }
}
