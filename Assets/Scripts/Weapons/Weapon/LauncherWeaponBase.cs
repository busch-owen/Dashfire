using UnityEngine;

public class LauncherWeaponBase : WeaponBase
{
    [SerializeField] private LauncherWeaponSO launcherWeaponSO;
    private Transform _cameraTransform;
    
    public override void UseWeapon()
    { 
        if(!OwnerObject.IsOwner) return;
        _cameraTransform ??= OwnerObject.GetComponentInChildren<Camera>().transform;
        if(!CanFire || Reloading || currentAmmo <= 0) return;
        OwnerObject?.AddForceInVectorRpc(-_cameraTransform.forward * launcherWeaponSO.LaunchForce);
        base.UseWeapon();
    }
}
