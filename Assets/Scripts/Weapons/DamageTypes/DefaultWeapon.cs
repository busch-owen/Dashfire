using UnityEngine;

public class DefaultWeapon : IWeaponDamage
{
    public int Damage;
    public int BulletsPerShot;
    public float XSpread;
    public float YSpread;
    public float SpreadVariation;
    public float BulletDistance;

    public GameObject objHitEffect;
    public GameObject playerHitEffect;

    private NetworkWeaponHandler _weaponHandler;

    public virtual void Attack(NetworkWeaponHandler handler)
    {
        handler.HitscanShotRequestRpc(BulletsPerShot, Damage, XSpread, YSpread, SpreadVariation, BulletDistance, objHitEffect.name, playerHitEffect.name);
    }
}
