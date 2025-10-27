using UnityEngine;

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

        public virtual void Initialize(MobAI ai, MobData data, Transform playerTransform)
        {
            mobAI = ai;
            mobData = data;
            player = playerTransform;
            animator = GetComponent<Animator>();
        }

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
        /// </summary>
        public virtual void OnAttackComplete() { }

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

