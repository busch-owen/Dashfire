using UnityEngine;

public class DefaultWeapon : IWeaponDamage
{
    public float Damage;
    public int BulletsPerShot;
    public float XSpread;
    public float YSpread;
    public float SpreadVariation;
    public float BulletDistance;
    private LayerMask _playerMask;
    
    public virtual void Attack()
    {
        _playerMask = LayerMask.GetMask("Default");

        for (var i = 0; i < BulletsPerShot; i++)
        {
            //spread math
            var firePos = Camera.main.transform;
            var fireDirection = firePos.forward;
            var spread = Vector3.zero;
            spread += firePos.right * Random.Range(-XSpread, XSpread);
            spread += firePos.up * Random.Range(-YSpread, YSpread);
            fireDirection += spread.normalized * Random.Range(0, SpreadVariation);

            RaycastHit hit;
            if (Physics.Raycast(firePos.position, fireDirection, out hit, BulletDistance, _playerMask))
            {
                // whatever logic you wanna do on raycast hit
            }
        }
    }
}
