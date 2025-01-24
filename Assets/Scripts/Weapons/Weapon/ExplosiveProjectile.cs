using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ExplosiveProjectile : NetworkBehaviour
{
    //private Rigidbody _rb;
    private GameObject _projectileCollision;
    [SerializeField] private ExplosionDataSO explosionData;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject prefabRef;
    [SerializeField] private float lifetime;

    private GameObject _hitObject;

    private ulong _castingPlayerClientId;
    private ulong _castingPlayerObjId;

    [SerializeField] private LayerMask playerMask;
    
    private void OnEnable()
    {
        _projectileCollision ??= GetComponentInChildren<Collider>().gameObject;
        _projectileCollision.SetActive(true);
        if(!IsOwner) return;
        Invoke(nameof(DespawnObjectRpc), lifetime);
    }

    private void OnCollisionEnter(Collision other)
    {
        _hitObject = other.gameObject;
        DealExplosiveDamageRpc();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void DealExplosiveDamageRpc()
    {
        if(!_hitObject) return;
        if(_hitObject.GetComponentInParent<PlayerController>())
            if (_hitObject.GetComponentInParent<PlayerController>().OwnerClientId == _castingPlayerClientId) return;
        _projectileCollision.SetActive(false);
        
        var hitPoint = transform.position;
        
        var playerController = _hitObject.gameObject.GetComponentInParent<PlayerController>();
        if (playerController)
        {
            if (playerController.OwnerClientId != _castingPlayerClientId)
            {
                playerController.TakeDamage(explosionData.ImpactDamage, false, _castingPlayerClientId, _castingPlayerObjId);
                var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                indicator.transform.position = transform.position;
                indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                indicator.UpdateDisplay(explosionData.ImpactDamage, false, 1);
            }
        }

        var hitColliders = Physics.OverlapSphere(hitPoint, explosionData.ExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.GetComponent<PlayerController>()) continue;
            var player = hitCollider.GetComponent<PlayerController>();
            var forceVector = (player.transform.position - hitPoint).normalized;
            if (!Physics.Raycast(hitPoint, forceVector, explosionData.ExplosionRadius, playerMask)) continue;
            player.ResetVelocity();
            player.AddForceInVector(forceVector * explosionData.ExplosionForce);

            if(!player.IsOwner) continue;
            
            if (player.OwnerClientId == _castingPlayerClientId)
            {
                player.TakeDamage(explosionData.ExplosionDamage / 10, false, _castingPlayerClientId, _castingPlayerObjId);
                NetworkManager.ConnectedClients.TryGetValue(_castingPlayerClientId, out var castingClientObj);
                if (castingClientObj != null)
                {
                    player.DisplayDamageIndicator(Quaternion.Euler(0, 0, 0));
                }
            }
            else
            {
                player.TakeDamage(explosionData.ExplosionDamage, false, _castingPlayerClientId, _castingPlayerObjId);
                var indicator = PoolManager.Instance.Spawn("DamageIndicator").GetComponent<DamageIndicator>();
                indicator.transform.position = transform.position;
                indicator.transform.rotation = Quaternion.Euler(0, 0, 0);
                indicator.UpdateDisplay(explosionData.ExplosionDamage, false, 1);
                
                NetworkManager.ConnectedClients.TryGetValue(_castingPlayerClientId, out var castingClientObj);
                if (castingClientObj != null)
                {
                    if(!castingClientObj.PlayerObject) break;
                    var tPos = castingClientObj.PlayerObject.transform.position;
                    var tRot = castingClientObj.PlayerObject.transform.rotation;

                    var direction = transform.position - tPos;

                    tRot = Quaternion.LookRotation(direction);
                    tRot.z = -tRot.y;
                    tRot.x = 0;
                    tRot.y = 0;

                    var currentForwards = new Vector3(0, 0, player.transform.eulerAngles.y);

                    var newRotation = tRot * Quaternion.Euler(currentForwards);
                    player.DisplayDamageIndicator(newRotation);
                }
            }
        }

        NetworkManager.SpawnManager.InstantiateAndSpawn(explosionEffect.GetComponent<NetworkObject>(), 0UL,
            true, false, false, transform.position, Quaternion.identity);
        
        if(!IsOwner) return;
        CancelInvoke(nameof(DespawnObjectRpc));
        DespawnObjectRpc();
    }

    public void SetCasterIds(ulong castClientId, ulong castObjId)
    {
        _castingPlayerClientId = castClientId;
        _castingPlayerObjId = castObjId;
    }

    [Rpc(SendTo.Server)]
    private void DespawnObjectRpc()
    {
        if (!IsServer) return;
        NetworkObject.Despawn();
    }
}
