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
        [SerializeField] private int zigzagSeed = 0; // 0 = random seed on spawn

        [Header("Sound Settings")]
        [SerializeField] private string[] screams = { "KumarnScream1", "KumarnScream2", "KumarnScream3" };
        [SerializeField] private string[] laughs = { "KumarnLaugh" };

        [Tooltip("Footsteps sounds when running")]
        [SerializeField] private string[] footstepsSounds = { "KumarnFootsteps1" };

        private float screamTimer;
        private Vector3 zigzagOffset;
        private float zigzagTime;
        private float pathUpdateTimer;
        private NavMeshAgent navAgent;
        private int currentZigzagCount;
        private float zigzagCycleTimer;
        private bool isZigzagging;
        
        // Seeded random values for unique zigzag patterns
        private float zigzagTimeOffset;
        private float zigzagFrequencyX;
        private float zigzagFrequencyZ;
        private float zigzagPhaseShift;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            screamTimer = screamInterval;
            navAgent = GetComponent<NavMeshAgent>();
            pathUpdateTimer = pathUpdateRate;
            
            // Initialize seeded random values for zigzag pattern
            InitializeZigzagSeed();
        }
        
        private void InitializeZigzagSeed()
        {
            // Use provided seed, or generate random one if seed is 0
            int seed = zigzagSeed;
            if (seed == 0)
            {
                seed = Random.Range(1, 100000);
            }
            
            // Initialize random with seed
            Random.State oldState = Random.state;
            Random.InitState(seed);
            
            // Generate unique zigzag parameters based on seed
            zigzagTimeOffset = Random.Range(0f, 100f);
            zigzagFrequencyX = Random.Range(0.8f, 1.5f);
            zigzagFrequencyZ = Random.Range(0.5f, 1.0f);
            zigzagPhaseShift = Random.Range(0f, Mathf.PI * 2f);
            
            // Restore previous random state
            Random.state = oldState;
            
            Debug.Log($"[KumarnBehavior] Initialized zigzag with seed {seed}: " +
                     $"timeOffset={zigzagTimeOffset:F2}, freqX={zigzagFrequencyX:F2}, " +
                     $"freqZ={zigzagFrequencyZ:F2}, phase={zigzagPhaseShift:F2}");
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
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D(footstepsSounds[Random.Range(0, footstepsSounds.Length)], transform.position);
            }
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
            if (MobAudioManager.instance != null)
            {
                string footstepSound = footstepsSounds[Random.Range(0, footstepsSounds.Length)];
                GameObject existingAudio = GameObject.Find($"TempAudio_{footstepSound}");
                
                if (existingAudio == null)
                {
                    MobAudioManager.instance.PlayAudio3D(footstepSound, transform.position);
                }
            }

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

                // Calculate zigzag offset using seeded random parameters
                zigzagTime += Time.deltaTime * 2f;
                float timeWithOffset = zigzagTime + zigzagTimeOffset;
                zigzagOffset = new Vector3(
                    Mathf.Sin(timeWithOffset * zigzagFrequencyX + zigzagPhaseShift) * zigzagIntensity,
                    0,
                    Mathf.Cos(timeWithOffset * zigzagFrequencyZ) * zigzagIntensity
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
        
        /// <summary>
        /// Set a specific seed for zigzag behavior (call before Initialize)
        /// </summary>
        public void SetZigzagSeed(int seed)
        {
            zigzagSeed = seed;
        }
    }
}

