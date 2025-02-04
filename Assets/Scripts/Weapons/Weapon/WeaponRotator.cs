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

    private void Update()
    {
        _weapon = GetComponentInChildren<WeaponBase>();
        if(_weapon)
            if (_weapon.AimDownSights)
                _movement = Vector3.zero;
        
        CalculateRotations();
    }

    public void GetInput(Vector2 input)
    {
        _movement = new Vector2(input.y, -input.x);
        _movement = Vector2.ClampMagnitude(_movement, rotationAmount);
    }
    
    private void CalculateRotations()
    {
        _swayVector = Vector2.SmoothDamp(_swayVector, _movement * rotationAmount, ref _gunSmoothVelocity, rotationSpeed * Time.deltaTime);
        
        transform.localEulerAngles = _swayVector;
        
    }
}
