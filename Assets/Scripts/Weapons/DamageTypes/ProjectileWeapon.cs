using Unity.Netcode;
using UnityEngine;

public class ProjectileWeapon : IWeaponDamage
{
    public NetworkObject ProjectileObject;
    public float ProjectileSpeed;
    
    public void Attack(NetworkItemHandler handler, ulong casterId)
    {
        handler.RequestProjectileFireRpc(ProjectileSpeed, casterId);
    }
}
