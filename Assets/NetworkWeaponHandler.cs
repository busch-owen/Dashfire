using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkWeaponHandler : NetworkBehaviour
{
    private PlayerController _controller;
    
    private void Start()
    {
        _controller = GetComponentInParent<PlayerController>();
    }

    [Rpc(SendTo.Server)]
    public void PickupNewWeaponRpc(ulong objToPickupId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objToPickupId, out var objToPickup);
        if (!objToPickup || objToPickup.transform.parent) return;

        
        var weaponPickup = objToPickup.GetComponent<WeaponPickup>();
        var newWeapon = PoolManager.Instance.Spawn(weaponPickup.AssignedWeapon.name);
        Debug.Log("Spawned Item");
        newWeapon.GetComponent<NetworkObject>().Spawn();
        newWeapon.GetComponent<NetworkObject>().TrySetParent(_controller.transform);
    }
}
