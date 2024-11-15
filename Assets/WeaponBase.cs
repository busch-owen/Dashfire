using System;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private WeaponBaseSO weaponSO;
    [SerializeField] private ParticleSystem muzzleFlash;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");

    private int _currentAmmo;
    
    private bool _canFire = true;
    private bool _reloading = false;
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _currentAmmo = weaponSO.AmmoCount;
    }

    private void Update()
    {
        //Reloads weapon automatically if below 0 bullets
        if (_currentAmmo <= 0 && !_reloading)
        {
            ReloadWeapon();
        }
    }

    //Action functions will only play animations for the moment
    public void UseWeapon()
    {
        //This simply handles the math and animations of shooting/using a weapon
        if(!_canFire || _reloading || _currentAmmo <= 0) return;
        _canFire = false;
        _currentAmmo--;
        _animator.SetTrigger(ShootTrigger);
        muzzleFlash.Play();
        weaponSO.Attack();
        Invoke(nameof(EnableFiring), weaponSO.FireRate);
    }

    public void ReloadWeapon()
    {
        if(!_canFire || _currentAmmo == weaponSO.AmmoCount || _reloading) return;
        _reloading = true;
        _animator.SetTrigger(ReloadTrigger);
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
