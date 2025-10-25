using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private Sprite fullHealthSprite;
    [SerializeField] private Sprite emptyHealthSprite;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private Transform healthObjectParent;
    private List<Image> currentHealthImages = new List<Image>();
    private int currentHealth;

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
    // Update is called once per frame
    void Update()
    {
        
    }
}
