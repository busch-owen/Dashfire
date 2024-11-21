using System;
using UnityEngine;

public class ExplosiveProjectile : PoolObject
{
    private Rigidbody _rb;
    private GameObject _projectileCollision;
    [SerializeField] private ExplosionDataSO explosionData;

    private void OnEnable()
    {
        _rb ??= GetComponent<Rigidbody>();
        _projectileCollision ??= GetComponentInChildren<Collider>().gameObject;
        _rb.isKinematic = false;
        _projectileCollision.SetActive(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        _rb.isKinematic = true;
        _projectileCollision.SetActive(false);

        var hitPoint = transform.position;
        
        //NOTE: THIS DOES NOT WORK - REWORK NEEDED
        
        RaycastHit hit;
        if (Physics.SphereCast(hitPoint, explosionData.ExplosionRadius, Vector3.up, out hit))
        {
            Debug.Log("Something");
            if (hit.transform.GetComponent<PlayerController>())
            {
                Debug.Log("'Something' was a player");
                if (Physics.Raycast(hitPoint, hit.transform.position - hitPoint))
                {
                    Debug.DrawRay(hitPoint, hit.transform.position - hitPoint);
                    Debug.Log("player unobstructed with vector: " + (hit.transform.position - hitPoint));
                }
            }
        }
        else
        {
            Debug.Log("Hit nothing");
        }
        
    }
}
