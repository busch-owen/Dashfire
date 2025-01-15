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

    private bool _singleUse;
    [SerializeField] private float singleUseDespawnTime;

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
        if (!playerController.IsOwner) return;
        var ammoReserve = playerController.GetComponentInChildren<AmmoReserve>();
        ammoReserve.ContainersDictionary[ammoType].AddToAmmo(ammoAmount);
        
        var canvasHandler = other.GetComponentInChildren<PlayerCanvasHandler>();
        var playerWeapon = playerController.EquippedWeapons[playerController.CurrentWeaponIndex];
        if(playerWeapon.reserve)
            canvasHandler.UpdateAmmo(playerWeapon.currentAmmo, playerWeapon.reserve.ContainersDictionary[playerWeapon.WeaponSO.RequiredAmmo].currentCount);

        if (_singleUse)
        {
            other.GetComponentInChildren<NetworkItemHandler>().DestroyPickupRpc(gameObject.GetComponent<NetworkObject>());
        }
        
        CollectPickup();
    }
    
    private void CollectPickup()
    {
        DisablePickupRpc();
    }
    
    private void DespawnPickup()
    {
        NetworkManager.Destroy(gameObject);
        //Destroy(gameObject);
        //GetComponent<NetworkObject>().Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisablePickupRpc()
    {
        QueueCountdownVisualsRpc();
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

    public void SetUpSingleUse()
    {
        _singleUse = true;
        Invoke(nameof(DespawnAmmoPickupRpc), singleUseDespawnTime);
    }

    public void SetAmmoType(AmmoType type)
    {
        ammoType = type;
        foreach (var visual in boxVisuals)
        {
            visual.SetActive(false);
        }
        boxVisuals[(int)ammoType].SetActive(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DespawnAmmoPickupRpc()
    {
        DespawnPickup();
    }
}
