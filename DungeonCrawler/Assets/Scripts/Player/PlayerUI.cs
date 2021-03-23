using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Stats")]

    [Header("UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    public ID CurrentRoom;

    void Start()
    {

    }

    void Update()
    {
        //updateHealth();
    }

    void updateHealth()
    {

    }

}
