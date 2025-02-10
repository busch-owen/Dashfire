using System;
using System.Collections;
using Steamworks.Ugc;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    #region Physics Variables

    [Header("Player Physics Attributes"), Space(10)] [SerializeField]
    private float gravitySpeed;

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
    
    [SerializeField] private GameObject bodyObj;
    [SerializeField] private GameObject headObj;
    [SerializeField] private GameObject hitboxes;

    private LayerMask _groundMask;
    private LayerMask _ignoreMask;
    private LayerMask _enemyMask;
    private LayerMask _aliveMask;
    private LayerMask _deadMask;

    #endregion

    #region UI Variables

    private PlayerCanvasHandler _canvasHandler;

    [SerializeField] private KillBanner killBanner;
    [SerializeField] private GameObject deathFade;

    private ScoreboardEntry _assignedScoreboard;

    #endregion

    #region Health and Armor Variables

    [field: Header("Player Health and Armor Attributes"), Space(10)]
    [field: SerializeField]
    public int MaxHealth { get; private set; }
    
    public bool IsDead { get; private set; }

    [field: SerializeField] public int MaxArmor { get; private set; }
    [SerializeField] private float armorDamping;

    [SerializeField] private float lastAttackingPlayerGraceTime;
    public int CurrentHealth { get; private set; }
    public int CurrentArmor { get; private set; }

    private SpawnPoint[] _spawnPoints;

    [SerializeField] private float deathTimer;

    private PlayerController _lastAttackingPlayer = null;

    #endregion

    #region Sound Variables

    [field: SerializeField] public AudioClip[] HitSound { get; private set; }
    [field: SerializeField] public AudioClip[] HeadShotSound { get; private set; }
    [field: SerializeField] public AudioClip[] DeathSound { get; private set; }

