using UnityEngine;

public class LauncherWeapon : IWeaponDamage
{
    private PlayerController _player;

    protected PoolManager LocalPoolManager;
    private LayerMask _playerMask;
}
