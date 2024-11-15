using System;
using UnityEngine;

public class WeaponRotator : MonoBehaviour
{
    [SerializeField] private float rotationAmount;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxRot;

    private Vector2 _movement;

    private void Update()
    {
        CalculateRotations();
    }

    public void GetInput(Vector2 input)
    {
        _movement = input;
    }
    
    private void CalculateRotations()
    {
        var xRot = Mathf.SmoothDamp(-_movement.x * rotationAmount * Time.fixedDeltaTime, 0, ref _movement.x,
            rotationSpeed);
        var yRot = Mathf.SmoothDamp(_movement.y * rotationAmount * Time.fixedDeltaTime, 0, ref _movement.y,
            rotationSpeed);
        xRot = Mathf.Clamp(xRot, -maxRot, maxRot);
        yRot = Mathf.Clamp(yRot, -maxRot, maxRot);

        transform.localEulerAngles = new Vector3(yRot, xRot, 0);
    }
}
