using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class AmmoPickup : NetworkBehaviour
{
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int ammoAmount;

    [SerializeField] private float respawnTime;
    
    [SerializeField] private GameObject[] boxVisuals;
    
    [SerializeField] private GameObject countdownObject;
    private Image _countdownBorder;
    private TMP_Text _countdownText;

    private void Start()
    {
        countdownObject.SetActive(false);
        foreach (var visual in boxVisuals)
        {
            visual.SetActive(false);
        }
        boxVisuals[(int)ammoType].SetActive(true);
        
        if (countdownObject)
        {
            countdownObject.SetActive(false);
            _countdownBorder = countdownObject.GetComponentInChildren<Image>();
            _countdownText = countdownObject.GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerController = other.GetComponent<PlayerController>();
        if (!playerController) return;
        var ammoReserve = other.GetComponentInChildren<AmmoReserve>();
        ammoReserve.ContainersDictionary[ammoType].AddToAmmo(ammoAmount);
        QueueCountdownVisualsRpc();
        
        var canvasHandler = other.GetComponentInChildren<PlayerCanvasHandler>();
        var playerWeapon = playerController.EquippedWeapons[playerController.CurrentWeaponIndex];
        if(playerWeapon.reserve)
            canvasHandler.UpdateAmmo(playerWeapon.currentAmmo, playerWeapon.reserve.ContainersDictionary[playerWeapon.WeaponSO.RequiredAmmo].currentCount);
        
        boxVisuals[(int)ammoType].SetActive(false);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void QueueCountdownVisualsRpc()
    {
        countdownObject.SetActive(true);
        StartCoroutine(CountdownVisualUpdate());
    }

    private IEnumerator CountdownVisualUpdate()
    {
        var count = respawnTime + 1;
        while (count > 1)
        {
            count -= Time.fixedDeltaTime;
            _countdownText.text = ((int)count).ToString();
            _countdownBorder.fillAmount = (count - 1) / respawnTime;
            yield return new WaitForFixedUpdate();
        }
        countdownObject.SetActive(false);
        boxVisuals[(int)ammoType].SetActive(true);
    }
}
