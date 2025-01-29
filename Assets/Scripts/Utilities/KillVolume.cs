using System;
using UnityEngine;
using Unity.Netcode;

public class KillVolume : NetworkBehaviour
{
    [SerializeField] private float cooldownTime;
    
    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc) return;
        if(!pc.IsOwner) return;
        KillPlayerRpc(pc.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void KillPlayerRpc(ulong objectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var playerObj);
        if (!playerObj) return;
        var controller = playerObj.GetComponent<PlayerController>();
        controller.TakeDamageRpc(9999, false, OwnerClientId, NetworkObjectId);
    }
    
}
