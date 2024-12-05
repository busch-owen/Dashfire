using UnityEngine;

public class ProjectileWeapon : IWeaponDamage
{
    public GameObject ProjectileObject;
    public float ProjectileSpeed;
    
    public void Attack(NetworkItemHandler handler, ulong casterId)
    {
        handler.RequestProjectileFireRpc(ProjectileObject.name, ProjectileSpeed, casterId);
    }
}
