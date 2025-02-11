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
    
    private Transform _firePoint;

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
        _firePoint = GetComponentInChildren<FirePoint>(includeInactive: false)?.transform;
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
        _firePoint = GetComponentInChildren<FirePoint>(includeInactive: false)?.transform;
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
        if (weapon.WeaponSO.ShootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.ShootSounds.Length);
            if(weapon.WeaponSO.ShootSounds.Length > 0)
                weapon.GetComponent<SoundHandler>()?.PlayClipWithRandPitch(weapon.WeaponSO.ShootSounds[randomShootSound]);
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
            var hitFirstTime = false;

            if (Physics.Raycast(firePos.position, fireDirection, out hit, bulletDistance, playerMask))
            {
                if (!hit.transform.GetComponentInParent<PlayerController>())
                {
                    hitFirstTime = false; 
                }
                else
                {
                    hitFirstTime = true; 
                    if (hit.transform.GetComponent<HeadCollision>())
                    {
                        var hitPlayer = hit.transform.gameObject.GetComponentInParent<PlayerController>();
                        var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                        indicator.transform.position = hit.point;
                        indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                        if(castingPlayer == hitPlayer) return;
                        hitPlayer.GetComponent<PlayerController>().TakeDamageRpc(bulletDamage * headshotMultiplier, true, OwnerClientId, castingPlayer.NetworkObjectId);
                        PlayNormalHeadshotSound();
                        indicator.UpdateDisplay(bulletDamage, true, headshotMultiplier);
                    }
                    else if (hit.transform.GetComponent<BodyCollision>())
                    {
                        var hitPlayer = hit.transform.gameObject.GetComponentInParent<PlayerController>();
                        var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                        indicator.transform.position = hit.point;
                        indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                        if (castingPlayer == hitPlayer) return;
                        hitPlayer.GetComponent<PlayerController>().TakeDamageRpc(bulletDamage, false, OwnerClientId,
                            castingPlayer.NetworkObjectId);
                        PlayNormalHitSound();
                        indicator.UpdateDisplay(bulletDamage, false, 1);
                    }
                }
            }
            
            if(Physics.SphereCast(firePos.position, weapon.WeaponSO.BulletRadius, fireDirection, out hit, bulletDistance, playerMask))
            {
                if (!hitFirstTime)
                {
                    if (hit.transform.GetComponent<HeadCollision>())
                    {
                        var hitPlayer = hit.transform.gameObject.GetComponentInParent<PlayerController>();
                        var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                        indicator.transform.position = hit.point;
                        indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                        if(castingPlayer == hitPlayer) return;
                        hitPlayer.GetComponent<PlayerController>().TakeDamageRpc(bulletDamage * headshotMultiplier, true, OwnerClientId, castingPlayer.NetworkObjectId);
                        PlayNormalHeadshotSound();
                        indicator.UpdateDisplay(bulletDamage, true, headshotMultiplier);
                    }
                    else if (hit.transform.GetComponent<BodyCollision>())
                    {
                        var hitPlayer = hit.transform.gameObject.GetComponentInParent<PlayerController>();
                        var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                        indicator.transform.position = hit.point;
                        indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                        if (castingPlayer == hitPlayer) return;
                        hitPlayer.GetComponent<PlayerController>().TakeDamageRpc(bulletDamage, false, OwnerClientId,
                            castingPlayer.NetworkObjectId);
                        PlayNormalHitSound();
                        indicator.UpdateDisplay(bulletDamage, false, 1);
                    }
                }
            }

            if(hit.transform)
                SpawnImpactParticlesRpc(hit.point, hit.normal, hit.transform.GetComponentInParent<PlayerController>() ? playerImpactName : objImpactName);
        }
    }
    
    [Rpc(SendTo.Everyone)]
    public void RequestMeleeAttackRpc(float width, float height, float depth, int damage)
    {
        var castingPlayer = GetComponentInParent<PlayerController>();
        var weapon = GetComponentInChildren<MeleeWeaponBase>();
        if(!weapon) return;
        if (weapon.WeaponSO.ShootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.ShootSounds.Length);
            if(weapon.WeaponSO.ShootSounds.Length > 0)
                weapon.GetComponent<SoundHandler>()?.PlayClipWithRandPitch(weapon.WeaponSO.ShootSounds[randomShootSound]);
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
                    hitPlayer.GetComponent<PlayerController>().TakeDamageRpc(damage, false, OwnerClientId, castingPlayer.NetworkObjectId);
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
            hitPlayer.GetComponent<SoundHandler>()?.PlayClipWithStaticPitch(hitPlayer.HitSound[randomHitSound]);
    }
    
    private void PlayNormalHeadshotSound()
    {
        var hitPlayer = GetComponentInParent<PlayerController>();
        var randomHitSound = Random.Range(0, hitPlayer.HeadShotSound.Length);
        if(hitPlayer.HitSound.Length > 0)
            hitPlayer.GetComponent<SoundHandler>()?.PlayClipWithStaticPitch(hitPlayer.HeadShotSound[randomHitSound]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RequestProjectileFireRpc(ulong casterId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        
        if (!casterObj) return;
        
        var playerController = casterObj.GetComponent<PlayerController>();
        //Getting references to all necessary objects
        
        GetComponentInChildren<ParticleSystem>()?.Play();
        var weapon = GetComponentInChildren<ProjectileWeaponBase>();
        if (weapon.WeaponSO.ShootSounds != null)
        {
            var randomShootSound = Random.Range(0, weapon.WeaponSO.ShootSounds.Length);
            if(weapon.WeaponSO.ShootSounds.Length > 0)
                weapon.GetComponent<SoundHandler>()?.PlayClipWithRandPitch(weapon.WeaponSO.ShootSounds[randomShootSound]);
        }
        
        if(!playerController.IsOwner) return;
        SpawnRocketRpc(casterId, _firePoint.transform.position, _firePoint.transform.rotation);
    }

    [Rpc(SendTo.Server)]
    private void SpawnRocketRpc(ulong casterId, Vector3 firePos, Quaternion fireRotation)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(casterId, out var casterObj);
        if (!casterObj) return;
        var weapon = GetComponentInChildren<ProjectileWeaponBase>();
        var projectileObject = weapon.ProjectileDamageType.ProjectileObject;
        var newProjectile = NetworkManager.SpawnManager.InstantiateAndSpawn
            (projectileObject, 0UL, true, false, false, firePos, fireRotation);
        
        newProjectile.GetComponent<ExplosiveProjectile>().SetCasterIdsRpc(casterObj.OwnerClientId, casterObj.NetworkObjectId);
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
        playerToRespawnObj.transform.rotation = _spawnPoints[randomSpawn].transform.rotation;
        var controller = playerToRespawnObj.GetComponent<PlayerController>();
        controller.ResetStatsRpc();
        controller.ResetVelocityRpc();
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
