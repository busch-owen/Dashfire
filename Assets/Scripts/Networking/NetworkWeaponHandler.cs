using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkWeaponHandler : NetworkBehaviour
{
    private PlayerController _controller;
    
    private void Start()
    {
        _controller = GetComponentInParent<PlayerController>();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponShotRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger("Shoot");
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void WeaponReloadRpc()
    {
        var assignedWeaponAnimator = GetComponentInChildren<Animator>();
        assignedWeaponAnimator.SetTrigger("Reload");
    }
}
