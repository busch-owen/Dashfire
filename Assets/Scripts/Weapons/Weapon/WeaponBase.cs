using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WeaponBase : NetworkBehaviour
{
    public Animator animator;

    [field: SerializeField] public WeaponBaseSO WeaponSO { get; private set; }
    
    public static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    public static readonly int ReloadTrigger = Animator.StringToHash("Reload");
    public static readonly int Equip = Animator.StringToHash("Equip");

    protected PlayerController OwnerObject;

    public int currentAmmo;

    protected bool Firing;
    protected bool CanFire = true;
    [field: SerializeField] public bool CanADS { get; private set; }
    protected bool Reloading;

    protected IWeaponDamage DamageType;
    
    protected NetworkItemHandler ItemHandler;
    public PlayerCanvasHandler CanvasHandler;

    [SerializeField] private Transform adsPosition;
    
    [SerializeField] private GameObject visualObject;
    protected CameraController CameraController;

    public AmmoReserve reserve;

    private Vector3 _weaponStartPos;
    
    [field: SerializeField] public Transform RightHandPos { get; private set; }
    [field: SerializeField] public Transform LeftHandPos { get; private set; }

    public bool AimDownSights { get; private set; }

    private SoundHandler _soundHandler;

    #region Unity Runtime Functions

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
        animator.keepAnimatorStateOnDisable = true;
        CameraController = GetComponentInParent<CameraController>();
        reserve = GetComponentInParent<AmmoReserve>();
        CanvasHandler = OwnerObject?.GetComponentInChildren<PlayerCanvasHandler>();
        CanFire = true;
        Reloading = false;
        if (visualObject)
            _weaponStartPos = visualObject.transform.localPosition;
        CanvasHandler?.SwapCrosshairImage(WeaponSO.CrosshairSprite);
        CanvasHandler?.UpdateWeaponVisuals();
        PlayEquipSound();
    }

    protected virtual void OnEnable()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        _soundHandler ??= GetComponent<SoundHandler>();
        Firing = false;
        animator ??= GetComponentInChildren<Animator>();
        animator.SetTrigger(Equip);
        CanvasHandler?.SwapCrosshairImage(WeaponSO.CrosshairSprite);
        PlayEquipSound();
    }

    protected virtual void OnDisable()
    {
        ReloadWeapon();
        AimDownSights = false;
        SensitivityHandler.Instance.ResetSens();
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

        if(!visualObject || !adsPosition) return;

        if (!OwnerObject.IsOwner) return;
        if (AimDownSights)
        {
            visualObject.transform.localPosition = Vector3.Lerp(visualObject.transform.localPosition, adsPosition.localPosition,
                WeaponSO.ADSSpeed * Time.deltaTime);
            CameraController.GetComponentInChildren<Camera>().fieldOfView = Mathf.Lerp(CameraController.GetComponentInChildren<Camera>().fieldOfView, WeaponSO.ADSFov, WeaponSO.ADSSpeed * Time.deltaTime);
        }
        else
        {
            visualObject.transform.localPosition = Vector3.Lerp(visualObject.transform.localPosition, _weaponStartPos,
            WeaponSO.ADSSpeed * Time.deltaTime);
            CameraController.GetComponentInChildren<Camera>().fieldOfView = Mathf.Lerp(CameraController.GetComponentInChildren<Camera>().fieldOfView, 90f, WeaponSO.ADSSpeed * Time.deltaTime);
        }
    }

    #endregion
    
    #region Weapon Functionality
    
    public virtual void UseWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        //This simply handles the math and animations of shooting/using a weapon
        if(!CanFire || Reloading || currentAmmo <= 0) return;
        
        if (WeaponSO.Automatic && !Firing)
        {
            Firing = true;
        }
        currentAmmo--;
        CanFire = false;
        animator?.SetTrigger(ShootTrigger);
        Invoke(nameof(EnableFiring), WeaponSO.FireRate);
        var localShake = GetComponentInParent<CameraShake>();
        localShake.Shake(WeaponSO.FireShakeMagnitude, WeaponSO.FireShakeDuration);
        ItemHandler.WeaponShotRpc();
        DamageType.Attack(ItemHandler);

        CanvasHandler.UpdateAmmo(currentAmmo, reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount, false);
    }

    public void ADS(bool state)
    {
        AimDownSights = state;
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
        if (!OwnerObject) return;
        if (!OwnerObject.IsOwner) return;
        if(!CanFire || currentAmmo == WeaponSO.AmmoCount || Reloading) return;
        
        if (reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount <= 0) return;
        
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
        if (WeaponSO.AmmoCount > reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount)
        {
            currentAmmo = reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount;
            reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount = 0;
        }
        else if (currentAmmo > 0)
        {
            var amountToFill = WeaponSO.AmmoCount - currentAmmo;
            currentAmmo = WeaponSO.AmmoCount;
            reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount -= amountToFill;
        }
        else
        {
            currentAmmo = WeaponSO.AmmoCount;
            reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount -= WeaponSO.AmmoCount;
        }
        
        Reloading = false;
        CanFire = true;
        if(isActiveAndEnabled)
            CanvasHandler.UpdateAmmo(currentAmmo, reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount, false);
    }

    public void ResetAmmo()
    {
        currentAmmo = WeaponSO.AmmoCount;
    }
    
    #endregion
    
    private void PlayEquipSound()
    {
        if(WeaponSO.EquipSounds == null) return;
        if(WeaponSO.EquipSounds.Length == 0) return;
        var randomHitSound = Random.Range(0, WeaponSO.EquipSounds.Length);
        GetComponent<SoundHandler>()?.PlayClipWithRandPitch(WeaponSO.EquipSounds[randomHitSound]);
    }
}
