using UnityEngine;
using UnityEngine.Events;
using AudioSystem;

namespace PlayerSystem
{
    /// <summary>
    /// Simple player health system for mobs to damage
    /// This is an example implementation - customize as needed for your game
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum player health")]
        [SerializeField] private float maxHealth = 100f;
        
        [Tooltip("Current health (visible for debugging)")]
        [SerializeField] private float currentHealth;
        
        [Tooltip("Can player take damage?")]
        [SerializeField] private bool isInvulnerable = false;

        [Header("Regeneration")]
        [Tooltip("Enable health regeneration")]
        [SerializeField] private bool enableRegen = true;
        
        [Tooltip("Health regenerated per second")]
        [SerializeField] private float regenRate = 2f;
        
        [Tooltip("Delay before regen starts after taking damage")]
        [SerializeField] private float regenDelay = 5f;

        [Header("Death Settings")]
        [Tooltip("What happens on death")]
        [SerializeField] private DeathBehavior deathBehavior = DeathBehavior.Respawn;
        
        [Tooltip("Time before respawn")]
        [SerializeField] private float respawnDelay = 3f;
        
        [Tooltip("Respawn location (if null, respawns at start position)")]
        [SerializeField] private Transform respawnPoint;

        [Header("Audio")]
        [Tooltip("Sound when taking damage")]
        [SerializeField] private string hurtSoundName = "PlayerHurt";
        
        [Tooltip("Sound on death")]
        [SerializeField] private string deathSoundName = "PlayerDeath";
        
        [Tooltip("Low health warning sound (loops)")]
        [SerializeField] private string lowHealthSoundName = "PlayerLowHealth";
        
        [Tooltip("Health percentage to trigger low health warning")]
        [SerializeField] private float lowHealthThreshold = 0.25f;

        [Header("Visual Feedback")]
        [Tooltip("Should screen flash red when hit?")]
        [SerializeField] private bool useDamageFlash = true;
        
        [Tooltip("Camera to apply damage shake")]
        [SerializeField] private Camera playerCamera;

        [Header("Events")]
        public UnityEvent OnDamageTaken;
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;
        public UnityEvent<float> OnHealthChanged; // Passes health percentage

        // Private variables
        private bool isDead = false;
        private float lastDamageTime;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isLowHealth = false;

        public enum DeathBehavior
        {
            Respawn,        // Respawn at checkpoint
            GameOver,       // Trigger game over
            Spectate        // Just die and watch
        }

        private void Awake()
        {
            currentHealth = maxHealth;
            startPosition = transform.position;
            startRotation = transform.rotation;

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            // Make sure this GameObject has the "Player" tag
            if (!gameObject.CompareTag("Player"))
            {
                Debug.LogWarning("PlayerHealth: GameObject should have 'Player' tag!");
                gameObject.tag = "Player";
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Health regeneration
            if (enableRegen && currentHealth < maxHealth)
            {
                if (Time.time - lastDamageTime >= regenDelay)
                {
                    Heal(regenRate * Time.deltaTime);
                }
            }

            // Low health warning
            CheckLowHealth();
        }

        /// <summary>
        /// Apply damage to the player
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead || isInvulnerable) return;

            currentHealth -= damage;
            lastDamageTime = Time.time;

            // Play hurt sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(hurtSoundName))
            {
                AudioManager.instance.PlayAudio(hurtSoundName);
            }

            // Visual feedback
            if (useDamageFlash)
            {
                // You can implement screen flash effect here
                // Example: Use Post Processing or UI overlay
            }

            // Invoke events
            OnDamageTaken?.Invoke();
            OnHealthChanged?.Invoke(GetHealthPercentage());

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }

            Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Heal the player
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(GetHealthPercentage());
        }

        /// <summary>
        /// Fully restore health
        /// </summary>
        public void FullHeal()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(1f);
        }

        /// <summary>
        /// Handle player death
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0;

            // Play death sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(deathSoundName))
            {
                AudioManager.instance.PlayAudio(deathSoundName);
            }

            // Stop low health sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(lowHealthSoundName))
            {
                AudioManager.instance.StopAudio(lowHealthSoundName);
            }

            // Invoke death event
            OnDeath?.Invoke();

            Debug.Log("Player died!");

            // Handle death based on behavior setting
            switch (deathBehavior)
            {
                case DeathBehavior.Respawn:
                    Invoke(nameof(Respawn), respawnDelay);
                    break;
                
                case DeathBehavior.GameOver:
                    // You can trigger your game over screen here
                    Debug.Log("GAME OVER");
                    break;
                
                case DeathBehavior.Spectate:
                    // Disable player control but keep camera active
                    DisablePlayerControl();
                    break;
            }
        }

        /// <summary>
        /// Respawn the player
        /// </summary>
        private void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;

            // Move to respawn point
            Vector3 respawnPos = respawnPoint != null ? respawnPoint.position : startPosition;
            Quaternion respawnRot = respawnPoint != null ? respawnPoint.rotation : startRotation;
            
            transform.position = respawnPos;
            transform.rotation = respawnRot;

            // Reset velocity if there's a rigidbody
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Invoke respawn event
            OnRespawn?.Invoke();
            OnHealthChanged?.Invoke(1f);

            Debug.Log("Player respawned!");
        }

        /// <summary>
        /// Check and handle low health warnings
        /// </summary>
        private void CheckLowHealth()
        {
            bool nowLowHealth = GetHealthPercentage() <= lowHealthThreshold;

            // Start low health sound
            if (nowLowHealth && !isLowHealth)
            {
                if (AudioManager.instance != null && !string.IsNullOrEmpty(lowHealthSoundName))
                {
                    AudioManager.instance.PlayAudio(lowHealthSoundName);
                }
            }
            // Stop low health sound
            else if (!nowLowHealth && isLowHealth)
            {
                if (AudioManager.instance != null && !string.IsNullOrEmpty(lowHealthSoundName))
                {
                    AudioManager.instance.StopAudio(lowHealthSoundName);
                }
            }

            isLowHealth = nowLowHealth;
        }

        /// <summary>
        /// Disable player control (for death/spectate)
        /// </summary>
        private void DisablePlayerControl()
        {
            // Disable any player movement scripts here
            // Example:
            // GetComponent<PlayerMovement>()?.enabled = false;
        }

        /// <summary>
        /// Get current health as percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
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

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }

        /// <summary>
        /// Set invulnerability
        /// </summary>
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }

        /// <summary>
        /// Set respawn point
        /// </summary>
        public void SetRespawnPoint(Transform newRespawnPoint)
        {
            respawnPoint = newRespawnPoint;
        }

        /// <summary>
        /// Force respawn immediately
        /// </summary>
        public void ForceRespawn()
        {
            Respawn();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw respawn point
            if (respawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(respawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, respawnPoint.position);
            }

            // Draw start position in editor
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}

