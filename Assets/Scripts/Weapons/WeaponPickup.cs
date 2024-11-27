using Unity.Netcode;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
    [field: SerializeField] public WeaponBase AssignedWeapon { get; private set; }
    
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInChildren<NetworkWeaponHandler>();
        if (!player) return;
        player.PickupNewWeaponRpc(NetworkObjectId);
        Destroy(gameObject);
    }
}
