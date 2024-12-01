using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageIndicator : MonoBehaviour
{
    private Rigidbody _rb;
    private TMP_Text _displayText;

    private Transform _targetCamera;

    private int _numberToDisplay;

    private Color _currentColor;

    [SerializeField] private Color regularColor;
    [SerializeField] private Color headshotColor;

    [SerializeField] private float lifetime;
    [SerializeField] private float launchForce;
    [SerializeField] private float xMax;
    [SerializeField] private float zMax;

    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _displayText = GetComponentInChildren<TMP_Text>();
        var xAmt = Random.Range(-xMax, xMax);
        var zAmt = Random.Range(-zMax, zMax);
        _rb.AddForce(new Vector3(xAmt, 1, zAmt) * launchForce, ForceMode.Impulse);
        Invoke(nameof(Despawn), lifetime);
    }

    private void Despawn()
    {
        PoolManager.Instance.DeSpawn(gameObject);
    }

    private void Update()
    {
        _targetCamera = FindFirstObjectByType<Camera>().transform;
        transform.LookAt(_targetCamera);
    }

    public void UpdateDisplay(int damage, bool headshot, float multiplier)
    {
        _currentColor = headshot ? headshotColor : regularColor;
        var displayDamage = (int)(damage * multiplier);
        _displayText.text = displayDamage.ToString();
        _displayText.color = _currentColor;
    }
}
