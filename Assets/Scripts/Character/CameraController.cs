using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private float xSens, ySens;

    private Vector2 _movement;

    [SerializeField] private Vector2 verticalLookRange;

    private CharacterController _controller;

    [SerializeField] private float cameraSideTilt;
    [SerializeField] private float cameraTiltSpeed;
    private float _currentCameraTilt;

    private float _yaw, _pitch;
    private float _xInput;

    private WeaponRotator _rotator;

    private float _cameraSmoothVelocity;

    private void Start()
    {
        LockCamera();
        _controller = GetComponentInParent<CharacterController>();
        _rotator = GetComponentInChildren<WeaponRotator>();
    }

    private void Update()
    {
        if(!IsOwner) return;
        RotateCamera();
    }

    private void RotateCamera()
    {
        _pitch -= _movement.y * ySens * Time.deltaTime;
        _yaw += _movement.x * xSens * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, verticalLookRange.x, verticalLookRange.y);
        _controller.transform.eulerAngles = new Vector3(0, _yaw, 0);
        
        _currentCameraTilt = Mathf.SmoothDamp(_currentCameraTilt, cameraSideTilt * -_xInput, ref _cameraSmoothVelocity, cameraTiltSpeed);
        transform.localEulerAngles = new Vector3(_pitch, 0, _currentCameraTilt);
        _rotator.GetInput(_movement);
    }

    public void LockCamera()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GetMoveInput(Vector2 input)
    {
        _xInput = input.x;
    }

    public void GetCameraInput(Vector2 input)
    {
        _movement = input;
    }
}
