using UnityEngine;

public class ProjectileWeapon : MonoBehaviour
{
    [field:SerializeField] public GameObject ProjectileObject { get; private set; }
    [field:SerializeField] public float ProjectileSpeed { get; private set; }
    
    public void Attack()
    {
        //Getting references to all necessary objects
        var firePos = Camera.main.transform;
        var newProjectile = PoolManager.Instance.Spawn(ProjectileObject.name).gameObject;
        newProjectile.transform.position = firePos.GetComponentInChildren<ParticleSystem>().transform.position;
        newProjectile.transform.rotation = firePos.transform.rotation;
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(firePos.forward * ProjectileSpeed, ForceMode.Impulse);
    }
}
