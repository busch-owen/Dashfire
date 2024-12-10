using System;
using UnityEngine;

public class WeaponRotator : MonoBehaviour
{
    [SerializeField] private float rotationAmount;
    [SerializeField] private float rotationSpeed;

    private Vector2 _gunSmoothVelocity; 
    private Vector2 _movement;
    private Vector2 _swayVector;

    private WeaponBase _weapon;

    private void Start()
    {
        _weapon = GetComponentInChildren<WeaponBase>();
    }

    private void Update()
    {
        if(_weapon)
            if (_weapon.AimDownSights)
                _movement = Vector3.zero;
        
        CalculateRotations();
    }

    public void GetInput(Vector2 input)
    {
        _movement = new Vector2(input.y, -input.x);
    }
    
    private void CalculateRotations()
    {
        _swayVector = Vector2.SmoothDamp(_swayVector, _movement.normalized * rotationAmount, ref _gunSmoothVelocity, rotationSpeed);

        transform.localEulerAngles = _swayVector;
    }
}
