using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Controls mob animations based on state and events
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class MobAnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [Tooltip("Name of the walking bool parameter in animator")]
        [SerializeField] private string walkingParameter = "IsWalking";
        
        [Tooltip("Name of the attack trigger parameter in animator")]
        [SerializeField] private string attackParameter = "Attack";
        
        [Tooltip("Name of the death trigger parameter in animator")]
        [SerializeField] private string deathParameter = "Death";
        
        [Tooltip("Name of the spawn trigger parameter in animator")]
        [SerializeField] private string spawnParameter = "Spawn";
        
        [Tooltip("Name of the hurt trigger parameter in animator")]
        [SerializeField] private string hurtParameter = "Hurt";

        [Header("Speed Settings")]
        [Tooltip("Update animation speed based on movement speed")]
        [SerializeField] private bool useAnimationSpeed = true;
        
        [Tooltip("Animation speed multiplier")]
        [SerializeField] private float speedMultiplier = 1f;

        [Header("Look At Player")]
        [Tooltip("Should mob's head look at player?")]
        [SerializeField] private bool enableHeadTracking = false;
        
        [Tooltip("Bone to use for head tracking")]
        [SerializeField] private Transform headBone;
        
        [Tooltip("Weight of head tracking (0-1)")]
        [SerializeField] private float headTrackingWeight = 0.5f;

        // Private variables
        private Animator animator;
        private MobAI mobAI;
        private Transform player;
        private int walkingHash;
        private int attackHash;
        private int deathHash;
        private int spawnHash;
        private int hurtHash;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            mobAI = GetComponent<MobAI>();

            // Convert parameter names to hashes for better performance
            walkingHash = Animator.StringToHash(walkingParameter);
            attackHash = Animator.StringToHash(attackParameter);
            deathHash = Animator.StringToHash(deathParameter);
            spawnHash = Animator.StringToHash(spawnParameter);
            hurtHash = Animator.StringToHash(hurtParameter);
        }

        private void Start()
        {
            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // Play spawn animation
            PlaySpawnAnimation();
        }

        private void Update()
        {
            if (animator == null) return;

            UpdateWalkingAnimation();
            UpdateAnimationSpeed();
        }

        private void UpdateWalkingAnimation()
        {
            if (mobAI == null) return;

            // Get velocity magnitude from the mob's movement
            float speed = GetComponent<UnityEngine.AI.NavMeshAgent>()?.velocity.magnitude ?? 0f;
            bool isWalking = speed > 0.1f;

            animator.SetBool(walkingHash, isWalking);
        }

        private void UpdateAnimationSpeed()
        {
            if (!useAnimationSpeed || mobAI == null) return;

            float speed = GetComponent<UnityEngine.AI.NavMeshAgent>()?.velocity.magnitude ?? 0f;
            animator.speed = Mathf.Lerp(1f, speedMultiplier, speed / 5f);
        }

        /// <summary>
        /// Play spawn animation
        /// </summary>
        public void PlaySpawnAnimation()
        {
            animator?.SetTrigger(spawnHash);
        }

        /// <summary>
        /// Play attack animation
        /// </summary>
        public void PlayAttackAnimation()
        {
            animator?.SetTrigger(attackHash);
        }

        /// <summary>
        /// Play death animation
        /// </summary>
        public void PlayDeathAnimation()
        {
            animator?.SetTrigger(deathHash);
        }

        /// <summary>
        /// Play hurt animation
        /// </summary>
        public void PlayHurtAnimation()
        {
            animator?.SetTrigger(hurtHash);
        }

        /// <summary>
        /// Set walking state manually
        /// </summary>
        public void SetWalking(bool walking)
        {
            animator?.SetBool(walkingHash, walking);
        }

        /// <summary>
        /// Get the animator component
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }

        // IK for head tracking (optional - requires humanoid rig)
        private void OnAnimatorIK(int layerIndex)
        {
            if (!enableHeadTracking || player == null || animator == null)
                return;

            // Look at player
            animator.SetLookAtWeight(headTrackingWeight);
            animator.SetLookAtPosition(player.position + Vector3.up * 1.5f); // Eye level
        }

        /// <summary>
        /// Enable or disable head tracking at runtime
        /// </summary>
        public void SetHeadTracking(bool enabled)
        {
            enableHeadTracking = enabled;
        }

        /// <summary>
        /// Set animation speed multiplier
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
            }
        }
    }
}

