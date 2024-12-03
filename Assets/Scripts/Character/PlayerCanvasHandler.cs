using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCanvasHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private CameraController _cameraController;
    private PlayerInputHandler _inputHandler;
    
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text ammoText;

    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInput;

    private void Start()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _inputHandler = GetComponentInParent<PlayerInputHandler>();
        _cameraController = _playerController.GetComponentInChildren<CameraController>();

        sensitivitySlider.value = _cameraController.Sens;

        optionsMenu.SetActive(false);
    }

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
    }
    
    public void UpdateArmor(int armor)
    {
        armorText.text = armor.ToString();
    }
    
    public void UpdateAmmo(int current, int max)
    {
        ammoText.text = $"{current}/{max}";
    }

    public void UpdateSensitivitySlider()
    {
        _cameraController.SetPlayerSensitivity(sensitivitySlider.value);
        sensitivityInput.placeholder.GetComponent<TMP_Text>().text = sensitivitySlider.value.ToString();
    }
    
    public void UpdateSensitivityInput()
    {
        _cameraController.SetPlayerSensitivity(float.Parse(sensitivityInput.text));
        sensitivityInput.placeholder.GetComponent<TMP_Text>().text = sensitivityInput.text;
        sensitivitySlider.value = float.Parse(sensitivityInput.text);
    }

    public void TogglePauseMenu()
    {
        optionsMenu.SetActive(!optionsMenu.activeSelf);
        if (optionsMenu.activeSelf)
        {
            _inputHandler.DisableInput();
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            _inputHandler.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void LeaveLobby()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
