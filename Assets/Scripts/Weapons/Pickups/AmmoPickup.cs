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

    private bool _onCooldown;

    public NetworkVariable<bool> singleUse = new(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Everyone);
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
        if(_onCooldown) return;
        var ammoReserve = playerController.GetComponentInChildren<AmmoReserve>();
        ammoReserve.ContainersDictionary[ammoType].AddToAmmo(ammoAmount);
        
        var canvasHandler = other.GetComponentInChildren<PlayerCanvasHandler>();
        var playerWeapon = playerController.EquippedWeapons[playerController.CurrentWeaponIndex];
        if(playerWeapon.reserve)
            canvasHandler.UpdateAmmo(playerWeapon.currentAmmo, playerWeapon.reserve.ContainersDictionary[playerWeapon.WeaponSO.RequiredAmmo].currentCount);

        _onCooldown = true;
        if (singleUse.Value)
        {
            other.GetComponentInChildren<NetworkItemHandler>().DestroyPickupRpc(gameObject.GetComponent<NetworkObject>());
            return;
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
        _onCooldown = false;
    }

    public void SetUpSingleUse()
    {
        singleUse.Value = true;
        Invoke(nameof(DespawnAmmoPickupRpc), singleUseDespawnTime);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetAmmoTypeRpc(AmmoType type)
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
