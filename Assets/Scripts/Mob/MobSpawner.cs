using System.Collections;
using UnityEngine;
using AudioSystem;

namespace MobSystem
{
    /// <summary>
    /// Spawns mobs from hidden locations (objects, ceiling, walls) with dramatic effects
    /// </summary>
    public class MobSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("The mob prefab to spawn")]
        [SerializeField] private GameObject mobPrefab;
        
        [Tooltip("Where the mob will spawn relative to this spawner")]
        [SerializeField] private Transform spawnPoint;
        
        [Tooltip("Type of spawn location")]
        [SerializeField] private SpawnType spawnType = SpawnType.Object;

        [Header("Spawn Trigger")]
        [Tooltip("How the spawn is triggered")]
        [SerializeField] private TriggerType triggerType = TriggerType.PlayerProximity;
        
        [Tooltip("Distance at which player triggers the spawn (for Proximity trigger)")]
        [SerializeField] private float triggerDistance = 5f;
        
        [Tooltip("Time before spawn happens automatically (for Timed trigger)")]
        [SerializeField] private float spawnDelay = 3f;

        [Header("Spawn Animation")]
        [Tooltip("Time it takes for mob to fully emerge")]
        [SerializeField] private float emergeDuration = 1.5f;
        
        [Tooltip("Should the mob break through the surface?")]
        [SerializeField] private bool useBreakEffect = true;
        
        [Tooltip("Particle effect when breaking through")]
        [SerializeField] private GameObject breakEffectPrefab;

        [Header("Audio")]
        [Tooltip("Sound to play before mob appears (tension sound)")]
        [SerializeField] private string preSpawnSoundName = "MobPreSpawn";
        
        [Tooltip("Sound to play when mob breaks through")]
        [SerializeField] private string spawnSoundName = "MobSpawn";
        
        [Tooltip("Delay between pre-spawn sound and actual spawn")]
        [SerializeField] private float audioWarningTime = 0.5f;

        [Header("Ceiling Specific Settings")]
        [Tooltip("Height above spawn point where mob starts (for ceiling spawns)")]
        [SerializeField] private float ceilingHeight = 3f;
        
        [Tooltip("Should mob drop from ceiling with physics?")]
        [SerializeField] private bool useCeilingDrop = true;

        [Header("Respawn Settings")]
        [Tooltip("Can this spawner spawn multiple mobs?")]
        [SerializeField] private bool canRespawn = false;
        
        [Tooltip("Time before respawning another mob")]
        [SerializeField] private float respawnTime = 30f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        // Private variables
        private bool hasSpawned = false;
        private bool isTriggered = false;
        private GameObject currentMob;
        private Transform player;

        public enum SpawnType
        {
            Object,     // Spawns from behind/inside an object
            Ceiling,    // Drops from ceiling
            Wall,       // Breaks through wall
            Floor       // Emerges from floor
        }

        public enum TriggerType
        {
            PlayerProximity,    // Spawns when player gets close
            Timed,              // Spawns after a set time
            Manual              // Only spawns when manually triggered via script
        }

        private void Start()
        {
            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // If no spawn point specified, use this object's position
            if (spawnPoint == null)
            {
                spawnPoint = transform;
            }

            // Auto-trigger for timed spawns
            if (triggerType == TriggerType.Timed)
            {
                StartCoroutine(TimedSpawn());
            }
        }

        private void Update()
        {
            // Check for proximity trigger
            if (triggerType == TriggerType.PlayerProximity && !hasSpawned && !isTriggered)
            {
                if (player != null && Vector3.Distance(transform.position, player.position) <= triggerDistance)
                {
                    TriggerSpawn();
                }
            }
        }

        /// <summary>
        /// Manually trigger the spawn from external scripts
        /// </summary>
        public void TriggerSpawn()
        {
            Debug.Log($"[{gameObject.name}] TriggerSpawn called! isTriggered={isTriggered}, hasSpawned={hasSpawned}, canRespawn={canRespawn}");
            
            // Check if already spawning or spawned
            if (isTriggered && !canRespawn)
            {
                Debug.LogWarning($"[{gameObject.name}] Already triggered, ignoring!");
                return;
            }
            
            if (!hasSpawned || canRespawn)
            {
                isTriggered = true;
                hasSpawned = true;  // Set immediately to prevent double-spawn
                Debug.Log($"[{gameObject.name}] Starting spawn sequence!");
                StartCoroutine(SpawnSequence());
            }
        }

        private IEnumerator TimedSpawn()
        {
            yield return new WaitForSeconds(spawnDelay);
            TriggerSpawn();
        }

        private IEnumerator SpawnSequence()
        {
            // hasSpawned is already set in TriggerSpawn() to prevent double-spawning

            // Play 3D pre-spawn warning sound at spawn location
            if (MobAudioManager.instance != null && !string.IsNullOrEmpty(preSpawnSoundName))
            {
                Debug.Log($"[{gameObject.name}] Playing pre-spawn sound '{preSpawnSoundName}' at time {Time.time}");
                MobAudioManager.instance.PlayAudio3D(preSpawnSoundName, spawnPoint.position);
            }

            // Wait for tension buildup
            yield return new WaitForSeconds(audioWarningTime);

            // Determine spawn position based on type
            Vector3 spawnPosition = GetSpawnPosition();
            Vector3 finalPosition = spawnPoint.position;

            // Instantiate the mob
            currentMob = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);

            // Play 3D spawn sound at the spawned enemy's position
            if (MobAudioManager.instance != null && !string.IsNullOrEmpty(spawnSoundName))
            {
                MobAudioManager.instance.PlayAudio3D(spawnSoundName, currentMob.transform.position);
            }

            // Spawn break effect
            if (useBreakEffect && breakEffectPrefab != null)
            {
                GameObject effect = Instantiate(breakEffectPrefab, finalPosition, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // Animate the emergence based on spawn type
            yield return StartCoroutine(AnimateEmergence(currentMob, spawnPosition, finalPosition));

            // Notify the mob AI that it has spawned
            MobAI mobAI = currentMob.GetComponent<MobAI>();
            if (mobAI != null)
            {
                mobAI.OnSpawned();
                
                // For horror effect, make mob immediately aware of player
                mobAI.ForceDetectPlayer();
            }

            // Handle respawn
            if (canRespawn)
            {
                StartCoroutine(RespawnTimer());
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 position = spawnPoint.position;

            switch (spawnType)
            {
                case SpawnType.Ceiling:
                    position += Vector3.up * ceilingHeight;
                    break;
                
                case SpawnType.Floor:
                    position += Vector3.down * 2f;
                    break;
                
                case SpawnType.Wall:
                    // Spawn slightly inside the wall
                    position += -transform.forward * 1f;
                    break;
                
                case SpawnType.Object:
                    // Spawn at the object's center, slightly hidden
                    position += Vector3.down * 0.5f;
                    break;
            }

            return position;
        }

        private IEnumerator AnimateEmergence(GameObject mob, Vector3 startPos, Vector3 endPos)
        {
            float elapsedTime = 0f;
            Transform mobTransform = mob.transform;

            // Different animations based on spawn type
            switch (spawnType)
            {
                case SpawnType.Ceiling:
                    yield return AnimateCeilingDrop(mobTransform, startPos, endPos);
                    break;
                
                case SpawnType.Floor:
                case SpawnType.Object:
                case SpawnType.Wall:
                    yield return AnimateRise(mobTransform, startPos, endPos);
                    break;
            }
        }

        private IEnumerator AnimateCeilingDrop(Transform mob, Vector3 startPos, Vector3 endPos)
        {
            if (useCeilingDrop)
            {
                // Dramatic drop
                float elapsedTime = 0f;
                while (elapsedTime < emergeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / emergeDuration;
                    
                    // Ease out curve for more impact
                    float curve = 1f - Mathf.Pow(1f - t, 3f);
                    mob.position = Vector3.Lerp(startPos, endPos, curve);
                    
                    yield return null;
                }
            }
            else
            {
                // Slow descent
                float elapsedTime = 0f;
                while (elapsedTime < emergeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / emergeDuration;
                    mob.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }
            }

            mob.position = endPos;
        }

        private IEnumerator AnimateRise(Transform mob, Vector3 startPos, Vector3 endPos)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < emergeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / emergeDuration;
                
                // Ease out for smooth emergence
                float curve = Mathf.Sin(t * Mathf.PI * 0.5f);
                mob.position = Vector3.Lerp(startPos, endPos, curve);
                
                yield return null;
            }

            mob.position = endPos;
        }

        private IEnumerator RespawnTimer()
        {
            yield return new WaitForSeconds(respawnTime);
            
            // Reset for next spawn
            hasSpawned = false;
            isTriggered = false;
            
            if (triggerType == TriggerType.Timed)
            {
                TriggerSpawn();
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

            // Draw trigger range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);

            // Draw spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPos, 0.3f);

            // Draw spawn type indicator
            Gizmos.color = Color.cyan;
            Vector3 startPos = GetSpawnPositionForGizmo();
            Gizmos.DrawLine(startPos, spawnPos);
            Gizmos.DrawWireSphere(startPos, 0.2f);

            // Draw arrow showing direction
            Vector3 direction = (spawnPos - startPos).normalized;
            DrawArrow(startPos, direction, 0.5f);
        }

        private Vector3 GetSpawnPositionForGizmo()
        {
            if (spawnPoint == null) return transform.position;
            
            Vector3 position = spawnPoint.position;

            switch (spawnType)
            {
                case SpawnType.Ceiling:
                    position += Vector3.up * ceilingHeight;
                    break;
                case SpawnType.Floor:
                    position += Vector3.down * 2f;
                    break;
                case SpawnType.Wall:
                    position += -transform.forward * 1f;
                    break;
                case SpawnType.Object:
                    position += Vector3.down * 0.5f;
                    break;
            }

            return position;
        }

        private void DrawArrow(Vector3 start, Vector3 direction, float length)
        {
            Vector3 end = start + direction * length;
            Gizmos.DrawLine(start, end);
            
            // Arrow head
            Vector3 right = Quaternion.Euler(0, 30, 0) * -direction * 0.2f;
            Vector3 left = Quaternion.Euler(0, -30, 0) * -direction * 0.2f;
            Gizmos.DrawLine(end, end + right);
            Gizmos.DrawLine(end, end + left);
        }

        /// <summary>
        /// Get the current spawned mob (if any)
        /// </summary>
        public GameObject GetCurrentMob() => currentMob;

        /// <summary>
        /// Check if this spawner has already spawned
        /// </summary>
        public bool HasSpawned() => hasSpawned;
    }
}

