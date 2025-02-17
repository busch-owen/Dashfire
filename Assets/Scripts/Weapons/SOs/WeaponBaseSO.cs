using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBase", menuName = "Scriptable Objects/WeaponBase")]
public class WeaponBaseSO : ScriptableObject
{
    [field: Header("Weapon Stats"), Space(5)]
    [field: SerializeField] public int AmmoCount { get; private set; }
    [field: SerializeField] public AmmoType RequiredAmmo { get; private set; }
    [field: SerializeField] public int BulletsPerShot { get; private set; }
    [field: SerializeField] public int Damage { get; private set; }
    [field: SerializeField] public float HeadshotMultiplier { get; private set; } = 1f;
    [field: SerializeField] public float ReloadTime { get; private set; }
    [field: SerializeField] public float PullOutTime { get; private set; }
    [field: SerializeField] public bool Automatic { get; private set; }
    [field: SerializeField] public float BulletDistance { get; private set; }
    [field: SerializeField] public float BulletRadius { get; private set; }
    [field: SerializeField] public float FireRate { get; private set; }
    [field: SerializeField] public float ADSSpeed { get; private set; }
    [field: SerializeField] public float ADSFov { get; private set; }
    [field: SerializeField] public float XSpread { get; private set; }
    [field: SerializeField] public float YSpread { get; private set; }
    [field: SerializeField] public float SpreadVariation { get; private set; }
    
    [field: SerializeField] public float MovementSpeedMultiplier { get; private set; } = 1f;
    
    [field: Space(5), Header("Weapon Effects"), Space(5)]

    [field: SerializeField] public GameObject objHitEffect { get; private set; }
    [field: SerializeField] public GameObject playerHitEffect { get; private set; }
    
    [field: SerializeField] public float FireShakeDuration { get; private set; }
    [field: SerializeField] public float FireShakeMagnitude { get; private set; }
    [field: SerializeField] public float HitShakeDuration { get; private set; }
    [field: SerializeField] public float HitShakeMagnitude { get; private set; }
    [field: SerializeField] public float HeadshotShakeDuration { get; private set; }
    [field: SerializeField] public float HeadshotShakeMagnitude { get; private set; }
    
    [field: Space(5), Header("Weapon Sounds"), Space(5)]
    [field: SerializeField] public AudioClip[] ShootSounds { get; private set; }
    [field: SerializeField] public AudioClip[] EquipSounds { get; private set; }

    [field: Space(5), Header("Crosshair Attributes"), Space(5)] 
    
    [field: SerializeField] public Sprite CrosshairSprite { get; private set; }
    
    [field: SerializeField] public Sprite UIIcon { get; private set; }
}
