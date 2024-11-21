using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Scriptable Objects/ProjectileWeapon")]
public class ProjectileWeaponSO : WeaponBaseSO
{
    [field:SerializeField] public GameObject ProjectileObject { get; private set; }
    [field:SerializeField] public float ProjectileSpeed { get; private set; }
    public override void Attack()
    {
        //Getting references to all necessary objects
        var firePos = Camera.main.transform;
        LocalPoolManager = FindFirstObjectByType<PoolManager>();
        var newProjectile = LocalPoolManager.Spawn(ProjectileObject.name).gameObject;
        newProjectile.transform.position = firePos.GetComponentInChildren<ParticleSystem>().transform.position;
        newProjectile.transform.rotation = firePos.transform.rotation;
        var projectileRb = newProjectile.GetComponent<Rigidbody>();
        
        projectileRb.AddForce(firePos.forward * ProjectileSpeed, ForceMode.Impulse);
    }
}
