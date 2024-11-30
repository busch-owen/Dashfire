using UnityEngine;

public class DefaultWeapon : IWeaponDamage
{
    public float Damage;
    public int BulletsPerShot;
    public float XSpread;
    public float YSpread;
    public float SpreadVariation;
    public float BulletDistance;

    private NetworkWeaponHandler _weaponHandler;

    public virtual void Attack(NetworkWeaponHandler handler)
    {
        handler.HitscanShotRequestRpc(BulletsPerShot, XSpread, YSpread, SpreadVariation, BulletDistance);
    }
}
