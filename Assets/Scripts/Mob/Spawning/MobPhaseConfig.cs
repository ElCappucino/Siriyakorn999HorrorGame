using UnityEngine;
using System.Collections.Generic;

namespace MobSystem
{
    /// <summary>
    /// Configuration for mob spawning and stats per phase
    /// </summary>
    [CreateAssetMenu(fileName = "MobPhaseConfig", menuName = "Mob System/Phase Config")]
    public class MobPhaseConfig : ScriptableObject
    {
        [Header("Mob Type Settings")]
        public MobTypeConfig[] mobTypes;

        [Header("Global Settings")]
        [Tooltip("Maximum total ghosts in scene at once")]
        public int maxTotalGhostsInScene = 5;
        
        [Tooltip("Spawn frequency in seconds")]
        public float spawnFrequency = 3f;
        
        [Tooltip("Phase when hybrid mobs start appearing (Phase 3)")]
        public int hybridStartPhase = 3;

        [Header("Hybrid Limits")]
        [Tooltip("Maximum non-hybrid ghosts before hybrid phase")]
        public int maxNonHybridBeforeHybrid = 6;
        
        [Tooltip("Maximum non-hybrid ghosts after hybrid phase")]
        public int maxNonHybridAfterHybrid = 4;
        
        [Tooltip("Maximum hybrid ghosts")]
        public int maxHybridGhosts = 2;

        /// <summary>
        /// Get mob type config by name
        /// </summary>
        public MobTypeConfig GetMobTypeConfig(string typeName)
        {
            foreach (var config in mobTypes)
            {
                if (config.typeName == typeName)
                {
                    return config;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all available mob types for a specific phase
        /// </summary>
        public List<MobTypeConfig> GetAvailableMobTypesForPhase(int phase)
        {
            List<MobTypeConfig> available = new List<MobTypeConfig>();
            
            foreach (var config in mobTypes)
            {
                if (phase >= config.availableFromPhase)
                {
                    available.Add(config);
                }
            }
            
            return available;
        }

        /// <summary>
        /// Get spawn weights for a specific phase
        /// </summary>
        public Dictionary<string, float> GetSpawnWeightsForPhase(int phase)
        {
            Dictionary<string, float> weights = new Dictionary<string, float>();
            
            foreach (var config in mobTypes)
            {
                if (phase >= config.availableFromPhase)
                {
                    float weight = config.GetSpawnRateForPhase(phase);
                    weights[config.typeName] = weight;
                }
            }
            
            return weights;
        }
    }

    [System.Serializable]
    public class MobTypeConfig
    {
        [Header("Basic Info")]
        public string typeName;
        public GameObject mobPrefab;
        public bool isHybrid = false;
        public int maxInScene = 2;
        public int availableFromPhase = 1;

        [Header("Spawn Rates (%)")]
        [Tooltip("Spawn rate before hybrid phase (Phases 1-2)")]
        [Range(0, 100)] public float spawnRateBeforeHybrid = 33f;
        
        [Tooltip("Spawn rate after hybrid phase (Phases 3-5)")]
        [Range(0, 100)] public float spawnRateAfterHybrid = 15f;

        [Header("Speed Settings (Time to Reach Player in seconds)")]
        [Tooltip("Speed for each phase (index 0 = Phase 1)")]
        public float[] timeToReachPlayerPerPhase = new float[5];

        /// <summary>
        /// Get spawn rate for a specific phase
        /// </summary>
        public float GetSpawnRateForPhase(int phase)
        {
            // Phase 3 is when hybrids start
            if (phase >= 3)
            {
                return spawnRateAfterHybrid;
            }
            else
            {
                return spawnRateBeforeHybrid;
            }
        }

        /// <summary>
        /// Get time to reach player for a specific phase
        /// </summary>
        public float GetTimeToReachPlayerForPhase(int phase)
        {
            if (phase < 1 || phase > timeToReachPlayerPerPhase.Length)
            {
                return timeToReachPlayerPerPhase[0]; // Default to phase 1
            }
            return timeToReachPlayerPerPhase[phase - 1];
        }

        /// <summary>
        /// Calculate mob speed based on time to reach player
        /// Assumes an average distance (can be adjusted)
        /// </summary>
        public float CalculateSpeedForPhase(int phase, float averageDistance = 20f)
        {
            float timeToReach = GetTimeToReachPlayerForPhase(phase);
            if (timeToReach <= 0) return 1f;
            
            // Speed = Distance / Time
            // return averageDistance / timeToReach;

            //Use As Raw Speed for now
            float rawSpeed = timeToReach;
            return rawSpeed;
        }
    }
}

