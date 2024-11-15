using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Scriptable Objects/WeaponBase")]
public class WeaponBaseSO : ScriptableObject
{
    [field: SerializeField] public int AmmoCount { get; private set; }
    [field: SerializeField] public float ReloadTime { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    [field: SerializeField] public float ADSSpeed { get; private set; }

    public virtual void Attack()
    {
        
    }
}
