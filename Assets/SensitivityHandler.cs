using System;
using UnityEngine;

public class SensitivityHandler : MonoBehaviour
{
    public static SensitivityHandler Instance;
    
    [field: SerializeField] public float Sens { get; private set; } = 0.1f;
    [field: SerializeField] public float ScopedSens { get; private set; } = 0.5f;
    [field: SerializeField] public float CurrentSens { get; private set; }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    
    public void SetPlayerSensitivity(float newSens)
    {
        Sens = newSens;
        CurrentSens = Sens;
    }
    
    public void SetPlayerScopeSensitivity(string newSens)
    {
        ScopedSens = float.Parse(newSens);
        CurrentSens = Sens;
    }

    public void SetScopedSens()
    {
        CurrentSens = Sens * ScopedSens;
    }

    public void ResetSens()
    {
        CurrentSens = Sens;
    }
}
