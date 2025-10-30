using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// ScriptableObject that defines the characteristics and behavior of a mob type
    /// </summary>
    [CreateAssetMenu(fileName = "New Mob Data", menuName = "Horror Game/Mob Data")]
    public class MobData : ScriptableObject
    {
        [Header("Mob Identity")]
        public MobType mobType;
        public string mobName;
        [TextArea(3, 5)]
        public string description;

        [Header("Movement")]
        public MovementType movementType;
        public SpeedTier speedTier;
        [Range(0.1f, 10f)]
        public float moveSpeed = 3.5f;
        [Range(0.5f, 5f)]
        public float rotationSpeed = 2f;

        [Header("Detection")]
        [Range(5f, 50f)]
        public float detectionRange = 20f;
        [Range(1f, 10f)]
        public float attackRange = 2f;
        [Range(0.5f, 5f)]
        public float attackCooldown = 2f;

        [Header("Combat")]
        [Range(5f, 100f)]
        public float health = 100f;
        [Range(5f, 50f)]
        public float attackDamage = 10f;

        [Header("Audio")]
        public AudioTheme audioTheme;
        public string detectionSoundName = "MobDetect";
        public string chaseSoundName = "MobChase";
        public string attackSoundName = "MobAttack";
        public string ambientSoundName = "MobAmbient";

        [Header("Special Behavior")]
        [Tooltip("Does this mob transform (e.g., bat to vampire)?")]
        public bool hasTransformation = false;
        [Tooltip("Distance to player when transformation occurs (0 = immediate)")]
        public float transformationDistance = 5f;
        [Tooltip("Does this mob pause before attacking?")]
        public bool pauseBeforeAttack = false;
        [Tooltip("Duration of pre-attack pause")]
        public float pauseDuration = 1f;
        [Tooltip("Does this mob perform jumpscare?")]
        public bool hasJumpscare = false;

        [Header("Behavior Settings")]
        [Tooltip("The behavior script type name (must inherit from MobBehavior)")]
        public string behaviorClassName;
        
        /// <summary>
        /// Get the speed value based on speed tier
        /// </summary>
        public float GetSpeedValue()
        {
            return moveSpeed;
        }

        /// <summary>
        /// Validate the mob data configuration
        /// </summary>
        public void OnValidate()
        {
            // Ensure speed matches tier roughly
            switch (speedTier)
            {
                case SpeedTier.Slow:
                    if (moveSpeed > 2.5f)
                        Debug.LogWarning($"{mobName}: Speed is too high for Slow tier");
                    break;
                case SpeedTier.Normal:
                    if (moveSpeed < 2.5f || moveSpeed > 4.5f)
                        Debug.LogWarning($"{mobName}: Speed should be between 2.5-4.5 for Normal tier");
                    break;
                case SpeedTier.Fast:
                    if (moveSpeed < 4.5f)
                        Debug.LogWarning($"{mobName}: Speed is too low for Fast tier");
                    break;
            }
        }
    }
}

