using UnityEngine;
using UnityEngine.Serialization;

public class MeleeWeaponBase : WeaponBase
{
    [SerializeField] private MeleeWeaponSO meleeWeapon;
    
    [field: SerializeField] public Transform HitPosition { get; private set; }
    
    protected override void Start()
    {
        OwnerObject = GetComponentInParent<PlayerController>();
        ItemHandler = GetComponentInParent<NetworkItemHandler>();
        animator = GetComponentInChildren<Animator>();
        animator.keepAnimatorStateOnDisable = true;
        CameraController = GetComponentInParent<CameraController>();
        CanvasHandler = OwnerObject?.GetComponentInChildren<PlayerCanvasHandler>();
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        animator.SetTrigger(Equip);
    }
    
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
