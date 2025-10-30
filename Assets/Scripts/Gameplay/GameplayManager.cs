using UnityEngine;
using MobSystem;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public bool isGameStart = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void StartGame()
    {
        isGameStart = true;
        
        // Start phase timer
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.StartPhaseTimer();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
