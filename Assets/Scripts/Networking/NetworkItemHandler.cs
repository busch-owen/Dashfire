using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkItemHandler : NetworkBehaviour
{
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Reload = Animator.StringToHash("Reload");

    private SpawnPoint[] _spawnPoints;
    
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Transform swordCheckPos;

    #region Unity Runtime Functions
    
    private void Start()
    {
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
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
        if (!playerController.CheckPickupSimilarity(newWeapon.GetComponent<WeaponBase>()))
        {
            PoolManager.Instance.DeSpawn(newWeapon);
            return;
        }
        GameObject lastEquippedWeapon = null;
        
        if (playerController.EquippedWeapons[playerController.CurrentWeaponIndex])
        {
            lastEquippedWeapon = playerController.EquippedWeapons[playerController.CurrentWeaponIndex].gameObject;
        }
        
        playerController.AssignNewWeapon(newWeapon.GetComponent<WeaponBase>());
        
        if (lastEquippedWeapon)
        {
            lastEquippedWeapon.SetActive(false);
        }
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
            if (!weapon) continue;
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
        GetComponentInChildren<ParticleSystem>()?.Play();
        var weapon = GetComponentInChildren<WeaponBase>();
        if (!weapon) return;
        if (weapon.WeaponSO.shootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.shootSounds.Length);
            if(weapon.WeaponSO.shootSounds.Length > 0)
                weapon.GetComponent<AudioSource>()?.PlayOneShot(weapon.WeaponSO.shootSounds[randomShootSound]);
        }
        
        if (!castingPlayer.IsOwner) return;
        
        for (var i = 0; i < bulletsPerShot; i++)
        {
            //spread math
            var firePos = GetComponentInParent<Camera>().transform;
            var fireDirection = firePos.forward;
            var spread = Vector3.zero;
            spread += firePos.right * Random.Range(-xSpread, xSpread);
            spread += firePos.up * Random.Range(-ySpread, ySpread);
            fireDirection += spread.normalized * Random.Range(0, spreadVariation);
            
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
                        RequestDealDamageRpc(hitPlayer.NetworkObjectId, OwnerClientId, castingPlayer.NetworkObjectId, bulletDamage * headshotMultiplier, true);
                        PlayNormalHeadshotSound();
                        indicator.UpdateDisplay(bulletDamage, true, headshotMultiplier);
                    }
                    else if(hit.transform.GetComponent<BodyCollision>())
                    {
                        RequestDealDamageRpc(hitPlayer.NetworkObjectId, OwnerClientId, castingPlayer.NetworkObjectId, bulletDamage, false);
                        PlayNormalHitSound();
                        indicator.UpdateDisplay(bulletDamage, false, 1);
                    }

                    //RequestHealthAndArmorUpdateRpc(hitPlayer.CurrentHealth, hitPlayer.CurrentArmor, hitPlayer.NetworkObjectId);
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
    public void RequestMeleeAttackRpc(float width, float height, float depth, int damage)
    {
        var castingPlayer = GetComponentInParent<PlayerController>();
        var weapon = GetComponentInChildren<MeleeWeaponBase>();
        if(!weapon) return;
        if (weapon.WeaponSO.shootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.shootSounds.Length);
            if(weapon.WeaponSO.shootSounds.Length > 0)
                weapon.GetComponent<AudioSource>()?.PlayOneShot(weapon.WeaponSO.shootSounds[randomShootSound]);
        }
        
        if (!castingPlayer.IsOwner) return;

        var boxExtents = new Vector3(width / 2, height / 2, depth / 2);
        
        var hits = Physics.OverlapBox(swordCheckPos.position, boxExtents, swordCheckPos.rotation ,playerMask);

        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerController>())
            {
                var hitPlayer = hit.GetComponentInParent<PlayerController>();
                if(castingPlayer == hitPlayer) continue;
                WeaponShotRpc();
                if (hitPlayer) 
                {
                    var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                    indicator.transform.position = hit.transform.position;
                    indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                    RequestDealDamageRpc(hitPlayer.NetworkObjectId, OwnerClientId, castingPlayer.NetworkObjectId, damage, false);
                    indicator.UpdateDisplay(damage, false, 1);
                }

                break;
            }
        }
        
    }
    
    private void PlayNormalHitSound()
    {
        var hitPlayer = GetComponentInParent<PlayerController>();
        var randomHitSound = Random.Range(0, hitPlayer.HitSound.Length);
        if(hitPlayer.HitSound.Length > 0)
            hitPlayer.GetComponent<AudioSource>()?.PlayOneShot(hitPlayer.HitSound[randomHitSound]);
    }
    
    private void PlayNormalHeadshotSound()
    {
        var hitPlayer = GetComponentInParent<PlayerController>();
        var randomHitSound = Random.Range(0, hitPlayer.HeadShotSound.Length);
        if(hitPlayer.HitSound.Length > 0)
            hitPlayer.GetComponent<AudioSource>()?.PlayOneShot(hitPlayer.HeadShotSound[randomHitSound]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RequestProjectileFireRpc(float projectileSpeed, ulong casterId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        
        GetComponentInChildren<ParticleSystem>()?.Play();
        var weapon = GetComponentInChildren<ProjectileWeaponBase>();
        if (weapon.WeaponSO.shootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.shootSounds.Length);
            if(weapon.WeaponSO.shootSounds.Length > 0)
                weapon.GetComponent<AudioSource>()?.PlayOneShot(weapon.WeaponSO.shootSounds[randomShootSound]);
        }
        
        if (!casterObj) return;
        
        //Getting references to all necessary objects
        
        if(!IsOwner) return;
        SpawnRocketRpc(casterId, projectileSpeed);
    }

    [Rpc(SendTo.Server)]
    private void SpawnRocketRpc(ulong casterId, float projectileSpeed)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        if (!casterObj) return;
        if(!IsOwner) return;
        var weapon = GetComponentInChildren<ProjectileWeaponBase>();
        var projectileObject = weapon.ProjectileDamageType.ProjectileObject;
        var firePos = casterObj.GetComponentInChildren<FirePoint>().transform;
        var newProjectile = NetworkManager.SpawnManager.InstantiateAndSpawn
            (projectileObject, casterId, true, false, false, firePos.position, firePos.rotation);
        
        SendProjectileRpc(newProjectile.NetworkObjectId, casterId, projectileSpeed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendProjectileRpc(ulong projectileId, ulong casterId, float projectileSpeed)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out var newProjectile);
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        
        if(!newProjectile) return;
        if(!casterObj) return;
        newProjectile.GetComponent<ExplosiveProjectile>().SetCasterIds(casterObj.OwnerClientId, casterObj.NetworkObjectId);
        var firePos = casterObj.GetComponentInChildren<FirePoint>().transform;
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;
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
        NetworkManager.Singleton.ConnectedClients.TryGetValue(hitPlayerId, out var hitPlayer);
        NetworkManager.Singleton.ConnectedClients.TryGetValue(castingPlayerId, out var castingPlayer);
        if (castingPlayer == null || hitPlayer == null) return;
        if (!castingPlayer.PlayerObject) return;
        if (!hitPlayer.PlayerObject) return;
        var castObj = castingPlayer.PlayerObject;
        var hitObj = hitPlayer.PlayerObject;
        hitObj.GetComponent<PlayerData>().PlayerDeaths.Value++;
        if (castingPlayer == hitPlayer) return;
        SendKillBannerDataRpc(castObj.NetworkObjectId, hitObj.NetworkObjectId);
        castObj.GetComponent<PlayerData>().PlayerFrags.Value++;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendKillBannerDataRpc(ulong castingObjId, ulong hitObjId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(castingObjId, out var castingObj);
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hitObjId, out var hitObj);
        if(!castingObj || ! hitObj) return;
        castingObj.GetComponent<PlayerController>().DisplayKillbanner(hitObj.GetComponent<PlayerData>().PlayerName.Value.ToString());
    }
    
    #endregion

    #region Health And Armor

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestDealDamageRpc(ulong hitPlayerObjId, ulong castingPlayerClientId, ulong castingPlayerObjId, float amount, bool headshot)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(hitPlayerObjId, out var hitPlayerObj);
        if(!hitPlayerObj) return;
        
        hitPlayerObj.GetComponent<PlayerController>().TakeDamage(amount, headshot, castingPlayerClientId, castingPlayerObjId);
    }

    [Rpc(SendTo.Everyone)]
    public void RequestHealthPickupRpc(ulong playerId, int healAmount)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObj);
        if(!playerObj) return;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.HealPlayer(healAmount);
    }
    
    [Rpc(SendTo.Everyone)]
    public void RequestArmorPickupRpc(ulong playerId, int armorAmount)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObj);
        if(!playerObj) return;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.HealArmor(armorAmount);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RespawnSpecificPlayerRpc(ulong playerToRespawnId, ulong castingPlayerId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerToRespawnId, out var playerToRespawnObj);
        if (!playerToRespawnObj) return;
        var randomSpawn = Random.Range(0, _spawnPoints.Length);
        playerToRespawnObj.transform.position = _spawnPoints[randomSpawn].transform.position;
        var controller = playerToRespawnObj.GetComponent<PlayerController>();
        controller.ResetStatsRpc();
        controller.ResetVelocity();
        controller.EquippedWeapons[0].gameObject.SetActive(true);
    }

    #endregion
    
    #region Weapon Animations and Effects
    
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponShotRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        if (!assignedWeaponAnimator) return;
        assignedWeaponAnimator.SetTrigger(Shoot);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponReloadRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        if (!assignedWeaponAnimator) return;
        assignedWeaponAnimator.SetTrigger(Reload);
    }
    
    #endregion

    #region Misc Item Logic

    [Rpc(SendTo.Server)]
    public void DestroyPickupRpc(NetworkObjectReference obj)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(obj.NetworkObjectId, out var newObj);
        if (!newObj) return;
        newObj.Despawn();
        Destroy(newObj);
    }

    #endregion
}
