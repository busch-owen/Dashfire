using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance; //all private fields for properties are denoted with an underscore in front of the name
    public static T Instance //this is our property and it works by, when we first use it finding the object of the type and assigning it to the instance, and after that it simply returns the instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
            }
            return _instance;
        }
    }
}