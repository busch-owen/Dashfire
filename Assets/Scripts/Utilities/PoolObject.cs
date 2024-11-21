using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    #region Despawn
    public void OnDeSpawn()
    {
        PoolManager.Instance.DeSpawn(this); //if we are told to despawn, talk to the pool manager and despawn
    }
    #endregion
}
