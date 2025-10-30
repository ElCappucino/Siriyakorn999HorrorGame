using UnityEngine;
using System.Collections;

namespace MobSystem
{
    /// <summary>
    /// Abstract base class for mob-specific behaviors
    /// Inherit from this to create unique behaviors for each mob type
    /// </summary>
    public abstract class MobBehavior : MonoBehaviour
    {
        protected MobAI mobAI;
        protected MobData mobData;
        protected Transform player;
        protected Animator animator;
        protected BoxCollider boxCollider;

        [Header("Talisman Indicator")]
        [Tooltip("Indicator settings (position, spacing, etc.)")]
        [SerializeField] protected IndicatorSettings indicatorSettings;
        
        [Tooltip("Prefab for question mark indicator")]
        [SerializeField] protected GameObject questionMarkIndicator;
        [Tooltip("Prefab for Thai talisman indicator")]
        [SerializeField] protected GameObject thaiTalismanIndicator;
        [Tooltip("Prefab for Electric talisman indicator")]
        [SerializeField] protected GameObject electricTalismanIndicator;
        [Tooltip("Prefab for Cross talisman indicator")]
        [SerializeField] protected GameObject crossTalismanIndicator;

        // Track collected talismans
        protected System.Collections.Generic.List<TalismanObject.TalismanType> collectedTalismans = new System.Collections.Generic.List<TalismanObject.TalismanType>();
        protected System.Collections.Generic.List<GameObject> indicatorObjects = new System.Collections.Generic.List<GameObject>();

        public virtual void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            mobAI = ai;
            mobData = data;
            player = playerTransform;
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider>();

            // Initialize indicators after a frame to ensure proper setup
            StartCoroutine(InitializeIndicatorsDelayed());
        }

        private IEnumerator InitializeIndicatorsDelayed()
        {
            yield return null; // Wait one frame
            InitializeTalismanIndicators();
        }

        #region Common Utility Methods

        /// <summary>
        /// Play a random sound from an array of sound names
        /// </summary>
        protected void PlayRandomSound(string[] soundNames, bool attached = true)
        {
            if (soundNames == null || soundNames.Length == 0 || MobAudioManager.instance == null)
                return;

            string randomSound = soundNames[Random.Range(0, soundNames.Length)];
            if (attached)
                MobAudioManager.instance.PlayAudio3DAttached(randomSound, gameObject);
            else
                MobAudioManager.instance.PlayAudio3D(randomSound, transform.position);
        }

