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

    private NetworkItemHandler _itemHandler;

    public virtual void Attack(NetworkItemHandler handler)
    {
        handler.HitscanShotRequestRpc(BulletsPerShot, Damage, XSpread, YSpread, SpreadVariation, BulletDistance, objHitEffect.name, playerHitEffect.name);
    }
}
