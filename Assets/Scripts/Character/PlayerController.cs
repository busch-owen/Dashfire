using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    #region Physics Variables
    
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

    [SerializeField] private GameObject headObj;
    [SerializeField] private GameObject bodyObj;
    [SerializeField] private GameObject glassesObj;

    private LayerMask _groundMask;
    
    #endregion

    #region UI Variables

    private PlayerCanvasHandler _canvasHandler;

    private ScoreboardEntry _assignedScoreboard;
    
    #endregion
    
    #region Health and Armor Variables
    
    [field: Header("Player Health and Armor Attributes"), Space(10)]
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public int MaxArmor { get; private set; }
    [SerializeField] private float armorDamping;
    public int CurrentHealth { get; private set; }
    public int CurrentArmor { get; private set; }

    private SpawnPoint[] _spawnPoints;

    #endregion

    #region Camera Variables
    
    [Space(10), Header("Camera FOV Attributes"), Space(10)]
    [SerializeField] private float walkingFOV;
    [SerializeField] private float sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;
    private Camera _camera;

    #endregion

    #region Weapon Attributes
    
    [field: Space(10), Header("Assigned Weapon Attributes"), Space(10)]

    [SerializeField] private WeaponBase starterWeapon;

    public WeaponBase[] EquippedWeapons { get; private set; } = new WeaponBase[2];
    public int CurrentWeaponIndex { get; private set; }

    private NetworkItemHandler _itemHandle;

    private NetworkPool _pool;
    
    #endregion

    #region Networking Variables

    public static event Action<GameObject> OnPlayerSpawned;
    public static event Action<GameObject> OnPlayerDespawned;

    #endregion

    #region Unity Runtime Functions
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(PlayerSpawnRoutine());
    }

    private IEnumerator PlayerSpawnRoutine()
    {
        yield return new WaitForSeconds(0.01f);
        OnPlayerSpawned?.Invoke(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnPlayerDespawned?.Invoke(gameObject);
    }

    private void Start()
    {
        _controller ??= GetComponent<CharacterController>();
        _camera ??= GetComponentInChildren<Camera>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        _itemHandle = GetComponentInChildren<NetworkItemHandler>();
        _canvasHandler = GetComponentInChildren<PlayerCanvasHandler>();
        _currentSpeed = groundedMoveSpeed;
        _groundMask = LayerMask.GetMask("Default");
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
        
        headObj.GetComponent<MeshRenderer>().material.color = GetComponent<PlayerData>().PlayerColor.Value;
        bodyObj.GetComponent<MeshRenderer>().material.color = GetComponent<PlayerData>().PlayerColor.Value;
        
        CurrentHealth = MaxHealth;
        CurrentArmor = 0;
        
        _canvasHandler.UpdateHealth(CurrentHealth);
        _canvasHandler.UpdateArmor(CurrentArmor);
        _canvasHandler.UpdateAmmo(0, 0);
        
        if (!IsOwner)
        {
            gameObject.name += "_CLIENT";
            _camera.enabled = false;
            _camera.GetComponent<AudioListener>().enabled = false;
            headObj.layer = LayerMask.NameToLayer("EnemyPlayer");
            bodyObj.layer = LayerMask.NameToLayer("EnemyPlayer");
            _canvasHandler.GetComponent<CanvasGroup>().alpha = 0;
        }
        else
        {
            gameObject.name += "_LOCAL";
            headObj.GetComponent<MeshRenderer>().enabled = false;
            bodyObj.GetComponent<MeshRenderer>().enabled = false;
            glassesObj.SetActive(false);
        }
        
        
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        CheckSpeed();
        UpdateGravity();
        MovePlayer();
        RoofCheck();
        _currentDrag = IsGrounded() ? friction : airDrag;
    }

    private void FixedUpdate()
    {
        if(!IsOwner) return;
        //Ping isn't working at the moment, will investigate more later
        //SendServerPingClientRpc();
    }

    #endregion

    #region Physics and Input

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

    #endregion
    
    #region Weapons

     public void AssignNewWeapon(WeaponBase newWeapon)
    {
        if (!EquippedWeapons[CurrentWeaponIndex])
        {
            newWeapon.transform.parent = _itemHandle.transform;
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.rotation = _itemHandle.transform.rotation;
            EquippedWeapons[CurrentWeaponIndex] = newWeapon.GetComponent<WeaponBase>();
        }
        else if(EquippedWeapons[CurrentWeaponIndex])
        {
            for (var i = 0; i < EquippedWeapons.Length; i++)
            {
                if (EquippedWeapons[i] != null) continue;
                newWeapon.transform.parent = _itemHandle.transform;
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _itemHandle.transform.rotation;
                EquippedWeapons[i] = newWeapon.GetComponent<WeaponBase>();
                newWeapon.gameObject.SetActive(false);
            }
        }
        else
        {
            newWeapon.transform.parent = _itemHandle.transform;
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.rotation = _itemHandle.transform.rotation;
            EquippedWeapons[CurrentWeaponIndex] = newWeapon.GetComponent<WeaponBase>();
        }
    }

    public void ChangeItemSlot(int index)
    {
        if (!IsOwner) return;
        if (index == CurrentWeaponIndex) return;
        if (EquippedWeapons[index] == null) return;
        CurrentWeaponIndex = index;
        _itemHandle.RequestWeaponSwapRpc(CurrentWeaponIndex, NetworkObjectId);
        _canvasHandler.UpdateAmmo(EquippedWeapons[CurrentWeaponIndex].currentAmmo, EquippedWeapons[CurrentWeaponIndex].WeaponSO.AmmoCount);
    }

    public void ShootLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex]) return;
        EquippedWeapons[CurrentWeaponIndex].UseWeapon();
    }

    public void CancelFireLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex]) return;
        EquippedWeapons[CurrentWeaponIndex].CancelFire();
    }
    public void ReloadLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex]) return;
        EquippedWeapons[CurrentWeaponIndex].ReloadWeapon();
    }

    #endregion
    
    #region Health and Armor

    public void TakeDamage(float damageToDeal, ulong dealerId)
    {
        var armorDamage = damageToDeal * armorDamping;
        var playerDamage = CurrentArmor > 0 ? damageToDeal - armorDamage : damageToDeal;
        
        if (CurrentArmor > 0)
        {
            CurrentArmor -= (int)armorDamage;
        }
        else
        {
            CurrentArmor = 0;
        }
        
        CurrentHealth -= (int)playerDamage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            HandleDeath(dealerId);
        }
    }

    public void HealPlayer(int healAmount)
    {
        CurrentHealth += healAmount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        UpdateStats();
    }
    
    public void HealArmor(int healAmount)
    {
        CurrentArmor += healAmount;
        if (CurrentArmor > MaxArmor)
        {
            CurrentArmor = MaxArmor;
        }
        UpdateStats();
    }

    private void UpdateStats()
    {
        _canvasHandler.UpdateHealth(CurrentHealth);
        _canvasHandler.UpdateArmor(CurrentArmor);
    }

    public void SetStats(int newHealth, int newArmor)
    {
        CurrentHealth = newHealth;
        CurrentArmor = newArmor;
        UpdateStats();
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentArmor = 0;
        UpdateStats();
    }

    private void HandleDeath(ulong castingId)
    {
        _itemHandle.RespawnSpecificPlayerRpc(NetworkObjectId, castingId);
    }

    #endregion

    #region Netcode Functions
    
    [ClientRpc]
    private void SendServerPingClientRpc()
    {
        UpdatePingServerRpc();
    }
    
    [ServerRpc]
    private void UpdatePingServerRpc()
    {
        if (!IsServer || !IsOwner) return;
        GetComponent<PlayerData>().PlayerPingMs.Value = NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
    }

    #endregion
}
