using System;
using TMPro;
using System.Collections;
using DG.Tweening;
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
    [SerializeField] private TMP_Text currentAmmoText;
    [SerializeField] private TMP_Text maxAmmoText;

    [SerializeField] private float pickupTextDelay;
    [SerializeField] private float pickupTextScaleDelay;
    
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

    [SerializeField] private TMP_Text
        armorPickupText,
        healthPickupText,
        ammoPickupText;

    private WaitForFixedUpdate _waitForFixed;
    private WaitForSeconds _waitFadeDelay;
    
    private WaitForSeconds _waitForPickupDelay;
    private WaitForSeconds _waitForPickupScaleDelay;

    [SerializeField] private Image crosshairImage;
    
    private void Awake()
    {
        _waitForFixed = new WaitForFixedUpdate();
        _waitFadeDelay = new WaitForSeconds(damageFadeOutDelay);
        _waitForPickupDelay = new WaitForSeconds(pickupTextDelay);
        _waitForPickupScaleDelay = new WaitForSeconds(pickupTextScaleDelay);
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
        
        sensitivitySlider.value = SensitivityHandler.Instance.Sens;
        scopeSensitivitySlider.value = SensitivityHandler.Instance.ScopedSens;

        gameplayOverlay.SetActive(true);
        optionsMenu.SetActive(false);
        deathOverlay.SetActive(false);
        
        armorPickupText.gameObject.SetActive(false);
        healthPickupText.gameObject.SetActive(false);
        ammoPickupText.gameObject.SetActive(false);
    }

    public void UpdateHealth(int health)
    {
        var previousValue = int.Parse(healthText.text);
        var difference = health - previousValue;
        if (difference > 0)
        {
            StartCoroutine(ShowPickupAmount(healthPickupText, difference.ToString()));
        }
        healthText.text = health.ToString();
    }
    
    public void UpdateArmor(int armor)
    {
        var previousValue = int.Parse(armorText.text);
        var difference = armor - previousValue;
        if (difference > 0)
        {
            StartCoroutine(ShowPickupAmount(armorPickupText, difference.ToString()));
        }
        armorText.text = armor.ToString();
    }
    
    public void UpdateAmmo(int current, int max, bool weaponSwap)
    {
        
        var previousValue = int.Parse(maxAmmoText.text);
        var difference = max - previousValue;
        if (difference > 0 && !weaponSwap)
        {
            StopCoroutine(ShowPickupAmount(ammoPickupText, difference.ToString()));
            StartCoroutine(ShowPickupAmount(ammoPickupText, difference.ToString()));
        }
        currentAmmoText.text = $"{current}/";
        maxAmmoText.text = $"{max}";
    }

    private IEnumerator ShowPickupAmount(TMP_Text textToChange, string newValue)
    {
        textToChange.gameObject.SetActive(true);
        textToChange.transform.localScale = Vector3.zero;
        textToChange.transform.DOScale(1, pickupTextScaleDelay);
        textToChange.text = $"+{newValue}";
        yield return _waitForPickupDelay;
        textToChange.transform.DOScale(0, pickupTextScaleDelay);
        yield return _waitForPickupScaleDelay;
        textToChange.gameObject.SetActive(false);
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
        deathText.text = $"Fragged by: {castingName}";
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

    public void SwapCrosshairImage(Sprite sprite)
    {
        crosshairImage.sprite = sprite;
    }

    public void LeaveLobby()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.Shutdown();
        SceneManager.LoadScene("LobbyScreen");
    }
}
