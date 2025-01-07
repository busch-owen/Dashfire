using Unity.Netcode;
using UnityEngine;

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
    
    private WeaponBase _currentWeapon;
    private GameObject _currentVisual;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if(!IsServer) return;
        SpawnNewWeaponRpc();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
            {
                PickUpWeapon(other);
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

    public void PickUpWeapon(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
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
        var player = other.GetComponentInChildren<PlayerController>();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestHealthPickupRpc(player.NetworkObjectId, HealthAmount);
    }

    public void PickUpArmor(Collider other)
    {
        var player = other.GetComponentInChildren<PlayerController>();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestArmorPickupRpc(player.NetworkObjectId, ArmorAmount);
    }

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
        Destroy(_currentVisual);
        _currentVisual = null;
        _currentWeapon = null;
    }
}
