using UnityEngine;

[CreateAssetMenu(fileName = "LauncherWeapon", menuName = "Scriptable Objects/LauncherWeapon")]
public class LauncherWeaponSO : WeaponBaseSO
{
    private WeaponBase _weapon;
    
    [field: SerializeField] public float LaunchForce { get; private set; }
}
