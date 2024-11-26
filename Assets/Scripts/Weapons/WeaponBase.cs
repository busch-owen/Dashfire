using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponBase : NetworkBehaviour
{
    private Animator _animator;

    [SerializeField] private WeaponBaseSO weaponSO;
    private ParticleSystem _muzzleFlash;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    private int _currentAmmo;

    private bool _firing;
    private bool _canFire = true;
    private bool _reloading;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    private void Start()
    {
        var newGunObject = Instantiate(weaponSO.GunObject, transform);
        _animator = newGunObject.GetComponent<Animator>();
        _muzzleFlash = newGunObject.GetComponentInChildren<ParticleSystem>();
        _currentAmmo = weaponSO.AmmoCount;
    }

    private void Update()
    {
        if (!IsOwner) return;
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
        //This simply handles the math and animations of shooting/using a weapon
        if(!_canFire || _reloading || _currentAmmo <= 0) return;
        if (weaponSO.Automatic && !_firing)
        {
            _firing = true;
        }
        _canFire = false;
        _currentAmmo--;
        _animator?.SetTrigger(ShootTrigger);
        //Reloads weapon automatically if below 0 bullets
        _muzzleFlash?.Play();
        weaponSO.Attack();
        Invoke(nameof(EnableFiring), weaponSO.FireRate);
    }

    public void CancelFire()
    {
        if (weaponSO.Automatic)
        {
            _firing = false;
        }
    }

    public void ReloadWeapon()
    {
        if(!_canFire || _currentAmmo == weaponSO.AmmoCount || _reloading) return;
        _reloading = true;
        _animator?.SetTrigger(ReloadTrigger);
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
