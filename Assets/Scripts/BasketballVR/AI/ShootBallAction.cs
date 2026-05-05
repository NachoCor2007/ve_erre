using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "ShootBallAction", menuName = "BasketballVR/AI/ShootBallAction")]
    public class ShootBallAction : NPCAction
    {
        [Header("Shooting Trajectory")]
        [SerializeField] private float _shootForce = 8f;
        [SerializeField] private float _upwardForce = 2f;

        private bool _ballShot;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _ballShot = false;
        }

        public override void Execute(NPCController npc)
        {
            if (npc.ball == null || _ballShot || npc.basketHoop == null)
            {
                return;
            }

            var ballController = npc.ball.GetComponent<BallController>();

            // Check if the NPC is currently holding the ball
            if (ballController != null && ballController.isHeld && ballController.holdPoint == npc.handTransform)
            {
                // 1. Look towards the hoop
                Vector3 lookAtHoop = npc.basketHoop.position;
                lookAtHoop.y = npc.transform.position.y; // Keep rotation on the Y axis
                npc.transform.LookAt(lookAtHoop);

                // 2. Calculate shooting velocity (inspired by ShootingController)
                Vector3 shootDirection = (npc.basketHoop.position - npc.handTransform.position).normalized;
                Vector3 shootVelocity = (shootDirection * _shootForce) + (Vector3.up * _upwardForce);

                // 3. Release the ball with the calculated velocity
                ballController.Release(shootVelocity);
                
                _ballShot = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            return _ballShot;
        }
    }
}

