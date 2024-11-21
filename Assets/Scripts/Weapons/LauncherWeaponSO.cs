using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Scriptable Objects/LauncherWeapon")]
public class LauncherWeaponSO : WeaponBaseSO
{
    private PlayerController _player;
    private WeaponBase _weapon;
    
    [field: SerializeField] public float LaunchForce { get; private set; }
    
    public override void Attack()
    {
        _player = FindFirstObjectByType<PlayerController>();
        var cameraTransform = _player.GetComponentInChildren<Camera>().transform;
        _player?.AddForceInVector(-cameraTransform.forward * LaunchForce);
        base.Attack();
    }
}
