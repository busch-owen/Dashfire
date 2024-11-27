using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Physics Attributes"), Space(10)]
    
    [SerializeField] private float gravitySpeed;
    [SerializeField] private float groundedMoveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float crouchMultiplier;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float groundDetectDistance;
    [SerializeField] private float groundDetectRadius;
    private float _currentDrag;
    [SerializeField] private float friction;
    [SerializeField] private float airDrag;
    private CharacterController _controller;
    private Vector3 _movement;
    private Vector3 _playerVelocity;
    private float _currentMoveSpeed;
    private float _currentSpeed;
    private PlayerInputHandler _inputHandler;

    private LayerMask _groundMask;
    
    [Space(10), Header("Camera FOV Attributes"), Space(10)]
    [SerializeField] private float walkingFOV;
    [SerializeField] private float sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;
    private Camera _camera;
    
    
    [field: Space(10), Header("Assigned Weapon Attributes"), Space(10)]
    [field: SerializeField] public WeaponBase LocalWeapon { get; private set; }

    private NetworkWeaponHandler _weaponHandle;

    private NetworkPool _pool;

    private void Awake()
    {
        
    }

    private void Start()
    {
        _controller ??= GetComponent<CharacterController>();
        _camera ??= GetComponentInChildren<Camera>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        _weaponHandle = GetComponentInChildren<NetworkWeaponHandler>();
        _currentSpeed = groundedMoveSpeed;
        _groundMask = LayerMask.GetMask("Default");
        if (!IsOwner)
        {
            _camera.enabled = false;
            return;
        }
    }
    
    
    
    private void Update()
    {
        UpdateWeaponTransform();
        if (!IsOwner) return;
        CheckSpeed();
        UpdateGravity();
        MovePlayer();
        RoofCheck();
        CheckForNewWeapon();
        _currentDrag = IsGrounded() ? friction : airDrag;
    }
    
    private void MovePlayer()
    {
        //Creates a movement vector based on forward and right vector
        var xMovement = transform.right * (_movement.x * _currentSpeed);
        var yMovement = transform.forward * (_movement.z * _currentSpeed);
        var newMovement = xMovement + yMovement;
        
        //Adds friction to the character controller's movement, so they don't stop on a dime
        var frictionMovement = Vector3.Lerp(_playerVelocity, newMovement, _currentDrag * Time.deltaTime);
        
        //applies all forces to a velocity value
        _playerVelocity = new Vector3(frictionMovement.x, _playerVelocity.y, frictionMovement.z);
        
        //moves player based on velocity
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    private void UpdateGravity()
    {
        if (IsGrounded())
        {
            _playerVelocity.y = Mathf.Max(_playerVelocity.y, -5);
        }
        _playerVelocity.y += gravitySpeed * Time.deltaTime;
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, groundDetectRadius, Vector3.down, out hit, groundDetectDistance, _groundMask);
    }

    private void RoofCheck()
    {
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, groundDetectRadius, Vector3.up, out hit, groundDetectDistance, _groundMask))
        {
            Debug.Log("Hit roof");
            _playerVelocity.y = -5;
        }
    }

    private void CheckSpeed()
    {
        //Adjusts FOV depending on how fast you are going
        if (!IsGrounded()) return;
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
            _currentSpeed = groundedMoveSpeed * sprintMultiplier;
        }
        else
        {
            _currentSpeed = groundedMoveSpeed;
        }
    }

    public void Jump()
    {
        //note: Character controller ground detection isn't very good, allowing the player to be ungrounded when moving down slopes or stairs
        //SO some issues I already have with the gravity and jump mechanics is that your velocity isn't cancelled when you hit a ceiling
        if (!IsGrounded()) return;
        _playerVelocity.y = 0;
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

    private void CheckForNewWeapon()
    {
        if (LocalWeapon) return;
        LocalWeapon = GetComponentInChildren<WeaponBase>();
        Debug.Log("found weapon");
    }

    private void UpdateWeaponTransform()
    {
        if (!LocalWeapon) return;
        LocalWeapon.transform.position = _weaponHandle.transform.position;
        LocalWeapon.transform.rotation = _weaponHandle.transform.rotation;
        Debug.Log("setting position");
    }

    public void ShootLocalWeapon()
    {
        if(!LocalWeapon) return;
        LocalWeapon.UseWeapon();
    }

    public void CancelFireLocalWeapon()
    {
        if(!LocalWeapon) return;
        LocalWeapon.CancelFire();
    }
    public void ReloadLocalWeapon()
    {
        if(!LocalWeapon) return;
        LocalWeapon.ReloadWeapon();
    }
}
