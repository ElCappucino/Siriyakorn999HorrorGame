using AudioSystem;
using MobSystem;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Multiplier")]
    [SerializeField] private List<float> multiplierChainValues = new List<float>();
    [SerializeField] private int maxChain = 5;
    [SerializeField] private int currentChain = 1;
    [SerializeField] private float multiplierChainTimeWindow = 3;
    private float currentChainTime;

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

        isGameStart = true;

        // Start phase timer
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.StartPhaseTimer();
        }

        AudioManager.instance.SwitchBGM("Gameplay");
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
                countdown_text.text = string.Format("{0:00}:{1:00}", 0, 0);
                if (!isGameFinish)
                {
                    ShowWinningScene();
                }

            }
        }

        // chain multiplier
        if (currentChain > 1)
        {
            currentChainTime += Time.deltaTime;

            if (currentChainTime > multiplierChainTimeWindow)
            {
                currentChain = 1;
                currentChainTime = 0;
                IncreaseMultiplier(multiplierChainValues[currentChain - 1]);
            }
        }

    }
    public PlayerManager GetPlayerManager()
    {
        return playerManager;
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

    public void IncreaseScore(int score, float timeSinceSpawn)
    {
        float TimeBonus = Mathf.Clamp(1 + (1 / timeSinceSpawn), 1, 2);
        currentScore += Mathf.CeilToInt(score * currentMultiplier * TimeBonus);
        scoreText.text = currentScore.ToString();
        scoreFeedbacks.PlayFeedbacks();

        // calculate chain bonus
        if (currentChainTime < multiplierChainTimeWindow)
        {
            currentChain++;
            IncreaseMultiplier(multiplierChainValues[currentChain - 1]);
            currentChainTime = 0;
        }

    }
    public void IncreaseMultiplier(float multiplier)
    {
        currentMultiplier = multiplier;
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
        AudioManager.instance.SwitchBGM("Main Menu");
    }

    public void ShowWinningScene()
    {
        isGameStart = false;
        isGameFinish = true;
        WinSceneUI.SetActive(true);
        WinSceneFeedbacks.PlayFeedbacks();
        WinSceneReport.UpdateText(currentScore, playerManager.currentHealth, talismanWritten, ghostExorcisted);
        AudioManager.instance.SwitchBGM("Main Menu");

    }

    /*private void OnEnable()
    {
        isGameStart = true;
    }*/
}
