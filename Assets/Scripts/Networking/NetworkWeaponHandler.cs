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

    [Rpc(SendTo.Everyone)]
    public void RequestWeaponSpawnRpc(string weaponName, ulong spawnTargetId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(spawnTargetId, out var spawnTargetObj);
        if (!spawnTargetObj) return;
        var playerController = spawnTargetObj.GetComponent<PlayerController>();
        var newWeapon = PoolManager.Instance.Spawn(weaponName);
        playerController.AssignNewWeapon(newWeapon.GetComponent<WeaponBase>());
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponShotRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger("Shoot");
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponReloadRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger("Reload");
    }
}
