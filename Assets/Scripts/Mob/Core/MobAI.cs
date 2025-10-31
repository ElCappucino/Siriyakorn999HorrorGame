using UnityEngine;
using UnityEngine.AI;

namespace MobSystem
{
    /// <summary>
    /// AI controller for enemy mobs that track and move towards the player
    /// Supports different mob types with unique behaviors
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MobAI : MonoBehaviour
    {
        [Header("Mob Configuration")]
        [Tooltip("The mob data defining this mob's characteristics")]
        [SerializeField] private MobData mobData;

        [Header("Target Settings")]
        [Tooltip("The player transform to chase. If null, will find GameObject with 'Player' tag")]
        public Transform player;

        [Header("Movement Settings")]
        [Tooltip("How close the mob needs to be to the player before stopping")]
        [SerializeField] private float stoppingDistance = 2f;
        
        [Tooltip("How fast the mob moves (can be overridden by MobData)")]
        [SerializeField] private float moveSpeed = 3.5f;
        
        [Tooltip("How fast the mob rotates to face the player")]
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Detection Settings")]
        [Tooltip("Maximum distance the mob can detect the player")]
        [SerializeField] private float detectionRange = 20f;
        
        [Tooltip("How often to update the path (in seconds)")]
        [SerializeField] private float pathUpdateInterval = 0.5f;

        [Header("Audio Settings")]
        [Tooltip("Sound to play when the mob spots the player")]
        [SerializeField] private string detectionSoundName = "MobDetect";
        
        [Tooltip("Sound to play while chasing (looping footsteps, etc.)")]
        [SerializeField] private string chaseSoundName = "MobChase";
        
        [Tooltip("Sound to play when attacking")]
        [SerializeField] private string attackSoundName = "MobAttack";

        [Header("Attack Settings")]
        [Tooltip("How close the mob needs to be to attack")]
        [SerializeField] private float attackRange = 2f;
        
        [Tooltip("Time between attacks")]
        [SerializeField] private float attackCooldown = 2f;
        
        [Tooltip("Damage dealt per attack")]
        [SerializeField] private float attackDamage = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        // Private variables
        private NavMeshAgent navAgent;
        private float pathUpdateTimer;
        private bool hasDetectedPlayer = false;
        private float lastAttackTime;
        private MobState currentState = MobState.Idle;
        private MobBehavior behavior;
        private bool isPaused = false;
        private Vector3 customDestination;

        // Animation parameters (if you have an animator)
        private Animator animator;
        private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");

        public enum MobState
        {
            Idle,
            Chasing,
            Attacking
        }

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            behavior = GetComponent<MobBehavior>();
            
            // Apply mob data if available
            if (mobData != null)
            {
                ApplyMobData();
            }
            
            // Configure NavMeshAgent
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = stoppingDistance;
            navAgent.angularSpeed = rotationSpeed * 100f;
        }

