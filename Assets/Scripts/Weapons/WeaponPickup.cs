using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (!player) return;
        player.Weapon.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
