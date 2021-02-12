using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    [Header("UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    public ID CurrentRoom;
    private int previousHealth = 0;

    // Start is called before the first frame update
    void Start()
    {
        //healthBar.maxValue = currentHealth = maxHealth;
        //healthBar.value = maxHealth;

    }

    // Update is called once per frame
    void Update()
    {
        //updateHealth();
    }

    void updateHealth()
    {
        if (previousHealth == currentHealth)
            return;

        previousHealth = currentHealth;
        healthBar.value = currentHealth;
        healthText.text = currentHealth + "/" + maxHealth;
    }

}
