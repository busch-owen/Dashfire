using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    private Animator _animator;

    [field: SerializeField] public WeaponBaseSO WeaponSO { get; private set; }
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    protected PlayerController OwnerObject;

    protected int CurrentAmmo;

    protected bool Firing;
    protected bool CanFire = true;
    protected bool Reloading;

    protected IWeaponDamage DamageType;
    
    private NetworkItemHandler _itemHandler;
    private PlayerCanvasHandler _canvasHandler;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    protected virtual void Awake()
    {
        
        DamageType ??= new DefaultWeapon()
        {
            Damage = WeaponSO.Damage,
            BulletsPerShot = WeaponSO.BulletsPerShot,
            XSpread = WeaponSO.XSpread,
            YSpread = WeaponSO.YSpread,
            SpreadVariation = WeaponSO.SpreadVariation,
            BulletDistance = WeaponSO.BulletDistance,
            objHitEffect = WeaponSO.objHitEffect,
            playerHitEffect = WeaponSO.playerHitEffect
        };
    }

    protected virtual void Start()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        _itemHandler = GetComponentInParent<NetworkItemHandler>();
        _animator = GetComponentInChildren<Animator>();
        _canvasHandler = OwnerObject.GetComponentInChildren<PlayerCanvasHandler>();
    }

    protected virtual void OnEnable()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        Firing = false;
        CanFire = true;
        Reloading = false;
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
        if (WeaponSO.Automatic && !Firing)
        {
            Firing = true;
        }
        CanFire = false;
        CurrentAmmo--;
        _animator?.SetTrigger(ShootTrigger);
        _itemHandler.WeaponShotRpc();
        //Reloads weapon automatically if below 0 bullets
        DamageType.Attack(_itemHandler);
        Invoke(nameof(EnableFiring), WeaponSO.FireRate);
        _canvasHandler.UpdateAmmo(CurrentAmmo, WeaponSO.AmmoCount);
    }

    public virtual void CancelFire()
    {
        if (!OwnerObject.IsOwner) return;
        if (WeaponSO.Automatic)
        {
            Firing = false;
        }
    }

    public virtual void ReloadWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        if(!CanFire || CurrentAmmo == WeaponSO.AmmoCount || Reloading) return;
        Reloading = true;
        _animator?.SetTrigger(ReloadTrigger);
        _itemHandler.WeaponReloadRpc();
        Invoke(nameof(AmmoReload), WeaponSO.ReloadTime);
    }
    
    protected virtual void EnableFiring()
    {
        CanFire = true;
    }

    protected virtual void AmmoReload()
    {
        CurrentAmmo = WeaponSO.AmmoCount;
        Reloading = false;
        CanFire = true;
        _canvasHandler.UpdateAmmo(CurrentAmmo, WeaponSO.AmmoCount);
    }

    public void ResetAmmo()
    {
        CurrentAmmo = WeaponSO.AmmoCount;
    }
}
