using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countdown_text;
    [SerializeField] private float startTime = 90.0f;
    private float remainingTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartTimer();
    }

    private void StartTimer()
    {
        remainingTime = startTime;
    }
    // Update is called once per frame
    void Update()
    {
        // Gameplay handler
        if (remainingTime >= 1)
        {
            remainingTime -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            countdown_text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

    }
}
