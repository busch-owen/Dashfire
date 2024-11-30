using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkWeaponHandler : NetworkBehaviour
{
    private PlayerController _controller;
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Reload = Animator.StringToHash("Reload");

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
        newWeapon.GetComponent<WeaponBase>().ResetAmmo();
        playerController.AssignNewWeapon(newWeapon.GetComponent<WeaponBase>());
    }

    [Rpc(SendTo.Everyone)]
    public void RequestWeaponSwapRpc(string newObjName, int newWeaponIndex, ulong spawnTargetId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(spawnTargetId, out var spawnTargetObj);
        if(!spawnTargetObj) return;
        var playerController = spawnTargetObj.GetComponent<PlayerController>();
        //Change this to be enabling and disabling instead of spawning and despawning
        var assignedWeapons = playerController.EquippedWeapons;
        foreach (var weapon in assignedWeapons)
        {
            weapon.gameObject.SetActive(false);
        }
        assignedWeapons[newWeaponIndex].gameObject.SetActive(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponShotRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger(Shoot);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponReloadRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger(Reload);
    }
}
