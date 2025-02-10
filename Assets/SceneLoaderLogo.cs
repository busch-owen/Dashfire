using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderLogo : MonoBehaviour
{
    [SerializeField] private float loadTime;

    private void Start()
    {
        Invoke(nameof(LoadInitialize), loadTime);
    }

    private void LoadInitialize()
    {
        SceneManager.LoadScene("Initialize");
    }
}
