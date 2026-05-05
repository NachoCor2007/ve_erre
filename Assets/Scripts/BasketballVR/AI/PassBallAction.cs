using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "PassBallAction", menuName = "BasketballVR/AI/PassBallAction")]
    public class PassBallAction : NPCAction
    {
        [SerializeField] private float _passForce = 10f;
        private bool _ballPassed;

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _ballPassed = false;
        }

        public override void Execute(NPCController npc)
        {
            if (npc.ball != null && !_ballPassed)
            {
                // Assuming the NPC "has" the ball if it's close enough
                if (Vector3.Distance(npc.transform.position, npc.ball.transform.position) < 2.0f) 
                {
                    Rigidbody ballRb = npc.ball.GetComponent<Rigidbody>();
                    if (ballRb != null)
                    {
                        Vector3 direction = (npc.playerTransform.position - npc.ball.transform.position).normalized;
                        ballRb.AddForce(direction * _passForce, ForceMode.Impulse);
                        _ballPassed = true;
                    }
                }
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            return _ballPassed;
        }
    }
}
