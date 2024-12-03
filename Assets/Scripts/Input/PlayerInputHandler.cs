using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private CameraController _camController;
    private PlayerCanvasHandler _canvasHandler;
    private CharacterControls _characterControls;

    private void OnEnable()
    {
        _playerController = GetComponent<PlayerController>();
        _camController = GetComponentInChildren<CameraController>();
        _canvasHandler = GetComponentInChildren<PlayerCanvasHandler>();

        if (_characterControls != null) return;

        _characterControls = new CharacterControls();

        _characterControls.PlayerMovement.Move.performed +=
            i => _playerController.GetPlayerInput(i.ReadValue<Vector2>());
        _characterControls.PlayerActions.Jump.performed += i => _playerController.Jump();
        _characterControls.PlayerActions.Sprint.started += i => _playerController.ToggleSprint(true);
        _characterControls.PlayerActions.Sprint.canceled += i => _playerController.ToggleSprint(false);
        _characterControls.PlayerActions.Shoot.started += i => _playerController.ShootLocalWeapon();
        _characterControls.PlayerActions.Shoot.canceled += i => _playerController.CancelFireLocalWeapon();
        _characterControls.PlayerActions.Reload.started += i => _playerController.ReloadLocalWeapon();

        _characterControls.PlayerActions.Item1Select.started += i => _playerController.ChangeItemSlot(0);
        _characterControls.PlayerActions.Item2Select.started += i => _playerController.ChangeItemSlot(1);

        _characterControls.PlayerMovement.Look.performed += i => _camController.GetCameraInput(i.ReadValue<Vector2>());
        _characterControls.PlayerMovement.Move.performed += i => _camController.GetMoveInput(i.ReadValue<Vector2>());

        _characterControls.PlayerActions.Pause.performed += i => _canvasHandler.TogglePauseMenu();

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
