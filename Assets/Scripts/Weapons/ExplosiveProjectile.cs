using System;
using UnityEngine;

public class ExplosiveProjectile : PoolObject
{
    private Rigidbody _rb;
    private GameObject _projectileCollision;
    [SerializeField] private ExplosionDataSO explosionData;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float lifetime;
    
    
    private LayerMask _playerMask;
    private PoolManager _pool;
    
    private void OnEnable()
    {
        _rb ??= GetComponent<Rigidbody>();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _projectileCollision ??= GetComponentInChildren<Collider>().gameObject;
        _rb.isKinematic = false;
        _projectileCollision.SetActive(true);
        _playerMask = LayerMask.GetMask("ControlledPlayer");
        _pool ??= FindFirstObjectByType<PoolManager>();
        Invoke(nameof(OnDeSpawn), lifetime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.GetComponent<PlayerController>()) return;
        _rb.isKinematic = true;
        _projectileCollision.SetActive(false);
        
        var hitPoint = transform.position;
        PlayerController player;
        var effect = _pool.Spawn(explosionEffect.name);
        effect.transform.position = hitPoint;
        
        var hitColliders = Physics.OverlapSphere(hitPoint, explosionData.ExplosionRadius);
        foreach (var collider in hitColliders)
        {
            if(!collider.GetComponent<PlayerController>()) continue;
            player = collider.GetComponent<PlayerController>();
            var forceVector = (player.transform.position - hitPoint).normalized;
            if (!Physics.Raycast(hitPoint, forceVector, explosionData.ExplosionRadius, _playerMask)) return;
            player.ResetVelocity();
            player.AddForceInVector(forceVector * explosionData.ExplosionForce);
        }
        OnDeSpawn();
    }
}
