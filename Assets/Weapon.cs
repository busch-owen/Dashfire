using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Animator _animator;
    
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");
    
    //Base weapon class, will eventually utilize scriptable objects to get data for each weapon

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    //Action functions will only play animations for the moment
    public void Shoot()
    {
        _animator.SetTrigger(ShootTrigger);
    }

    public void Reload()
    {
        _animator.SetTrigger(ReloadTrigger);
    }
}
