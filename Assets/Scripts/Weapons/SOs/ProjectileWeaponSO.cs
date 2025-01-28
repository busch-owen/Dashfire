using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Scriptable Objects/ProjectileWeapon")]
public class ProjectileWeaponSO : WeaponBaseSO
{
    [field: SerializeField] public NetworkObject ProjectileObject { get; private set; }
}
