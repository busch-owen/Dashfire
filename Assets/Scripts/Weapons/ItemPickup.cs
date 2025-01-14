using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Weapon, Health, Armor
}

public class ItemPickup : NetworkBehaviour
{
    [field: SerializeField] public WeaponBase[] AssignedWeapons { get; private set; }
    [field: SerializeField] public int HealthAmount { get; private set; }
    [field: SerializeField] public int ArmorAmount { get; private set; }

    [SerializeField] private ItemType itemType;
    
    [SerializeField] private float respawnTime;
    
    [SerializeField] private Transform rotatingHandle;

    private bool _onCooldown;
    
    [SerializeField] private GameObject healthVisual;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private GameObject countdownObject;
    private Image _countdownBorder;
    private TMP_Text _countdownText;
    
    private WeaponBase _currentWeapon;
    private GameObject _currentVisual;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (countdownObject)
        {
            countdownObject.SetActive(false);
            _countdownBorder = countdownObject.GetComponentInChildren<Image>();
            _countdownText = countdownObject.GetComponentInChildren<TMP_Text>();
        }
        
        if(!IsServer) return;
        
        DisableHealRpc();
        DisableShieldRpc();
        
        switch (itemType)
        {
            case ItemType.Weapon:
            {
                SpawnNewWeaponRpc();
                break;
            }
            case ItemType.Health:
            {
                EnableHealRpc();
                break;
            }
            case ItemType.Armor:
            {
                EnableShieldRpc();
                break;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerController>()) return;
        if(countdownObject)
            QueueCountdownVisualsRpc();
        switch (itemType)
        {
            case ItemType.Weapon:
            {
                PickUpPrompt(other);
                //PickUpWeapon(other);
                break;
            }
            case ItemType.Health:
            {
                PickUpHeal(other);
                break;
            }
            case ItemType.Armor:
            {
                PickUpArmor(other);
                break;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<PlayerController>()) return;
        switch (itemType)
        {
            case ItemType.Weapon:
            {
                pickupPrompt.SetActive(false);
                other.GetComponent<PlayerController>().RemoveWeaponPickup();
                break;
            }
        }
    }

    private void PickUpPrompt(Collider other)
    {
        var playerController = other.GetComponentInChildren<PlayerController>();
        if(!_currentWeapon) return;
        if (playerController.IsOwner)
            pickupPrompt.SetActive(true);
        playerController.AllowWeaponPickup(this);
    }

    public void PickUpWeapon(PlayerController controller)
    {
        if(!_currentWeapon) return;
        var player = controller;
        if(!player) return;
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        if (player.EquippedWeapons[player.CurrentWeaponIndex] != null)
        {
            if (_currentWeapon.WeaponSO == player.EquippedWeapons[player.CurrentWeaponIndex].WeaponSO)
            {
                return;
            }

            if (player.IsOwner)
            {
                networkHandler.RequestWeaponSpawnRpc(_currentWeapon.name, player.NetworkObjectId);
                ClearSpawnedItemsRpc();
                Invoke(nameof(SpawnNewWeaponRpc), respawnTime);
            }
            return;
        }

        if (player.IsOwner)
        {
            networkHandler.RequestWeaponSpawnRpc(_currentWeapon.name, player.NetworkObjectId);
            ClearSpawnedItemsRpc();
            Invoke(nameof(SpawnNewWeaponRpc), respawnTime);
        }
    }

    private void PickUpHeal(Collider other)
    {
        if(_onCooldown) return;
        _onCooldown = true;
        var player = other.GetComponentInChildren<PlayerController>();
        if(!player.IsOwner) return;
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestHealthPickupRpc(player.NetworkObjectId, HealthAmount);
        DisableHealRpc();
        Invoke(nameof(EnableHealRpc), respawnTime);
    }

    private void PickUpArmor(Collider other)
    {
        if(_onCooldown) return;
        _onCooldown = true;
        var player = other.GetComponentInChildren<PlayerController>();
        if(!player.IsOwner) return;
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestArmorPickupRpc(player.NetworkObjectId, ArmorAmount);
        DisableShieldRpc();
        Invoke(nameof(EnableShieldRpc), respawnTime);
    }

    #region Weapon Pickup RPCs

    [Rpc(SendTo.Server)]
    private void SpawnNewWeaponRpc()
    {
        var randWeapon = Random.Range(0, AssignedWeapons.Length);
        SendPickUpInfoRpc(randWeapon);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendPickUpInfoRpc(int randPicked)
    {
        _currentWeapon = AssignedWeapons[randPicked];
        _currentVisual = Instantiate(_currentWeapon, rotatingHandle).gameObject;
        _currentVisual.GetComponent<WeaponBase>().enabled = false;
        _currentVisual.GetComponentInChildren<Animator>().enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    private void ClearSpawnedItemsRpc()
    {
        if(!_currentWeapon) return;
        pickupPrompt.SetActive(false);
        Destroy(_currentVisual);
        _currentVisual = null;
        _currentWeapon = null;
    }

    #endregion

    #region Health and Shield Pickup RPCs

    [Rpc(SendTo.Everyone)]
    private void EnableHealRpc()
    {
        healthVisual.SetActive(true);
        _onCooldown = false;
    }
    
    [Rpc(SendTo.Everyone)]
    private void DisableHealRpc()
    {
        healthVisual.SetActive(false);
    }
    
    [Rpc(SendTo.Everyone)]
    private void EnableShieldRpc()
    {
        shieldVisual.SetActive(true);
        _onCooldown = false;
    }
    
    [Rpc(SendTo.Everyone)]
    private void DisableShieldRpc()
    {
        shieldVisual.SetActive(false);
    }

    #endregion

    [Rpc(SendTo.ClientsAndHost)]
    private void QueueCountdownVisualsRpc()
    {
        countdownObject.SetActive(true);
        Debug.Log("enabled");
        StartCoroutine(CountdownVisualUpdate());
    }

    private IEnumerator CountdownVisualUpdate()
    {
        var count = respawnTime;
        while (count > 0)
        {
            count -= Time.fixedDeltaTime;
            _countdownText.text = ((int)count).ToString();
            _countdownBorder.fillAmount = count / respawnTime;
            yield return new WaitForFixedUpdate();
        }
        countdownObject.SetActive(false);
        Debug.Log("disabled");
    }
}
