using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Smol Sadako behavior: Smaller, faster version of Sadako
    /// Combines electricity effects with child voice
    /// </summary>
    public class SmolSadakoBehavior : MobBehavior
    {
        [Header("Smol Sadako Settings")]
        [SerializeField] private float crawlAnimationSpeed = 1.2f;
        [SerializeField] private GameObject electricityEffect;
        [SerializeField] private float electricityInterval = 2.5f;
        [SerializeField] private bool playChildGiggle = true;
        [SerializeField] private float giggleInterval = 4f;

        private float electricityTimer;
        private float giggleTimer;
        private readonly int crawlSpeedHash = Animator.StringToHash("CrawlSpeed");

        public override void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            base.Initialize(ai, data, playerTransform);
            electricityTimer = electricityInterval;
            giggleTimer = giggleInterval;
        }

        public override void OnSpawned()
        {
            // Play creepy child giggle on spawn
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3D("SmolSadakoGiggle", transform.position);
            }
        }

        public override void OnStartChasing()
        {
            // Play electricity zap when starting chase
            PlayElectricityEffect();
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Update crawl animation speed (faster than regular Sadako)
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

            // Periodically play child giggle
            if (playChildGiggle)
            {
                giggleTimer -= Time.deltaTime;
                if (giggleTimer <= 0f)
                {
                    PlayGiggle();
                    giggleTimer = giggleInterval;
                }
            }
        }

        public override void OnStartAttacking()
        {
            // Play both electricity and giggle when attacking
            PlayElectricityEffect();
            PlayGiggle();
        }

        private void PlayElectricityEffect()
        {
            PlayElectricityEffect(electricityEffect, 1f);
        }

        private void PlayGiggle()
        {
            string[] giggles = { "ChildGiggle1", "ChildGiggle2", "ChildGiggle3" };
            PlayRandomSound(giggles, attached: true);
        }

        public override float GetSpeedMultiplier()
        {
            // Faster than regular Sadako but not as fast as Kumarn
            return 1.1f;
        }

        protected override System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType>
            {
                TalismanObject.TalismanType.Lighting,
                TalismanObject.TalismanType.Thai
            };
        }

        protected override void OnTalismanCollected(TalismanObject.TalismanType talismanType)
        {
            // Play effects based on which talisman was collected
            if (talismanType == TalismanObject.TalismanType.Lighting)
            {
                PlayElectricityEffect();
            }
            else if (talismanType == TalismanObject.TalismanType.Thai)
            {
                PlayGiggle();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleTalismanCollision(collision);
        }
    }
}

