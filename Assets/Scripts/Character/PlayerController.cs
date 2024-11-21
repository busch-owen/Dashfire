using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Player Physics Attributes"), Space(10)]
    
    [SerializeField] private float gravitySpeed;
    [SerializeField] private float groundedMoveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float crouchMultiplier;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float friction;
    private CharacterController _controller;
    private Vector3 _movement;
    private Vector3 _playerVelocity;
    private float _currentMoveSpeed;
    private float _currentSpeed;
    private bool _isSprinting;
    
    [Space(10), Header("Camera FOV Attributes"), Space(10)]
    [SerializeField] private float walkingFOV;
    [SerializeField] private float sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;
    private Camera _camera;
    
    
    [field: Space(10), Header("Assigned Weapon Attributes"), Space(10)]
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
        UpdateGravity();
        MovePlayer();
    }
    
    private void MovePlayer()
    {
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

    private void UpdateGravity()
    {
        if (_controller.isGrounded && _playerVelocity.y < 0) _playerVelocity.y = 0;
        _playerVelocity.y += gravitySpeed * Time.deltaTime;
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
        //note: Character controller ground detection isn't very good, allowing the player to be ungrounded when moving down slopes or stairs
        //SO some issues I already have with the gravity and jump mechanics is that your velocity isn't cancelled when you hit a ceiling
        if (!_controller.isGrounded) return;
        _playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravitySpeed);
    }

    public void ResetVelocity()
    {
        _playerVelocity = Vector3.zero;
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
