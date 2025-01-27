using System;
using TMPro;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCanvasHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private CameraController _cameraController;
    private Camera _camera;
    private Canvas _canvas;
    private PlayerInputHandler _inputHandler;
    private AmmoReserve _reserve;

    [SerializeField] private GameObject gameplayOverlay;
    [SerializeField] private GameObject deathOverlay;
    
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text ammoText;
    
    [SerializeField] private TMP_Text deathText;

    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInput;
    [SerializeField] private Slider scopeSensitivitySlider;
    [SerializeField] private TMP_InputField scopeSensitivityInput;
    
    [SerializeField] private GameObject damageIndicator;
    [SerializeField] private CanvasGroup screenIndicator;
    [SerializeField] private float damageFadeOutDelay;
    [SerializeField] private float indicatorFadeSpeed;
    [SerializeField] private float indicatorVignetteIntensity;
    [SerializeField] private Color damageVignetteColor;

    private WaitForFixedUpdate _waitForFixed;
    private WaitForSeconds _waitFadeDelay;


    private void Awake()
    {
        _waitForFixed = new WaitForFixedUpdate();
        _waitFadeDelay = new WaitForSeconds(damageFadeOutDelay);
    }

    private void Start()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _inputHandler = GetComponentInParent<PlayerInputHandler>();
        _cameraController = _playerController.GetComponentInChildren<CameraController>();
        _camera = _cameraController.GetComponent<Camera>();
        _canvas = GetComponent<Canvas>();
        _reserve = GetComponentInParent<AmmoReserve>();

        damageIndicator.GetComponentInChildren<CanvasGroup>().alpha = 0f;
        screenIndicator.alpha = 0;
        
        sensitivitySlider.value = _cameraController.Sens;

        gameplayOverlay.SetActive(true);
        optionsMenu.SetActive(false);
        deathOverlay.SetActive(false);
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
    
    public void UpdateScopeSensitivitySlider()
    {
        _cameraController.SetPlayerScopeSensitivity(scopeSensitivitySlider.value);
        scopeSensitivityInput.placeholder.GetComponent<TMP_Text>().text = scopeSensitivitySlider.value.ToString();
    }
    
    public void UpdateScopeSensitivityInput()
    {
        _cameraController.SetPlayerScopeSensitivity(float.Parse(scopeSensitivityInput.text));
        scopeSensitivityInput.placeholder.GetComponent<TMP_Text>().text = scopeSensitivityInput.text;
        scopeSensitivitySlider.value = float.Parse(scopeSensitivityInput.text);
    }

    public void TogglePauseMenu()
    {
        if (!_playerController.IsOwner) return;
        optionsMenu.SetActive(!optionsMenu.activeSelf);
        if (optionsMenu.activeSelf)
        {
            _inputHandler.DisableInput();
            _playerController.ResetInputs();
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            _inputHandler.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void EnableDeathOverlay(string castingName)
    {
        gameplayOverlay.SetActive(false);
        deathOverlay.SetActive(true);
        deathText.text = $"Fragged by: {castingName}!";
    }
    
    public void DisableDeathOverlay()
    {
        gameplayOverlay.SetActive(true);
        deathOverlay.SetActive(false);
    }

    public IEnumerator ShowDamageIndicator(Quaternion rotation)
    {
        var indicatorGroup = damageIndicator.GetComponentInChildren<CanvasGroup>();
        indicatorGroup.alpha = 1;
        screenIndicator.alpha = indicatorVignetteIntensity;
        damageIndicator.transform.localRotation = rotation;
        yield return _waitFadeDelay;
        
        while (indicatorGroup.alpha > 0.1f)
        {
            indicatorGroup.alpha = Mathf.Lerp(indicatorGroup.alpha, 0f, indicatorFadeSpeed);
            screenIndicator.alpha = Mathf.Lerp(screenIndicator.alpha, 0, indicatorFadeSpeed);
            yield return _waitForFixed;
        }
        
        screenIndicator.alpha = 0f;
        indicatorGroup.alpha = 0f;
        yield return null;
    }

    public void LeaveLobby()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.Shutdown();
        SceneManager.LoadScene("LobbyScreen");
    }
}
