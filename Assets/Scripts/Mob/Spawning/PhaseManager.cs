using UnityEngine;
using System;

namespace MobSystem
{
    /// <summary>
    /// Manages game phases with time-based progression
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        [Header("Phase Settings")]
        [Tooltip("Phase durations in seconds")]
        [SerializeField] private float[] phaseDurations = new float[] 
        { 
            30f,    // Phase 1: 0:00 - 0:30
            40f,    // Phase 2: 0:30 - 1:10
            55f,    // Phase 3: 1:10 - 2:05
            46f,    // Phase 4: 2:05 - 2:51
            35f     // Phase 5: 2:51 - 3:26
        };

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Events
        public event Action<int> OnPhaseChanged;

        // Private variables
        private float gameStartTime;
        private float elapsedTime;
        private int currentPhase = 1;
        private bool gameStarted = false;

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
            // Don't start timing until game starts
            gameStarted = false;
        }

        public void StartPhaseTimer()
        {
            gameStartTime = Time.time;
            gameStarted = true;
            currentPhase = 1;
            
            if (showDebugInfo)
            {
                Debug.Log("PhaseManager: Game started, Phase 1 begins");
            }
            
            OnPhaseChanged?.Invoke(currentPhase);
        }

        private void Update()
        {
            if (!gameStarted) return;

            elapsedTime = Time.time - gameStartTime;

            // Check for phase transitions
            int newPhase = CalculateCurrentPhase();
            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                
                if (showDebugInfo)
                {
                    Debug.Log($"PhaseManager: Phase changed to {currentPhase}");
                }
                
                OnPhaseChanged?.Invoke(currentPhase);
            }
        }

        private int CalculateCurrentPhase()
        {
            float cumulativeTime = 0f;
            
            for (int i = 0; i < phaseDurations.Length; i++)
            {
                cumulativeTime += phaseDurations[i];
                if (elapsedTime < cumulativeTime)
                {
                    return i + 1; // Phase numbers start at 1
                }
            }
            
            // If we've exceeded all phase durations, stay at last phase
            return phaseDurations.Length;
        }

        public int GetCurrentPhase()
        {
            return currentPhase;
        }

        public float GetElapsedTime()
        {
            return elapsedTime;
        }

        public float GetPhaseProgress()
        {
            float cumulativeTime = 0f;
            float phaseStartTime = 0f;
            
            for (int i = 0; i < currentPhase - 1; i++)
            {
                cumulativeTime += phaseDurations[i];
            }
            
            phaseStartTime = cumulativeTime;
            float phaseDuration = phaseDurations[currentPhase - 1];
            float timeInPhase = elapsedTime - phaseStartTime;
            
            return Mathf.Clamp01(timeInPhase / phaseDuration);
        }

        public bool IsGameStarted()
        {
            return gameStarted;
        }

        public string GetPhaseTimeString()
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        private void OnGUI()
        {
            if (!showDebugInfo || !gameStarted) return;

            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, 300, 30), $"Phase: {currentPhase} | Time: {GetPhaseTimeString()}");
            GUI.Label(new Rect(10, 40, 300, 30), $"Phase Progress: {GetPhaseProgress() * 100f:F1}%");
        }
    }
}

