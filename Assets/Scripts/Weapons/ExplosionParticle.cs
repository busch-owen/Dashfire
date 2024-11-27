using System;
using Unity.Netcode;
using UnityEngine;

public class ExplosionParticle : NetworkBehaviour
{
    private ParticleSystem _particle;
    private void OnEnable()
    {
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        Invoke(nameof(OnDeSpawn), _particle.main.duration * 2);
    }

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
