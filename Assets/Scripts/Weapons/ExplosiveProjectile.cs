using Unity.Netcode;
using UnityEngine;

public class ExplosiveProjectile : NetworkBehaviour
{
    private Rigidbody _rb;
    private GameObject _projectileCollision;
    [SerializeField] private ExplosionDataSO explosionData;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject prefabRef;
    [SerializeField] private float lifetime;
    
    
    private LayerMask _playerMask;
    
    private void OnEnable()
    {
        CancelInvoke(nameof(OnDeSpawn));
        _rb ??= GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _projectileCollision ??= GetComponentInChildren<Collider>().gameObject;
        _projectileCollision.SetActive(true);
        _playerMask = LayerMask.GetMask("ControlledPlayer");
        Invoke(nameof(OnDeSpawn), lifetime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.GetComponent<PlayerController>()) return;
        _rb.isKinematic = true;
        _projectileCollision.SetActive(false);
        
        var hitPoint = transform.position;
        PlayerController player;
        var effect = PoolManager.Instance.Spawn(explosionEffect.name);
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

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
