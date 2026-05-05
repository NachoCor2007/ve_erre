using UnityEngine;

namespace BasketballVR.AI
{
    public abstract class NPCAction : ScriptableObject
    {
        public virtual void Initialize(NPCController npc) { }
        public abstract void Execute(NPCController npc);
        public abstract bool IsFinished(NPCController npc);
    }
}

