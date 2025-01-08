using System;
using System.Collections.Generic;
using UnityEngine;

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
}
