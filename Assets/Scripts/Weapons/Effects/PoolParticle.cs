using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PoolParticle : NetworkBehaviour
{
    [SerializeField] private GameObject prefabRef;
    private ParticleSystem _particle;
    public override void OnNetworkSpawn()
    {
        _particle ??= GetComponent<ParticleSystem>();
        _particle.Play();
        StartCoroutine(WaitForDespawn());
    }

    private IEnumerator WaitForDespawn()
    {
        yield return new WaitForSeconds(_particle.main.duration * 4);
        OnDeSpawnRpc();
    }

    [Rpc(SendTo.Server)]
    private void OnDeSpawnRpc()
    {
        NetworkObject.Despawn();
    }
}
