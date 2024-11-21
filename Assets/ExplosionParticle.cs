using System;
using UnityEngine;

public class ExplosionParticle : PoolObject
{
    private ParticleSystem _particle;
    private void OnEnable()
    {
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        Invoke(nameof(OnDeSpawn), _particle.main.duration * 2);
    }
}
