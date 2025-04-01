using System;
using System.IO;
using UnityEngine;

public class SettingsSaver : MonoBehaviour
{
    private SensitivityHandler _sensHandler;

    public void SaveStats(PlayerSettingsData dataToSave)
    {
        //TODO : Make this more scalable so I don't have to manually write out every data type I want to save

        var json = JsonUtility.ToJson(dataToSave);
        var path = Application.persistentDataPath + "/playerdata.json";
        File.WriteAllText(path, json);
    }

    public PlayerSettingsData LoadStats()
    {
        var path = Application.persistentDataPath + "/playerdata.json";
        if (!File.Exists(path)) return null;
        
        var json = File.ReadAllText(path);
        var loadedData = JsonUtility.FromJson<PlayerSettingsData>(json);
        return loadedData;
    }
}

[Serializable]
public class PlayerSettingsData
{
    public int qualityPreset;
    public bool limitingFrameRate;
    public bool doVerticalSync;
    public int frameRateLimit;
}
