using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;
using MobSystem;

public class PlayerManager : MonoBehaviour
{
    

    [Header("Health")]
    [SerializeField] private int maxHealth;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite emptyHealthSprite;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private Transform healthObjectParent;
    private List<Image> currentHealthImages = new List<Image>();
    public int currentHealth;

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

        

        talismanInfo.InitDict();

    }
    public void UpdateCurrentTalismanType(string type)
    {
        Debug.Log("type = " + type);
        string result = type.ToLower();
        switch (result)
        {
            case "lightning":
                currentTalismanType = TalismanObject.TalismanType.Lighting;
                break;
            case "cross":
                currentTalismanType = TalismanObject.TalismanType.Cross;
                break;
            case "stun":
                currentTalismanType = TalismanObject.TalismanType.Stun;
                break;
            case "thai":
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
    

    public void DecreaseHealth()
    {
        currentHealth--;
        currentHealthImages[currentHealth].sprite = emptyHealthSprite;
        cameraControl.HurtEffect.PlayFeedbacks();
        if (currentHealth <= 0 && !GameplayManager.Instance.isGameFinish)
        {
            GameplayManager.Instance.ShowGameOverScene();
        }
    }

    public void ShootTalisman(InputAction.CallbackContext context)
    {
        if (GameplayManager.Instance.isGameStart && 
            !cameraControl.isHoldTalisman && 
            currentTalismanType != TalismanObject.TalismanType.Normal)
        {
            if (context.performed)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 aimPoint;
                if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance))
                    aimPoint = hit.point;
                else
                    aimPoint = ray.origin + ray.direction * maxAimDistance; // aim far away
                Debug.Log("cameraControl.handPivot.transform.rotation.y = " + cameraControl.handPivot.transform.eulerAngles.y);
                Quaternion rotation = Quaternion.Euler(0, cameraControl.handPivot.transform.eulerAngles.y, 0);
                GameObject proj = Instantiate(talismanPrefab, talismanSpawnPos.position, rotation);
                Rigidbody rb = proj.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (aimPoint - talismanSpawnPos.position).normalized;
                    rb.linearVelocity = dir * projectileSpeed;
                }
                proj.GetComponentInChildren<TalismanObject>().InitEffect(currentTalismanType);

                currentActiveTalisman.SetActive(false);
                currentActiveTalisman = talismanInfo.talismanInfoDict[TalismanObject.TalismanType.Normal].vfxObject;
                currentTalismanType = TalismanObject.TalismanType.Normal;
                currentActiveTalisman.SetActive(true);

                GameplayManager.Instance.WriteTalisman();
                cameraControl.shootEffect.PlayFeedbacks();

                Destroy(proj, 3.0f);
            }
        }
        else
        {
            //Debug.Log("GameplayManager.Instance.isGameStart && !cameraControl.isHoldTalisman");
        }    
        
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
