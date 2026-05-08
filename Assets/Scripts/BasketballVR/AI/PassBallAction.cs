using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "PassBallAction", menuName = "BasketballVR/AI/PassBallAction")]
    public class PassBallAction : NPCAction
    {
        [SerializeField] private float _passForce = 10f;
        [SerializeField] private float _upwardForce = 2f;
        private bool _ballPassed;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _ballPassed = false;
        }

        public override void Execute(NPCController npc)
        {
            if (npc.ball == null || _ballPassed)
            {
                return;
            }

            var ballController = npc.ball.GetComponent<BallController>();

            // Check if the NPC is currently holding the ball
            if (ballController != null && ballController.isHeld && ballController.holdPoint == npc.handTransform)
            {
                // Calculate pass direction and apply force by releasing the ball
                Vector3 passDirection = (npc.playerTransform.position - npc.transform.position).normalized;
                Vector3 passVelocity = (passDirection * _passForce) + (Vector3.up * _upwardForce);
                
                // Mover bola un poco hacia adelante de la mano para que no roce colisionadores
                ballController.transform.position += passDirection * 0.2f;

                ballController.Release(passVelocity);
                
                // NOTIFICAR que la pelota fue lanzada para aplicar cooldown al CatchTrigger del NPC
                var catchTrigger = npc.GetComponentInChildren<NPCCatchTrigger>();
                if (catchTrigger != null)
                {
                    catchTrigger.NotifyBallReleased();
                }

                _ballPassed = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            return _ballPassed;
        }
    }
}
