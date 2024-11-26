using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private CameraController _camController;
    private CharacterControls _characterControls;
    
    private void OnEnable()
    {
        //if (!IsOwner) return;
        _playerController = GetComponent<PlayerController>();
        _camController = GetComponentInChildren<CameraController>();
        
        if (_characterControls != null) return;

        _characterControls = new CharacterControls();

        _characterControls.PlayerMovement.Move.performed += i => _playerController.GetPlayerInput(i.ReadValue<Vector2>());
        _characterControls.PlayerActions.Jump.performed += i => _playerController.Jump();
        _characterControls.PlayerActions.Sprint.started += i => _playerController.ToggleSprint(true);
        _characterControls.PlayerActions.Sprint.canceled += i => _playerController.ToggleSprint(false);
        _characterControls.PlayerActions.Shoot.started += i => _playerController.LocalWeapon.UseWeapon();
        _characterControls.PlayerActions.Shoot.canceled += i => _playerController.LocalWeapon.CancelFire();
        _characterControls.PlayerActions.Reload.started += i => _playerController.LocalWeapon.ReloadWeapon();

        _characterControls.PlayerMovement.Look.performed += i => _camController.GetCameraInput(i.ReadValue<Vector2>());
        _characterControls.PlayerMovement.Move.performed += i => _camController.GetMoveInput(i.ReadValue<Vector2>());
        
        _characterControls.Enable();
    }

    public void DisableInput()
    {
        _characterControls.Disable(); 
    }

    public void EnableInput()
    {
        _characterControls.Enable();
    }

    private void OnDestroy()
    {
        _characterControls.Disable(); 
    }
}
