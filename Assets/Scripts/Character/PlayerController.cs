using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    private Vector3 _movement;

    [SerializeField] private float gravitySpeed;

    [SerializeField] private float groundedMoveSpeed;
    private float _currentMoveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float crouchMultiplier;
    private float _currentSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float friction;

    private Vector3 _playerVelocity;

    [SerializeField] private float walkingFOV;
    [SerializeField] private float sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;

    private Camera _camera;

    private bool _isCrouching;
    private bool _isSprinting;

    [field: SerializeField] public WeaponBase Weapon { get; private set; }

    private void Start()
    {
        _controller ??= GetComponent<CharacterController>();
        _camera ??= GetComponentInChildren<Camera>();
        _currentSpeed = groundedMoveSpeed;
    }

    private void Update()
    {
        CheckSpeed();
        MovePlayer();
        
        if (_isCrouching && _isSprinting && _controller.isGrounded)
        {
            _isSprinting = false;
            _currentSpeed = groundedMoveSpeed;
        }
    }
    
    private void MovePlayer()
    {
        if (_controller.isGrounded && _playerVelocity.y < 0) _playerVelocity.y = 0;
        _playerVelocity.y += gravitySpeed * Time.deltaTime;
        //Creates a movement vector based on forward and right vector
        var xMovement = transform.right * (_movement.x * _currentSpeed);
        var yMovement = transform.forward * (_movement.z * _currentSpeed);
        var newMovement = xMovement + yMovement;
        
        //Adds friction to the character controller's movement, so they don't stop on a dime
        var frictionMovement = Vector3.Lerp(_playerVelocity, newMovement, friction * Time.deltaTime);
        
        //applies all forces to a velocity value
        _playerVelocity = new Vector3(frictionMovement.x, _playerVelocity.y, frictionMovement.z);
        
        //moves player based on velocity
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    private void CheckSpeed()
    {
        //Adjusts FOV depending on how fast you are going
        if (!_controller.isGrounded) return;
        _camera.fieldOfView = _playerVelocity.magnitude switch
        {
            >= 7f => Mathf.Lerp(_camera.fieldOfView, sprintingFOV, fovAdjustSpeed * Time.deltaTime),
            _ => Mathf.Lerp(_camera.fieldOfView, walkingFOV, fovAdjustSpeed * Time.deltaTime)
        };
    }

    public void ToggleSprint(bool newSprint)
    {
        if(_isCrouching) return;
        //Simply changes your current speed to a sprint speed or a walk speed;
        if (newSprint)
        {
            _isSprinting = true;
            _currentSpeed = groundedMoveSpeed * sprintMultiplier;
        }
        else
        {
            _isSprinting = false;
            _currentSpeed = groundedMoveSpeed;
        }
    }

    public void Jump()
    {
        //note: Player controller ground detection isn't very good, allowing the player to be ungrounded when moving down slopes or stairs
        if (!_controller.isGrounded) return;
        _playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravitySpeed);
        _controller.height = _isCrouching ? 1 : 2;
    }

    public void Crouch(bool crouchState)
    {
        _isCrouching = crouchState;
        _controller.height = _isCrouching ? 1 : 2;
        if (!_controller.isGrounded) return;
        _currentSpeed = _isCrouching ? groundedMoveSpeed * crouchMultiplier : groundedMoveSpeed;
    }

    public void AddForceInVector(Vector3 vector)
    {
        //used to add forces to the player from external sources ex. explosions, jump boosts, etc.
        _playerVelocity += vector;
    }

    public void GetPlayerInput(Vector2 input)
    {
        _movement = new Vector3(input.x, 0, input.y);
    }
}
