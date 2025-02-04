using Unity.Netcode;
using UnityEngine;

public class ProjectileWeapon : IWeaponDamage
{
    public NetworkObject ProjectileObject;
    private FirePoint _firePoint;
    
    public void Attack(NetworkItemHandler handler, ulong casterId)
    {
        handler.RequestProjectileFireRpc(casterId);
    }
}
