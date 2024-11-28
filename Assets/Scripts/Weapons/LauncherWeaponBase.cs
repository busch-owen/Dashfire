using UnityEngine;

public class LauncherWeaponBase : WeaponBase
{
    [SerializeField] private LauncherWeaponSO launcherWeaponSO;
    
    public override void UseWeapon()
    {
       base.UseWeapon();
       if(!CanFire || Reloading || CurrentAmmo <= 0) return;
       OwnerObject ??= GetComponentInParent<PlayerController>();
       var cameraTransform = OwnerObject.GetComponentInChildren<Camera>().transform;
       Debug.LogFormat($"Owner exists: {OwnerObject}, Camera Exists: {cameraTransform}");
       OwnerObject?.AddForceInVector(-cameraTransform.forward * launcherWeaponSO.LaunchForce);
    }
}
