using UnityEngine;

[CreateAssetMenu(fileName = "MeleeWeaponBase", menuName = "Scriptable Objects/MeleeWeaponBase")]
public class MeleeWeaponSO : WeaponBaseSO
{
    [field: SerializeField] public float HitBoxWidth { get; private set; }
    [field: SerializeField] public float HitBoxHeight { get; private set; }
    [field: SerializeField] public float HitBoxDepth { get; private set; }
}
