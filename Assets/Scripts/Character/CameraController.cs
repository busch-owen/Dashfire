using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : NetworkBehaviour
{
    [field: SerializeField] public float Sens { get; private set; } = 0.1f;
    [field: SerializeField] public float ScopedSens { get; private set; } = 0.5f;
    private float _currentSens;
    
    private Vector2 _movement;

    [SerializeField] private Vector2 verticalLookRange;

    private CharacterController _controller;
    private PlayerController _player;

    [SerializeField] private NetworkAnimator animator;

    [SerializeField] private float cameraSideTilt;
    [SerializeField] private float cameraTiltSpeed;
    private float _currentCameraTilt;

    private float _yaw, _pitch;
    private float _xInput;

    private WeaponRotator _rotator;

    private float _cameraSmoothVelocity;

    private Transform _deathCamTarget;

    [SerializeField] private Transform standardCamPosition;
    [SerializeField] private Transform deathCamPosition;
    [SerializeField] private float camTransitionSpeed;

    private Transform _cameraStartTransform;

    private void Start()
    {
        LockCamera();
        _controller = GetComponentInParent<CharacterController>();
        _player = GetComponentInParent<PlayerController>();
        _rotator = GetComponentInChildren<WeaponRotator>();
        transform.position = standardCamPosition.position;
        _cameraStartTransform = GetComponentInChildren<Camera>().transform;
        _currentSens = Sens;
    }

    private void Update()
    {
        if(!IsOwner) return;
        if (_player.IsDead)
        {
            HandleDeathCam();
        }
        else
        {
            RotateCamera();
        }
    }

    private void RotateCamera()
    {
        _pitch -= _movement.y * _currentSens;
        _yaw += _movement.x * _currentSens;
        _pitch = Mathf.Clamp(_pitch, verticalLookRange.x, verticalLookRange.y);
        _controller.transform.eulerAngles = new Vector3(0, _yaw, 0);
        
        animator.Animator.SetFloat("LookAngle", -_pitch);
        
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

    public void SetPlayerSensitivity(float newSens)
    {
        Sens = newSens;
        _currentSens = Sens;
    }
    
    public void SetPlayerScopeSensitivity(float newSens)
    {
        ScopedSens = newSens;
        _currentSens = Sens;
    }

    public void SetScopedSens()
    {
        _currentSens = Sens * ScopedSens;
    }

    public void ResetSens()
    {
        _currentSens = Sens;
    }

    public void ResetInput()
    {
        _movement = Vector3.zero;
    }

    public void ResetCameraTransform()
    {
        transform.localPosition = standardCamPosition.localPosition;
        transform.localRotation = standardCamPosition.localRotation;
        var childCamera = GetComponentInChildren<Camera>();
        childCamera.transform.localPosition = _cameraStartTransform.localPosition;
        childCamera.transform.localRotation = _cameraStartTransform.localRotation;
    }

    public void SetDeathCamTarget(Transform target)
    {
        _deathCamTarget = target;
    }
    
    private void HandleDeathCam()
    {
        transform.position = Vector3.Lerp(transform.position, deathCamPosition.position, camTransitionSpeed);
        transform.LookAt(_deathCamTarget);
    }
}
