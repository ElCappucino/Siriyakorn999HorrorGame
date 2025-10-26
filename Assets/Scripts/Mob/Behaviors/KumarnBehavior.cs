using UnityEngine;
using UnityEngine.AI;

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
        
        [Header("Zigzag Movement")]
        [SerializeField] private bool zigzagMovement = true;
        [SerializeField] private float zigzagIntensity = 2f;
        [SerializeField] private float pathUpdateRate = 0.1f;
        [SerializeField] private int maxZigzagTimes = 5;
        [SerializeField] private float zigzagCycleDuration = 2f;

        private float screamTimer;
        private Vector3 zigzagOffset;
        private float zigzagTime;
        private float pathUpdateTimer;
        private NavMeshAgent navAgent;
        private int currentZigzagCount;
        private float zigzagCycleTimer;
        private bool isZigzagging;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            screamTimer = screamInterval;
            navAgent = GetComponent<NavMeshAgent>();
            pathUpdateTimer = pathUpdateRate;
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
            // Reset zigzag count for new chase
            currentZigzagCount = 0;
            zigzagCycleTimer = zigzagCycleDuration;
            isZigzagging = zigzagMovement && maxZigzagTimes > 0;
            
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
            if (isZigzagging)
            {
                // Track zigzag cycles
                zigzagCycleTimer -= Time.deltaTime;
                if (zigzagCycleTimer <= 0f)
                {
                    currentZigzagCount++;
                    zigzagCycleTimer = zigzagCycleDuration;
                    
                    // Check if reached max zigzags
                    if (currentZigzagCount >= maxZigzagTimes)
                    {
                        isZigzagging = false;
                        
                        // Play a final scream when stopping zigzag
                        if (MobAudioManager.instance != null)
                        {
                            string[] screams = { "KumarnScream1", "KumarnScream2", "KumarnScream3" };
                            string randomScream = screams[Random.Range(0, screams.Length)];
                            MobAudioManager.instance.PlayAudio3D(randomScream, transform.position);
                        }
                    }
                }
                
                // Calculate zigzag offset
                zigzagTime += Time.deltaTime * 2f;
                zigzagOffset = new Vector3(
                    Mathf.Sin(zigzagTime) * zigzagIntensity,
                    0,
                    Mathf.Cos(zigzagTime * 0.7f) * zigzagIntensity
                );
            }
            else
            {
                // No zigzag offset when not zigzagging
                zigzagOffset = Vector3.zero;
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
            // Apply zigzag offset if currently zigzagging
            if (isZigzagging && player != null && navAgent != null)
            {
                // Update path frequently for smooth zigzag
                pathUpdateTimer -= Time.deltaTime;
                if (pathUpdateTimer <= 0f)
                {
                    Vector3 targetPos = player.position + zigzagOffset;
                    navAgent.SetDestination(targetPos);
                    pathUpdateTimer = pathUpdateRate;
                }
                
                // Update speed multiplier
                navAgent.speed = mobData.moveSpeed * GetSpeedMultiplier();
                
                // We're handling movement, so return true
                return true;
            }
            
            // Use default movement if zigzag is disabled or exhausted
            return false;
        }

        public override float GetSpeedMultiplier()
        {
            // Kumarn is fast, even faster when done zigzagging
            return isZigzagging ? 1.2f : 1.4f;
        }
    }
}

