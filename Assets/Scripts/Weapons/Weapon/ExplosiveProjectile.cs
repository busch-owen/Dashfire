using System.Collections;
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

    private readonly NetworkVariable<bool> _alreadyTriggered = new();
    
    private void OnEnable()
    {
        CancelInvoke(nameof(DespawnObjectRpc));
        _projectileCollision ??= GetComponentInChildren<Collider>().gameObject;
        _projectileCollision.SetActive(true);
        Invoke(nameof(DespawnObjectRpc), lifetime);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        HandlePhysicsRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HandlePhysicsRpc()
    {
        if(IsServer)
            _alreadyTriggered.Value = false;
        var projectileRb = GetComponent<Rigidbody>();
        projectileRb.isKinematic = false;
        projectileRb.linearVelocity = Vector3.zero;
        projectileRb.angularVelocity = Vector3.zero;
        projectileRb.AddForce(transform.forward * explosionData.ProjectileSpeed, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        _hitObject = other.gameObject;
        if(_alreadyTriggered.Value) return;
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
                playerController.TakeDamageRpc(explosionData.ImpactDamage, false, _castingPlayerClientId, _castingPlayerObjId);
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
            player.ResetVelocityRpc();
            player.AddForceInVectorRpc(forceVector * explosionData.ExplosionForce);

            Debug.Log(hitCollider.name);
            
            if (player.OwnerClientId == _castingPlayerClientId)
            {
                player.TakeDamageRpc(explosionData.ExplosionDamage / 10, false, _castingPlayerClientId, _castingPlayerObjId);
                NetworkManager.ConnectedClients.TryGetValue(_castingPlayerClientId, out var castingClientObj);
                if (castingClientObj != null)
                {
                    player.DisplayDamageIndicator(Quaternion.Euler(0, 0, 0));
                }
            }
            else
            {
                player.TakeDamageRpc(explosionData.ExplosionDamage, false, _castingPlayerClientId, _castingPlayerObjId);
                Debug.Log("dealt damage");
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
                    Debug.Log("spawned indicator");
                }
            }
        }
        
        SpawnParticleEffectRpc();
        CancelInvoke(nameof(DespawnObjectRpc));
        DespawnObjectRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnParticleEffectRpc()
    {
        NetworkManager.SpawnManager.InstantiateAndSpawn(explosionEffect.GetComponent<NetworkObject>(), 0UL,
            true, false, false, transform.position, Quaternion.identity);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetCasterIdsRpc(ulong castClientId, ulong castObjId)
    {
        _castingPlayerClientId = castClientId;
        _castingPlayerObjId = castObjId;
    }

    [Rpc(SendTo.Server)]
    private void DespawnObjectRpc()
    {
        if (!IsServer) return;
        _alreadyTriggered.Value = true;
        NetworkObject.Despawn();
    }
}
