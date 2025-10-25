using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;

        for (int i = 0; i < maxHealth; i++)
        {
            var healthUI = Instantiate(healthPrefab, healthObjectParent);
            currentHealthImages.Add(healthUI.GetComponent<Image>());
        }
    }

    public void DecreaseHealth()
    {
        currentHealth--;
        currentHealthImages[currentHealth].sprite = emptyHealthSprite;
    }

    public void ShootTalisman(InputAction.CallbackContext context)
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
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (aimPoint - talismanSpawnPos.position).normalized;
                rb.linearVelocity = dir * projectileSpeed;
            }

            Destroy(proj, 3.0f);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
