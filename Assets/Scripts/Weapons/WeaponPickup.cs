using Unity.Netcode;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
    [field: SerializeField] public WeaponBase AssignedWeapon { get; private set; }
    
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInChildren<PlayerController>();
        var networkHandler = player.GetComponentInChildren<NetworkWeaponHandler>();
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
}
