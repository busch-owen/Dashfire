using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkWeaponHandler : NetworkBehaviour
{
    private PlayerController _controller;
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Reload = Animator.StringToHash("Reload");

    private LayerMask _playerMask;

    #region Unity Runtime Functions
    
    private void Start()
    {
        _controller = GetComponentInParent<PlayerController>();
    }
    
    #endregion

    #region Weapon Swapping and Spawning
    
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
    
    #endregion

    #region Weapon Shooting Logic

    [Rpc(SendTo.Everyone)]
    public void HitscanShotRequestRpc(int BulletsPerShot, float XSpread, float YSpread, float SpreadVariation, float BulletDistance)
    {
        _playerMask = LayerMask.GetMask("Default");

        for (var i = 0; i < BulletsPerShot; i++)
        {
            //spread math
            var firePos = GetComponentInParent<Camera>().transform;
            var fireDirection = firePos.forward;
            var spread = Vector3.zero;
            spread += firePos.right * UnityEngine.Random.Range(-XSpread, XSpread);
            spread += firePos.up * UnityEngine.Random.Range(-YSpread, YSpread);
            fireDirection += spread.normalized * UnityEngine.Random.Range(0, SpreadVariation);

            RaycastHit hit;
            if (Physics.Raycast(firePos.position, fireDirection, out hit, BulletDistance, _playerMask))
            {
                // whatever logic you wanna do on raycast hit
                Debug.Log("hit point: " + hit.point);
            }
        }
    }
    
    #endregion

    #region Weapon Animations and Effects
    
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
    
    #endregion
}
