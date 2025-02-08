using TMPro;
using UnityEngine;

public class OptionsHandler : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    private void Start()
    {
        LoadQualityPreset();
    }

    private void LoadQualityPreset()
    {
        if(!graphicsDropdown) return;
        graphicsDropdown.value = QualitySettings.GetQualityLevel();
    }
    
    public void ChangeQualityPreset(int preset)
    {
        QualitySettings.SetQualityLevel(preset);
    }
}
