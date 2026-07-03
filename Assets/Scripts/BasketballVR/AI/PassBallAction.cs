using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "PassBallAction", menuName = "BasketballVR/AI/PassBallAction")]
    public class PassBallAction : NPCAction
    {
        [Header("Pass Settings")]
        [Tooltip("Horizontal speed for the chest pass, and the fallback speed if a bounce pass is calculated to be too fast.")]
        [SerializeField] private float _passForce = 6.0f;
        [Tooltip("Maximum allowed horizontal speed for a bounce pass to prevent bullet passes.")]
        [SerializeField] private float _maxPassSpeed = 8.0f;
        [Tooltip("Minimum distance at which the dummy will choose to perform a bounce pass. Below this distance, it will perform a direct chest pass.")]
        [SerializeField] private float _bouncePassDistanceThreshold = 4.0f;
        [Tooltip("Vertical distance down from the main camera (headset) to target the player's chest/thorax region (lower than head).")]
        [SerializeField] private float _playerChestOffsetFromHead = 0.5f;
        [Tooltip("Vertical height from the floor to target the player's chest (fallback if camera is null).")]
        [SerializeField] private float _playerChestOffsetFromFloor = 1.1f;

        private bool _ballPassed;

        public override string Description
        {
            get => !string.IsNullOrEmpty(_description) ? _description : "Recibir pelota de compañero";
        }

        public override void ResetState()
        {
            base.ResetState();
            _ballPassed = false;
        }

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _ballPassed = false;

            if (npc.Animator != null)
            {
                npc.Animator.SetBool("isWaiting", true);
                npc.Animator.SetBool("isRunning", false);
            }
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
                // Target the player's center of mass (chest/thorax)
                Vector3 targetPosition;
                if (Camera.main != null)
                {
                    targetPosition = Camera.main.transform.position + Vector3.down * _playerChestOffsetFromHead;
                }
                else if (npc.playerTransform != null)
                {
                    targetPosition = npc.playerTransform.position + Vector3.up * _playerChestOffsetFromFloor;
                }
                else
                {
                    targetPosition = npc.transform.position + npc.transform.forward * 3f + Vector3.up * _playerChestOffsetFromFloor;
                }

                Vector3 startPosition = npc.handTransform.position;
                Vector3 passDirection = (targetPosition - startPosition).normalized;
                
                // Calculate distance on the horizontal plane
                Vector3 startPosXZ = new Vector3(startPosition.x, 0f, startPosition.z);
                Vector3 targetPosXZ = new Vector3(targetPosition.x, 0f, targetPosition.z);
                float distanceXZ = Vector3.Distance(startPosXZ, targetPosXZ);

                Vector3 passVelocity = Vector3.zero;
                float gravityY = Physics.gravity.y;

                // Dynamically decide pass type based on distance
                if (distanceXZ >= _bouncePassDistanceThreshold)
                {
                    float yFloor = GetFloorHeight(startPosition);
                    
                    float eta = 0.85f; // Horizontal velocity retention coefficient after bounce
                    float e = 0.75f;   // Vertical coefficient of restitution (bounciness) of the ball
                    
                    // Calculate minimum time ratio r = t2/t1 needed to reach target height T.y
                    float rMin = (targetPosition.y - yFloor) / (e * Mathf.Max(startPosition.y - yFloor, 0.1f));
                    float maxF = 1f / (1f + eta * rMin);
                    
                    // Choose f (fraction of the distance where bounce occurs) to be safe (well below maxF to keep arc gentle)
                    float f = Mathf.Min(0.35f, maxF * 0.65f);
                    f = Mathf.Max(f, 0.1f); // clamp lower bound to prevent divide-by-zero
                    
                    float r = (1f - f) / (eta * f);
                    
                    float numerator = (targetPosition.y - yFloor) + e * r * (yFloor - startPosition.y);
                    float denominator = 0.5f * gravityY * (r * r + e * r);
                    
                    // Verify the pass is physically solvable and calculated speed is within safe limits
                    bool success = false;
                    if (numerator < 0f && denominator < 0f)
                    {
                        float t1 = Mathf.Sqrt(numerator / denominator);
                        float d1 = f * distanceXZ;
                        float vxz = d1 / Mathf.Max(t1, 0.01f);
                        
                        // Only proceed with the bounce pass if it is within our speed limit
                        if (vxz <= _maxPassSpeed)
                        {
                            float vy = (yFloor - startPosition.y - 0.5f * gravityY * t1 * t1) / Mathf.Max(t1, 0.01f);
                            Vector3 horizontalDirection = (targetPosXZ - startPosXZ).normalized;
                            passVelocity = horizontalDirection * vxz + Vector3.up * vy;
                            success = true;
                        }
                    }
                    
                    if (!success)
                    {
                        // Fallback to chest pass if the bounce pass is too fast or not solvable
                        float t = distanceXZ / Mathf.Max(_passForce, 0.1f);
                        Vector3 horizontalDirection = (targetPosXZ - startPosXZ).normalized;
                        passVelocity = horizontalDirection * _passForce;
                        passVelocity.y = (targetPosition.y - startPosition.y - 0.5f * gravityY * t * t) / Mathf.Max(t, 0.01f);
                    }
                }
                else
                {
                    // Chest Pass: Calculate perfect projectile trajectory to reach target's chest directly
                    float t = distanceXZ / Mathf.Max(_passForce, 0.1f);
                    Vector3 horizontalDirection = (targetPosXZ - startPosXZ).normalized;
                    passVelocity = horizontalDirection * _passForce;
                    passVelocity.y = (targetPosition.y - startPosition.y - 0.5f * gravityY * t * t) / Mathf.Max(t, 0.01f);
                }

                // Move ball slightly forward along the pass direction to prevent colliding with throwing NPC
                ballController.transform.position += passDirection * 0.2f;

                ballController.Release(passVelocity);
                
                // NOTIFY that the ball was released to apply cooldown to the NPC's CatchTrigger
                var catchTrigger = npc.GetComponentInChildren<NPCCatchTrigger>();
                if (catchTrigger != null)
                {
                    catchTrigger.NotifyBallReleased();
                }

                _ballPassed = true;
                IsActionSuccessful = true;
            }
        }

        private float GetFloorHeight(Vector3 startPos)
        {
            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 10f))
            {
                return hit.point.y;
            }
            return 0f; // Default floor level
        }

        public override bool IsFinished(NPCController npc)
        {
            if (_ballPassed)
            {
                if (npc.Animator != null)
                {
                    npc.Animator.SetBool("isWaiting", false);
                }
                return true;
            }
            return false;
        }
    }
}
