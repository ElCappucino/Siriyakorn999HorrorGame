using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;

public class GameplayManager : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text scoreMultiplierText;
    [SerializeField] private MMF_Player scoreFeedbacks;
    [SerializeField] private MMF_Player multiplierFeedbacks;
    private int currentScore;
    private float currentMultiplier;
    private int talismanWritten;
    private int ghostExorcisted;

    [Header("Countdown")]
    [SerializeField] private TMP_Text countdown_text;
    [SerializeField] private float startTime = 15.0f;
    private float remainingTime;

    [Header("Win/Lose Scene")]
    [SerializeField] private GameObject WinSceneUI;
    [SerializeField] private GameObject LoseSceneUI;
    [SerializeField] private ResultReportUI LoseSceneReport;
    [SerializeField] private ResultReportUI WinSceneReport;
    [SerializeField] private MMF_Player WinSceneFeedbacks;
    [SerializeField] private MMF_Player LoseSceneFeedbacks;

    public static GameplayManager Instance;
    public bool isGameStart = false;
    public bool isGameFinish = false;
    public PlayerManager playerManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartTimer();

        currentMultiplier = 1.0f;
    }


    // Update is called once per frame
    void Update()
    {
        // Gameplay handler
        if (isGameStart)
        {
            if (remainingTime >= 0)
            {
                remainingTime -= Time.deltaTime;
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);

                countdown_text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                if (!isGameFinish)
                {
                    ShowWinningScene();
                }

            }
        }

    }
    public void GhostExorcised()
    {
        ghostExorcisted++;
    }
    public void WriteTalisman()
    {
        talismanWritten++;
    }
    public void StartGame()
    {
        Debug.Log("StartGame");
        isGameStart = true;

        scoreText.text = currentScore.ToString();
        scoreMultiplierText.text = "x" + currentMultiplier.ToString("F1");
    }
    private void StartTimer()
    {
        remainingTime = startTime;
    }

    public void IncreaseScore(int score)
    {
        currentScore += Mathf.CeilToInt(score * currentMultiplier);
        scoreText.text = currentScore.ToString();
        scoreFeedbacks.PlayFeedbacks();
    }
    public void IncreaseMultiplier(float multiplier)
    {
        currentMultiplier += multiplier;
        scoreMultiplierText.text = "x" + currentMultiplier.ToString("F1");
        multiplierFeedbacks.PlayFeedbacks();
    }

    public void ShowGameOverScene()
    {
        isGameStart = false;
        isGameFinish = true;
        LoseSceneUI.SetActive(true);
        LoseSceneFeedbacks.PlayFeedbacks();
        LoseSceneReport.UpdateText(currentScore, playerManager.currentHealth, talismanWritten, ghostExorcisted);
    }

    public void ShowWinningScene()
    {
        isGameStart = false;
        isGameFinish = true;
        WinSceneUI.SetActive(true);
        WinSceneFeedbacks.PlayFeedbacks();
        WinSceneReport.UpdateText(currentScore, playerManager.currentHealth, talismanWritten, ghostExorcisted);

    }

    /*private void OnEnable()
    {
        isGameStart = true;
    }*/
}
