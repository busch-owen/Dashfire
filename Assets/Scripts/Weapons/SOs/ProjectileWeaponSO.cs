using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Scriptable Objects/ProjectileWeapon")]
public class ProjectileWeaponSO : WeaponBaseSO
{
    [field: Space(5), Header("Weapon Specific"), Space(5)]
    [field: SerializeField] public NetworkObject ProjectileObject { get; private set; }
}
