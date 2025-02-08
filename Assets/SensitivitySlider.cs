using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    [SerializeField] private bool IsScopeSlider;
    
    private void Start()
    {
        var slider = GetComponentInChildren<Slider>();
        var field = GetComponentInChildren<TMP_InputField>();
        

        slider.onValueChanged.AddListener(delegate{UpdateSensitivitySlider(field, slider);});
        field.onValueChanged.AddListener(delegate{UpdateSensitivityInput(field, slider);});
        if (IsScopeSlider)
        {
            slider.value = SensitivityHandler.Instance.ScopedSens;
            field.text = SensitivityHandler.Instance.ScopedSens.ToString();
            slider.onValueChanged.AddListener(delegate{SensitivityHandler.Instance.SetPlayerScopeSensitivity(slider.value.ToString());});
            field.onValueChanged.AddListener(SensitivityHandler.Instance.SetPlayerScopeSensitivity);
            return;
        }
        slider.value = SensitivityHandler.Instance.Sens;
        field.text = SensitivityHandler.Instance.Sens.ToString();
        slider.onValueChanged.AddListener(SensitivityHandler.Instance.SetPlayerSensitivity);
        field.onValueChanged.AddListener(delegate{SensitivityHandler.Instance.SetPlayerSensitivity(slider.value);});
    }
    
    public void UpdateSensitivitySlider(TMP_InputField sensitivityInput, Slider sensitivitySlider)
    {
        try
        {
            sensitivityInput.text = sensitivitySlider.value.ToString();
        }
        catch (Exception e)
        {
            return;
        }
    }
    
    public void UpdateSensitivityInput(TMP_InputField sensitivityInput, Slider sensitivitySlider)
    {
        try
        {
            sensitivitySlider.value = float.Parse(sensitivityInput.text);
        }
        catch (Exception e)
        {
            return;
        }
    }
}
