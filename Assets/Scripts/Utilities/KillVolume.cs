using System;
using UnityEngine;
using Unity.Netcode;

public class KillVolume : NetworkBehaviour
{
    private bool _onCooldown;
    [SerializeField] private float cooldownTime;
    
    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc) return;
        if(_onCooldown) return;
        KillPlayerRpc(pc.NetworkObjectId);
        _onCooldown = true;
        Invoke(nameof(RemoveCooldown), cooldownTime);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void KillPlayerRpc(ulong objectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var playerObj);
        if (!playerObj) return;
        var controller = playerObj.GetComponent<PlayerController>();
        controller.TakeDamageRpc(9999, false, OwnerClientId, NetworkObjectId);
    }

    private void RemoveCooldown()
    {
        _onCooldown = false;
    }
}
