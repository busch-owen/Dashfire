using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float xSens, ySens;

    private Vector2 _movement;

    [SerializeField] private Vector2 verticalLookRange;

    private CharacterController _controller;

    private float _yaw, _pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _controller = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        _pitch -= _movement.y * ySens;
        _yaw += _movement.x * xSens;
        _pitch = Mathf.Clamp(_pitch, verticalLookRange.x, verticalLookRange.y);
        transform.localEulerAngles = new Vector3(_pitch, 0, 0);
        _controller.transform.eulerAngles = new Vector3(0, _yaw, 0);
    }

    public void GetCameraInput(Vector2 input)
    {
        _movement = input;
    }
}
