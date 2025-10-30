using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MobSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text scoreMultiplierText;
    [SerializeField] private MMF_Player scoreFeedbacks;
    [SerializeField] private MMF_Player multiplierFeedbacks;
    private int currentScore;
    private float currentMultiplier;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite emptyHealthSprite;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private Transform healthObjectParent;
    private List<Image> currentHealthImages = new List<Image>();
    private int currentHealth;

    [Header("Shooting Talisman")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject talismanPrefab;
    [SerializeField] private float projectileSpeed = 30f;
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private Transform talismanSpawnPos;

    [SerializeField] private CameraControl cameraControl;

    [Header("Holding Talisman")]
    [SerializeField] TalismanInfoList talismanInfo;
    public TalismanObject.TalismanType currentTalismanType;
    [SerializeField] private GameObject currentActiveTalisman;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;

        for (int i = 0; i < maxHealth; i++)
        {
            var healthUI = Instantiate(healthPrefab, healthObjectParent);
            currentHealthImages.Add(healthUI.GetComponent<Image>());
        }

        currentMultiplier = 1.0f;

        talismanInfo.InitDict();

    }
    public void UpdateCurrentTalismanType(string type)
    {
        switch (type)
        {
            case "Lightning":
                currentTalismanType = TalismanObject.TalismanType.Lighting;
                break;
            case "Cross":
                currentTalismanType = TalismanObject.TalismanType.Cross;
                break;
            case "Stun":
                currentTalismanType = TalismanObject.TalismanType.Stun;
                break;
            case "Thai":
                currentTalismanType = TalismanObject.TalismanType.Thai;
                break;
            default:
                Debug.Log("No match");
                break;
        }

        if (currentActiveTalisman != null)
            currentActiveTalisman.SetActive(false);

        currentActiveTalisman = talismanInfo.talismanInfoDict[currentTalismanType].vfxObject;
        currentActiveTalisman.SetActive(true);
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

    public void DecreaseHealth()
    {
        currentHealth--;
        currentHealthImages[currentHealth].sprite = emptyHealthSprite;
    }

    public void ShootTalisman(InputAction.CallbackContext context)
    {
        if (GameplayManager.Instance.isGameStart && !cameraControl.isHoldTalisman)
        {
            if (context.performed)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 aimPoint;
                if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance))
                    aimPoint = hit.point;
                else
                    aimPoint = ray.origin + ray.direction * maxAimDistance; // aim far away

                GameObject proj = Instantiate(talismanPrefab, talismanSpawnPos.position, Quaternion.identity);
                Rigidbody rb = proj.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (aimPoint - talismanSpawnPos.position).normalized;
                    rb.linearVelocity = dir * projectileSpeed;
                }
                proj.GetComponentInChildren<TalismanObject>().InitEffect(currentTalismanType);

                Destroy(proj, 3.0f);
            }
        }
        
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
