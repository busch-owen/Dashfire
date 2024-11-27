using System;
using Unity.Netcode;
using UnityEngine;

public class BulletTracer : NetworkBehaviour
{
    private ParticleSystem _particleSystem;

    private void OnEnable()
    {
        _particleSystem ??= GetComponent<ParticleSystem>();
        Invoke(nameof(OnDeSpawn), _particleSystem.main.startLifetime.constant);
    }

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