        private void Start()
        {
            // Find player if not assigned
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning("MobAI: No player found! Make sure your player has the 'Player' tag.");
                }
            }

            pathUpdateTimer = pathUpdateInterval;

            // Initialize behavior
            if (behavior != null)
            {
                behavior.Initialize(this, mobData, player);
            }
        }

        /// <summary>
        /// Apply settings from MobData
        /// </summary>
        private void ApplyMobData()
        {
            moveSpeed = mobData.moveSpeed;
            rotationSpeed = mobData.rotationSpeed;
            detectionRange = mobData.detectionRange;
            attackRange = mobData.attackRange;
            attackCooldown = mobData.attackCooldown;
            attackDamage = mobData.attackDamage;
            
            // Apply audio settings
            detectionSoundName = mobData.detectionSoundName;
            chaseSoundName = mobData.chaseSoundName;
            attackSoundName = mobData.attackSoundName;
        }

        private void Update()
        {
            if (player == null || isPaused) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // State machine
            switch (currentState)
            {
                case MobState.Idle:
                    HandleIdleState(distanceToPlayer);
                    break;
                case MobState.Chasing:
                    HandleChasingState(distanceToPlayer);
                    break;
                case MobState.Attacking:
                    HandleAttackingState(distanceToPlayer);
                    break;
            }

            // Update animations
            UpdateAnimations();
        }

        private void HandleIdleState(float distanceToPlayer)
        {
            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnIdleUpdate(distanceToPlayer);
            }

            // Check if player is in detection range
            if (distanceToPlayer <= detectionRange)
            {
                // Player detected!
                hasDetectedPlayer = true;
                currentState = MobState.Chasing;
                
                // Play 3D detection sound from enemy position
                if (MobAudioManager.instance != null && !string.IsNullOrEmpty(detectionSoundName))
                {
                    MobAudioManager.instance.PlayAudio3DAttached(detectionSoundName, gameObject);
                }

                // Call behavior hook
                if (behavior != null)
                {
                    behavior.OnStartChasing();
                }
            }
        }

        private void HandleChasingState(float distanceToPlayer)
        {
            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnChasingUpdate(distanceToPlayer);

                // Check if behavior wants custom movement
                if (behavior.CustomMovement())
                {
                    // Behavior handles movement
                    // Still update speed multiplier
                    navAgent.speed = moveSpeed * behavior.GetSpeedMultiplier();
                }
                else
                {
                    // Default NavMesh movement
                    UpdateDefaultMovement(distanceToPlayer);
                }
            }
            else
            {
                // Default NavMesh movement
                UpdateDefaultMovement(distanceToPlayer);
            }

            // Check if close enough to attack
            if (distanceToPlayer <= attackRange)
            {
                currentState = MobState.Attacking;
                navAgent.isStopped = true;

                // Call behavior hook
                if (behavior != null)
                {
                    behavior.OnStartAttacking();
                }
            }
            // Check if player escaped detection range
            else if (distanceToPlayer > detectionRange)
            {
                currentState = MobState.Idle;
                navAgent.isStopped = true;
            }
        }

        private void UpdateDefaultMovement(float distanceToPlayer)
        {
            // Update path to player periodically
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0f)
            {
                Vector3 destination = customDestination != Vector3.zero ? customDestination : player.position;

                if (navAgent != null && navAgent.isOnNavMesh)
                {
                    navAgent.SetDestination(destination);
                }
                else
                {
                    // Retry next frame so agent has time to land on NavMesh
                    StartCoroutine(WaitAndSetDestination(destination));
                }

                pathUpdateTimer = pathUpdateInterval;
                customDestination = Vector3.zero;
            }


            // Apply speed multiplier from behavior
            if (behavior != null)
            {
                navAgent.speed = moveSpeed * behavior.GetSpeedMultiplier();
            }

        }

        private System.Collections.IEnumerator WaitAndSetDestination(Vector3 destination)
        {
            yield return null; // wait 1 frame
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(destination);
            }
        }


        private void HandleAttackingState(float distanceToPlayer)
        {
            Debug.Log("HandleAttackingState. name = " + gameObject.name);
            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnAttackingUpdate(distanceToPlayer);
            }

            // Stop moving and face the player
            navAgent.isStopped = true;
            FaceTarget(player.position);

            // Attack if cooldown is ready
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }

            // If player moved away, go back to chasing
            if (distanceToPlayer > attackRange)
            {
                currentState = MobState.Chasing;
                navAgent.isStopped = false;
            }
        }

        private void Attack()
        {
            // Check if behavior wants to handle attack
            bool useDefaultAttack = true;
            if (behavior != null)
            {
                useDefaultAttack = behavior.OnAttack();
            }

            if (useDefaultAttack)
            {
                // Play attack animation
                if (animator != null)
                {
                    animator.SetTrigger(isAttackingHash);
                }

                // Play 3D attack sound from enemy position
                if (MobAudioManager.instance != null && !string.IsNullOrEmpty(attackSoundName))
                {
                    MobAudioManager.instance.PlayAudio3DAttached(attackSoundName, gameObject);
                }

                // Deal damage to player
                /*PlayerSystem.PlayerHealth playerHealth = player.GetComponent<PlayerSystem.PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"Mob attacked player for {attackDamage} damage!");
                }
                else
                {
                    Debug.LogWarning("Player doesn't have PlayerHealth component!");
                }*/

                PlayerManager playerManager = GameplayManager.Instance.GetPlayerManager();
                if (playerManager == null)
                {
                    Debug.LogWarning("Player doesn't have PlayerHealth component!");
                }
                else
                {
                    playerManager.DecreaseHealth();
                }


            }

            // Call behavior hook after attack
            if (behavior != null)
            {
                behavior.OnAttackComplete();
            }
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep rotation only on Y axis
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            // Update walking animation
            bool isWalking = currentState == MobState.Chasing && navAgent.velocity.magnitude > 0.1f;
            animator.SetBool(isWalkingHash, isWalking);
        }

        /// <summary>
        /// Call this from MobSpawner after the mob spawns
        /// </summary>
        public void OnSpawned()
        {
            currentState = MobState.Idle;
            hasDetectedPlayer = false;
            navAgent.isStopped = false;

            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnSpawned();
            }
        }

        /// <summary>
        /// Force the mob to detect and chase the player immediately
        /// </summary>
        public void ForceDetectPlayer()
        {
            if (player != null)
            {
                hasDetectedPlayer = true;
                currentState = MobState.Chasing;
                
                // Play 3D detection sound from enemy position
                if (MobAudioManager.instance != null && !string.IsNullOrEmpty(detectionSoundName))
                {
                    MobAudioManager.instance.PlayAudio3DAttached(detectionSoundName, gameObject);
                }

                // Call behavior hook
                if (behavior != null)
                {
                    behavior.OnStartChasing();
                }
            }
        }

        /// <summary>
        /// Set a custom destination for the next movement update
        /// Used by behaviors that need custom movement patterns
        /// </summary>
        public void SetCustomDestination(Vector3 destination)
        {
            customDestination = destination;
        }

        /// <summary>
        /// Pause or unpause the mob's AI
        /// Used by behaviors that need to pause (e.g., Kumarn Bat Wing pause before attack)
        /// </summary>
        public void SetPaused(bool paused)
        {
            isPaused = paused;

            if (navAgent == null) return;

            // If agent is not yet on NavMesh, retry next frame
            if (!navAgent.isOnNavMesh)
            {
                StartCoroutine(WaitAndPause(paused));
                return;
            }

            navAgent.isStopped = paused;
        }

        private System.Collections.IEnumerator WaitAndPause(bool paused)
        {
            // wait one frame for agent to get placed onto navmesh
            yield return null;

            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = paused;
            }
        }


        /// <summary>
        /// Get the current state of the mob
        /// </summary>
        public MobState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Get the mob data
        /// </summary>
        public MobData GetMobData()
        {
            return mobData;
        }

        /// <summary>
        /// Set the mob data (useful for runtime spawning)
        /// </summary>
        public void SetMobData(MobData data)
        {
            mobData = data;
            if (mobData != null)
            {
                ApplyMobData();
            }
        }

        /// <summary>
        /// Set the move speed dynamically (for phase-based scaling)
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
            if (navAgent != null)
            {
                navAgent.speed = speed;
            }
        }

        /// <summary>
        /// Get the current move speed
        /// </summary>
        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        /// <summary>
        /// Immediately destroy this mob (disables all components first for instant effect)
        /// </summary>
        public void DestroyMob()
        {
            // Disable AI to stop all updates immediately
            this.enabled = false;
            
            // Disable NavMeshAgent to stop movement immediately
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            // Disable animator to stop animations
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            // Disable behavior script
            if (behavior != null)
            {
                behavior.enabled = false;
            }
            
            // Mark as destroyed and destroy the GameObject
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Stopping distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);

            // Line to player
            if (player != null)
            {
                Gizmos.color = currentState == MobState.Chasing ? Color.red : Color.gray;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}