#endregion

    #region Camera Variables
    
    [Space(10), Header("Camera FOV Attributes"), Space(10)]
    [SerializeField] private float walkingFOV;
    [SerializeField] private float sprintingFOV;
    [SerializeField] private float fovAdjustSpeed;
    private Camera _camera;
    private CameraController _cameraController;

    #endregion

    #region Weapon Attributes
    
    [field: Space(10), Header("Assigned Weapon Attributes"), Space(10)]

    [SerializeField] private WeaponBase starterWeapon;

    [SerializeField] private AmmoPickup deathPickup;

    public WeaponBase[] EquippedWeapons { get; private set; } = new WeaponBase[2];
    public int CurrentWeaponIndex { get; private set; }

    private NetworkItemHandler _itemHandle;
    [SerializeField] private Transform firstPersonItemHandler;
    [SerializeField] private Transform thirdPersonItemHandler;

    private NetworkPool _pool;

    private AmmoReserve _reserve;

    private ItemPickup _currentPickup;
    private bool _pickupAvailable;
    
    public bool InventoryFull { get; private set; }
    
    #endregion

    #region Networking Variables
    public static event Action<GameObject> OnPlayerSpawned;
    public static event Action<GameObject> OnPlayerDespawned;
    public event Action OnHostQuit;

    private PlayerController _hostPlayer;

    #endregion

    #region Various Variables

    private PlayerData _playerData;

    private WaitForFixedUpdate _waitForFixed;
    private WaitForSeconds _waitForDeathTimer;

    [SerializeField] private Transform rightHandControl;
    [SerializeField] private Transform leftHandControl;

    #endregion

    #region Animation

    [SerializeField] private NetworkAnimator animator;

    #endregion

    #region Unity Runtime Functions
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(PlayerSpawnRoutine());
        var playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in playerControllers)
        {
            if (player.IsHost)
            {
                _hostPlayer = player;
            }
        }
        GetComponent<PlayerData>().PlayerFrags.OnValueChanged += PlayDeathSound;
        _hostPlayer.OnHostQuit += DisconnectOnHostLeave;
    }

    private IEnumerator PlayerSpawnRoutine()
    {
        yield return new WaitForSeconds(0.01f);
        OnPlayerSpawned?.Invoke(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        if (IsHost)
        {
            OnHostQuit?.Invoke();
        }
        base.OnNetworkDespawn();
        OnPlayerDespawned?.Invoke(gameObject);
        GetComponent<PlayerData>().PlayerFrags.OnValueChanged -= PlayDeathSound;
    }
    
    private void DisconnectOnHostLeave()
    {
        SceneManager.LoadScene("LobbyScreen");
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        _itemHandle = GetComponentInChildren<NetworkItemHandler>();
        _controller ??= GetComponent<CharacterController>();
        _camera ??= GetComponentInChildren<Camera>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        _canvasHandler = GetComponentInChildren<PlayerCanvasHandler>();
        _cameraController = GetComponentInChildren<CameraController>();
        _currentSpeed = groundedMoveSpeed;
        _groundMask = LayerMask.GetMask("Default");
        _aliveMask = LayerMask.NameToLayer("ControlledPlayer");
        _enemyMask = LayerMask.NameToLayer("EnemyPlayer");
        _deadMask = LayerMask.NameToLayer("DeadPlayer");
        _ignoreMask = LayerMask.NameToLayer("Ignore Raycast");
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
        _waitForFixed = new WaitForFixedUpdate();
        _waitForDeathTimer = new WaitForSeconds(deathTimer);
        _reserve = GetComponent<AmmoReserve>();
        _playerData = GetComponent<PlayerData>();
        
        CurrentHealth = MaxHealth;
        CurrentArmor = 0;
        
        _canvasHandler.UpdateHealth(CurrentHealth);
        _canvasHandler.UpdateArmor(CurrentArmor);
        _canvasHandler.UpdateAmmo(0, 0, true);
        
        IsDead = false;
        
        UpdateColors();
        
        if (IsOwner)
        {
            gameObject.name += "_LOCAL";
            //gameObject.layer = _aliveMask;
            //bodyObj.layer = _aliveMask;
            var localColliders = hitboxes.GetComponentsInChildren<Collider>();
            foreach (var col in localColliders)
            {
                col.gameObject.layer = _aliveMask;
            }
            DisableBodyVisuals();
            ChangeItemHandleTPRpc();
            _itemHandle.RequestWeaponSpawnRpc(starterWeapon.name, NetworkObjectId);
        }
        else
        {
            gameObject.name += "_CLIENT";
            _camera.enabled = false;
            _camera.gameObject.tag = "SecondaryCamera";
            _camera.GetComponent<AudioListener>().enabled = false;
            //gameObject.layer = _enemyMask;
            //bodyObj.layer = _enemyMask;
            var localColliders = hitboxes.GetComponentsInChildren<Collider>();
            foreach (var col in localColliders)
            {
                col.gameObject.layer = _enemyMask;
            }
            _canvasHandler.GetComponent<CanvasGroup>().alpha = 0;
        }
    }
    
    private void Update()
    {
        if (!IsOwner) return;

        if (!IsDead)
        {
            CheckSpeed();
            UpdateGravity();
            MovePlayer();
            RoofCheck();
        }
        _currentDrag = IsGrounded() ? friction : airDrag;
        animator.Animator.SetBool("Grounded", IsGrounded());
    }

    private void FixedUpdate()
    {
        UpdateHandPosition();
        if(!IsOwner) return;
        UpdatePingRpc();
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
        
        animator.Animator.SetFloat("MovementX", _playerVelocity.normalized.z);
        animator.Animator.SetFloat("MovementY", _playerVelocity.normalized.x);
        var horizontalVector = new Vector2(_playerVelocity.normalized.x, _playerVelocity.normalized.z);
        var moving = !Mathf.Approximately(horizontalVector.magnitude, 0);
        animator.Animator.SetBool("Moving", moving);
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
        if(EquippedWeapons[CurrentWeaponIndex])
            if (EquippedWeapons[CurrentWeaponIndex].AimDownSights) return;
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
            _currentSpeed = groundedMoveSpeed * sprintMultiplier * EquippedWeapons[CurrentWeaponIndex].WeaponSO.MovementSpeedMultiplier;
        }
        else
        {
            _currentSpeed = groundedMoveSpeed * EquippedWeapons[CurrentWeaponIndex].WeaponSO.MovementSpeedMultiplier;
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

    [Rpc(SendTo.ClientsAndHost)]
    public void ResetVelocityRpc()
    {
        if(!IsOwner) return;
        _playerVelocity = Vector3.zero;
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void AddForceInVectorRpc(Vector3 vector)
    {
        //used to add forces to the player from external sources ex. explosions, jump boosts, etc.
        if(!IsOwner) return;
        _playerVelocity += vector;
    }

    public void GetPlayerInput(Vector2 input)
    {
        _movement = new Vector3(input.x, 0, input.y);
    }

    public void ResetInputs()
    {
        _movement = Vector3.zero;
        _cameraController.ResetInput();
    }

    #endregion
    
    #region Weapons

    public void AssignNewWeapon(WeaponBase newWeapon)
    {
        if (!newWeapon || IsDead) return;
        _itemHandle = GetComponentInChildren<NetworkItemHandler>();

        if (InventoryFull)
        {
            if (!CheckPickupSimilarity(newWeapon)) return;
            EquippedWeapons[CurrentWeaponIndex] = null;
            newWeapon.transform.parent = _itemHandle.transform;
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.rotation = _itemHandle.transform.rotation;
            EquippedWeapons[CurrentWeaponIndex] = newWeapon.GetComponent<WeaponBase>();
            EquippedWeapons[CurrentWeaponIndex].gameObject.SetActive(true);
        }
        else if (!EquippedWeapons[CurrentWeaponIndex]) // No current weapon in equipped slot
        {
            newWeapon.transform.parent = _itemHandle.transform;
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.rotation = _itemHandle.transform.rotation;
            CurrentWeaponIndex = 0;
            EquippedWeapons[CurrentWeaponIndex] = newWeapon.GetComponent<WeaponBase>();
        }
        else if (EquippedWeapons[CurrentWeaponIndex]) // If there is an equipped item
        {
            if (!CheckPickupSimilarity(newWeapon)) return;
            for (var i = 0; i < EquippedWeapons.Length; i++) //Check if there is an empty slot
            {
                if (EquippedWeapons[i]) continue; // if not empty, check next one, if all full, continue
                newWeapon.transform.parent = _itemHandle.transform;
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.rotation = _itemHandle.transform.rotation;
                EquippedWeapons[i] = newWeapon.GetComponent<WeaponBase>();
                CurrentWeaponIndex = i;
                newWeapon.gameObject.SetActive(true);
                InventoryFull = i + 1 >= EquippedWeapons.Length;
            }
        }
        
        if(!IsOwner) return;
        _canvasHandler?.UpdateAmmo(EquippedWeapons[CurrentWeaponIndex].currentAmmo, _reserve.ContainersDictionary[EquippedWeapons[CurrentWeaponIndex].WeaponSO.RequiredAmmo].currentCount, true);
        _currentSpeed = groundedMoveSpeed * EquippedWeapons[CurrentWeaponIndex].WeaponSO.MovementSpeedMultiplier;
    }

    public bool CheckPickupSimilarity(WeaponBase weaponToTest)
    {
        if (!weaponToTest) return false;
        foreach (var weapon in EquippedWeapons)
        {
            if (weapon == null) continue;
            if (weapon.WeaponSO == weaponToTest.WeaponSO) return false;
        }
        return true;
    }
    
    private void UpdateHandPosition()
    {
        if(!EquippedWeapons[CurrentWeaponIndex]) return;
        if(!EquippedWeapons[CurrentWeaponIndex].LeftHandPos || !EquippedWeapons[CurrentWeaponIndex].RightHandPos) return;
        rightHandControl.transform.position = EquippedWeapons[CurrentWeaponIndex].RightHandPos.position;
        leftHandControl.transform.position = EquippedWeapons[CurrentWeaponIndex].LeftHandPos.position;
    }

    public void ChangeItemSlot(float index)
    {
        if (!IsOwner || IsDead) return;
        if (index <= 0)
        {
            if (0 == CurrentWeaponIndex) return;
            if (EquippedWeapons[0] == null) return;
            UpdateEquippedIndexRpc(NetworkObjectId, 0);
            _itemHandle.RequestWeaponSwapRpc(CurrentWeaponIndex, NetworkObjectId);
            _canvasHandler.UpdateAmmo(EquippedWeapons[CurrentWeaponIndex].currentAmmo, _reserve.ContainersDictionary[EquippedWeapons[CurrentWeaponIndex].WeaponSO.RequiredAmmo].currentCount, true);
            _currentSpeed = groundedMoveSpeed * EquippedWeapons[0].WeaponSO.MovementSpeedMultiplier;
        }
        else
        {
            if (1 == CurrentWeaponIndex) return;
            if (EquippedWeapons[1] == null) return;
            UpdateEquippedIndexRpc(NetworkObjectId, 1);
            _itemHandle.RequestWeaponSwapRpc(CurrentWeaponIndex, NetworkObjectId);
            _canvasHandler.UpdateAmmo(EquippedWeapons[CurrentWeaponIndex].currentAmmo, _reserve.ContainersDictionary[EquippedWeapons[CurrentWeaponIndex].WeaponSO.RequiredAmmo].currentCount, true);
            _currentSpeed = groundedMoveSpeed * EquippedWeapons[1].WeaponSO.MovementSpeedMultiplier;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateEquippedIndexRpc(ulong target, int index)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(target, out var targetObj);
        if (!targetObj) return;
        targetObj.GetComponent<PlayerController>().CurrentWeaponIndex = index;
    }

    public void ShootLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex] || IsDead) return;
        EquippedWeapons[CurrentWeaponIndex].UseWeapon();
    }

    public void AimLocalWeapon(bool state)
    {
        if(!EquippedWeapons[CurrentWeaponIndex] || IsDead) return;
        if(!EquippedWeapons[CurrentWeaponIndex].CanADS) return;
        EquippedWeapons[CurrentWeaponIndex].StopAllCoroutines();
        EquippedWeapons[CurrentWeaponIndex].ADS(state);
        if (state)
        {
            SensitivityHandler.Instance.SetScopedSens();
            return;
        }
        SensitivityHandler.Instance.ResetSens();
    }

    public void CancelFireLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex] || IsDead) return;
        EquippedWeapons[CurrentWeaponIndex].CancelFire();
    }
    public void ReloadLocalWeapon()
    {
        if(!EquippedWeapons[CurrentWeaponIndex] || IsDead) return;
        EquippedWeapons[CurrentWeaponIndex].ReloadWeapon();
    }

    public void AllowWeaponPickup(ItemPickup pickup)
    {
        _pickupAvailable = true;
        _currentPickup = pickup;
    }

    public void RemoveWeaponPickup()
    {
        _pickupAvailable = false;
        _currentPickup = null;
    }

    public void InteractWithPickup()
    {
        if (!_currentPickup || !_pickupAvailable) return;
        if (!CheckPickupSimilarity(_currentPickup.CurrentWeapon)) return;
        _currentPickup.PickUpWeapon(this);
    }

    #endregion
    
    #region Health and Armor

    [Rpc(SendTo.ClientsAndHost)]
    public void TakeDamageRpc(float damageToDeal, bool headshot, ulong dealerClientId, ulong dealerNetworkId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(dealerNetworkId, out var castingPlayer);
        if (!castingPlayer) return;

        if(castingPlayer.GetComponent<PlayerController>())
            _lastAttackingPlayer = castingPlayer.GetComponent<PlayerController>();
        CancelInvoke(nameof(ResetLastAttackingPlayer));
        Invoke(nameof(ResetLastAttackingPlayer), lastAttackingPlayerGraceTime);
        
        if(!IsOwner || IsDead) return;
        
        //Damage math logic
        var armorDamage = damageToDeal * armorDamping;
        var playerDamage = CurrentArmor > 0 ? damageToDeal - armorDamage : damageToDeal;
        
        if (CurrentArmor > 0)
        {
            if (CurrentArmor - (int)armorDamage <= 0)
            {
                var remainder = (int)armorDamage - CurrentArmor;
                CurrentArmor = 0;
                CurrentHealth -= remainder;
            }
            else
            {
                CurrentArmor -= (int)armorDamage;
            }
        }
        else
        {
            CurrentArmor = 0;
        }
        
        

        CurrentHealth -= (int)playerDamage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            StartCoroutine(HandleDeath(dealerClientId, dealerNetworkId));
        }
        
        UpdateStats();
        
        
        var camShake = GetComponentInChildren<CameraShake>();
        var weaponSo = castingPlayer?.GetComponentInChildren<WeaponBase>()?.WeaponSO;
        if (weaponSo)
        {
            if (headshot)
            {
                camShake.Shake(weaponSo.HeadshotShakeMagnitude, weaponSo.HeadshotShakeDuration);
            }
            else
            {
                camShake.Shake(weaponSo.HitShakeMagnitude, weaponSo.HitShakeDuration);
            }
        }
        
        var tPos = castingPlayer.transform.position;
        var tRot = castingPlayer.transform.rotation;

        var direction = transform.position - tPos;

        if (direction != Vector3.zero)
        {
            tRot = Quaternion.LookRotation(direction);
            tRot.z = -tRot.y;
            tRot.x = 0;
            tRot.y = 0;
        }

        var currentForwards = new Vector3(0, 0, transform.eulerAngles.y);

        var newRotation = tRot * Quaternion.Euler(currentForwards);
        DisplayDamageIndicator(newRotation);
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

    public void DisplayDamageIndicator(Quaternion rotation)
    {
        //Damage indicator logic
        _canvasHandler.StopAllCoroutines();
        StartCoroutine(_canvasHandler.ShowDamageIndicator(rotation));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ResetStatsRpc()
    {
        CurrentHealth = MaxHealth;
        CurrentArmor = 0;
        IsDead = false;
        
        var localColliders = hitboxes.GetComponentsInChildren<Collider>();
        foreach (var col in localColliders)
        {
            col.enabled = true;
        }

        foreach (var ammo in _reserve.ContainersDictionary)
        {
            _reserve.ContainersDictionary[ammo.Key].ResetAmmo();
        }
        
        var networkTransform = GetComponent<NetworkTransform>();
        networkTransform.Interpolate = true;
        if (!IsOwner)
        {
            EnableBodyVisuals();
        }
        UpdateStats();
        _controller.enabled = true;
        SensitivityHandler.Instance.ResetSens();
        gameObject.layer = _ignoreMask;
    }

    private IEnumerator HandleDeath(ulong castingId, ulong networkId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var castingObj);
        if (!castingObj) yield break;

        if (_lastAttackingPlayer) castingObj = _lastAttackingPlayer.GetComponent<NetworkObject>();
        
        IsDead = true;

        gameObject.layer = _deadMask;
        
        UpdateVisualsOnDeathRpc();

        deathFade.SetActive(true);
        
        EquippedWeapons[CurrentWeaponIndex].gameObject.SetActive(false);
        
        if (castingObj.GetComponent<KillVolume>())
        {
            _canvasHandler.EnableDeathOverlay("The Pit");
            UpdateDeathsOnPitRpc();
        }
        else
        {
            if(IsOwner)
                _itemHandle.UpdateScoreboardAmountsOnKillRpc(OwnerClientId, castingObj.OwnerClientId);
            _canvasHandler.EnableDeathOverlay(castingObj.GetComponent<PlayerData>().PlayerName.Value.ToString());
        }
        
        SpawnAmmoBoxRpc();

        _cameraController.SetDeathCamTarget(castingObj.transform);
        
        yield return _waitForDeathTimer;
        _cameraController.ResetCameraTransform();
        _itemHandle.RespawnSpecificPlayerRpc(NetworkObjectId, castingId);
        _canvasHandler.DisableDeathOverlay();
        ClearEquippedWeaponsRpc();
    }

    private void ResetLastAttackingPlayer()
    {
        _lastAttackingPlayer = null;
    }

    [Rpc(SendTo.Server)]
    private void UpdateDeathsOnPitRpc()
    {
        if(_playerData.PlayerFrags.Value != 0)
            _playerData.PlayerFrags.Value--;
        _playerData.PlayerDeaths.Value++;
    }

    [Rpc(SendTo.Server)]
    private void SpawnAmmoBoxRpc()
    {
        var newPickupObj = NetworkManager.SpawnManager.InstantiateAndSpawn(deathPickup.GetComponent<NetworkObject>(), 0UL, true, false, false, transform.position, Quaternion.identity);
        var newPickup = newPickupObj.GetComponent<AmmoPickup>();
        newPickup.SetUpSingleUse();
        if (_lastAttackingPlayer)
        {
            newPickup.SetAmmoTypeRpc(_lastAttackingPlayer.EquippedWeapons[_lastAttackingPlayer.CurrentWeaponIndex].WeaponSO.RequiredAmmo);
            return;
        }

        newPickup.SetAmmoTypeRpc(EquippedWeapons[CurrentWeaponIndex].WeaponSO.RequiredAmmo);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClearEquippedWeaponsRpc()
    {
        for (var i = 0; i < EquippedWeapons.Length; i++)
        {
            if (!EquippedWeapons[i]) continue;
            EquippedWeapons[i].gameObject.SetActive(false);
            EquippedWeapons[i] = null;
        }
        InventoryFull = false;
        
        _itemHandle.RequestWeaponSpawnRpc(starterWeapon.name, NetworkObjectId);
    }
    
    public void PlayDeathSound(int oldValue, int newValue)
    {
        var randSound = UnityEngine.Random.Range(0, DeathSound.Length);
        if(!IsOwner) return;
        GetComponent<SoundHandler>()?.PlayClipWithStaticPitch(DeathSound[randSound]);
    }

    #endregion

    #region UI Stuff
    
    public void DisplayKillbanner(string name)
    {
        killBanner.StartCoroutine(killBanner.DisplayKillBanner(name));
    }
    
    #endregion

    #region Netcode Functions
    
    [Rpc(SendTo.Server)]
    private void UpdatePingRpc()
    {
        _playerData.PlayerPingMs.Value =
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateVisualsOnDeathRpc()
    {
        //hide equipped weapon on death this should change in the future to have the weapons reset to default
        EquippedWeapons[CurrentWeaponIndex].gameObject.SetActive(false);
        //Additionally, hide the mesh renderers for the client on death
        _controller.enabled = false;
        var localColliders = hitboxes.GetComponentsInChildren<Collider>();
        foreach (var col in localColliders)
        {
            col.enabled = false;
        }

        var networkTransform = GetComponent<NetworkTransform>();
        networkTransform.Interpolate = false;
        if (!IsOwner)
        {
            DisableBodyVisuals();
        }
    }

    [Rpc(SendTo.NotMe)]
    private void ChangeItemHandleTPRpc()
    {
        if(IsOwner) return;
        _itemHandle = GetComponentInChildren<NetworkItemHandler>();
        _itemHandle.transform.parent = thirdPersonItemHandler;
    }

    #endregion

    #region Visual Updates

    private void DisableBodyVisuals()
    {
        var visuals = bodyObj.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in visuals)
        {
            mesh.enabled = false;
        }
        bodyObj.GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
    
    private void EnableBodyVisuals()
    {
        var visuals = bodyObj.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in visuals)
        {
            mesh.enabled = true;
        }
        bodyObj.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }

    private void UpdateColors()
    {
        var visuals = bodyObj.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in visuals)
        {
            mesh.material.color = _playerData.PlayerColor.Value;
        }
        headObj.GetComponent<MeshRenderer>().material.color = Color.white;
        headObj.GetComponentInChildren<SpriteRenderer>().color = _playerData.PlayerColor.Value;
    }

    #endregion
    
    
}
