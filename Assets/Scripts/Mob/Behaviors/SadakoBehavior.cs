using System.Collections;
using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Sadako behavior: Slow crawling ghost with electricity effects
    /// </summary>
    public class SadakoBehavior : MobBehavior
    {
        [Header("Sadako Settings")]
        [SerializeField] private float crawlAnimationSpeed = 0.8f;
        [SerializeField] private GameObject electricityEffect;
        [SerializeField] private float electricityInterval = 3f;

        private float electricityTimer;
        private readonly int crawlSpeedHash = Animator.StringToHash("CrawlSpeed");

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            electricityTimer = electricityInterval;
        }

        public override void OnSpawned()
        {
            // Play eerie crawling spawn sound
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("SadakoSpawn", transform.position);
            }
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Update crawl animation speed
            if (animator != null)
            {
                animator.SetFloat(crawlSpeedHash, crawlAnimationSpeed);
            }

            // Periodically play electricity effect
            electricityTimer -= Time.deltaTime;
            if (electricityTimer <= 0f)
            {
                PlayElectricityEffect();
                electricityTimer = electricityInterval;
            }
        }

        private void PlayElectricityEffect()
        {
            PlayElectricityEffect(electricityEffect, 1f);
        }

        protected override System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType>
            {
                TalismanObject.TalismanType.Lighting
            };
        }

        protected override void OnTalismanCollected(TalismanObject.TalismanType talismanType)
        {
            // Play electricity effect when hit by lightning talisman
            PlayElectricityEffect();
        }

        public override void OnTakeDamage(float damage)
        {
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleTalismanCollision(collision);
        }
        
        public override float GetSpeedMultiplier()
        {
            // Sadako is always slow
            return 1f;
        }

        public override void OnStartAttacking()
        {
            // Play electricity surge when starting attack
            PlayElectricityEffect();
        }

        // Note: OnAttack and OnAttackComplete use base class default behavior
        // (allows default attack and destroys mob after attack)
    }
}

