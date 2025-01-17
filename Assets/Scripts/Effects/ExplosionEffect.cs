using System;
using Unity.Netcode;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioSource audioSource;
    
    private void OnEnable()
    {
        audioSource.PlayOneShot(explosionClip);
    }
}
