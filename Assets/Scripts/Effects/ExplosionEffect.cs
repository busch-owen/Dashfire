using System;
using Unity.Netcode;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private SoundHandler audioSource;
    
    private void OnEnable()
    {
        audioSource.PlayClipWithRandPitch(explosionClip);
    }
}
