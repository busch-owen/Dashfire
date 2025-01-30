using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundHandler : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField] private Vector2 pitchRange = new (0.9f, 1.1f);

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayClipWithRandPitch(AudioClip clip)
    {
        if (!clip) return;
        if(!_audioSource) return;
        var randPitch = Random.Range(pitchRange.x, pitchRange.y);
        _audioSource.pitch = randPitch;
        _audioSource.PlayOneShot(clip);
    }
    
    public void PlayClipWithStaticPitch(AudioClip clip)
    {
        if (!clip) return;
        if(!_audioSource) return;
        _audioSource.pitch = 1;
        _audioSource.PlayOneShot(clip);
    }
}
