using UnityEngine;
using AudioSystem;

namespace MobSystem
{
    /// <summary>
    /// Handles mob health, damage, and death
    /// </summary>
    public class MobHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health of the mob")]
        [SerializeField] private float maxHealth = 100f;
        
        [Tooltip("Current health (for debugging)")]
        [SerializeField] private float currentHealth;

        [Header("Death Settings")]
        [Tooltip("Time before destroying the mob after death")]
        [SerializeField] private float deathDelay = 3f;
        
        [Tooltip("Should the body ragdoll on death?")]
        [SerializeField] private bool useRagdoll = true;
        
        [Tooltip("Effect to spawn on death")]
        [SerializeField] private GameObject deathEffectPrefab;

        [Header("Audio")]
        [Tooltip("Sound to play when taking damage")]
        [SerializeField] private string hurtSoundName = "MobHurt";
        
        [Tooltip("Sound to play on death")]
        [SerializeField] private string deathSoundName = "MobDeath";
        
        [Tooltip("Randomize hurt sound pitch for variety")]
        [SerializeField] private bool randomizeHurtPitch = true;
        
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        [Header("Damage Feedback")]
        [Tooltip("Material to flash when hit (optional)")]
        [SerializeField] private Material damageMaterial;
        
        [Tooltip("How long to show damage material")]
        [SerializeField] private float damageFlashDuration = 0.1f;
        
        [Tooltip("Should mob be knocked back when hit?")]
        [SerializeField] private bool useKnockback = true;
        
        [Tooltip("Force of knockback")]
        [SerializeField] private float knockbackForce = 5f;

        [Header("Score Variables")]
        [Tooltip("Base score to calculate when got exorcised")]
        [SerializeField] private int baseScore;
        [SerializeField] private float timeSinceSpawn;

        // Private variables
        private bool isDead = false;
        private Renderer[] renderers;
        private Material[] originalMaterials;
        private Rigidbody rb;
        private MobAI mobAI;
        private MobBehavior behavior;
        private Animator animator;
        private readonly int deathHash = Animator.StringToHash("Death");


        private void Awake()
        {
            currentHealth = maxHealth;
            rb = GetComponent<Rigidbody>();
            mobAI = GetComponent<MobAI>();
            behavior = GetComponent<MobBehavior>();
            animator = GetComponent<Animator>();
            
            // Store original materials for damage flash
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }

            // init time since spawn
            timeSinceSpawn = 0;
        }

        private void Update()
        {
            timeSinceSpawn += Time.deltaTime;
        }

        /// <summary>
        /// Apply damage to the mob
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        /// <param name="damageSource">Position where damage came from (for knockback direction)</param>
        public void TakeDamage(float damage, Vector3 damageSource = default)
        {
            if (isDead) return;

            currentHealth -= damage;

            // Play hurt sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(hurtSoundName))
            {
                if (randomizeHurtPitch)
                {
                    AudioManager.instance.PlayRandomPitchAudio(hurtSoundName, minPitch, maxPitch);
                }
                else
                {
                    AudioManager.instance.PlayAudio(hurtSoundName);
                }
            }

            // Visual feedback
            StartCoroutine(DamageFlash());

            // Knockback
            if (useKnockback && rb != null && damageSource != default)
            {
                Vector3 knockbackDirection = (transform.position - damageSource).normalized;
                knockbackDirection.y = 0.5f; // Add slight upward force
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }

            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnTakeDamage(damage);
            }

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Overload for damage without source position
        /// </summary>
        public void TakeDamage(float damage)
        {
            TakeDamage(damage, default);
        }

        /// <summary>
        /// Instantly kill the mob
        /// </summary>
        public void Die()
        {
            if (isDead) return;
            
            isDead = true;
            currentHealth = 0;

            // Play death sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(deathSoundName))
            {
                AudioManager.instance.PlayAudio(deathSoundName);
            }

            // Call behavior hook
            if (behavior != null)
            {
                behavior.OnDeath();
            }

            // Disable AI
            if (mobAI != null)
            {
                mobAI.enabled = false;
            }

            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 5f);
            }

            // Death animation or ragdoll
            if (useRagdoll)
            {
                EnableRagdoll();
            }
            else if (animator != null)
            {
                animator.SetTrigger(deathHash);
            }

            GameplayManager.Instance.GhostExorcised();
            GameplayManager.Instance.IncreaseScore(baseScore, timeSinceSpawn);

            // Destroy after delay
            Destroy(gameObject, deathDelay);
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (damageMaterial == null || renderers.Length == 0)
                yield break;

            // Apply damage material
            foreach (Renderer rend in renderers)
            {
                rend.material = damageMaterial;
            }

            yield return new WaitForSeconds(damageFlashDuration);

            // Restore original materials
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = originalMaterials[i];
            }
        }

        private void EnableRagdoll()
        {
            // Disable animator
            if (animator != null)
            {
                animator.enabled = false;
            }

            // Enable physics on all rigidbodies
            Rigidbody[] ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody ragdollRb in ragdollRigidbodies)
            {
                ragdollRb.isKinematic = false;
                ragdollRb.detectCollisions = true;
            }

            // Disable collider on main body
            Collider mainCollider = GetComponent<Collider>();
            if (mainCollider != null)
            {
                mainCollider.enabled = false;
            }
        }

        /// <summary>
        /// Heal the mob by a specified amount
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        /// <summary>
        /// Get current health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        /// <summary>
        /// Check if mob is dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }

        /// <summary>
        /// Get current health value
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// Get max health value
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw health bar above mob
            if (Application.isPlaying && !isDead)
            {
                Vector3 healthBarPos = transform.position + Vector3.up * 2.5f;
                float healthPercent = currentHealth / maxHealth;
                
                // Background
                Gizmos.color = Color.red;
                Gizmos.DrawCube(healthBarPos, new Vector3(1f, 0.1f, 0.01f));
                
                // Health
                Gizmos.color = Color.green;
                Vector3 healthSize = new Vector3(1f * healthPercent, 0.1f, 0.01f);
                Vector3 healthPos = healthBarPos - new Vector3((1f - healthPercent) * 0.5f, 0, 0);
                Gizmos.DrawCube(healthPos, healthSize);
            }
        }
    }
}

