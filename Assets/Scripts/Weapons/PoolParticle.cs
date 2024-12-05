using Unity.Netcode;
using UnityEngine;

public class PoolParticle : NetworkBehaviour
{
    [SerializeField] private GameObject prefabRef;
    private ParticleSystem _particle;
    private void OnEnable()
    {
        CancelInvoke(nameof(OnDeSpawn));
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        Invoke(nameof(OnDeSpawn), _particle.main.duration * 2);
    }

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
