using UnityEngine;
using UnityEngine.AI;

namespace MobSystem
{
    /// <summary>
    /// AI controller for enemy mobs that track and move towards the player
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MobAI : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The player transform to chase. If null, will find GameObject with 'Player' tag")]
        public Transform player;

        [Header("Movement Settings")]
        [Tooltip("How close the mob needs to be to the player before stopping")]
        [SerializeField] private float stoppingDistance = 2f;
        
        [Tooltip("How fast the mob moves")]
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
        }

        private void Update()
        {
            if (player == null) return;

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
            // Check if player is in detection range
            if (distanceToPlayer <= detectionRange)
            {
                // Player detected!
                hasDetectedPlayer = true;
                currentState = MobState.Chasing;
                
                // Play 3D detection sound from enemy position
                if (MobAudioManager.instance != null && !string.IsNullOrEmpty(detectionSoundName))
                {
                    MobAudioManager.instance.PlayAudio3D(detectionSoundName, transform.position);
                }
            }
        }

        private void HandleChasingState(float distanceToPlayer)
        {
            // Update path to player periodically
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0f)
            {
                navAgent.SetDestination(player.position);
                pathUpdateTimer = pathUpdateInterval;
            }

            // Check if close enough to attack
            if (distanceToPlayer <= attackRange)
            {
                currentState = MobState.Attacking;
                navAgent.isStopped = true;
            }
            // Check if player escaped detection range
            else if (distanceToPlayer > detectionRange)
            {
                currentState = MobState.Idle;
                navAgent.isStopped = true;
            }
        }

        private void HandleAttackingState(float distanceToPlayer)
        {
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
            // Play attack animation
            if (animator != null)
            {
                animator.SetTrigger(isAttackingHash);
            }

            // Play 3D attack sound from enemy position
            if (MobAudioManager.instance != null && !string.IsNullOrEmpty(attackSoundName))
            {
                MobAudioManager.instance.PlayAudio3D(attackSoundName, transform.position);
            }

            // Deal damage to player
            PlayerSystem.PlayerHealth playerHealth = player.GetComponent<PlayerSystem.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Mob attacked player for {attackDamage} damage!");
            }
            else
            {
                Debug.LogWarning("Player doesn't have PlayerHealth component!");
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
                    MobAudioManager.instance.PlayAudio3D(detectionSoundName, transform.position);
                }
            }
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

