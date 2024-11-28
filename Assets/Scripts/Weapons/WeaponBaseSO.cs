using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Scriptable Objects/WeaponBase")]
public class WeaponBaseSO : ScriptableObject
{
    [field: SerializeField] public int AmmoCount { get; private set; }
    [field: SerializeField] public int BulletsPerShot { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float ReloadTime { get; private set; }
    [field: SerializeField] public bool Automatic { get; private set; }
    [field: SerializeField] public float BulletDistance { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    [field: SerializeField] public float ADSSpeed { get; private set; }
    [field: SerializeField] public float XSpread { get; private set; }
    [field: SerializeField] public float YSpread { get; private set; }
    [field: SerializeField] public float SpreadVariation { get; private set; }
}
