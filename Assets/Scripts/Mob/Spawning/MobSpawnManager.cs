using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

namespace MobSystem
{
    /// <summary>
    /// Manages multiple mob spawners and coordinates waves/events
    /// Use this for controlling spawn sequences and difficulty scaling
    /// </summary>
    public class MobSpawnManager : MonoBehaviour
    {
        [Header("Spawner Management")]
        [Tooltip("All spawners in the level")]
        [SerializeField] private List<MobSpawner> allSpawners = new List<MobSpawner>();
        
        [Tooltip("Auto-find all spawners in scene on start")]
        [SerializeField] private bool autoFindSpawners = true;

        [Header("Wave Settings")]
        [Tooltip("Enable wave-based spawning")]
        [SerializeField] private bool useWaveSystem = false;
        
        [Tooltip("Number of spawners to activate per wave")]
        [SerializeField] private int spawnersPerWave = 3;
        
        [Tooltip("Delay between waves")]
        [SerializeField] private float timeBetweenWaves = 15f;
        
        [Tooltip("Delay between individual spawns in a wave")]
        [SerializeField] private float delayBetweenSpawns = 2f;

        [Header("Difficulty Scaling")]
        [Tooltip("Increase mob count over time")]
        [SerializeField] private bool scaleDifficulty = true;
        
        [Tooltip("Additional spawners per wave")]
        [SerializeField] private int spawnerIncreasePerWave = 1;
        
        [Tooltip("Maximum spawners per wave")]
        [SerializeField] private int maxSpawnersPerWave = 10;

        [Header("Audio")]
        [Tooltip("Sound to play when wave starts")]
        [SerializeField] private string waveStartSoundName = "WaveStart";

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Private variables
        private int currentWave = 0;
        private int activeSpawnerCount = 0;
        private List<GameObject> activeMobs = new List<GameObject>();
        private bool waveInProgress = false;

        private void Start()
        {
            if (autoFindSpawners)
            {
                FindAllSpawners();
            }

            if (useWaveSystem)
            {
                StartCoroutine(WaveSystem());
            }
        }

        private void Update()
        {
            // Clean up destroyed mobs from list
            activeMobs.RemoveAll(mob => mob == null);

            if (showDebugInfo)
            {
                // You can display this in UI later
                Debug.Log($"Wave: {currentWave} | Active Mobs: {activeMobs.Count}");
            }
        }

        /// <summary>
        /// Find all MobSpawners in the scene
        /// </summary>
        private void FindAllSpawners()
        {
            MobSpawner[] foundSpawners = FindObjectsOfType<MobSpawner>();
            allSpawners = new List<MobSpawner>(foundSpawners);
            
            if (showDebugInfo)
            {
                Debug.Log($"MobSpawnManager: Found {allSpawners.Count} spawners in scene");
            }
        }

        /// <summary>
        /// Wave-based spawn system coroutine
        /// </summary>
        private IEnumerator WaveSystem()
        {
            while (true)
            {
                // Wait for previous wave to clear or timer
                yield return new WaitForSeconds(timeBetweenWaves);

                // Start next wave
                currentWave++;
                StartWave();

                // Wait for wave to complete
                while (waveInProgress)
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        /// <summary>
        /// Start a new wave of spawns
        /// </summary>
        public void StartWave()
        {
            if (allSpawners.Count == 0)
            {
                Debug.LogWarning("MobSpawnManager: No spawners available!");
                return;
            }

            waveInProgress = true;

            // Calculate how many spawners to use this wave
            int spawnersToUse = spawnersPerWave;
            if (scaleDifficulty)
            {
                spawnersToUse += (currentWave - 1) * spawnerIncreasePerWave;
                spawnersToUse = Mathf.Min(spawnersToUse, maxSpawnersPerWave);
                spawnersToUse = Mathf.Min(spawnersToUse, allSpawners.Count);
            }

            // Play wave start sound
            if (AudioManager.instance != null && !string.IsNullOrEmpty(waveStartSoundName))
            {
                AudioManager.instance.PlayAudio(waveStartSoundName);
            }

            if (showDebugInfo)
            {
                Debug.Log($"Starting Wave {currentWave} with {spawnersToUse} spawners");
            }

            // Start spawning
            StartCoroutine(SpawnWave(spawnersToUse));
        }

        /// <summary>
        /// Spawn a wave of mobs
        /// </summary>
        private IEnumerator SpawnWave(int count)
        {
            // Get random spawners that haven't spawned yet (or can respawn)
            List<MobSpawner> availableSpawners = new List<MobSpawner>();
            foreach (var spawner in allSpawners)
            {
                if (!spawner.HasSpawned() || spawner.GetComponent<MobSpawner>() != null)
                {
                    availableSpawners.Add(spawner);
                }
            }

            // If not enough available, use all spawners
            if (availableSpawners.Count < count)
            {
                availableSpawners = new List<MobSpawner>(allSpawners);
            }

            // Shuffle and take count
            ShuffleList(availableSpawners);
            for (int i = 0; i < count && i < availableSpawners.Count; i++)
            {
                availableSpawners[i].TriggerSpawn();
                
                // Track spawned mob
                GameObject spawnedMob = availableSpawners[i].GetCurrentMob();
                if (spawnedMob != null)
                {
                    activeMobs.Add(spawnedMob);
                }

                yield return new WaitForSeconds(delayBetweenSpawns);
            }

            waveInProgress = false;
        }

        /// <summary>
        /// Manually trigger a specific number of spawns
        /// </summary>
        public void TriggerSpawns(int count)
        {
            StartCoroutine(SpawnWave(count));
        }

        /// <summary>
        /// Trigger all spawners at once
        /// </summary>
        public void TriggerAllSpawners()
        {
            foreach (var spawner in allSpawners)
            {
                spawner.TriggerSpawn();
            }
        }

        /// <summary>
        /// Trigger a random spawner
        /// </summary>
        public void TriggerRandomSpawner()
        {
            if (allSpawners.Count > 0)
            {
                int randomIndex = Random.Range(0, allSpawners.Count);
                allSpawners[randomIndex].TriggerSpawn();
            }
        }

        /// <summary>
        /// Get count of currently active mobs
        /// </summary>
        public int GetActiveMobCount()
        {
            return activeMobs.Count;
        }

        /// <summary>
        /// Get current wave number
        /// </summary>
        public int GetCurrentWave()
        {
            return currentWave;
        }

        /// <summary>
        /// Add a spawner to the manager
        /// </summary>
        public void RegisterSpawner(MobSpawner spawner)
        {
            if (!allSpawners.Contains(spawner))
            {
                allSpawners.Add(spawner);
            }
        }

        /// <summary>
        /// Remove a spawner from the manager
        /// </summary>
        public void UnregisterSpawner(MobSpawner spawner)
        {
            allSpawners.Remove(spawner);
        }

        /// <summary>
        /// Clear all active mobs
        /// </summary>
        public void ClearAllMobs()
        {
            foreach (var mob in activeMobs)
            {
                if (mob != null)
                {
                    Destroy(mob);
                }
            }
            activeMobs.Clear();
        }

        /// <summary>
        /// Shuffle a list (Fisher-Yates algorithm)
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (allSpawners == null || allSpawners.Count == 0) return;

            // Draw connections between manager and spawners
            Gizmos.color = Color.cyan;
            foreach (var spawner in allSpawners)
            {
                if (spawner != null)
                {
                    Gizmos.DrawLine(transform.position, spawner.transform.position);
                }
            }

            // Draw manager position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}

