using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    private GameObject _currentMenu;
    
    public void OpenSpecificMenu(GameObject menuToOpen)
    {
        CloseCurrentMenu();
        _currentMenu = menuToOpen;
        _currentMenu.SetActive(true);
    }

    public void CloseCurrentMenu()
    {
        if (!_currentMenu) return;
        _currentMenu.SetActive(false);
        _currentMenu = null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
