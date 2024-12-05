using UnityEngine;

public class ProjectileWeaponBase : WeaponBase
{
    [SerializeField] ProjectileWeaponSO projectileWeaponSo;
    private Transform _cameraTransform;
    private ProjectileWeapon ProjetileDamageType;

    protected override void Awake()
    {
        base.Awake();
        ProjetileDamageType = new ProjectileWeapon()
        {
            ProjectileObject = projectileWeaponSo.ProjectileObject,
            ProjectileSpeed = projectileWeaponSo.ProjectileSpeed
        };
    }
    
    public override void UseWeapon()
    {
        ProjetileDamageType.Attack();
    }
}