using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponBase : NetworkBehaviour
{
    private Animator _animator;

    [SerializeField] private WeaponBaseSO weaponSO;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    private PlayerController _ownerObject;

    private int _currentAmmo;

    private bool _firing;
    private bool _canFire = true;
    private bool _reloading;
    
    private NetworkWeaponHandler _weaponHandler;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        
    }

    private void Start()
    {
        _ownerObject = GetComponentInParent<PlayerController>();
        _weaponHandler = GetComponentInParent<NetworkWeaponHandler>();
    }

    private void OnEnable()
    {
        _ownerObject = GetComponentInParent<PlayerController>();
        _currentAmmo = weaponSO.AmmoCount;
    }

    private void Update()
    {
        
        if (_currentAmmo <= 0 && !_reloading)
        {
            ReloadWeapon();
        }
        if (_firing)
        {
            UseWeapon();
        }
    }

    //Action functions will only play animations for the moment
    public void UseWeapon()
    {
        if (!_ownerObject.IsOwner) return;
        //This simply handles the math and animations of shooting/using a weapon
        if(!_canFire || _reloading || _currentAmmo <= 0) return;
        if (weaponSO.Automatic && !_firing)
        {
            _firing = true;
        }
        _canFire = false;
        _currentAmmo--;
        _animator?.SetTrigger(ShootTrigger);
        _weaponHandler.WeaponShotRpc();
        //Reloads weapon automatically if below 0 bullets
        weaponSO.Attack();
        Invoke(nameof(EnableFiring), weaponSO.FireRate);
    }

    public void CancelFire()
    {
        if (!_ownerObject.IsOwner) return;
        if (weaponSO.Automatic)
        {
            _firing = false;
        }
    }

    public void ReloadWeapon()
    {
        if (!_ownerObject.IsOwner) return;
        if(!_canFire || _currentAmmo == weaponSO.AmmoCount || _reloading) return;
        _reloading = true;
        _animator?.SetTrigger(ReloadTrigger);
        _weaponHandler.WeaponReloadRpc();
        Invoke(nameof(AmmoReload), weaponSO.ReloadTime);
    }
    
    private void EnableFiring()
    {
        _canFire = true;
    }

    private void AmmoReload()
    {
        _currentAmmo = weaponSO.AmmoCount;
        _reloading = false;
        _canFire = true;
    }
}
