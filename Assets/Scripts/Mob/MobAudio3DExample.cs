using UnityEngine;

namespace MobSystem
{
    /// <summary>
    /// Example script showing how to use 3D audio with dynamically spawned enemies
    /// </summary>
    public class MobAudio3DExample : MonoBehaviour
    {
        [Header("Example Usage")]
        [Tooltip("Play a 3D sound when this enemy spawns")]
        [SerializeField] private string spawnSoundName = "MobSpawn";
        
        [Tooltip("Play footstep sounds attached to enemy")]
        [SerializeField] private string footstepSoundName = "MobFootstep";
        
        [Tooltip("Play attack sound at enemy position")]
        [SerializeField] private string attackSoundName = "MobAttack";
        
        // Store reference to looping audio source
        private AudioSource loopingFootsteps;

        void Start()
        {
            // Example 1: Play one-shot 3D sound at enemy position (when enemy spawns)
            // Sound plays once and cleanup is automatic
            PlaySpawnSound();
        }

        void PlaySpawnSound()
        {
            if (MobAudioManager.instance != null)
            {
                // Simple way - plays 3D sound at this enemy's position
                MobAudioManager.instance.PlayAudio3D(spawnSoundName, transform.position);
            }
        }

        public void PlayAttackSound()
        {
            if (MobAudioManager.instance != null)
            {
                // Play attack sound at enemy position with custom distance settings
                // Players will hear from 1 to 20 units away
                MobAudioManager.instance.PlayAudio3DCustom(
                    attackSoundName, 
                    transform.position, 
                    minDistance: 1f, 
                    maxDistance: 20f
                );
            }
        }

        public void StartFootstepLoop()
        {
            if (MobAudioManager.instance != null && loopingFootsteps == null)
            {
                // Attach looping sound to enemy - follows the enemy as it moves!
                loopingFootsteps = MobAudioManager.instance.PlayAudio3DAttached(
                    footstepSoundName, 
                    gameObject
                );
                
                // The returned AudioSource can be controlled
                if (loopingFootsteps != null)
                {
                    loopingFootsteps.loop = true; // Make sure it loops
                }
            }
        }

        public void StopFootstepLoop()
        {
            if (loopingFootsteps != null)
            {
                loopingFootsteps.Stop();
                Destroy(loopingFootsteps.gameObject);
                loopingFootsteps = null;
            }
        }

        void OnDestroy()
        {
            // Clean up any attached audio sources when enemy is destroyed
            StopFootstepLoop();
        }

        // ========== INTEGRATION WITH EXISTING SCRIPTS ==========

        // For MobAI.cs - replace the attack sound call with:
        /*
        void AttackExample()
        {
            if (MobAudioManager.instance != null)
            {
                // Play attack sound from enemy position
                MobAudioManager.instance.PlayAudio3D("MobAttack", transform.position);
            }
        }
        */

        // For MobSpawner.cs - when spawning an enemy:
        /*
        void OnEnemySpawned(GameObject spawnedEnemy)
        {
            if (MobAudioManager.instance != null)
            {
                // Play spawn sound at the spawned enemy's position
                MobAudioManager.instance.PlayAudio3D("MobSpawn", spawnedEnemy.transform.position);
            }
        }
        */
    }
}

