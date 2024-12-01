using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkItemHandler : NetworkBehaviour
{
    private PlayerController _controller;
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Reload = Animator.StringToHash("Reload");

    [SerializeField] private LayerMask playerMask;

    #region Unity Runtime Functions
    
    private void Start()
    {
        _controller = GetComponentInParent<PlayerController>();
    }
    
    #endregion

    #region Weapon Swapping and Spawning
    
    [Rpc(SendTo.Everyone)]
    public void RequestWeaponSpawnRpc(string weaponName, ulong spawnTargetId, ulong pickupObjId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(spawnTargetId, out var spawnTargetObj);
        if (!spawnTargetObj) return;
        var playerController = spawnTargetObj.GetComponent<PlayerController>();
        var newWeapon = PoolManager.Instance.Spawn(weaponName);
        newWeapon.GetComponent<WeaponBase>().ResetAmmo();
        playerController.AssignNewWeapon(newWeapon.GetComponent<WeaponBase>());
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(pickupObjId, out var pickupObj);
        Destroy(pickupObj?.gameObject);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestWeaponSwapRpc(int newWeaponIndex, ulong spawnTargetId)
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

    [Rpc(SendTo.ClientsAndHost)]
    public void HitscanShotRequestRpc(int bulletsPerShot, int bulletDamage, float xSpread, float ySpread, float spreadVariation, float bulletDistance, string objImpactName, string playerImpactName)
    {
        for (var i = 0; i < bulletsPerShot; i++)
        {
            //spread math
            var firePos = GetComponentInParent<Camera>().transform;
            var fireDirection = firePos.forward;
            var spread = Vector3.zero;
            spread += firePos.right * UnityEngine.Random.Range(-xSpread, xSpread);
            spread += firePos.up * UnityEngine.Random.Range(-ySpread, ySpread);
            fireDirection += spread.normalized * UnityEngine.Random.Range(0, spreadVariation);

            RaycastHit hit;
            if (Physics.Raycast(firePos.position, fireDirection, out hit, bulletDistance, playerMask))
            {
                var hitPlayer = hit.transform.gameObject.GetComponent<PlayerController>();
                if (hitPlayer)
                {
                    hitPlayer.TakeDamage(bulletDamage);
                    RequestHealthAndArmorUpdateRpc(hitPlayer.CurrentHealth, hitPlayer.CurrentArmor, hitPlayer.NetworkObjectId);
                    
                    SpawnImpactParticlesRpc(hit.point, hit.normal, playerImpactName);
                }
                else
                {
                    SpawnImpactParticlesRpc(hit.point, hit.normal, objImpactName);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnImpactParticlesRpc(Vector3 hitPoint, Vector3 normalDir, string effectName)
    {
        var hitEffect = PoolManager.Instance.Spawn(effectName);
        hitEffect.transform.position = hitPoint;
        hitEffect.transform.forward = normalDir;
    }
    
    #endregion

    #region Health And Armor

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestHealthAndArmorUpdateRpc(int health, int armor, ulong playerId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObj);
        if(!playerObj) return;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.SetStats(health, armor);
        
    }

    [Rpc(SendTo.Everyone)]
    public void RequestHealthPickupRpc(ulong playerId, int healAmount, ulong pickupObjId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObj);
        if(!playerObj) return;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.HealPlayer(healAmount);
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(pickupObjId, out var pickupObj);
        Destroy(pickupObj?.gameObject);
    }
    
    [Rpc(SendTo.Everyone)]
    public void RequestArmorPickupRpc(ulong playerId, int armorAmount, ulong pickupObjId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObj);
        if(!playerObj) return;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.HealArmor(armorAmount);
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(pickupObjId, out var pickupObj);
        Destroy(pickupObj?.gameObject);
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
