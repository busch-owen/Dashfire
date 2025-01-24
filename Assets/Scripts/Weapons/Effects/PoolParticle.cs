using Unity.Netcode;
using UnityEngine;

public class PoolParticle : NetworkBehaviour
{
    [SerializeField] private GameObject prefabRef;
    private ParticleSystem _particle;
    public override void OnNetworkSpawn()
    {
        CancelInvoke(nameof(OnDeSpawn));
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        Invoke(nameof(OnDeSpawn), _particle.main.duration * 4);
    }

    private void OnDeSpawn()
    {
        NetworkObject.Despawn();
    }
}
