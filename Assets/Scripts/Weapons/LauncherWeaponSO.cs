using UnityEngine;

[CreateAssetMenu(fileName = "LauncherWeapon", menuName = "Scriptable Objects/LauncherWeapon")]
public class LauncherWeaponSO : WeaponBaseSO
{
    private PlayerController _player;
    private WeaponBase _weapon;
    
    [field: SerializeField] public float LaunchForce { get; private set; }
    
    public override void Attack()
    {
        var playersInScene = FindObjectsByType<PlayerController>(sortMode: FindObjectsSortMode.None);
        foreach (var player in playersInScene)
        {
            if (!player.IsOwner)
            {
                continue;
            }
            _player = FindFirstObjectByType<PlayerController>();
        }
        
        var cameraTransform = _player.GetComponentInChildren<Camera>().transform;
        _player?.AddForceInVector(-cameraTransform.forward * LaunchForce);
        base.Attack();
    }
}
