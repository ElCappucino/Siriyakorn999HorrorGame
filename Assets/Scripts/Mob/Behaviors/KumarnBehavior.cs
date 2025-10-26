using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Kumarn behavior: Fast running child ghost with screaming
    /// Action: running frantically, sometimes screaming
    /// </summary>
    public class KumarnBehavior : MobBehavior
    {
        [Header("Kumarn Settings")]
        [SerializeField] private float screamInterval = 5f;
        [SerializeField] private float screamChance = 0.3f;
        [SerializeField] private bool zigzagMovement = true;
        [SerializeField] private float zigzagIntensity = 2f;

        private float screamTimer;
        private Vector3 zigzagOffset;
        private float zigzagTime;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            screamTimer = screamInterval;
        }

        public override void OnSpawned()
        {
            // Play child ghost spawn sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("KumarnLaugh", transform.position);
            }
        }

        public override void OnStartChasing()
        {
            // Kumarn might scream when it starts chasing
            if (Random.value < 0.5f)
            {
                PlayScream();
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Random screaming while chasing
            screamTimer -= Time.deltaTime;
            if (screamTimer <= 0f)
            {
                if (Random.value < screamChance)
                {
                    PlayScream();
                }
                screamTimer = screamInterval;
            }

            // Erratic zigzag movement for more unsettling behavior
            if (zigzagMovement)
            {
                zigzagTime += Time.deltaTime * 2f;
                zigzagOffset = new Vector3(
                    Mathf.Sin(zigzagTime) * zigzagIntensity,
                    0,
                    Mathf.Cos(zigzagTime * 0.7f) * zigzagIntensity
                );
            }
        }

        private void PlayScream()
        {
            if (MobAudioManager.instance != null)
            {
                // Play random scream from Thai child voice set
                string[] screams = { "KumarnScream1", "KumarnScream2", "KumarnScream3" };
                string randomScream = screams[Random.Range(0, screams.Length)];
                MobAudioManager.instance.PlayAudio3D(randomScream, transform.position);
            }
        }

        public override bool CustomMovement()
        {
            // Apply zigzag offset if enabled
            if (zigzagMovement && player != null)
            {
                Vector3 targetPos = player.position + zigzagOffset;
                mobAI.SetCustomDestination(targetPos);
                return true;
            }
            return false;
        }

        public override float GetSpeedMultiplier()
        {
            // Kumarn is fast
            return 1.2f;
        }
    }
}

