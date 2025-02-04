using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum AmmoType
{
    Standard, Shotgun, Explosive, Energy
}

public class AmmoReserve : MonoBehaviour
{
    public Dictionary<AmmoType, AmmoContainer> ContainersDictionary = new();
    [SerializeField] private AmmoContainer[] ammoTypes;

    private void Awake()
    {
        foreach (var type in ammoTypes)
        {
            ContainersDictionary.Add(type.type, type);
            ContainersDictionary[type.type].ResetAmmo();
        }
    }
}

[Serializable]
public class AmmoContainer
{
    public string name;
    public AmmoType type;
    public int maxCount;
    public int currentCount;
    public GameObject visualObject;
    public int respawnAmount;

    public void AddToAmmo(int amount)
    {
        currentCount += amount;
        if(currentCount <= maxCount) return;
        currentCount = maxCount;
    }

    public void ResetAmmo()
    {
        currentCount = respawnAmount;
    }
}
