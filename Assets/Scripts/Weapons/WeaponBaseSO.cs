using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Scriptable Objects/WeaponBase")]
public class WeaponBaseSO : ScriptableObject
{
    [field: SerializeField] public int AmmoCount { get; private set; }
    [field: SerializeField] public float BulletsPerShot { get; private set; }
    [field: SerializeField] public float ReloadTime { get; private set; }
    [field: SerializeField] public bool Automatic { get; private set; }
    [field: SerializeField] public float BulletDistance { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    [field: SerializeField] public float ADSSpeed { get; private set; }
    [field: SerializeField] public float XSpread { get; private set; }
    [field: SerializeField] public float YSpread { get; private set; }
    [field: SerializeField] public float SpreadVariatiom { get; private set; }
    
    [field: SerializeField] public GameObject GunObject { get; private set; }

    protected PoolManager LocalPoolManager;
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
            fireDirection += spread.normalized * Random.Range(0, SpreadVariatiom);
            
            RaycastHit hit;
            if (Physics.Raycast(firePos.position, fireDirection, out hit, BulletDistance, _playerMask))
            {
                // whatever logic you wanna do on raycast hit
            }
        }
    }
}
