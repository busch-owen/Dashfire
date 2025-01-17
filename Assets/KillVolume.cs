using System;
using UnityEngine;
using Unity.Netcode;

public class KillVolume : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc) return;
        
        KillPlayerRpc(pc.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void KillPlayerRpc(ulong objectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var playerObj);
        if (!playerObj) return;
        var controller = playerObj.GetComponent<PlayerController>();
        controller.TakeDamage(9999, OwnerClientId, NetworkObjectId);
    }
}
