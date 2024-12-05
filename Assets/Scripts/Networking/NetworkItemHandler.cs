using Unity.Netcode;
using UnityEngine;

public class NetworkItemHandler : NetworkBehaviour
{
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Reload = Animator.StringToHash("Reload");

    private SpawnPoint[] _spawnPoints;
    
    [SerializeField] private LayerMask playerMask;

    #region Unity Runtime Functions
    
    private void Start()
    {
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
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

    [Rpc(SendTo.Everyone)]
    public void HitscanShotRequestRpc(int bulletsPerShot, int bulletDamage, float headshotMultiplier, float xSpread, float ySpread, float spreadVariation, float bulletDistance, string objImpactName, string playerImpactName)
    {
        var castingPlayer = GetComponentInParent<PlayerController>();
        if (!castingPlayer.IsOwner) return;
        
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
                
                var hitPlayer = hit.transform.gameObject.GetComponentInParent<PlayerController>();
                if(castingPlayer == hitPlayer) return;
                if (hitPlayer) 
                {
                    var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                    indicator.transform.position = hit.point;
                    indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                    if (hit.transform.GetComponent<HeadCollision>())
                    {
                        hitPlayer.TakeDamage(bulletDamage * headshotMultiplier, castingPlayer.OwnerClientId);
                        indicator.UpdateDisplay(bulletDamage, true, headshotMultiplier);
                    }
                    else if(hit.transform.GetComponent<BodyCollision>())
                    {
                        hitPlayer.TakeDamage(bulletDamage, castingPlayer.OwnerClientId);
                        indicator.UpdateDisplay(bulletDamage, false, 1);
                    }
                    
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

    [Rpc(SendTo.Everyone)]
    public void RequestProjectileFireRpc(string projectileObjectName, float projectileSpeed, ulong casterId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        //Getting references to all necessary objects
        var newProjectile = PoolManager.Instance.Spawn(projectileObjectName);
        var firePos = casterObj.GetComponentInChildren<FirePoint>().transform;
        newProjectile.transform.position = firePos.position;
        newProjectile.transform.rotation = firePos.transform.rotation;
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(firePos.transform.forward * projectileSpeed, ForceMode.Impulse);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnImpactParticlesRpc(Vector3 hitPoint, Vector3 normalDir, string effectName)
    {
        var hitEffect = PoolManager.Instance.Spawn(effectName);
        hitEffect.transform.position = hitPoint;
        hitEffect.transform.forward = normalDir;
    }

    [Rpc(SendTo.Server)]
    public void UpdateScoreboardAmountsOnKillRpc(ulong hitPlayerId, ulong castingPlayerId)
    {
        NetworkManager.Singleton.ConnectedClients[hitPlayerId].PlayerObject.GetComponent<PlayerData>().PlayerDeaths.Value++;
        NetworkManager.Singleton.ConnectedClients[castingPlayerId].PlayerObject.GetComponent<PlayerData>().PlayerFrags.Value++;
        Debug.Log("Scoreboard Updated");
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

    [Rpc(SendTo.ClientsAndHost)]
    public void RespawnSpecificPlayerRpc(ulong playerToRespawnId, ulong castingPlayerId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerToRespawnId, out var playerToRespawnObj);
        if (!playerToRespawnObj) return;
        var randomSpawn = Random.Range(0, _spawnPoints.Length);
        playerToRespawnObj.transform.position = _spawnPoints[randomSpawn].transform.position;
        var controller = playerToRespawnObj.GetComponent<PlayerController>();
        controller.ResetStats();
        if(!controller.IsOwner) return;
        UpdateScoreboardAmountsOnKillRpc(controller.OwnerClientId, castingPlayerId);
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
