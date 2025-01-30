using UnityEngine;

[CreateAssetMenu(fileName = "LauncherWeapon", menuName = "Scriptable Objects/LauncherWeapon")]
public class LauncherWeaponSO : WeaponBaseSO
{
    private WeaponBase _weapon;
    
    [field: Space(5), Header("Weapon Specific"), Space(5)]
    [field: SerializeField] public float LaunchForce { get; private set; }
}
