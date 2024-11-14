using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    private Vector3 _movement;

    [SerializeField] private float gravitySpeed;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    private float _currentSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float friction;

    [SerializeField] private Vector3 playerVelocity;

    [SerializeField] private float walkingFOV, sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;

    private Camera _camera;

    private bool _grounded;

    private void Start()
    {
        _controller ??= GetComponent<CharacterController>();
        _camera ??= GetComponentInChildren<Camera>();
        _currentSpeed = moveSpeed;
    }

    private void Update()
    {
        MovePlayer();
        CheckSpeed();
    }

    private void MovePlayer()
    {
        var xMovement = transform.right * (_movement.x * _currentSpeed);
        var yMovement = transform.forward * (_movement.z * _currentSpeed);
        var newMovement = xMovement + yMovement;
        
        if (_controller.isGrounded && playerVelocity.y < 0) playerVelocity.y = 0;
        var frictionMovement = Vector3.Lerp(playerVelocity, newMovement, friction * Time.deltaTime);
        playerVelocity = new Vector3(frictionMovement.x, playerVelocity.y, frictionMovement.z);
        playerVelocity.y += gravitySpeed * Time.deltaTime;
        _controller.Move(playerVelocity * Time.deltaTime);
    }

    private void CheckSpeed()
    {
        _camera.fieldOfView = playerVelocity.magnitude switch
        {
            >= 7f => Mathf.Lerp(_camera.fieldOfView, sprintingFOV, fovAdjustSpeed * Time.deltaTime),
            _ => Mathf.Lerp(_camera.fieldOfView, walkingFOV, fovAdjustSpeed * Time.deltaTime)
        };
    }

    public void ToggleSprint(bool newSprint)
    {
        _currentSpeed = newSprint switch
        {
            true => moveSpeed * sprintMultiplier,
            false => moveSpeed
        };
    }

    public void Jump()
    {
        if (!_controller.isGrounded) return;
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravitySpeed);
    }

    public void GetPlayerInput(Vector2 input)
    {
        _movement = new Vector3(input.x, 0, input.y);
    }
}