        /// <summary>
        /// Play electricity visual and audio effect
        /// </summary>
        protected void PlayElectricityEffect(GameObject effectPrefab, float duration = 1f)
        {
            // Visual effect
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, duration);
            }

            // Audio effect
            if (MobAudioManager.instance != null)
            {
                MobAudioManager.instance.PlayAudio3DAttached("ElectricityZap", gameObject);
            }
        }

        /// <summary>
        /// Handle talisman collision (new key-based system)
        /// Collects required talismans instead of dealing damage
        /// </summary>
        protected void HandleTalismanCollision(Collision collision)
        {
            // Try to find TalismanObject component
            TalismanObject talisman = collision.gameObject.GetComponent<TalismanObject>();
            if (talisman == null)
            {
                talisman = collision.gameObject.GetComponentInParent<TalismanObject>();
                if (talisman == null)
                {
                    talisman = collision.gameObject.GetComponentInChildren<TalismanObject>();
                }
            }

            if (talisman == null)
                return;

            Debug.Log($"Talisman hit {gameObject.name}! Type: {talisman.CurrentType}");

            // Check if it's a stun talisman
            if (talisman.CurrentType == TalismanObject.TalismanType.Stun)
            {
                StartCoroutine(StunCoroutine());
                return;
            }

            // Check if this talisman is required
            var requiredTalismans = GetRequiredTalismans();
            if (requiredTalismans.Contains(talisman.CurrentType) && !collectedTalismans.Contains(talisman.CurrentType))
            {
                // Collect this talisman
                collectedTalismans.Add(talisman.CurrentType);
                Debug.Log($"Collected {talisman.CurrentType} talisman! ({collectedTalismans.Count}/{requiredTalismans.Count})");

                // Update indicator
                UpdateTalismanIndicator(talisman.CurrentType);

                // Call custom handler
                OnTalismanCollected(talisman.CurrentType);

                // Check if all talismans collected
                if (collectedTalismans.Count >= requiredTalismans.Count)
                {
                    OnAllTalismansCollected();
                    KillMob();
                }
            }
            else
            {
                Debug.Log($"{talisman.CurrentType} talisman is not required for this mob!");
            }
        }

        /// <summary>
        /// Stun/pause the mob for the duration specified in mobData
        /// </summary>
        protected IEnumerator StunCoroutine()
        {
            if (mobAI != null)
            {
                mobAI.SetPaused(true);
                yield return new WaitForSeconds(mobData.stunDuration);
                mobAI.SetPaused(false);
            }
        }

        /// <summary>
        /// Get the list of required talismans to defeat this mob
        /// Override in derived classes to specify requirements
        /// </summary>
        protected virtual System.Collections.Generic.List<TalismanObject.TalismanType> GetRequiredTalismans()
        {
            return new System.Collections.Generic.List<TalismanObject.TalismanType> { TalismanObject.TalismanType.Thai };
        }

        /// <summary>
        /// Called when a correct talisman is collected
        /// Override to add custom effects
        /// </summary>
        protected virtual void OnTalismanCollected(TalismanObject.TalismanType talismanType)
        {
            // Optional override
        }

        /// <summary>
        /// Called when all required talismans are collected
        /// </summary>
        protected virtual void OnAllTalismansCollected()
        {
            Debug.Log($"{gameObject.name} has been defeated with all required talismans!");
        }

        /// <summary>
        /// Kill the mob (called when all talismans collected)
        /// </summary>
        protected void KillMob()
        {
            MobHealth mobHealth = GetComponent<MobHealth>();
            if (mobHealth != null)
            {
                mobHealth.TakeDamage(9999f, transform.position); // Overkill damage
            }
            else
            {
                // If no health component, destroy directly
                if (mobAI != null)
                {
                    mobAI.DestroyMob();
                }
            }
        }

        /// <summary>
        /// Initialize talisman indicators above the mob
        /// </summary>
        protected void InitializeTalismanIndicators()
        {
            if (questionMarkIndicator == null) return;

            var requiredTalismans = GetRequiredTalismans();
            int count = requiredTalismans.Count;

            // Use default settings if none assigned
            if (indicatorSettings == null)
            {
                Debug.LogWarning($"{gameObject.name}: No IndicatorSettings assigned! Using default values.");
                CreateDefaultIndicators(count);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                // Get position from settings
                Vector3 localPosition = indicatorSettings.GetIndicatorPosition(i, count);
                
                // Create indicator
                GameObject indicator = Instantiate(questionMarkIndicator, transform);
                indicator.transform.localPosition = localPosition;
                
                // Apply rotation from settings
                if (indicatorSettings.rotation != Vector3.zero)
                {
                    indicator.transform.localRotation = Quaternion.Euler(indicatorSettings.rotation);
                }
                
                // Apply scale from settings
                if (indicatorSettings.scale != 1f)
                {
                    indicator.transform.localScale = Vector3.one * indicatorSettings.scale;
                }
                
                // Add bobbing component if enabled
                if (indicatorSettings.enableBobbing)
                {
                    var bobber = indicator.AddComponent<IndicatorBobbing>();
                    bobber.bobbingSpeed = indicatorSettings.bobbingSpeed;
                    bobber.bobbingAmount = indicatorSettings.bobbingAmount;
                }
                
                indicatorObjects.Add(indicator);
            }
        }

        /// <summary>
        /// Create indicators with default settings (fallback)
        /// </summary>
        private void CreateDefaultIndicators(int count)
        {
            float defaultHeight = 2.5f;
            float defaultSpacing = 0.5f;
            float totalWidth = (count - 1) * defaultSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(startX + (i * defaultSpacing), defaultHeight, 0f);
                GameObject indicator = Instantiate(questionMarkIndicator, transform);
                indicator.transform.localPosition = offset;
                indicatorObjects.Add(indicator);
            }
        }

        /// <summary>
        /// Update indicator when a talisman is collected
        /// </summary>
        protected void UpdateTalismanIndicator(TalismanObject.TalismanType collectedType)
        {
            var requiredTalismans = GetRequiredTalismans();
            int index = requiredTalismans.IndexOf(collectedType);

            if (index >= 0 && index < indicatorObjects.Count)
            {
                GameObject oldIndicator = indicatorObjects[index];
                Vector3 localPosition = oldIndicator.transform.localPosition;
                Quaternion localRotation = oldIndicator.transform.localRotation;
                Vector3 localScale = oldIndicator.transform.localScale;
                Transform parent = oldIndicator.transform.parent;

                Destroy(oldIndicator);

                // Get the correct indicator prefab
                GameObject newIndicatorPrefab = GetIndicatorPrefab(collectedType);
                if (newIndicatorPrefab != null)
                {
                    GameObject newIndicator = Instantiate(newIndicatorPrefab, parent);
                    newIndicator.transform.localPosition = localPosition;
                    newIndicator.transform.localRotation = localRotation;
                    newIndicator.transform.localScale = localScale;
                    
                    // Re-apply bobbing if enabled
                    if (indicatorSettings != null && indicatorSettings.enableBobbing)
                    {
                        var bobber = newIndicator.AddComponent<IndicatorBobbing>();
                        bobber.bobbingSpeed = indicatorSettings.bobbingSpeed;
                        bobber.bobbingAmount = indicatorSettings.bobbingAmount;
                    }
                    
                    indicatorObjects[index] = newIndicator;
                }
            }
        }

        /// <summary>
        /// Get the correct indicator prefab for a talisman type
        /// </summary>
        protected GameObject GetIndicatorPrefab(TalismanObject.TalismanType type)
        {
            switch (type)
            {
                case TalismanObject.TalismanType.Thai:
                    return thaiTalismanIndicator;
                case TalismanObject.TalismanType.Lighting:
                    return electricTalismanIndicator;
                case TalismanObject.TalismanType.Cross:
                    return crossTalismanIndicator;
                default:
                    return questionMarkIndicator;
            }
        }

        /// <summary>
        /// Clean up indicators when mob is destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            foreach (var indicator in indicatorObjects)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }
            indicatorObjects.Clear();
        }

        #endregion

        /// <summary>
        /// Called when the mob is spawned
        /// </summary>
        public virtual void OnSpawned() { }

        /// <summary>
        /// Called every frame while in Idle state
        /// </summary>
        public virtual void OnIdleUpdate(float distanceToPlayer) { }

        /// <summary>
        /// Called when transitioning from Idle to Chasing
        /// </summary>
        public virtual void OnStartChasing() { }

        /// <summary>
        /// Called every frame while in Chasing state
        /// </summary>
        public virtual void OnChasingUpdate(float distanceToPlayer) { }

        /// <summary>
        /// Called when transitioning to Attacking state
        /// </summary>
        public virtual void OnStartAttacking() { }

        /// <summary>
        /// Called every frame while in Attacking state
        /// </summary>
        public virtual void OnAttackingUpdate(float distanceToPlayer) { }

        /// <summary>
        /// Called when performing an attack
        /// Return true to allow default attack, false to use custom attack
        /// </summary>
        public virtual bool OnAttack()
        {
            return true; // Allow default attack behavior
        }

        /// <summary>
        /// Called after an attack is completed
        /// Default behavior: Destroy mob if ShouldDestroyAfterAttack() returns true
        /// </summary>
        public virtual void OnAttackComplete()
        {
            if (ShouldDestroyAfterAttack())
            {
                if (mobAI != null)
                {
                    mobAI.DestroyMob();
                }
            }
        }

        /// <summary>
        /// Override to specify whether this mob should be destroyed after attacking
        /// Default: true (most mobs destroy themselves after one attack)
        /// </summary>
        protected virtual bool ShouldDestroyAfterAttack()
        {
            return true; // Default behavior: destroy after attack
        }

        /// <summary>
        /// Called when the mob takes damage
        /// </summary>
        public virtual void OnTakeDamage(float damage) { }

        /// <summary>
        /// Called when the mob dies
        /// </summary>
        public virtual void OnDeath() { }

        /// <summary>
        /// Custom movement logic (if needed)
        /// Return true to override default movement, false to use default
        /// </summary>
        public virtual bool CustomMovement()
        {
            return false; // Use default NavMesh movement
        }

        /// <summary>
        /// Get custom movement speed multiplier (for dynamic speed changes)
        /// </summary>
        public virtual float GetSpeedMultiplier()
        {
            return 1f;
        }
    }
}

