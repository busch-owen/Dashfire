using UnityEngine;
using UnityEngine.Serialization;

public class MeleeWeaponBase : WeaponBase
{
    [SerializeField] private MeleeWeaponSO meleeWeapon;
    
    public override void UseWeapon()
    {
        if (!OwnerObject.IsOwner) return;
        if(!CanFire) return;
        CanFire = false;
        animator?.SetTrigger(ShootTrigger);
        ItemHandler ??= GetComponentInParent<NetworkItemHandler>();
        ItemHandler.WeaponShotRpc();
        ItemHandler.RequestMeleeAttackRpc(meleeWeapon.HitBoxWidth, meleeWeapon.HitBoxHeight, meleeWeapon.HitBoxDepth, meleeWeapon.Damage);
        Invoke(nameof(EnableFiring), WeaponSO.FireRate);
    }
}
