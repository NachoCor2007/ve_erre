using UnityEngine;

namespace BasketballVR.AI
{
    [CreateAssetMenu(fileName = "ReceiveBallAction", menuName = "BasketballVR/AI/ReceiveBallAction")]
    public class ReceiveBallAction : NPCAction
    {
        [SerializeField] private AudioClip _triggerSound;
        [SerializeField] private float _soundVolume = 1.0f;
        [SerializeField] private float _soundMinDistance = 5.0f;
        [SerializeField] private float _soundMaxDistance = 100.0f;

        private bool _hasReceivedBall = false;

        public override string Description
        {
            get => !string.IsNullOrEmpty(_description) ? _description : "Pasar pelota a compañero";
        }

        public override void ResetState()
        {
            base.ResetState();
            _hasReceivedBall = false;
        }

        public override void Initialize(NPCController npc)
        {
            base.Initialize(npc);
            _hasReceivedBall = false;
            npc.NavMeshAgent.isStopped = true; // The NPC waits in place

            if (npc.Animator != null)
            {
                npc.Animator.SetBool("isWaiting", true);
                npc.Animator.SetBool("isRunning", false);
            }

            var glowController = npc.GetComponentInChildren<EmissiveGlowController>();
            if (glowController != null)
            {
                glowController.StartGlow();
            }

            if (_triggerSound != null)
            {
                PlaySoundAtPoint(_triggerSound, npc.transform.position, _soundMinDistance, _soundMaxDistance, _soundVolume);
            }
        }

        public override void Execute(NPCController npc)
        {
            if (_hasReceivedBall || npc.ball == null)
            {
                return;
            }

            // The actual catch logic is now handled by NPCCatchTrigger.
            var ballController = npc.ball.GetComponent<BallController>();
            if (ballController != null && ballController.isHeld && ballController.holdPoint == npc.handTransform)
            {
                _hasReceivedBall = true;
                IsActionSuccessful = true;
            }
        }

        public override bool IsFinished(NPCController npc)
        {
            // The action is finished once the ball has been received.
            if (_hasReceivedBall)
            {
                npc.NavMeshAgent.isStopped = false; // Allow movement for the next action
                if (npc.Animator != null)
                {
                    npc.Animator.SetBool("isWaiting", false);
                }

                var glowController = npc.GetComponentInChildren<EmissiveGlowController>();
                if (glowController != null)
                {
                    glowController.StopGlow();
                }

                return true;
            }
            return false;
        }

        private void PlaySoundAtPoint(AudioClip clip, Vector3 position, float minDistance, float maxDistance, float volume)
        {
            if (clip == null) return;
            GameObject go = new GameObject("TempAudio_" + clip.name);
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f; // 3D sound
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.volume = volume;
            source.Play();
            Destroy(go, clip.length);
        }
    }
}
