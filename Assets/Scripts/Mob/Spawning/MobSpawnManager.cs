using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

namespace MobSystem
{
    /// <summary>
    /// Manages mob spawning with phase-based difficulty scaling
    /// Based on ListSpawner pattern with phase system integration
    /// </summary>
    public class MobSpawnManager : MonoBehaviour
    {
        public static MobSpawnManager Instance { get; private set; }

        [Header("Phase Configuration")]
        [Tooltip("Phase configuration with mob stats and spawn rates")]
        [SerializeField] private MobPhaseConfig phaseConfig;
        
        [Header("List Spawners")]
        [Tooltip("List of ListSpawner components that define spawn locations")]
        [SerializeField] private List<ListSpawner> listSpawners = new List<ListSpawner>();
        
        [Tooltip("Auto-find all ListSpawners in scene on start")]
        [SerializeField] private bool autoFindListSpawners = true;

        [Header("Gameplay Manager")]
        [Tooltip("The GameplayManager to check if the game is started")]
        [SerializeField] private GameplayManager gameplayManager;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Private variables
        private List<GameObject> activeMobs = new List<GameObject>();
        private Dictionary<string, int> activeMobCountByType = new Dictionary<string, int>();
        private float lastSpawnTime = 0f;
        private int currentPhase = 1;
        private bool isGameStarted = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {

            // Wait for game to start
            StartCoroutine(WaitForGameStart());

        }

        private void InitializeListSpawners()
        {
            // Auto-find ListSpawners in scene if enabled
            if (autoFindListSpawners)
            {
                ListSpawner[] foundSpawners = FindObjectsByType<ListSpawner>(FindObjectsSortMode.None);
                listSpawners = new List<ListSpawner>(foundSpawners);
            }
            
            // Initialize all spawners
            foreach (var spawner in listSpawners)
            {
                if (spawner != null)
                {
                    spawner.Initialize();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"MobSpawnManager: Initialized {listSpawners.Count} ListSpawners");
                foreach (var spawner in listSpawners)
                {
                    if (spawner != null)
                    {
                        Debug.Log($"  - {spawner.GetSpawnerID()}: {spawner.GetSpawnPointCount()} spawn points, {spawner.GetAvailableMobPrefabs().Count} mob types");
                    }
                }
            }
            
            if (listSpawners.Count == 0)
            {
                Debug.LogWarning("MobSpawnManager: No ListSpawners found! Mobs will not spawn.");
            }
        }

        private IEnumerator WaitForGameStart()
        {
            while (!gameplayManager.isGameStart)
            {
                Debug.Log("while (!gameplayManager.isGameStart) = " +!gameplayManager.isGameStart);
                yield return null;
            }
            Debug.Log("while (!gameplayManager.isGameStart) = " + !gameplayManager.isGameStart);
            isGameStarted = true;
            
            if (showDebugInfo)
            {
                Debug.Log("MobSpawnManager: Game started, phase spawning active");
            }

            // Initialize list spawners
            InitializeListSpawners();

            // Get GameplayManager if not assigned
            if (gameplayManager == null && GameplayManager.Instance != null)
            {
                gameplayManager = GameplayManager.Instance;
            }

            // Subscribe to phase changes
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;
            }
        }

