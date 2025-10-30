using UnityEngine;

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
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
