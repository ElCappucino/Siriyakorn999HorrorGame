using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace MobSystem
{
    /// <summary>
    /// Vampire behavior: Bat form that can optionally transform to humanoid when close to player
    /// Creates jumpscare effect on transformation
    /// Can be configured as pure bat form by disabling transformation
    /// </summary>
    public class VampireBehavior : MobBehavior
    {
        [Header("Vampire Settings")]
        [Tooltip("Enable transformation from bat to humanoid form. Disable for pure bat mode.")]
        [SerializeField] private bool enableTransformation = true;
        [SerializeField] private GameObject batModel;
        [SerializeField] private GameObject humanoidModel;
        [SerializeField] private float flyHeight = 2f;
        [SerializeField] private float transformEffectDuration = 0.5f;
        [SerializeField] private GameObject transformEffect;
        [SerializeField] private float pathUpdateRate = 0.2f;

        private bool isInBatForm = true;
        private bool isTransforming = false;
        private bool hasTransformed = false;
        private NavMeshAgent navAgent;
        private float pathUpdateTimer;

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            
            navAgent = GetComponent<NavMeshAgent>();
            pathUpdateTimer = pathUpdateRate;
            
            // Start in bat form
            if (batModel != null) batModel.SetActive(true);
            if (humanoidModel != null) humanoidModel.SetActive(false);
        }

        public override void OnSpawned()
        {
            // Play bat wing flapping sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("BatWings", gameObject);
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Check if transformation is enabled and if close enough to transform
            if (enableTransformation && isInBatForm && !isTransforming && !hasTransformed)
            {
                if (distanceToPlayer <= mobData.transformationDistance)
                {
                    StartCoroutine(TransformToBat());
                }
            }
            // Note: Flying movement is now handled in CustomMovement()
        }

        private IEnumerator TransformToBat()
        {
            isTransforming = true;

            // Play transformation sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("VampireTransform", transform.position);
            }

            // Spawn transformation effect
            if (transformEffect != null)
            {
                GameObject effect = Instantiate(transformEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Brief pause (jumpscare moment)
            yield return new WaitForSeconds(transformEffectDuration);

            // Switch models
            if (batModel != null) batModel.SetActive(false);
            if (humanoidModel != null) humanoidModel.SetActive(true);
            isInBatForm = false;
            hasTransformed = true;

            // Trigger jumpscare if player is looking
            if (mobData.hasJumpscare)
            {
                TriggerJumpscare();
            }

            // Play vampire humanoid sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("VampireRoar", gameObject);
            }

            isTransforming = false;
        }

        private void TriggerJumpscare()
        {
            // TODO: Trigger jumpscare system
            Debug.Log("JUMPSCARE!");
        }

        public override bool CustomMovement()
        {
            // Use custom flying movement in bat form
            if (isInBatForm && player != null && navAgent != null)
            {
                // Update path to player periodically
                pathUpdateTimer -= Time.deltaTime;
                if (pathUpdateTimer <= 0f)
                {
                    // Set destination to player position (NavMesh will handle XZ movement)
                    navAgent.SetDestination(player.position);
                    pathUpdateTimer = pathUpdateRate;
                }

                // Update speed multiplier
                navAgent.speed = mobData.moveSpeed * GetSpeedMultiplier();

                // Manually adjust Y position for flying height
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, player.position.y + flyHeight, Time.deltaTime * 2f);
                transform.position = pos;

                // We're handling movement
                return true;
            }

            // Use default movement when not in bat form
            return false;
        }

        public override float GetSpeedMultiplier()
        {
            // Faster in bat form, normal in humanoid form
            return isInBatForm ? 1.3f : 1f;
        }

        protected override System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType>
            {
                TalismanObject.TalismanType.Cross
            };
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleTalismanCollision(collision);
        }
    }
}

