using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{
    private PlayerSettingsData _settingsData;
    
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_InputField frameRateField;
    [SerializeField] private Toggle limitFrameRateToggle;
    [SerializeField] private Toggle vSyncToggle;

    private SettingsSaver _settingsSaver;

    private void Start()
    {
        _settingsSaver = FindFirstObjectByType<SettingsSaver>();
        
        _settingsData = _settingsSaver.LoadStats() ?? new()
        {
            qualityPreset = QualitySettings.GetQualityLevel(),
            frameRateLimit = 60,
            limitingFrameRate = true,
            doVerticalSync = true
        };

        LoadSavedSettings();
    }

    private void LoadSavedSettings()
    {
        if (graphicsDropdown)
        {
            graphicsDropdown.value = _settingsData.qualityPreset;
        }
        if (frameRateField)
        {
            frameRateField.text = _settingsData.frameRateLimit.ToString();
        }
        if (limitFrameRateToggle)
        {
            limitFrameRateToggle.isOn = Application.targetFrameRate > 0;
        }
        if (vSyncToggle)
        {
            limitFrameRateToggle.isOn = QualitySettings.vSyncCount > 0;
        }
    }

    public void ChangeQualityPreset(int preset)
    {
        QualitySettings.SetQualityLevel(preset);
        _settingsData.qualityPreset = preset;
    }

    public void ChangeFrameRateLimit(string value)
    {
        if(_settingsData.limitingFrameRate) return;
        _settingsData.frameRateLimit = int.Parse(value);
        Application.targetFrameRate = _settingsData.frameRateLimit;
        
    }

    public void LimitFrameRate(bool state)
    {
        _settingsData.limitingFrameRate = state;
        switch (_settingsData.limitingFrameRate)
        {
            case true:
                Application.targetFrameRate = _settingsData.frameRateLimit;
                break;
            case false:
                Application.targetFrameRate = -1;
                break;
        }
    }

    public void SetVSyncState(bool state)
    {
        _settingsData.doVerticalSync = state;
        QualitySettings.vSyncCount = _settingsData.doVerticalSync ? -1 : 1;
    }

    public void SaveCurrentStats()
    {
        _settingsSaver.SaveStats(_settingsData);
    }
}
