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

    private ulong _castingPlayerClientId;
    private ulong _castingPlayerObjId;
    
    private LayerMask _playerMask;
    
    private void OnEnable()
    {
        CancelInvoke(nameof(OnDeSpawn));
        _rb ??= GetComponent<Rigidbody>();
        _rb.isKinematic = false;
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
        if(_hitObject.GetComponentInParent<PlayerController>())
            if (_hitObject.GetComponentInParent<PlayerController>().OwnerClientId == _castingPlayerClientId) return;
        _rb.isKinematic = true;
        _projectileCollision.SetActive(false);
        
        var hitPoint = transform.position;
        
        var playerController = _hitObject.gameObject.GetComponentInParent<PlayerController>();
        if (playerController)
        {
            if (playerController.OwnerClientId != _castingPlayerClientId)
            {
                playerController.TakeDamage(explosionData.ImpactDamage, _castingPlayerClientId);
                var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                indicator.transform.position = _hitObject.transform.position;
                indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                indicator.UpdateDisplay(explosionData.ImpactDamage, false, 1);
            }
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

            if (player.OwnerClientId == _castingPlayerClientId)
                //Potentially deal less self damage with rocket jumps
                player.TakeDamage(explosionData.ExplosionDamage / 10, _castingPlayerClientId);
            else
            {
                player.TakeDamage(explosionData.ExplosionDamage, _castingPlayerClientId);
                var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                indicator.transform.position = _hitObject.transform.position;
                indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                indicator.UpdateDisplay(explosionData.ExplosionDamage, false, 1);
            }
        }
        OnDeSpawn();
    }

    public void SetCasterIds(ulong castClientId, ulong castObjId)
    {
        _castingPlayerClientId = castClientId;
        _castingPlayerObjId = castObjId;
    }

    private void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }
}
