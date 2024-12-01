using TMPro;
using UnityEngine;

public class PlayerCanvasHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text ammoText;
    
    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
    }
    
    public void UpdateArmor(int armor)
    {
        armorText.text = armor.ToString();
    }
    
    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current}/{max}";
    }
}
