using System;
using UnityEngine;

public class MenuInputHandler : MonoBehaviour
{
    private MenuControls _menuControls;
    private MenuHandler _menuHandler;
    private PlayerCanvasHandler _canvasHandler;

    private void OnEnable()
    {
        _menuHandler ??= FindFirstObjectByType<MenuHandler>();
        _canvasHandler ??= GetComponent<PlayerCanvasHandler>();
        
        if (_menuControls != null) return;

        _menuControls = new();
        
        if(_menuHandler)
            _menuControls.MenuActions.Cancel.started += i => _menuHandler?.CloseCurrentMenu();

        if (_canvasHandler)
            _menuControls.MenuActions.Cancel.started += i => _canvasHandler.TogglePauseMenu();
        
        _menuControls.Enable();
    }

    private void OnDestroy()
    {
        _menuControls.Disable();
    }
}
