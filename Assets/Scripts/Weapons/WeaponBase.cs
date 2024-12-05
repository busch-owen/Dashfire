using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponBase : NetworkBehaviour
{
    public Animator animator;

    [field: SerializeField] public WeaponBaseSO WeaponSO { get; private set; }
    
    public static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    public static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    protected PlayerController OwnerObject;

    public int currentAmmo;

    protected bool Firing;
    protected bool CanFire = true;
    protected bool Reloading;

    protected IWeaponDamage DamageType;
    
    protected NetworkItemHandler ItemHandler;
    [FormerlySerializedAs("_canvasHandler")] public PlayerCanvasHandler CanvasHandler;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    protected virtual void Awake()
    {
        
        DamageType ??= new DefaultWeapon()
        {
            Damage = WeaponSO.Damage,
            HeadshotMultiplier = WeaponSO.HeadshotMultiplier,
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
        ItemHandler = GetComponentInParent<NetworkItemHandler>();
        animator = GetComponentInChildren<Animator>();
        CanvasHandler = OwnerObject?.GetComponentInChildren<PlayerCanvasHandler>();
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
        
        if (currentAmmo <= 0 && !Reloading)
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
        if(!CanFire || Reloading || currentAmmo <= 0) return;
        if (WeaponSO.Automatic && !Firing)
        {
            Firing = true;
        }
        CanFire = false;
        currentAmmo--;
        animator?.SetTrigger(ShootTrigger);
        ItemHandler.WeaponShotRpc();
        //Reloads weapon automatically if below 0 bullets
        DamageType.Attack(ItemHandler);
        Invoke(nameof(EnableFiring), WeaponSO.FireRate);
        CanvasHandler.UpdateAmmo(currentAmmo, WeaponSO.AmmoCount);
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
        if(!CanFire || currentAmmo == WeaponSO.AmmoCount || Reloading) return;
        Reloading = true;
        animator?.SetTrigger(ReloadTrigger);
        ItemHandler.WeaponReloadRpc();
        Invoke(nameof(AmmoReload), WeaponSO.ReloadTime);
    }
    
    protected virtual void EnableFiring()
    {
        CanFire = true;
    }

    protected virtual void AmmoReload()
    {
        currentAmmo = WeaponSO.AmmoCount;
        Reloading = false;
        CanFire = true;
        CanvasHandler.UpdateAmmo(currentAmmo, WeaponSO.AmmoCount);
    }

    public void ResetAmmo()
    {
        currentAmmo = WeaponSO.AmmoCount;
    }
}
