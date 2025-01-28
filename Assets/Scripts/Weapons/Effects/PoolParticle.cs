using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PoolParticle : NetworkBehaviour
{
    [SerializeField] private GameObject prefabRef;
    private ParticleSystem _particle;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CancelInvoke(nameof(OnDeSpawnRpc));
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        Invoke(nameof(OnDeSpawnRpc), _particle.main.duration * 4);
    }

    [Rpc(SendTo.Server)]
    private void OnDeSpawnRpc()
    {
        NetworkObject.Despawn();
    }
}
