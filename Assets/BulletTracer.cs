using System;
using UnityEngine;

public class BulletTracer : PoolObject
{
    private ParticleSystem _particleSystem;

    private void OnEnable()
    {
        _particleSystem ??= GetComponent<ParticleSystem>();
        Invoke(nameof(OnDeSpawn), _particleSystem.main.startLifetime.constant);
    }
}
