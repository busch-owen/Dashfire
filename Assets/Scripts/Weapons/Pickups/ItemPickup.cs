using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
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

    private NetworkVariable<bool> _onCooldown = new(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Everyone);
    
    [SerializeField] private GameObject healthVisual;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private GameObject countdownObject;
    [SerializeField] private GameObject lightObject;
    
    private Image _countdownBorder;
    private TMP_Text _countdownText;
    
    public WeaponBase CurrentWeapon { get; private set; }
    private GameObject _currentVisual;
    
    [SerializeField] private AudioClip pickupSound;
    
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
        if(_onCooldown.Value) return;
        switch (itemType)
        {
            case ItemType.Weapon:
            {
                PickUpPrompt(other);
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
        if(!CurrentWeapon) return;
        if (playerController.IsOwner)
            pickupPrompt.SetActive(true);
        playerController.AllowWeaponPickup(this);
    }

    public void PickUpWeapon(PlayerController controller)
    {
        if(!CurrentWeapon) return;
        var player = controller;
        if(!player) return;
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        
        if (player.EquippedWeapons[player.CurrentWeaponIndex] != null)
        {
            if (CurrentWeapon.WeaponSO == player.EquippedWeapons[player.CurrentWeaponIndex].WeaponSO)
            {
                return;
            }

            if (player.IsOwner)
            {
                if(countdownObject)
                    QueueCountdownVisualsRpc();
                networkHandler.RequestWeaponSpawnRpc(CurrentWeapon.name, player.NetworkObjectId);
                ClearSpawnedItemsRpc();
                Invoke(nameof(SpawnNewWeaponRpc), respawnTime);
                player.GetComponent<SoundHandler>().PlayClipWithRandPitch(pickupSound);
            }
            return;
        }

        if (player.IsOwner)
        {
            if(countdownObject)
                QueueCountdownVisualsRpc();
            networkHandler.RequestWeaponSpawnRpc(CurrentWeapon.name, player.NetworkObjectId);
            ClearSpawnedItemsRpc();
            Invoke(nameof(SpawnNewWeaponRpc), respawnTime);
        }
    }

    private void PickUpHeal(Collider other)
    {
        if(_onCooldown.Value) return;
        _onCooldown.Value = true;
        var player = other.GetComponentInChildren<PlayerController>();
        if(!player.IsOwner) return;
        if(player.CurrentHealth >= player.MaxHealth) return;
        if(countdownObject)
            QueueCountdownVisualsRpc();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestHealthPickupRpc(player.NetworkObjectId, HealthAmount);
        DisableHealRpc();
        other.GetComponent<SoundHandler>().PlayClipWithRandPitch(pickupSound);
    }

    private void PickUpArmor(Collider other)
    {
        if(_onCooldown.Value) return;
        _onCooldown.Value = true;
        var player = other.GetComponentInChildren<PlayerController>();
        if(!player.IsOwner) return;
        if(player.CurrentArmor >= player.MaxArmor) return;
        if(countdownObject)
            QueueCountdownVisualsRpc();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestArmorPickupRpc(player.NetworkObjectId, ArmorAmount);
        DisableShieldRpc();
        other.GetComponent<SoundHandler>().PlayClipWithRandPitch(pickupSound);
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
        CurrentWeapon = AssignedWeapons[randPicked];
        _currentVisual = Instantiate(CurrentWeapon, rotatingHandle).gameObject;
        _currentVisual.GetComponent<WeaponBase>().enabled = false;
        _currentVisual.GetComponentInChildren<Animator>().enabled = false;
        lightObject.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    private void ClearSpawnedItemsRpc()
    {
        if(!CurrentWeapon) return;
        pickupPrompt.SetActive(false);
        lightObject.SetActive(false);
        Destroy(_currentVisual);
        _currentVisual = null;
        CurrentWeapon = null;
    }

    #endregion

    #region Health and Shield Pickup RPCs

    [Rpc(SendTo.Everyone)]
    private void EnableHealRpc()
    {
        healthVisual.SetActive(true);
        _onCooldown.Value = false;
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
        _onCooldown.Value = false;
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
        
        switch (itemType)
        {
            case ItemType.Health:
                EnableHealRpc();
                break;
            case ItemType.Armor:
                EnableShieldRpc();
                break;
        }
    }
}
