using UnityEngine;

public class ProjectileWeapon : IWeaponDamage
{
    public GameObject ProjectileObject;
    public float ProjectileSpeed;
    
    public void Attack()
    {
        //Getting references to all necessary objects
        var firePos = Camera.main.transform;
        var newProjectile = PoolManager.Instance.Spawn(ProjectileObject.name);
        newProjectile.transform.position = firePos.GetComponentInChildren<ParticleSystem>().transform.position;
        newProjectile.transform.rotation = firePos.transform.rotation;
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(firePos.forward * ProjectileSpeed, ForceMode.Impulse);
    }
}
