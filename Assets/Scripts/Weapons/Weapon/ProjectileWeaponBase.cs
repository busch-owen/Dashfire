using UnityEngine;

public class ProjectileWeaponBase : WeaponBase
{
    [SerializeField] ProjectileWeaponSO projectileWeaponSo;
    private Transform _cameraTransform;
    public ProjectileWeapon ProjectileDamageType { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ProjectileDamageType = new ProjectileWeapon
        {
            ProjectileObject = projectileWeaponSo.ProjectileObject,
            ProjectileSpeed = projectileWeaponSo.ProjectileSpeed
        };
    }
    
    public override void UseWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        if (currentAmmo <= 0) return;
        if(!CanFire || Reloading || currentAmmo <= 0) return;
        if (WeaponSO.Automatic && !Firing)
        {
            Firing = true;
        }
        CanFire = false;
        currentAmmo--;
        animator?.SetTrigger(ShootTrigger);
        var localShake = GetComponentInParent<CameraShake>();
        localShake.Shake(WeaponSO.FireShakeMagnitude, WeaponSO.FireShakeDuration);
        ProjectileDamageType.Attack(ItemHandler, GetComponentInParent<PlayerController>().NetworkObjectId);
        Invoke(nameof(EnableFiring), WeaponSO.FireRate);
        CanvasHandler.UpdateAmmo(currentAmmo, reserve.ContainersDictionary[WeaponSO.RequiredAmmo].currentCount);
    }
}