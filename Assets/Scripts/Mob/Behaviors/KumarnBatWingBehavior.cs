using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace MobSystem
{
    /// <summary>
    /// Kumarn Bat Wing behavior: Fast flying/hovering child ghost with pause before attack
    /// Similar to Vampire but with child voice and pause mechanic
    /// </summary>
    public class KumarnBatWingBehavior : MobBehavior
    {
        [Header("Kumarn Bat Wing Settings")]
        [SerializeField] private float hoverHeight = 1.5f;
        [SerializeField] private float hoverBobSpeed = 2f;
        [SerializeField] private float hoverBobAmount = 0.3f;
        [SerializeField] private GameObject batWingEffect;
        [SerializeField] private float pauseDistance = 3f;
        [SerializeField] private float pathUpdateRate = 0.2f;

        private bool isPaused = false;
        private bool hasPausedThisChase = false;
        private float hoverTime = 0f;
        private Vector3 originalPosition;
        private NavMeshAgent navAgent;
        private float pathUpdateTimer;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            
            navAgent = GetComponent<NavMeshAgent>();
            pathUpdateTimer = pathUpdateRate;
            originalPosition = transform.position;
        }

        public override void OnSpawned()
        {
            // Play child scream with bat wing sounds
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("KumarnBatSpawn", transform.position);
                MobAudioManager.instance.PlayAudio3D("BatWings", transform.position);
            }

            // Spawn bat wing effect
            if (batWingEffect != null)
            {
                Instantiate(batWingEffect, transform, false);
            }
        }

        public override void OnStartChasing()
        {
            hasPausedThisChase = false;
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Pause before attacking when close enough
            if (!hasPausedThisChase && !isPaused && distanceToPlayer <= pauseDistance)
            {
                StartCoroutine(PauseBeforeAttack());
            }
            // Note: Hovering movement is now handled in CustomMovement()
        }

        private IEnumerator PauseBeforeAttack()
        {
            isPaused = true;
            hasPausedThisChase = true;

            // Stop movement
            mobAI.SetPaused(true);

            // Play ominous sound during pause
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("KumarnStare", gameObject);
            }

            // Stare at player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

            // Wait for pause duration
            yield return new WaitForSeconds(mobData.pauseDuration);

            // Resume with aggressive sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("KumarnScream1", gameObject);
            }

            mobAI.SetPaused(false);
            isPaused = false;
        }

        public override void OnStartAttacking()
        {
            // Play combined child scream and bat screech
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("KumarnBatAttack", gameObject);
            }
        }

        public override bool CustomMovement()
        {
            // Use custom hovering movement
            if (player != null && navAgent != null)
            {
                // Update path to player periodically (unless paused)
                if (!isPaused)
                {
                    pathUpdateTimer -= Time.deltaTime;
                    if (pathUpdateTimer <= 0f)
                    {
                        navAgent.SetDestination(player.position);
                        pathUpdateTimer = pathUpdateRate;
                    }

                    // Update speed multiplier
                    navAgent.speed = mobData.moveSpeed * GetSpeedMultiplier();
                }

                // Hovering/flying behavior with bobbing
                hoverTime += Time.deltaTime * hoverBobSpeed;
                float bobOffset = Mathf.Sin(hoverTime) * hoverBobAmount;
                
                Vector3 pos = transform.position;
                pos.y = player.position.y + hoverHeight + bobOffset;
                transform.position = pos;

                return true;
            }

            return false;
        }

        public override float GetSpeedMultiplier()
        {
            // Fast like Kumarn
            return isPaused ? 0f : 1.3f;
        }

        protected override System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType>
            {
                TalismanObject.TalismanType.Thai,
                TalismanObject.TalismanType.Cross
            };
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleTalismanCollision(collision);
        }
    }
}

