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

    private GameObject _hitObject;

    private ulong _castingPlayerId;
    
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
        _hitObject = other.gameObject;
        DealExplosiveDamageRpc();
    }
    
    private void DealExplosiveDamageRpc()
    {
        _rb.isKinematic = true;
        _projectileCollision.SetActive(false);
        
        var hitPoint = transform.position;
        
        var playerController = _hitObject.gameObject.GetComponentInParent<PlayerController>();
        if (playerController)
        {
            if (playerController.OwnerClientId != _castingPlayerId)
                playerController.TakeDamage(explosionData.ExplosionDamage, playerController.OwnerClientId);
        }
        
        var effect = PoolManager.Instance.Spawn(explosionEffect.name);
        effect.transform.position = hitPoint;

        var hitColliders = Physics.OverlapSphere(hitPoint, explosionData.ExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.GetComponent<PlayerController>()) continue;
            var player = hitCollider.GetComponent<PlayerController>();
            var forceVector = (player.transform.position - hitPoint).normalized;
            if (!Physics.Raycast(hitPoint, forceVector, explosionData.ExplosionRadius, _playerMask)) continue;
            player.ResetVelocity();
            player.AddForceInVector(forceVector * explosionData.ExplosionForce);

            if (player.OwnerClientId == _castingPlayerId)
                //Potentially deal less self damage with rocket jumps
                player.TakeDamage(explosionData.ExplosionDamage / 10, player.OwnerClientId);
            else
                player.TakeDamage(explosionData.ExplosionDamage, player.OwnerClientId);
        }
        OnDeSpawn();
    }

    public void SetCasterId(ulong castId)
    {
        _castingPlayerId = castId;
    }

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