        private void Update()
        {
            if (!isGameStarted) return;

            // Clean up destroyed mobs from list
            activeMobs.RemoveAll(mob => mob == null);

            // Phase-based spawning
            if (Time.time - lastSpawnTime >= phaseConfig.spawnFrequency)
            {
                TrySpawnMob();
                lastSpawnTime = Time.time;
            }

            // Debug info
            if (showDebugInfo)
            {
                int hybridCount = GetHybridMobCount();
                int nonHybridCount = GetNonHybridMobCount();
                Debug.Log($"Phase: {currentPhase} | Total: {activeMobs.Count}/{phaseConfig.maxTotalGhostsInScene} | Non-Hybrid: {nonHybridCount} | Hybrid: {hybridCount}");
            }
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            }
        }

        #region Phase System Methods

        /// <summary>
        /// Called when phase changes
        /// </summary>
        private void OnPhaseChanged(int newPhase)
        {
            currentPhase = newPhase;
            
            if (showDebugInfo)
            {
                Debug.Log($"MobSpawnManager: Phase changed to {currentPhase}");
            }
            
            // Update speeds of all active mobs
            UpdateAllMobSpeeds();
        }

        /// <summary>
        /// Try to spawn a mob based on current phase rules
        /// </summary>
        private void TrySpawnMob()
        {
            // Check if we've reached max total ghosts
            if (activeMobs.Count >= phaseConfig.maxTotalGhostsInScene)
            {
                return;
            }

            // Get available mob types for current phase
            List<MobTypeConfig> availableTypes = phaseConfig.GetAvailableMobTypesForPhase(currentPhase);
            if (availableTypes.Count == 0)
            {
                Debug.LogWarning("No available mob types for current phase!");
                return;
            }

            // Filter by type limits
            List<MobTypeConfig> spawnableTypes = new List<MobTypeConfig>();
            foreach (var mobType in availableTypes)
            {
                if (CanSpawnMobType(mobType))
                {
                    spawnableTypes.Add(mobType);
                }
            }

            if (spawnableTypes.Count == 0)
            {
                if (showDebugInfo)
                {
                    Debug.Log("All mob types at max capacity");
                }
                return;
            }

            // Select a mob type based on spawn weights
            MobTypeConfig selectedType = SelectMobTypeByWeight(spawnableTypes);
            if (selectedType == null)
            {
                return;
            }

            // Spawn the mob
            SpawnMob(selectedType);
        }

        /// <summary>
        /// Check if we can spawn a specific mob type
        /// </summary>
        private bool CanSpawnMobType(MobTypeConfig mobType)
        {
            // Check per-type limit
            int currentCount = GetMobCountByType(mobType.typeName);
            if (currentCount >= mobType.maxInScene)
            {
                return false;
            }

            // Check hybrid/non-hybrid limits
            if (currentPhase >= phaseConfig.hybridStartPhase)
            {
                if (mobType.isHybrid)
                {
                    int hybridCount = GetHybridMobCount();
                    if (hybridCount >= phaseConfig.maxHybridGhosts)
                    {
                        return false;
                    }
                }
                else
                {
                    int nonHybridCount = GetNonHybridMobCount();
                    if (nonHybridCount >= phaseConfig.maxNonHybridAfterHybrid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Select a mob type based on spawn weights
        /// </summary>
        private MobTypeConfig SelectMobTypeByWeight(List<MobTypeConfig> types)
        {
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var type in types)
            {
                totalWeight += type.GetSpawnRateForPhase(currentPhase);
            }

            if (totalWeight <= 0)
            {
                return types[Random.Range(0, types.Count)];
            }

            // Random selection based on weight
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var type in types)
            {
                currentWeight += type.GetSpawnRateForPhase(currentPhase);
                if (randomValue <= currentWeight)
                {
                    return type;
                }
            }

            return types[types.Count - 1];
        }

        /// <summary>
        /// Spawn a mob of the specified type
        /// </summary>
        private void SpawnMob(MobTypeConfig mobType)
        {
            if (mobType.mobPrefab == null)
            {
                Debug.LogWarning($"Mob prefab is null for type: {mobType.typeName}");
                return;
            }

            if (listSpawners.Count == 0)
            {
                Debug.LogWarning("No ListSpawners available!");
                return;
            }

            // Find a suitable ListSpawner for this mob type
            ListSpawner selectedSpawner = SelectSpawnerForMob(mobType.mobPrefab);
            if (selectedSpawner == null)
            {
                Debug.LogWarning($"No suitable spawner found for mob type: {mobType.typeName}");
                return;
            }

            // Use the ListSpawner to spawn the mob
            GameObject mob = selectedSpawner.SpawnMob(mobType.mobPrefab);
            
            if (mob == null)
            {
                Debug.LogWarning($"Failed to spawn {mobType.typeName}");
                return;
            }

            // Get MobAI component and set speed
            MobAI mobAI = mob.GetComponent<MobAI>();
            if (mobAI != null)
            {
                float speed = mobType.CalculateSpeedForPhase(currentPhase);
                mobAI.SetMoveSpeed(speed);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Spawned {mobType.typeName} at '{selectedSpawner.GetSpawnerID()}' (Phase {currentPhase}, Speed {speed:F2})");
                }
            }

            // Track the mob
            activeMobs.Add(mob);
            
            // Update type count
            if (!activeMobCountByType.ContainsKey(mobType.typeName))
            {
                activeMobCountByType[mobType.typeName] = 0;
            }
            activeMobCountByType[mobType.typeName]++;

            // Add component to track mob type
            MobTypeTracker tracker = mob.AddComponent<MobTypeTracker>();
            tracker.typeName = mobType.typeName;
            tracker.isHybrid = mobType.isHybrid;
            tracker.spawnManager = this;
        }

        /// <summary>
        /// Select a spawner for a specific mob prefab
        /// </summary>
        private ListSpawner SelectSpawnerForMob(GameObject mobPrefab)
        {
            // Get all spawners that can spawn this mob type
            List<ListSpawner> validSpawners = new List<ListSpawner>();
            
            foreach (var spawner in listSpawners)
            {
                if (spawner != null && spawner.CanSpawnMobType(mobPrefab))
                {
                    validSpawners.Add(spawner);
                }
            }

            if (validSpawners.Count == 0)
            {
                // If no spawner has this mob in their list, use any spawner
                // (assuming empty list means "can spawn any")
                foreach (var spawner in listSpawners)
                {
                    if (spawner != null && spawner.GetAvailableMobPrefabs().Count == 0)
                    {
                        validSpawners.Add(spawner);
                    }
                }
            }

            if (validSpawners.Count == 0)
            {
                return null;
            }

            // Select a random spawner from valid ones
            return validSpawners[Random.Range(0, validSpawners.Count)];
        }

        /// <summary>
        /// Update speeds of all active mobs for new phase
        /// </summary>
        private void UpdateAllMobSpeeds()
        {
            foreach (GameObject mob in activeMobs)
            {
                if (mob == null) continue;

                MobTypeTracker tracker = mob.GetComponent<MobTypeTracker>();
                if (tracker == null) continue;

                MobTypeConfig config = phaseConfig.GetMobTypeConfig(tracker.typeName);
                if (config == null) continue;

                MobAI mobAI = mob.GetComponent<MobAI>();
                if (mobAI != null)
                {
                    float newSpeed = config.CalculateSpeedForPhase(currentPhase);
                    mobAI.SetMoveSpeed(newSpeed);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"Updated {tracker.typeName} speed to {newSpeed:F2} for phase {currentPhase}");
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mob is destroyed
        /// </summary>
        public void OnMobDestroyed(string typeName)
        {
            if (activeMobCountByType.ContainsKey(typeName))
            {
                activeMobCountByType[typeName]--;
                if (activeMobCountByType[typeName] < 0)
                {
                    activeMobCountByType[typeName] = 0;
                }
            }
        }

        /// <summary>
        /// Get count of mobs by type name
        /// </summary>
        private int GetMobCountByType(string typeName)
        {
            if (activeMobCountByType.ContainsKey(typeName))
            {
                return activeMobCountByType[typeName];
            }
            return 0;
        }

        /// <summary>
        /// Get count of hybrid mobs
        /// </summary>
        private int GetHybridMobCount()
        {
            int count = 0;
            foreach (GameObject mob in activeMobs)
            {
                if (mob == null) continue;
                MobTypeTracker tracker = mob.GetComponent<MobTypeTracker>();
                if (tracker != null && tracker.isHybrid)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get count of non-hybrid mobs
        /// </summary>
        private int GetNonHybridMobCount()
        {
            int count = 0;
            foreach (GameObject mob in activeMobs)
            {
                if (mob == null) continue;
                MobTypeTracker tracker = mob.GetComponent<MobTypeTracker>();
                if (tracker != null && !tracker.isHybrid)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get count of currently active mobs
        /// </summary>
        public int GetActiveMobCount()
        {
            return activeMobs.Count;
        }

        /// <summary>
        /// Get count of specific mob type
        /// </summary>
        public int GetMobCountByTypeName(string typeName)
        {
            return GetMobCountByType(typeName);
        }

        /// <summary>
        /// Get current phase number
        /// </summary>
        public int GetCurrentPhase()
        {
            return currentPhase;
        }

        /// <summary>
        /// Check if game has started
        /// </summary>
        public bool IsGameStarted()
        {
            return isGameStarted;
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
            activeMobCountByType.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (listSpawners == null || listSpawners.Count == 0) return;

            // Draw connections to list spawners
            Gizmos.color = Color.yellow;
            foreach (var spawner in listSpawners)
            {
                if (spawner != null)
                {
                    Gizmos.DrawLine(transform.position, spawner.transform.position);
                }
            }

            // Draw manager position
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
        }

        #endregion
    }

    /// <summary>
    /// Helper component to track mob type information
    /// </summary>
    public class MobTypeTracker : MonoBehaviour
    {
        public string typeName;
        public bool isHybrid;
        public MobSpawnManager spawnManager;

        private void OnDestroy()
        {
            if (spawnManager != null)
            {
                spawnManager.OnMobDestroyed(typeName);
            }
        }
    }
}


