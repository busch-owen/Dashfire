using Unity.Netcode;
using UnityEngine;

public enum ItemType
{
    Weapon, Health, Armor
}

public class ItemPickup : NetworkBehaviour
{
    [field: SerializeField] public WeaponBase AssignedWeapon { get; private set; }
    [field: SerializeField] public int HealthAmount { get; private set; }
    [field: SerializeField] public int ArmorAmount { get; private set; }

    [SerializeField] private ItemType itemType;
    
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
        if (!player) return;
        if (player.EquippedWeapons[player.CurrentWeaponIndex] != null)
        {
            if (AssignedWeapon.WeaponSO == player.EquippedWeapons[player.CurrentWeaponIndex].WeaponSO)
            {
                return;
            }

            if (player.IsOwner)
                networkHandler.RequestWeaponSpawnRpc(AssignedWeapon.name, player.NetworkObjectId, NetworkObjectId);
            return;
        }
        if (player.IsOwner)
            networkHandler.RequestWeaponSpawnRpc(AssignedWeapon.name, player.NetworkObjectId, NetworkObjectId);
    }

    private void PickUpHeal(Collider other)
    {
        var player = other.GetComponentInChildren<PlayerController>();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestHealthPickupRpc(player.NetworkObjectId, HealthAmount, NetworkObjectId);
    }

    public void PickUpArmor(Collider other)
    {
        var player = other.GetComponentInChildren<PlayerController>();
        var networkHandler = player.GetComponentInChildren<NetworkItemHandler>();
        networkHandler.RequestArmorPickupRpc(player.NetworkObjectId, ArmorAmount, NetworkObjectId);
    }
}
