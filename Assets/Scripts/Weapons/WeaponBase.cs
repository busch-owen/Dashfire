using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    private Animator _animator;

    [SerializeField] private WeaponBaseSO weaponSO;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    protected PlayerController OwnerObject;

    protected int CurrentAmmo;

    protected bool Firing;
    protected bool CanFire = true;
    protected bool Reloading;

    protected IWeaponDamage DamageType;
    
    private NetworkWeaponHandler _weaponHandler;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    protected virtual void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        DamageType ??= new DefaultWeapon()
        {
            Damage = weaponSO.Damage,
            BulletsPerShot = weaponSO.BulletsPerShot,
            XSpread = weaponSO.XSpread,
            YSpread = weaponSO.YSpread,
            SpreadVariation = weaponSO.SpreadVariation,
            BulletDistance = weaponSO.BulletDistance
        };
    }

    protected virtual void Start()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        _weaponHandler = GetComponentInParent<NetworkWeaponHandler>();
    }

    protected virtual void OnEnable()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        CurrentAmmo = weaponSO.AmmoCount;
    }

    protected virtual void Update()
    {
        
        if (CurrentAmmo <= 0 && !Reloading)
        {
            ReloadWeapon();
        }
        if (Firing)
        {
            UseWeapon();
        }
    }

    //Action functions will only play animations for the moment
    public virtual void UseWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        //This simply handles the math and animations of shooting/using a weapon
        if(!CanFire || Reloading || CurrentAmmo <= 0) return;
        if (weaponSO.Automatic && !Firing)
        {
            Firing = true;
        }
        CanFire = false;
        CurrentAmmo--;
        _animator?.SetTrigger(ShootTrigger);
        _weaponHandler.WeaponShotRpc();
        //Reloads weapon automatically if below 0 bullets
        DamageType.Attack();
        Invoke(nameof(EnableFiring), weaponSO.FireRate);
    }

    public virtual void CancelFire()
    {
        if (!OwnerObject.IsOwner) return;
        if (weaponSO.Automatic)
        {
            Firing = false;
        }
    }

    public virtual void ReloadWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        if(!CanFire || CurrentAmmo == weaponSO.AmmoCount || Reloading) return;
        Reloading = true;
        _animator?.SetTrigger(ReloadTrigger);
        _weaponHandler.WeaponReloadRpc();
        Invoke(nameof(AmmoReload), weaponSO.ReloadTime);
    }
    
    protected virtual void EnableFiring()
    {
        CanFire = true;
    }

    protected virtual void AmmoReload()
    {
        CurrentAmmo = weaponSO.AmmoCount;
        Reloading = false;
        CanFire = true;
    }
}
