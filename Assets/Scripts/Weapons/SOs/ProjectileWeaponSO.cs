using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Scriptable Objects/ProjectileWeapon")]
public class ProjectileWeaponSO : WeaponBaseSO
{
    [field: SerializeField] public float ProjectileSpeed { get; private set; }
    [field: SerializeField] public GameObject ProjectileObject { get; private set; }
}
