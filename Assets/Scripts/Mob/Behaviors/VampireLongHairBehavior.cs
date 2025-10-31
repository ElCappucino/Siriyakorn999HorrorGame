using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace MobSystem
{
    /// <summary>
    /// Vampire Long Hair behavior: Bat that can optionally transform immediately to humanoid
    /// No need to get close to player - transforms right away after spawn (if enabled)
    /// Combines electricity and bat sounds
    /// Can be configured as pure bat form by disabling transformation
    /// </summary>
    public class VampireLongHairBehavior : MobBehavior
    {
        [Header("Vampire Long Hair Settings")]
        [Tooltip("Enable transformation from bat to humanoid form. Disable for pure bat mode.")]
        [SerializeField] private bool enableTransformation = true;
        [SerializeField] private GameObject batModel;
        [SerializeField] private GameObject humanoidModel;
        [SerializeField] private float flyHeight = 2.5f;
        [SerializeField] private float transformDelay = 1.5f;
        [SerializeField] private GameObject transformEffect;
        [SerializeField] private GameObject electricityEffect;
        [SerializeField] private float pathUpdateRate = 0.2f;

        private bool isInBatForm = true;
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
            // Play bat sound with electricity
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("BatWings", gameObject);
                MobAudioManager.instance.PlayAudio3DAttached("ElectricityAmbient", gameObject);
            }

            // Transform immediately after a short delay (only if transformation is enabled)
            if (enableTransformation)
            {
                StartCoroutine(TransformImmediately());
            }
        }

        private IEnumerator TransformImmediately()
        {
            // Brief bat flight
            yield return new WaitForSeconds(transformDelay);

            // Transform without needing to be close to player
            yield return StartCoroutine(TransformToHumanoid());
        }

        private IEnumerator TransformToHumanoid()
        {
            // Play transformation sound with electricity
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("VampireTransform", transform.position);
                MobAudioManager.instance.PlayAudio3D("ElectricityZap", transform.position);
            }

            // Spawn transformation effect
            if (transformEffect != null)
            {
                GameObject effect = Instantiate(transformEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Spawn electricity effect
            if (electricityEffect != null)
            {
                GameObject elecEffect = Instantiate(electricityEffect, transform.position, Quaternion.identity, transform);
                Destroy(elecEffect, 2f);
            }

            // Brief pause
            yield return new WaitForSeconds(0.5f);

            // Switch models
            if (batModel != null) batModel.SetActive(false);
            if (humanoidModel != null) humanoidModel.SetActive(true);
            isInBatForm = false;
            hasTransformed = true;

            // Play vampire roar
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("VampireRoar", gameObject);
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Note: Flying movement is now handled in CustomMovement()
        }

        public override void OnStartAttacking()
        {
            // Play electricity surge on attack
            PlayElectricityEffect(electricityEffect, 1f);
        }

        public override bool CustomMovement()
        {
            // Use custom flying movement only in bat form
            if (isInBatForm && player != null && navAgent != null)
            {
                // Update path to player periodically
                pathUpdateTimer -= Time.deltaTime;
                if (pathUpdateTimer <= 0f)
                {
                    navAgent.SetDestination(player.position);
                    pathUpdateTimer = pathUpdateRate;
                }

                // Update speed multiplier
                navAgent.speed = mobData.moveSpeed * GetSpeedMultiplier();

                // Manually adjust Y position for flying height
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, player.position.y + flyHeight, Time.deltaTime * 2f);
                transform.position = pos;

                return true;
            }

            // Use default movement when not in bat form
            return false;
        }

        public override float GetSpeedMultiplier()
        {
            // Normal speed in humanoid form
            return isInBatForm ? 1.2f : 1f;
        }

        protected override System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType>
            {
                TalismanObject.TalismanType.Lighting,
                TalismanObject.TalismanType.Cross
            };
        }

        protected override void OnTalismanCollected(TalismanObject.TalismanType talismanType)
        {
            // Play electricity effect for any talisman collected
            PlayElectricityEffect(electricityEffect, 1f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleTalismanCollision(collision);
        }
    }
}

